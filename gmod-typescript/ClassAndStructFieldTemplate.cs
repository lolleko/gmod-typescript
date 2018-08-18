using System;
using System.Collections.Generic;
using System.Text;

namespace gmod_typescript
{
    public class ClassAndStructFieldTemplate : TypedTemplate
    {
        public string Name
        {
            get; set;
        }

        public string Default
        {
            get; set;
        }

        public bool MembersOnly { get; set; }

        public ClassAndStructFieldTemplate(string raw, Article article) : base(raw, article)
        {
            Name = GetValue(2);
            TypeRaw = GetValue(1);
            Description = GetValue(3);
            Default = GetValue(4);
        }

        public override string ToString()
        {
            if (Default != "") {
                return $"{Name}?: {Type};\n";
            }
            return $"{Name}: {Type};\n";
        }

        public override string ToDocString()
        {
            string result = "/**\n";
            result += DescriptionToDocComment(Description);
            if (Default != "")
            {
                result +=  $" * default: {Default}\n";
            }
            result += " */\n";

            return result;
        }
    }
}
