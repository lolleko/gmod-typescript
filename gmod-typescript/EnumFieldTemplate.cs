using System;
namespace gmod_typescript
{
	public class EnumFieldTemplate : WikiTemplate
    {
        public string Name
        {
            get => GetValue(1);
        }

        public string Value
        {
            get => GetValue(2);
        }

        public string Description
        {
            get => GetValue(3);
        }

        public EnumFieldTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            return $"{Name} = {Value},\n";
        }

        public override string ToDocString()
        {
            string result = "/**\n";
            result += $" * {Description}\n";
            result += " */\n";

            return result;
        }
    }
}
