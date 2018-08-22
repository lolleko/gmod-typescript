using System;
using System.Collections.Generic;
using System.Text;

namespace gmod_typescript
{
    class StructureArticle : Article
    {
        public StructureTemplate Structure { get; set; }

        public string Name { get; set; }

        public StructureArticle(string url) : base(url)
        {
            Structure = GetTemplate<StructureTemplate>("Structure");
            Name = Title;
        }

        public override string ToString()
        {
            string result = "/**\n";
            result += Structure.ToDocString();
            result += " */\n";
            result += $"interface {Name} {{\n";
            result += Structure + "\n";
            result += "}\n";

            return result;
        }
    }
}
