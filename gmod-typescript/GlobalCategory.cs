using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace gmod_typescript
{
    class GlobalCategory : WikiObject
    {
        public override string ToString()
        {
            // TODO hack because Error declared in JS/TS not to bad since this func is broken in gmod anyway
            return new FunctionCategoryArticle("Category:Global", GetPagesInCategory("Global").Where(f => f != "Global/Error"), CategoryType.Global).ToString();
        }
    }
}
