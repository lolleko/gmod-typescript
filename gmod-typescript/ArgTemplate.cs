using System;
using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class ArgTemplate : TypedTemplate
    {
        public string Name
        {
            get => EscapeName(GetValue("Name"));
        }

        public string Default
        {
            get => GetValue("default");
        }

        public bool IsVarArg
        {
            get => GetValue("Type") == "vararg";
        }

        public bool IsOptional
        {
            get; set;
        }

        public ArgTemplate(string raw, Article article) : base(raw, article)
        {
            IsOptional = Default != "";
        }

        public override string ToString() {
            string vararg = "";
            if (IsVarArg)
            {
                vararg = "...";
            }
            if (IsOptional && !IsVarArg) {
                return $"{vararg}{Name}?: {Type}";
            }
            return $"{vararg}{Name}: {Type}";
        }

        public override string ToDocString()
        {
            if (Default != "")
            {
                return DescriptionToDocComment($"@param {Name} [={Default}] {Description}");
            }
            return DescriptionToDocComment($"@param {Name} {Description}");
        }
    }
}
