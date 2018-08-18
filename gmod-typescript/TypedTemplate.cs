using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class TypedTemplate : WikiTemplate
    {
        public string TypeRaw
        {
            get; set;
        }

        public string Type
        {
            get
            {
                List<string> escapedTypes = new List<string>();
                foreach (string type in TypeRaw.Split("|")) {
                    string resType = type.Trim();
                    if (resType == "number")
                    {
                        // search the description for an enum
                        var match = Regex.Match(Description, @"{{Enum\|([\w\W]*?)}}");
                        if (match.Success)
                        {
                            resType = match.Groups[1].Value;
                            // TODO Hack because of duplicate identifier PLAYER
                            if (resType == "PLAYER")
                            {
                                resType = "PLAYER_ANIM";
                            }
                        }
                    } else if (resType == "table")
                    {
                        // search the description for a struct
                        var match = Regex.Match(Description, @"{{Struct\|([\w\W]*?)}}");
                        if (match.Success)
                        {
                            resType = match.Groups[1].Value;
                        }
                    } else if (resType != "table" && resType != "number" && resType != "string") {
                        // search for more type info
                        var matches = Regex.Matches(Description, @"{{Type\|([\w\W]*?)}}");
                        if (matches.Count == 1)
                        {
                            resType = matches[0].Groups[1].Value;
                        }
                    }
                    escapedTypes.Add(EscapeType(resType));
                }
                return string.Join(" | ", escapedTypes);
            }
        }

        public string Description
        {
            get; set;
        }

        public TypedTemplate(string raw, Article article) : base(raw, article)
        {
            Description = GetValue("Desc");
            TypeRaw = GetValue("Type");
        }
    }
}