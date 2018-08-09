using System;
namespace gmod_typescript
{
    public class FunctionTemplate : WikiTemplate
    {

        public string Description
        {
            get => GetValue("Description");
        }

        public string Realm
        {
            get => GetValue("Realm");
        }

        public string IsClass
        {
            get => GetValue("IsClass");
        }

        public string Name
        {
            get => GetValue("Name");
        }

        public FunctionTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            return Name;
        }

        public override string ToDocString()
        {
            return $" * {Description}\n";
        }
    }
}
