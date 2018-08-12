using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class EnumerationsCategory : WikiObject
    {
        public override string ToString()
        {
            var enums = GetPagesInCategory("Enumerations");
            // TODO Stencil is just a collection for enums and not valid...
            var enumList = enums
                .Where(e => e != "Enums/STENCIL")
                .AsParallel()
                .Select(e => new EnumerationArticle(e));
            return string.Join("\n", enumList);
        }
    }
}
