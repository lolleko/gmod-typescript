using System;
using System.Collections.Generic;
using System.Text;

namespace gmod_typescript
{
    public class ClassAndStructFieldTemplate : TypedTemplate
    {
        public string Name
        {
            get => GetValue(2);
        }

        public override string TypeRaw
        {
            get => GetValue(1);
        }

        public override string Description
        {
            get => GetValue(3);
        }

        public string Default
        {
            get => GetValue(4);
        }

        public bool MembersOnly { get; set; }

        public ClassAndStructFieldTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            return $"{Name}: {Type};\n";
        }

        public override string ToDocString()
        {
            string result = "/**\n";
            result += $" * {Description}\n";
            if (Default != "")
            {
                result +=  $"* default: {Default}\n";
            }
            result += " */\n";

            return result;
        }
    }
}
