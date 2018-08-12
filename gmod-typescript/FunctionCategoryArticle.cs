using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class FunctionCategoryArticle : Article
    {
        public List<FunctionArticle> WikiFunctions { get; set; }

        public CategoryType Type { get; set; }

        public String Extends { get; set; }

        public ClassFieldsTemplate ClassFields { get => GetTemplate<ClassFieldsTemplate>("ClassFields"); }

        public FunctionCategoryArticle(string url, IEnumerable<string> functions, CategoryType type = CategoryType.Class, string extends = "") : base(url)
        {
            bool prependFunc = false;
            bool prependDeclare = true;
            if (type == CategoryType.Library || type == CategoryType.Global)
            {
                prependFunc = true;
            }

            if (type == CategoryType.Library)
            {
                prependDeclare = false;
            }

            WikiFunctions = functions
                .AsParallel()
                .Select(func => new FunctionArticle(func, prependFunc, prependDeclare))
                .ToList();
            Type = type;
            Extends = extends;
        }

        public override string ToString()
        {
            string result = "/**\n";
            result += $" * {Raw.Replace("\n", "\n * ")} \n";
            result += " */\n";
            string indent = "";
            switch (Type)
            {
                case CategoryType.Class:
                    result += $"declare class {Title.Replace("Category:", "").Replace("_Hooks", "").Replace("WEAPON", "SWEP")}";
                    if (Extends != "")
                    {
                        result += $" extends {Extends}";
                    }
                    result += " {\n";
                    indent = new string(' ', 4);
                    break;
                case CategoryType.Library:
                    result += $"declare namespace {Title.Replace("Category:", "")} {{\n";
                    indent = new string(' ', 4);
                    break;
                case CategoryType.Interface:
                    result += $"interface {Title.Replace("Category:", "")} {{\n";
                    indent = new string(' ', 4);
                    break;
            }

            // Body
            string body = "";
            if (ClassFields != null)
            {
                body += ClassFields;
            }
            body += string.Join("", WikiFunctions.Select(f => f + "\n"));

            // indent
            if (Type != CategoryType.Global)
            {
                result += indent + body.Remove(body.Length - 2).Replace("\n", $"\n{indent}");
            } else
            {
                result += body;
            }

            if (Type != CategoryType.Global)
            {
                result += "\n}\n";
            }
            return result;
        }
    }
}
