using System;
using System.Collections.Generic;
using System.Linq;

namespace gmod_typescript
{
    public class EnumerationTemplate : WikiTemplate
    {

        public string Description
        {
            get => GetValue("Description");
        }

        public bool MembersOnly
        {
            get => !Fields.Any(f => f.MembersOnly == false);
        }

        public List<EnumFieldTemplate> Fields
        {
            get => Article.GetTemplates<EnumFieldTemplate>("EnumField");
        }


        public EnumerationTemplate(string raw, Article article) : base(raw, article)
        {

        }

        public override string ToString()
        {
            string fieldsString = string.Join("\n", Fields.Select(field => field.ToDocString() + field));
            string indent = new String(' ', 4);
            fieldsString = indent + fieldsString.Remove(fieldsString.Length - 1).Replace("\n", $"\n{indent}");
            return fieldsString;
        }

        public override string ToDocString()
        {
            return $" * {Description}\n";
        }
    }
}
