using System;
namespace gmod_typescript
{
	public class EnumFieldTemplate : WikiTemplate
    {
        public string Name
        {
            get; set;
        }

        public string Value
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public bool MembersOnly { get; set; }

        public EnumFieldTemplate(string raw, Article article) : base(raw, article)
        {
            Name = GetValue(1);
            // for http://wiki.garrysmod.com/page/Enums/SNDLVL
            if (Name.IndexOf("<br/>") != -1)
            {
                Name = Name.Substring(0, Name.IndexOf("<br/>"));
            }
            MembersOnly = true;
            if (Name.IndexOf('.') != -1)
            {
                Name = Name.Substring(Name.IndexOf('.') + 1);
                MembersOnly = false;
            }
            // TODO For stencilcompare/operations
            Name = Name.Replace("{{#titleparts:{{SUBPAGENAME}}||-1}}", article.Title);

            Value = GetValue(2);

            Description = GetValue(3);
        }

        public override string ToString()
        {
            return $"{Name} = {Value},\n";
        }

        public override string ToDocString()
        {
            string result = "/**\n";
            result += $" * {Description}\n";
            result += " */\n";

            return result;
        }
    }
}
