using System;
using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class WikiTemplate : WikiObject
    {
        public Article Article { get; set; }

        public string Raw { get; set; }

        public WikiTemplate(string raw, Article article)
        {
            Raw = raw;
            Article = article;
        }

        public string GetValue(string selector)
        {
            Match match = Regex.Match(Raw, @"(?i)\|" + selector + "=(.*)");
            return match.Success ? match.Groups[1].Value : "";
        }

        public string GetValue(int selector)
        {
            MatchCollection matches = Regex.Matches(Raw, @"([\s\w]*)[\|}]");
            if (matches[selector] != null && matches[selector].Success) {
                return matches[selector].Groups[1].Value;
            }
            return "";
        }
    }
}
