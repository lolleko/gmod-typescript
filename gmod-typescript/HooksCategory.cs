using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class HooksCategory : WikiObject
    {
        public override string ToString()
        {
            var funcs = GetGmodFunctionsByCategory("Hooks");
            var classList = (from funcGroup in funcs.AsParallel()
                             select new FunctionCategoryArticle("Category:" + funcGroup.Key + "_Hooks", funcGroup.AsEnumerable(), CategoryType.Class)).ToList();
            return string.Join("\n", classList);
        }
    }
}
