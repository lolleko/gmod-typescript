using System;
namespace gmod_typescript
{
    public class PanelTemplate : WikiTemplate
    {

        public string Description
        {
            get => GetValue("Description");
        }

        public string Parent
        {
            get => GetValue("Parent");
        }

        public PanelTemplate(string raw, Article article) : base(raw, article)
        {

        }
    }
}
