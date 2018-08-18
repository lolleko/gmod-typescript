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

        // Todo this might also require a fix for mutlilne fields???
        public string GetValue(string selector)
        {
            // We have to check a couple of cases
            int valueStart = Raw.IndexOf("|" + selector + "=", StringComparison.CurrentCultureIgnoreCase);
            if (valueStart == -1)
            {
                valueStart = Raw.IndexOf("|" + selector + " =", StringComparison.CurrentCultureIgnoreCase);
            }
            if (valueStart != -1)
            {
                valueStart += selector.Length + 2;
                int braceCount = 0;
                int valueSize = 0;
                foreach (char c in Raw.Substring(valueStart))
                {
                    if (c == '{' || c == '[')
                    {
                        braceCount++;
                    }
                    else if (c == '}' || c == ']')
                    {
                        braceCount--;
                    }
                    // End of field
                    if (braceCount == 0 && c == '|')
                    {
                        break;
                    }
                    // End of template
                    if (braceCount < 0) {
                        break;
                    }
                    valueSize++;
                }
                string result = Raw.Substring(valueStart, valueSize).Trim(' ', '\n');
                return result;
            }


            return "";
        }

        // TODO fix value extraction if: {{Structurefield|abc|Mydescription is awesome and linked to {{ClassFunctions|Entity|FireBullets}} lol}}
        public string GetValue(int selector)
        {
            int bracketsOpen = 0;
            int topLevelPipesEncountered = 0;
            int charsSkipped = 0;
            int valueStart = -1;
            int valueEnd = -1;
            string escapedRaw = Raw.Replace("||", "__?__");
            foreach (char c in escapedRaw) {
                if (c == '{' || c == '[') {
                    bracketsOpen++;
                }
                if (c == '}' || c == ']') {
                    bracketsOpen--;
                }

                if (c == '|' && bracketsOpen == 2) {
                    topLevelPipesEncountered++;
                }

                charsSkipped++;

                if (bracketsOpen == 2 && topLevelPipesEncountered == selector && valueStart == -1) {
                    valueStart = charsSkipped;
                }

                if ((bracketsOpen == 2 && topLevelPipesEncountered == selector + 1) || bracketsOpen == 0)
                {
                    valueEnd = charsSkipped;
                    // drop the brackets at the end
                    if (bracketsOpen == 0) {
                        valueEnd--;
                    }
                    break;
                }
            }

            if (valueStart != -1) {
                string res = escapedRaw.Substring(valueStart, valueEnd - valueStart - 1).Replace("__?__", "||");
                return res;
            }
                
            return "";
        }
    }
}
