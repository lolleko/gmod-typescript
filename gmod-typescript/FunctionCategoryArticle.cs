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

        public String CategoryTitle
        {
            get; set;
        }

        public CategoryType Type { get; set; }

        public String Extends { get; set; }

        public String CustomConstructor { get; set; }

        public ClassFieldsTemplate ClassFields { get => GetTemplate<ClassFieldsTemplate>("ClassFields"); }

        public PanelTemplate Panel { get => GetTemplate<PanelTemplate>("Panel"); }

        public bool IsHook { get => CategoryTitle.Contains("Hooks"); }

        public FunctionCategoryArticle(IGrouping<string, string> functionGroup,
                                       CategoryType type = CategoryType.Class,
                                       string extends = "")
            : this("Category:" + functionGroup.Key, functionGroup.AsEnumerable(), type, extends)
        {

        }

        public FunctionCategoryArticle(string url, IEnumerable<string> functions, CategoryType type = CategoryType.Class, string extends = "") : base(url)
        {
            CategoryTitle = Title.Replace("Category:", "");

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

            AccessModifier access = AccessModifier.None;
            if (IsHook) {
                access = AccessModifier.Protected;
            }

            WikiFunctions = functions
                //.AsParaallel()
                .Select(func => new FunctionArticle(func, prependFunc, prependDeclare, "", access))
                .ToList();
            Type = type;
            Extends = extends;
            if (Panel != null)
            {
                Extends = EscapeType(Panel.Parent);
            }
            CustomConstructor = "";
        }

        public override string ToString()
        {
            string result = "/**\n";
            result += DescriptionToDocComment(Raw);
            if (CustomConstructor != "") {
                result += $" * !CustomConstructor {CustomConstructor}";
            }
            result += " */\n";
            string indent = "";
            switch (Type)
            {
                case CategoryType.Class:
                    result += $"declare class {CategoryTitle.Replace("_Hooks", "").Replace("WEAPON", "SWEP")}";
                    if (Extends != "")
                    {
                        result += $" extends {Extends}";
                    }
                    result += " {\n";
                    indent = new string(' ', 4);
                    break;
                case CategoryType.Library:
                    result += $"declare namespace {CategoryTitle} {{\n";
                    indent = new string(' ', 4);
                    break;
                case CategoryType.Interface:
                    result += $"interface {CategoryTitle} {{\n";
                    indent = new string(' ', 4);
                    break;
            }

            // Body
            string body = "";
            if (ClassFields != null)
            {
                body += ClassFields + "\n";
            }
            body += string.Join("", WikiFunctions.Select(f => f + "\n"));

            // indent
            if (Type != CategoryType.Global && body != "")
            {
                result += indent + body.Remove(body.Length - 2).Replace("\n", $"\n{indent}");
            }
            else
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
