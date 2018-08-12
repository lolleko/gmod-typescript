using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace gmod_typescript
{
    class LibraryCategory : WikiObject
    {
        public override string ToString()
        {
            var funcs = GetGmodFunctionsByCategory("Library_Functions");
            var classList = funcs
                .Where(f => f.Key != "util.worldpicker" && f.Key != "jit" && f.Key != "Global")
                .AsParallel()
                .Select(funcGroup => new FunctionCategoryArticle("Category:" + funcGroup.Key, funcGroup.AsEnumerable(), CategoryType.Library)).ToList();
            return string.Join("\n", classList);
        }
    }
}
