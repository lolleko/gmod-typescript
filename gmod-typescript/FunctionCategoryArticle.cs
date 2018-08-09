using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class FunctionCategoryArticle : Article
    {
        public IEnumerable<FunctionArticle> WikiFunctions { get; set; }

        public CategoryType Type { get; set; }

        public String Extends { get; set; }

        public FunctionCategoryArticle(string url, IEnumerable<string> functions, CategoryType type = CategoryType.Class, string extends = "") : base(url)
        {
            WikiFunctions = from func in functions.AsParallel()
                            select new FunctionArticle(func);
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
                    result += $"class {Title.Replace("Category:", "").Replace("_Hooks", "")}";
                    if (Extends != "")
                    {
                        result += $" extends {Extends}";
                    }
                    result += " {\n";
                    indent = new string(' ', 4);
                    break;
                case CategoryType.Library:
                    result += "namespace {\n";
                    indent = new string(' ', 4);
                    break;
            }
            string body = string.Join("", WikiFunctions.Select(f => f.ToDocString() + f + "\n"));
            result += indent + body.Remove(body.Length - 2).Replace("\n", $"\n{indent}");

            if (Type != CategoryType.Global)
            {
                result += "\n}\n";
            }
            return result;
        }
    }
}
