using System;
namespace gmod_typescript
{
    public class FunctionArticle : Article
    {
        public Returns Returns { get; set; }

        public FunctionTemplate Function { get; set; }

        public Args Arguments { get; set; }

        public bool PrependFunc { get; set; }

        public FunctionArticle(string url, bool prependFunction = false) : base(url)
        {
            Returns = new Returns(GetTemplates<RetTemplate>("Ret"));
            Arguments = new Args(GetTemplates<ArgTemplate>("Arg"));
            Function = GetTemplate<FunctionTemplate>("Func");
            if (Function == null) {
                Function = GetTemplate<FunctionTemplate>("Hook");
            }
            PrependFunc = prependFunction;
        }

        public override string ToString()
        {
            string func = "";
            if (PrependFunc) {
                func = "function ";
            }
            return $"{func}{Title}({Arguments}): {Returns};\n";
        }

        public override string ToDocString()
        {
            string result = "/**\n";
            result += Function.ToDocString();
            result += Arguments.ToDocString();
            result += Returns.ToDocString();
            result += " */\n";
            return result;
        }
    }
}
