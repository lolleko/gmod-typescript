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
            if (Enumeration.MembersOnly)
            {
                result += " * !CompileMembersOnly \n";
            }
            result += " */\n";
            // TODO Hack because of duplicate identifier PLAYER
            string name = Title == "PLAYER" ? "PLAYER_ANIM" : Title;
            result += $"declare enum {name} {{\n";
            result += Enumeration + "\n";
            result += "}\n";

            return result;
        }
    }
}
