using System;
using System.Linq;

namespace gmod_typescript
{
    public class FunctionArticle : Article
    {
        public Returns Returns { get; set; }

        public FunctionTemplate Function { get; set; }

        public Args Arguments { get; set; }

        public Args ArgumentsWithoutOptionals { get; set; }

        public bool RemoveOptionals { get; set; }

        public bool PrependFunc { get; set; }

        public bool PrependDeclare { get; set; }

        public FunctionArticle(string url, bool prependFunction = false, bool prependDeclare = false) : base(url)
        {
            Returns = new Returns(GetTemplates<RetTemplate>("Ret"));

            var args = GetTemplates<ArgTemplate>("Arg");
            Arguments = new Args(args);
            if (args != null)
            {
                var firstOptional = args.SkipWhile(a => !a.IsOptional);
                RemoveOptionals = !firstOptional.All(a => a.IsOptional);

                if (RemoveOptionals)
                {
                    ArgumentsWithoutOptionals = new Args(args.Where(a => !a.IsOptional).ToList());

                    Arguments = new Args(args.TakeWhile(a => !a.IsOptional).Concat(firstOptional.Select(a =>
                        {
                            // Copy elements && make all optional
                            var arg = new ArgTemplate(a.Raw, a.Article)
                            {
                                IsOptional = true
                            };
                            return arg;
                        })).ToList()
                    );
                }
            }


            Function = GetTemplate<FunctionTemplate>("Func");
            if (Function == null)
            {
                Function = GetTemplate<FunctionTemplate>("Hook");
            }
            PrependFunc = prependFunction;
            PrependDeclare = prependDeclare;
        }

        public override string ToString()
        {
            // Default
            string result = "";
            string func = "";
            if (PrependFunc)
            {
                if (PrependDeclare)
                {
                    func += "declare ";
                }
                func += "function ";
            }
            result += "/**\n";
            if (Function != null)
            {
                result += Function.ToDocString();
            }
            if (Arguments != null)
            {
                result += Arguments.ToDocString();
            }
            if (Returns != null)
            {
                result += Returns.ToDocString();
            }
            result += " */\n";
            result += $"{func}{Title}({Arguments}): {Returns};\n";
            if (RemoveOptionals)
            {
                result += "\n";
                result += "/**\n";
                if (Function != null)
                {
                    result += Function.ToDocString();
                }
                if (Arguments != null)
                {
                    result += ArgumentsWithoutOptionals.ToDocString();
                }
                if (Returns != null)
                {
                    result += Returns.ToDocString();
                }
                result += " */\n";
                result += $"{func}{Title}({ArgumentsWithoutOptionals}): {Returns};\n";
            }
            return result;
        }
    }
}
