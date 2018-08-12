using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class TypedTemplate : WikiTemplate
    {
        public virtual string TypeRaw
        {
            get => GetValue("Type");
        }

        public string Type
        {
            get
            {
                string type = TypeRaw;
                // search the description for an enum
                if (type == "number")
                {
                    var match = Regex.Match(Description, @"{{Enum\|([\w\W]*?)}}");
                    if (match.Success)
                    {
                        type = match.Groups[1].Value;
                        // TODO Hack because of duplicate identifier PLAYER
                        if (type == "PLAYER")
                        {
                            type = "PLAYER_ANIM";
                        }
                    }
                }
                return EscapeType(type);
            }
        }

        public virtual string Description
        {
            get => GetValue("Desc");
        }

        public TypedTemplate(string raw, Article article) : base(raw, article)
        {

        }
    }
}