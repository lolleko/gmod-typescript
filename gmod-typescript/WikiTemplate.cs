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
            // remove braces escape and split
            var values = Raw.Substring(2, Raw.Length - 4).Replace("||", "__?__").Split("|");
            if (selector < values.Length)
            {
                return values[selector].Replace("__?__", "||");
            }
            return "";
        }
    }
}
