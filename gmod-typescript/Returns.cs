using System;
using System.Collections.Generic;
using System.Linq;

namespace gmod_typescript
{
    public class Returns : WikiObject
    {
        public List<RetTemplate> RetList { get; set; }

        public bool IsTupleReturn { get => RetList.Count > 1; }

        public Returns(List<RetTemplate> returns)
        {
            RetList = returns;
        }

        public override string ToString()
        {
            if (RetList == null)
            {
                return "void";
            }
            if (IsTupleReturn)
            {
                return $"[{string.Join(", ", RetList)}]";
            }
            return string.Join("", RetList);
        }

        public override string ToDocString()
        {
            if (RetList == null)
            {
                return "";
            }
            string result = " * @returns ";
            if (IsTupleReturn)
            {
                result += $"[{string.Join(", ", RetList.Select(ret => ret.ToDocString()))}]";
            } else {
                result += string.Join(", ", RetList.Select(ret => ret.ToDocString()));
            }
            result += "\n";
            if (IsTupleReturn) {
                result += " * !TupleReturn\n";
            }
            return result;
        }
    }
}
