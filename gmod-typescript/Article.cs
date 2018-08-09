using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace gmod_typescript
{
    public class Article : WikiObject
    {
        public string Url { get; set; }

        public string Raw { get; set; }

        public string Title
        {
            get => Url.Substring(Url.LastIndexOf('/') + 1);
        }

        public Article(string url)
        {
            string escapedUrl = "http://wiki.garrysmod.com/page/" + url.Replace(".", "%2E") + "?action=raw";
            Raw = WikiRequest(escapedUrl);
            Url = url;
        }

        public T GetTemplate<T>(string templateName) where T : WikiTemplate
        {
            Match match = Regex.Match(Raw, @"(?x) {{" + templateName + " ( (?: [^{}]+ | (?<open>{{) | (?<-open>}}) )* (?(open)(?!)) ) }}");
            if (match.Success)
            {
                return (T)Activator.CreateInstance(typeof(T), match.Value, this);
            }
            return null;
        }

        public List<T> GetTemplates<T>(string templateName) where T : WikiTemplate
        {
            MatchCollection matches = Regex.Matches(Raw, @"(?x) {{" + templateName + " ( (?: [^{}]+ | (?<open>{{) | (?<-open>}}) )* (?(open)(?!)) ) }}");
            List<T> templates = (
                from Match match in matches
                where match.Success
                select (T)Activator.CreateInstance(typeof(T), match.Value, this)
            ).ToList();
            return templates.Count > 0 ? templates : null;
        }
    }
}
