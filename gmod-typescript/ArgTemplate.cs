using System;
namespace gmod_typescript
{
    public class ArgTemplate : WikiTemplate
    {
        public string Type
        {
            get => EscapeType(GetValue("Type"));
        }

        public string Name
        {
            get => EscapeName(GetValue("Name"));
        }

        public string Description
        {
            get => GetValue("Desc");
        }

        public string Default
        {
            get => GetValue("default");
        }

        public ArgTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString() {
            if (Default != "") {
                return $"{Name}?: {Type}";
            }
            return $"{Name}: {Type}";
        }

        public override string ToDocString()
        {
            if (Default != "")
            {
                return $" * @param {Name} [={Default}] {Description}";
            }
            return $" * @param {Name} {Description}";
        }
    }
}
