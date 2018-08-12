using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gmod_typescript
{
    public class ClassFieldsTemplate : WikiTemplate
    {
        public List<ClassAndStructFieldTemplate> ClassFields
        {
            get => Article.GetTemplates<ClassAndStructFieldTemplate>("ClassField");
        }

        public ClassFieldsTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            return string.Join("\n", ClassFields.Select(field => field.ToDocString() + field));
        }
    }
}
