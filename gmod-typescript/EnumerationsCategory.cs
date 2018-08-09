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
            var enumList = (from enumeration in enums.AsParallel()
                             select new EnumerationArticle(enumeration)).ToList();
            return string.Join("\n", enumList);
        }
    }
}
