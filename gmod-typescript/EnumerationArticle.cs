using System;
namespace gmod_typescript
{
    public class EnumerationArticle : Article
    {
        public EnumerationTemplate Enumeration { get; set; }

        public EnumerationArticle(string url) : base(url)
        {
            Enumeration = GetTemplate<EnumerationTemplate>("Enumeration");
        }

        public override string ToString()
        {
            string result = "/**\n";
            result += Enumeration.ToDocString();
            result += " */\n";
            result += $"enum {Title} {{\n";
            result += Enumeration + "\n";
            result += "}\n";

            return result;
        }
    }
}
