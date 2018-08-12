using System;
namespace gmod_typescript
{
    public class RetTemplate : TypedTemplate
    {

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
