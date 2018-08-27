using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class SerializerTypescript : Serializer
    {
        public override string SerializeArgument(JsonType.Argument argument)
        {
            string varargDots = argument.IsVarArg ? "..." : "";
            string varargBrackets = argument.IsVarArg ? "[]" : "";
            string optional = argument.IsOptional ? "?" : "";
            string type = argument.IsVarArg ? $"({argument.Type})" : argument.Type;
            return $"{varargDots}{argument.Name}{optional}: {type}{varargBrackets}";
        }

        public string SerializeArgumentDoc(JsonType.Argument argument)
        {
            string defaultVal = "";
            if (argument.IsOptional) {
                defaultVal = $"[={argument.Default}]";
            }
            return DescriptionToDocComment($"@param {argument.Name} {defaultVal} {argument.Description}");
        }

        public override string SerializeEnum(JsonType.Enum _enum)
        {
            string fieldsString = IndentLines(string.Join("\n", _enum.EnumFields.Select(SerializeEnumField)));
            string result = "/**\n";
            result += DescriptionToDocComment(_enum.Description);
            result += " */\n";
            result += $"declare enum {_enum.Name} {{\n{fieldsString}\n}}\n";
            return result;
        }

        public override string SerializeEnumField(JsonType.EnumField enumField)
        {
            string result = $"/**\n";
            result += DescriptionToDocComment(enumField.Description);
            result += " */\n";
            result += $"{enumField.Name} = {enumField.Value},\n";
            return result;
        }

        public override string SerializeExample(JsonType.Example example)
        {
            if (example.Code == "" && example.Description == "") {
                return "";
            }
            example.Code = example.Code.Replace("*/", "").Replace("--", "//");
            return $"@example\n\n{example.Code}\n\n// {example.Description}";
        }

        public override string SerializeField(JsonType.Field field)
        {
            string optional = field.IsOptional ? "?" : "";

            string result = $"/**\n";
            result += DescriptionToDocComment(field.Description);
            result += " */\n";
            result += $"{field.Name}{optional}: {field.Type};\n";
            return result;
        }

        public override string SerializeFunction(JsonType.Function function)
        {
            string returns = "";
            if (function.IsConstructor) {
                returns = "";
            } else if (function.Returns.Count == 0)
            {
                returns = ": void";
            } else if (function.Returns.Count > 1)
            {
                returns = $": [{string.Join(", ", function.Returns.Select(SerializeReturn))}]";
            } else { 
                returns = ": " + SerializeReturn(function.Returns[0]);
            }
            return $"{function.Name}({string.Join(", ", function.Arguments.Select(SerializeArgument))}){returns};\n";
        }

        public string SerializeFunctionDoc(JsonType.Function function)
        {
            string result = "/**\n";
            result += DescriptionToDocComment(function.Description);
            result += string.Join("", function.Arguments.Select(SerializeArgumentDoc));
            if (function.Returns.Count == 1)
            {
                result += DescriptionToDocComment($"@returns {SerializeReturnDoc(function.Returns[0])}");
            }
            else if (function.Returns.Count > 1)
            {
                result += DescriptionToDocComment($"@returns [{string.Join(", ", function.Returns.Select(SerializeReturnDoc))}]");
            }
            result += DescriptionToDocComment(string.Join("", function.Examples.Select(SerializeExample)));
            result += " */\n";

            return result;
        }

        public override string SerializeFunctionCollection(JsonType.FunctionCollection functionCollection)
        {
            string result = "/**\n";
            result += DescriptionToDocComment(functionCollection.Description);
            if (functionCollection.CustomConstructor != "")
            {
                result += $" * !CustomConstructor {functionCollection.CustomConstructor}";
            }
            result += DescriptionToDocComment(string.Join("", functionCollection.Examples.Select(SerializeExample)));
            result += " */\n";
            string indent = "";
            switch (functionCollection.CollectionType)
            {
                case JsonType.CollectionType.Class:
                    result += $"declare class {functionCollection.Name}";
                    if (functionCollection.Extends != "")
                    {
                        result += $" extends {functionCollection.Extends}";
                    }
                    result += " {\n";
                    indent = new string(' ', 4);
                    break;
                case JsonType.CollectionType.Library:
                    result += $"declare namespace {functionCollection.Name} {{\n";
                    indent = new string(' ', 4);
                    break;
            }


            string body = "";
            if (functionCollection.ClassFields.Count > 0)
            {
                body += string.Join("\n", functionCollection.ClassFields.Select(SerializeField)) + "\n";
            }

            body += string.Join("\n", functionCollection.Functions.Select(f => {
                string prefix = JsonType.Extension.SerializeEnum(f.AccessModifier);
                if (functionCollection.CollectionType == JsonType.CollectionType.Library) {
                    prefix = "function";
                } else if (functionCollection.CollectionType == JsonType.CollectionType.Global) {
                    prefix = "declare function";
                }

                return $"{SerializeFunctionDoc(f)}{prefix} {SerializeFunction(f)}";
            }));

            // indent
            if (functionCollection.CollectionType != JsonType.CollectionType.Global && body != "")
            {
                result += IndentLines(body);
            }
            else
            {
                result += body;
            }

            if (functionCollection.CollectionType != JsonType.CollectionType.Global)
            {
                result += "\n}\n";
            }
            return result;
        }

        public override string SerializeReturn(JsonType.Return _return)
        {
            return _return.Type;
        }

        public string SerializeReturnDoc(JsonType.Return _return)
        {
            return _return.Description;
        }

        public override string SerializeStructure(JsonType.Structure structure)
        {
            string fieldsString = string.Join("\n", structure.StructureFields.Select(SerializeField));

            string result = "/**\n";
            result += DescriptionToDocComment(structure.Description);
            result += " */\n";
            result += $"interface {structure.Name} {{\n";
            result += IndentLines(fieldsString) + "\n";
            result += "}\n";

            return result;
        }

        public string DescriptionToDocComment(string desc)
        {
            desc = desc.Replace("<pre>", "```");
            desc = desc.Replace("</pre>", "```");
            desc = desc.Replace("<br/>", "\n");
            desc = desc.Replace("<br />", "\n");
            desc = desc.Replace("<br>", "\n");

            var notes = Scrapper.GetTemplates(desc, "Note");
            notes.ForEach(note =>
            {
                string noteContent = Scrapper.GetTemplateValue(note, 1);
                desc = desc.Replace(note, $"**Note:**\n>{noteContent}\n\n");
            });

            var bugs = Scrapper.GetTemplates(desc, "Bug");
            bugs.ForEach(bug =>
            {
                string bugContent = Scrapper.GetTemplateValue(bug, 2);
                if (bugContent == "")
                {
                    bugContent = Scrapper.GetTemplateValue(bug, 1);
                }
                string bugId = Scrapper.GetTemplateValue(bug, "Issue");
                if (bugId == "") {
                    bugId = Scrapper.GetTemplateValue(bug, "Request");
                }
                if (bugId == "")
                {
                    bugId = Scrapper.GetTemplateValue(bug, "Pull");
                }
                if (bugId == "")
                {
                    bugId = Scrapper.GetTemplateValue(bug, 2);
                    if (int.TryParse(bugId, out int _)) {
                        bugContent = Scrapper.GetTemplateValue(bug, 1);
                    }
                }
                if (int.TryParse(bugId, out int __))
                {
                    desc = desc.Replace(bug, $"**Bug [#{bugId}](https://github.com/Facepunch/garrysmod-issues/issues/{bugId}):**\n>{bugContent}\n");
                } else {
                    if (bugContent == "Fixed=") {
                        bugContent = "FIXED IN NEXT UPDATE: " + Scrapper.GetTemplateValue(bug, 1);
                    }
                    desc = desc.Replace(bug, $"**Bug:**\n>{bugContent}\n");
                }
            });

            var deprecated = Scrapper.GetTemplate(desc, "Deprecated");
            if (deprecated != "") {
                string deprecatedContent = Scrapper.GetTemplateValue(deprecated, 1);
                if (deprecatedContent != "") {
                    desc = desc.Replace(deprecated, $"**Deprecated:**\n>{deprecatedContent}\n\n");
                } else {
                    desc = desc.Replace(deprecated, $"**Deprecated!**\n");
                }
            }

            var internalTemplate = Scrapper.GetTemplate(desc, "Internal");
            if (internalTemplate != "")
            {
                {
                    desc = desc.Replace(internalTemplate, $"**This is an internal function or feature.**\n>This means you will be able to use it, but you really shouldn't.\n\n");
                }
            }

            desc = desc.Trim();

            // Replace more than 2 occurences of newlines
            desc = Regex.Replace(desc, @"\n{2,}", "\n\n");

            if (desc == "") {
                return "";
            }
            return $" * {desc.Replace("\n", "\n * ")} \n";
        }

        public string IndentLines(string lines) {
            string indent = new String(' ', 4);
            return indent + lines.Remove(lines.Length - 1).Replace("\n", $"\n{indent}");
        }
    }
}
