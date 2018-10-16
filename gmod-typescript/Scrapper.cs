using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class Scrapper
    {
        private static HttpClient Client = new HttpClient { BaseAddress = new Uri("http://wiki.garrysmod.com/"), Timeout = TimeSpan.FromSeconds(30) };

        private static int RateLimit = 50;

        private static SemaphoreSlim RateLimitSemaphore = new SemaphoreSlim(20);

        private static Timer RateLimitTimer = new Timer(Scrapper.ResetRateLimit, null, 1000, 1000);

        private static void ResetRateLimit(object state)
        {
            RateLimitSemaphore.Release(RateLimit);
        }

        public JsonType.WikiData Data;

        public Scrapper()
        {
            Data = new JsonType.WikiData
            {
                FunctionCollections = new List<JsonType.FunctionCollection>(),
                Enums = new List<JsonType.Enum>(),
                Structures = new List<JsonType.Structure>()
            };

            var taskList = new List<Task>();

            // Enums
            taskList.Add(Task.Run(() =>
            {
                var enumPages = GetPagesInCategory("Enumerations");
                Data.Enums.AddRange(enumPages.Select(ParseEnum));
            }));

            // Structures
            taskList.Add(Task.Run(() =>
            {
                var structPages = GetPagesInCategory("Structures");
                Data.Structures.AddRange(structPages.Select(ParseStructure));
            }));

            // Function collections
            taskList.Add(Task.Run(() =>
            {
                var globalCollection = ParseFunctionCollection("Global", GetPagesInCategory("Global"));
                globalCollection.CollectionType = JsonType.CollectionType.Global;
                Data.FunctionCollections.Add(globalCollection);
            }));

            taskList.Add(Task.Run(() =>
            {
                var hooks = GetGmodFunctionsByCategory("Hooks");
                Data.FunctionCollections.AddRange(hooks.Select(grp => ParseFunctionCollection(grp.Key, grp.AsEnumerable())));
            }));

            taskList.Add(Task.Run(() =>
            {
                // Remove Panel because Panel is already retrieved by the panel task
                var classes = GetGmodFunctionsByCategory("Class_Functions").Where(cl => cl.Key != "Panel");
                Data.FunctionCollections.AddRange(classes.Select(grp => ParseFunctionCollection(grp.Key, grp.AsEnumerable())));
            }));

            taskList.Add(Task.Run(() =>
            {
                var libs = GetGmodFunctionsByCategory("Library_Functions");
                Data.FunctionCollections.AddRange(libs.Select(grp =>
                {
                    var coll = ParseFunctionCollection(grp.Key, grp.AsEnumerable());
                    coll.CollectionType = JsonType.CollectionType.Library;
                    return coll;
                }));
            }));

            taskList.Add(Task.Run(() =>
            {
                var panels = GetPagesInCategory("Panels").Distinct();
                Data.FunctionCollections.AddRange(panels.Select(p => ParseFunctionCollection(p, GetPagesInCategory(p))));
            }));

            Task.WhenAll(taskList.ToArray()).Wait();
        }

        public JsonType.Argument ParseArgument(string raw)
        {
            string rawName = GetTemplateValue(raw, "Name");
            string rawType = GetTemplateValue(raw, "Type");
            string defaultValue = GetTemplateValue(raw, "default");
            string description = GetTemplateValue(raw, "Desc");
            bool isVarArg = rawType == "vararg" || rawName.Contains("...");
            return new JsonType.Argument
            {
                Name = EscapeName(rawName),
                Description = description,
                Default = defaultValue,
                IsOptional = defaultValue != "" && !isVarArg,
                IsVarArg = isVarArg,
                Type = EscapeType(rawType, description)
            };
        }

        public JsonType.Return ParseReturn(string raw)
        {
            string rawType = GetTemplateValue(raw, "Type");
            string description = GetTemplateValue(raw, "Desc");
            return new JsonType.Return
            {
                Description = description,
                Type = EscapeType(rawType, description)
            };
        }

        public JsonType.Function ParseFunction(string url)
        {
            string raw = GetArticle(url);
            string rawType = GetTemplateValue(raw, "Type");
            if (raw.Contains("#REDIRECT")) {
                raw = GetArticle(Regex.Match(raw, @"#REDIRECT \[\[(.*?)\]\]").Groups[1].Value);
            }
            string description = GetTemplateValue(raw, "Description");
            if (raw.Contains("Not a function")) {
                return new JsonType.Function
                {
                    Name = UrlToTitle(url),
                    Description = raw,
                    Realm = JsonType.Realm.SharedAndMenu,
                    Examples = new List<JsonType.Example>(),
                    IsConstructor = false,
                    AccessModifier = JsonType.AccessModifier.Public,
                    Arguments = new List<JsonType.Argument>(),
                    Returns = new List<JsonType.Return>()
                };
            }
            string realm = GetTemplateValue(raw, "Realm");
            if (realm == "") {
                realm = "Client";
            }
            return new JsonType.Function
            {
                Name = UrlToTitle(url),
                Description = description,
                Realm = JsonType.Extension.DeserializeEnum<JsonType.Realm>(realm), //(QuickType.Realm)QuickType.RealmConverter.Singleton.ReadJson(new JTokenReader(realm.ToLower()), null, null, new JsonSerializer()),
                Examples = GetTemplates(raw, "Example").Select(ParseExample).ToList(),
                IsConstructor = false,
                AccessModifier = JsonType.AccessModifier.Public,
                Arguments = GetTemplates(raw, "Arg").Select(ParseArgument).ToList(),
                Returns = GetTemplates(raw, "Ret").Select(ParseReturn).ToList()
            };
        }

        public JsonType.FunctionCollection ParseFunctionCollection(string category, IEnumerable<string> functionList)
        {
            string raw = GetArticle("Category:" + category);
            string description = raw;
            string extends = "";
            string panel = GetTemplate(raw, "Panel");
            if (panel != "") {
                description = GetTemplateValue(panel, "Description");
                extends = GetTemplateValue(panel, "Parent");
            }
            return new JsonType.FunctionCollection
            {
                Name = category.Replace("_Hooks", ""),
                IsHook = category.Contains("_Hooks"),
                IsPureAbstract = (panel != ""),
                CollectionType = JsonType.CollectionType.Class,
                Description = description,
                Examples = GetTemplates(raw, "Example").Select(ParseExample).ToList(),
                Extends = extends,
                ClassFields = GetTemplates(raw, "ClassField").Select(ParseField).ToList(),
                Functions = functionList.Select(ParseFunction).ToList(),
                CustomConstructor = ""
            };
        }

        public JsonType.Field ParseField(string raw) {
            string rawType = GetTemplateValue(raw, 1);
            string description = GetTemplateValue(raw, 3);
            string defaultVal = GetTemplateValue(raw, 4);
            return new JsonType.Field
            {
                Name = GetTemplateValue(raw, 2),
                Type = EscapeType(rawType, description),
                Default = defaultVal,
                Description = description,
                IsOptional = defaultVal != ""
            };
        }

        public JsonType.Enum ParseEnum(string url) {
            string raw = GetArticle(url);
            List<JsonType.EnumField> fields = GetTemplates(raw, "EnumField").Select(ParseEnumField).ToList();
            fields.ForEach(field => field.Name = field.Name.Replace("{{#titleparts:{{SUBPAGENAME}}||-1}}", UrlToTitle(url)));
            var membersOnly = !fields.Any(field => field.Name.IndexOf('.') != -1);
            if (!membersOnly) {
                fields.ForEach(field => field.Name = field.Name.Substring(field.Name.IndexOf('.') + 1));
            }
            return new JsonType.Enum
            {
                Name = UrlToTitle(url),
                Description = GetTemplateValue(raw, "Description"),
                EnumFields = fields,
                IsMembersOnly = membersOnly
            };
        }

        public JsonType.EnumField ParseEnumField(string raw) {
            string rawName = GetTemplateValue(raw, 1);
            // TODO hack for http://wiki.garrysmod.com/page/Enums/SNDLVL
            if (rawName.IndexOf("<br/>") != -1)
            {
                rawName = rawName.Substring(0, rawName.IndexOf("<br/>"));
            }
            return new JsonType.EnumField
            {
                Name = rawName,
                Value = long.Parse(GetTemplateValue(raw, 2)),
                Description = GetTemplateValue(raw, 3)
            };
        }

        public JsonType.Example ParseExample(string raw) {
            return new JsonType.Example
            {
                Code = GetTemplateValue(raw, "Code"),
                Description = GetTemplateValue(raw, "Description")
            };
        }

        public JsonType.Structure ParseStructure(string url) {
            string raw = GetArticle(url);
            return new JsonType.Structure
            {
                Name = UrlToTitle(url),
                Description = GetTemplateValue(raw, "Description"),
                StructureFields = GetTemplates(raw, "StructureField").Select(ParseField).ToList()
            };
        }

        private static List<string> FindTemplates(string raw, string templateName)
        {
            List<string> templateStrings = new List<string>();

            int start = 0;
            // We have to check a couple of cases in case tamplateName = "ClassFields" | "ClassField"
            int templateStart = raw.IndexOf("{{" + templateName + "\n", start);
            if (templateStart == -1)
            {
                templateStart = raw.IndexOf("{{" + templateName + "|", start);
            }
            if (templateStart == -1)
            {
                templateStart = raw.IndexOf("{{" + templateName + " ", start);
            }
            if (templateStart == -1)
            {
                templateStart = raw.IndexOf("{{" + templateName + "", start);
            }
            while (templateStart != -1)
            {
                int braceCount = 0;
                int templateSize = 0;
                foreach (char c in raw.Substring(templateStart))
                {
                    if (c == '{')
                    {
                        braceCount++;
                    }
                    else if (c == '}')
                    {
                        braceCount--;
                    }
                    templateSize++;
                    if (braceCount == 0)
                    {
                        break;
                    }
                }
                templateStrings.Add(raw.Substring(templateStart, templateSize));
                templateStart = raw.IndexOf("{{" + templateName, templateStart + templateSize);
            }

            return templateStrings;
        }

        public static string GetTemplate(string raw, string templateName)
        {
            var templateStrings = FindTemplates(raw, templateName);
            return templateStrings.Count > 0 ? templateStrings[0] : "";
        }

        public static List<string> GetTemplates(string raw, string templateName)
        {
            return FindTemplates(raw, templateName);
        }

        public static string GetTemplateValue(string raw, string selector)
        {
            // We have to check a couple of cases
            int valueStart = raw.IndexOf("|" + selector + "=", StringComparison.CurrentCultureIgnoreCase);
            if (valueStart == -1)
            {
                valueStart = raw.IndexOf("|" + selector + " =", StringComparison.CurrentCultureIgnoreCase);
            }
            if (valueStart != -1)
            {
                valueStart += selector.Length + 2;
                int braceCount = 0;
                int valueSize = 0;
                foreach (char c in raw.Substring(valueStart))
                {
                    if (c == '{' || c == '[')
                    {
                        braceCount++;
                    }
                    else if (c == '}' || c == ']')
                    {
                        braceCount--;
                    }
                    // End of field
                    if (braceCount == 0 && c == '|')
                    {
                        break;
                    }
                    // End of template
                    if (braceCount < 0)
                    {
                        break;
                    }
                    valueSize++;
                }
                string result = raw.Substring(valueStart, valueSize).Trim(' ', '\n');
                return result;
            }


            return "";
        }

        public static string GetTemplateValue(string raw, int selector)
        {
            int bracketsOpen = 0;
            int topLevelPipesEncountered = 0;
            int charsSkipped = 0;
            int valueStart = -1;
            int valueEnd = -1;
            string escapedRaw = raw.Replace("||", "__?__");
            foreach (char c in escapedRaw)
            {
                if (c == '{' || c == '[')
                {
                    bracketsOpen++;
                }
                if (c == '}' || c == ']')
                {
                    bracketsOpen--;
                }

                if (c == '|' && bracketsOpen == 2)
                {
                    topLevelPipesEncountered++;
                }

                charsSkipped++;

                if (bracketsOpen == 2 && topLevelPipesEncountered == selector && valueStart == -1)
                {
                    valueStart = charsSkipped;
                }

                if ((bracketsOpen == 2 && topLevelPipesEncountered == selector + 1) || bracketsOpen == 0)
                {
                    valueEnd = charsSkipped;
                    // drop the brackets at the end
                    if (bracketsOpen == 0)
                    {
                        valueEnd--;
                    }
                    break;
                }
            }

            if (valueStart != -1)
            {
                string res = escapedRaw.Substring(valueStart, valueEnd - valueStart - 1).Replace("__?__", "||");
                return res;
            }

            return "";
        }

        protected Dictionary<string, string> escapeTypeDict = new Dictionary<string, string>
        {
            {"function", "Function"},
            {"vararg", "any"},
            {"nil", "undefined"}
        };

        protected Dictionary<string, string> escapeNameDict = new Dictionary<string, string>
        {
            {"function", "func"},
            {"var", "variable"},
            {"string", "str"},
            {"class", "classRef"},
            {"new", "newVal"},
            {"default", "defaultVal"}
        };

        public string EscapeTypeName(string type)
        {
            if (escapeTypeDict.ContainsKey(type))
            {
                type = escapeTypeDict[type];
            }
            return EscapeSpecialChars(type);
        }

        public string EscapeName(string name)
        {
            if (escapeNameDict.ContainsKey(name))
            {
                name = escapeNameDict[name];
            }
            return EscapeSpecialChars(name);
        }

        private string EscapeSpecialChars(string str)
        {
            // if function type is determined by arrow func dont replace spaces
            if (!(str.Contains("(") && str.Contains(")") && str.Contains("=>")))
            {
                str = str.Replace(" ", "_");
            }
            return str.Replace("/", "_").Replace("...", "args");
        }

        public string EscapeType(string rawType, string description = "")
        {
            List<string> escapedTypes = new List<string>();

            if (description != "")
            {
                Func<string, IEnumerable<string>> regexToTypeList = (regex) =>
                Regex.Matches(description, regex)
                     .OfType<Match>()
                     .Where(m => m.Success)
                     .Select(m => EscapeTypeName(m.Groups[1].Value.Trim()));

                rawType = rawType.Trim();
                if (rawType == "number")
                {
                    var vals = regexToTypeList(@"{{Enum\|([\w\W]*?)}}")
                        .Where(val => val != "STENCIL")
                        .Select(val => val == "PLAYER" ? "PLAYER_ANIM" : val);
                    escapedTypes.AddRange(vals);
                }
                else if (rawType == "table")
                {
                    if (description.Contains("table of tables", StringComparison.CurrentCultureIgnoreCase)) {
                        escapedTypes.Add("table[]");
                    }
                    else if (description.Contains("table of", StringComparison.CurrentCultureIgnoreCase) || description.Contains("list of", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var structTypes = regexToTypeList(@"{{Struct\|([\w\W]*?)}}");
                        var types = regexToTypeList(@"{{Type\|([\w\W]*?)}}");

                        if (structTypes.Count() > 0)
                        {
                            escapedTypes.Add(structTypes.First() + "[]");
                        } else if (types.Count() > 0)
                        {
                            escapedTypes.Add(types.First() + "[]");
                        }
                    } else
                    {
                        escapedTypes.AddRange(regexToTypeList(@"{{Struct\|([\w\W]*?)}}"));
                    }
                }
                else if (rawType != "table" && rawType != "number" && rawType != "string")
                {
                    escapedTypes.AddRange(regexToTypeList(@"{{Type\|([\w\W]*?)}}"));
                }
            }

            if (escapedTypes.Count == 0)
            {
                escapedTypes.Add(EscapeTypeName(rawType));
            }

            return string.Join(" | ", escapedTypes);
        }

        public string UrlToTitle(string url) {
            return url.Substring(url.LastIndexOf('/') + 1);
        }

        public string WikiRequest(string url)
        {
            Console.WriteLine(url);
            string fileName = url;
            List<char> invalidChars = new List<char> { '=', '/', ':', '&', '?', '.' };
            invalidChars.ForEach(c => fileName = fileName.Replace(c, '_'));
            // Escape uppercases
            fileName = Regex.Replace(fileName, @"([A-Z])", "_$1");
            string filePath = "../wikiData/" + fileName;
            if (!File.Exists(filePath))
            {
                RateLimitSemaphore.Wait();
                try
                {
                    string pageBody = Client.GetStringAsync(url).Result;
                    File.WriteAllText(filePath, pageBody);
                }
                catch
                {
                    throw new Exception("Error while requesting: " + url + " (uri: " + new Uri(Client.BaseAddress, url) + ") / " + filePath);
                }
            }
            return File.ReadAllText(filePath);
        }

        public IEnumerable<string> GetPagesInCategory(string category)
        {
            string url = $"api.php?action=query&list=categorymembers&cmtitle=Category:{category}&cmlimit=10000&format=json";
            string raw = WikiRequest(url);
            var cats = JObject.Parse(raw)["query"]["categorymembers"]
                              .ToArray()
                              .Select(cat => cat["title"].ToString().Replace(' ', '_').Replace("Category:", ""))
                              .ToList();
            return cats;
        }

        public ILookup<string, string> GetGmodFunctionsByCategory(string category)
        {
            var topCat = GetPagesInCategory(category);
            // TODO hook hack
            string hooks = "";
            if (category == "Hooks")
            {
                hooks = "_Hooks";
            }
            var res = topCat.ToLookup(f => f.Substring(0, f.LastIndexOf('/')) + hooks);
            return res;
        }

        public string GetArticle(string url) {
            string pageQuery = WikiRequest("api.php?action=query&prop=revisions&rvprop=content&format=json&titles=" + url);
            var pagesObj = (JObject)JObject.Parse(pageQuery)["query"]["pages"];
            return pagesObj.Values().First()["revisions"][0]["*"].ToString();
        }
    }
}
