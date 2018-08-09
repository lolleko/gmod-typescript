using System;
namespace gmod_typescript
{
    public class RetTemplate : WikiTemplate
    {
        public string Type
        {
            get => EscapeType(GetValue("Type"));
        }

        public string Description
        {
            get => GetValue("Desc");
        }

        public RetTemplate(string raw, Article article) : base(raw, article)
        {
            
        }

        public override string ToString()
        {
            return $"{Type}";
        }

        public override string ToDocString()
        {
            return $"{Description}";
        }
    }
}
