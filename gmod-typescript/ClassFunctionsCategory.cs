using System;
using System.Linq;

namespace gmod_typescript
{
    public class ClassFunctionsCategory : WikiObject
    {
        public override string ToString()
        {
            var funcs = GetGmodFunctionsByCategory("Class_Functions");
            var classList = (from funcGroup in funcs.AsParallel()
                             select new FunctionCategoryArticle("Category:" + funcGroup.Key, funcGroup.AsEnumerable(), CategoryType.Class)).ToList();
            return string.Join("\n", classList);
        }
    }
}
