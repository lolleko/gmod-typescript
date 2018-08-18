using System;
using System.Collections.Generic;
using System.Linq;

namespace gmod_typescript
{
    public class Args : WikiObject
    {

        public List<ArgTemplate> Arguments { get; set; }

        public Args(List<ArgTemplate> arguments)
        {
            Arguments = arguments;
        }

        public override string ToString()
        {
            if (Arguments == null) {
                return "";
            }
            return $"{string.Join(", ", Arguments)}";
        }

        public override string ToDocString()
        {
            if (Arguments == null)
            {
                return "";
            }
            return string.Join("", Arguments.Select(arg => arg.ToDocString()));
        }

    }
}
