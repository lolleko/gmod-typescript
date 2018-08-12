using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gmod_typescript
{
    class StructureTemplate : WikiTemplate
    {
        public List<ClassAndStructFieldTemplate> StructureFields
        {
            get => Article.GetTemplates<ClassAndStructFieldTemplate>("StructureField");
        }

        public string Description
        {
            get => GetValue("Description");
        }

        public StructureTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            return string.Join("\n", StructureFields.Select(field => field.ToDocString() + field));
        }

        public override string ToDocString()
        {
            return $" * {Description}\n";
        }
    }
}
