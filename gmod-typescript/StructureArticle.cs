using System;
using System.Collections.Generic;
using System.Text;

namespace gmod_typescript
{
    class StructureArticle : Article
    {
        public StructureTemplate Structure { get; set; }

        public StructureArticle(string url) : base(url)
        {
            Structure = GetTemplate<StructureTemplate>("Structure");
        }

        public override string ToString()
        {
            string result = "/**\n";
            result += Structure.ToDocString();
            result += " */\n";
            result += $"interface {Title} {{\n";
            result += Structure + "\n";
            result += "}\n";

            return result;
        }
    }
}
