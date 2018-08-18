using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace gmod_typescript
{
    public class WikiObject
    {
        private static HttpClient Client = new HttpClient { BaseAddress = new Uri("http://wiki.garrysmod.com/"), Timeout = TimeSpan.FromSeconds(30) };

        private static int RateLimit = 50;

        private static SemaphoreSlim RateLimitSemaphore = new SemaphoreSlim(20);

        private static Timer RateLimitTimer = new Timer(WikiObject.ResetRateLimit, null, 1000, 1000);

        private static void ResetRateLimit(object state) {
            RateLimitSemaphore.Release(RateLimit);
        }

        protected Dictionary<string, string> escapeTypeDict = new Dictionary<string, string>
        {
            {"function", "Function"},
            {"vararg", "any[]"},
            //{"Vector", "Vec"},
            //{"Angle", "Ang"},
            //{"Entity", "ENTITY"},
            //{"Player", "PLAYER"},
            //{"Player", "Ply"},
            //{"Weapon", "SWEP"},
            //{"NPC", "ENTITY"},
            //{"NextBot", "NEXTBOT"},
            //{"Panel", "PANEL"}
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

        public string EscapeType(string type)
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
            if (!(str.Contains("(") && str.Contains(")") && str.Contains("=>"))) {
                str = str.Replace(" ", "_");
            }
            return str.Replace("/", "_").Replace("...", "args");
        }

        public string DescriptionToDocComment(string desc)
        {
            return $" * {desc.Replace("\n", "\n * ")} \n";
        }

        public static string WikiRequest(string url)
        {
            Console.WriteLine(url);
            string fileName = Convert.ToBase64String(Encoding.UTF8.GetBytes(WebUtility.UrlEncode(url.Replace(' ', '_'))));
            string filePath = "../wikiData/" + fileName;
            if (!File.Exists(filePath)) {
                RateLimitSemaphore.Wait();
                try {
                    string pageBody = Client.GetStringAsync(Uri.EscapeUriString(url)).Result;
                    File.WriteAllText(filePath, pageBody);
                } catch {
                    throw new Exception("Error while requesting: " + url + " (uri: " + new Uri(Client.BaseAddress, url) + ") / "+ filePath);
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
                              .Select(cat => cat["title"].ToString().Replace(' ', '_'))
                              .ToList();
            return cats;
        }

        public ILookup<string, string> GetGmodFunctionsByCategory(string category)
        {
            var topCat = GetPagesInCategory(category);
            // TODO hock hack
            string hooks = "";
            if (category == "Hooks") {
                hooks = "_Hooks";
            }
            var res = topCat.ToLookup(f => f.Substring(0, f.LastIndexOf('/')) + hooks);
            return res;
        }

        public virtual string ToDocString()
        {
            return "";
        }
    }
}
