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

        public Article(string url, string raw = "")
        {
            if (raw != "") {
                Raw = raw;
            } else{
                Raw = WikiRequest("page/" + url.Replace(".", "%2E") + "?action=raw");
            }
            Url = url;
        }

        private List<string> FindTemplates(string templateName)
        {
            List<string> templateStrings = new List<string>();

            int start = 0;
            // We have to check a couple of cases in case tamplateName = "ClassFields" | "ClassField"
            int templateStart = Raw.IndexOf("{{" + templateName + "\n", start);
            if (templateStart == -1)
            {
                templateStart = Raw.IndexOf("{{" + templateName + "|", start);
            }
            if (templateStart == -1)
            {
                templateStart = Raw.IndexOf("{{" + templateName + " ", start);
            }
            while (templateStart != -1)
            {
                int braceCount = 0;
                int templateSize = 0;
                foreach (char c in Raw.Substring(templateStart))
                {
                    if (c == '{')
                    {
                        braceCount++;
                    }
                    else if (c == '}')
                    {
                        braceCount--;
                    }
                    templateSize++;
                    if (braceCount == 0)
                    {
                        break;
                    }
                }
                templateStrings.Add(Raw.Substring(templateStart, templateSize));
                templateStart = Raw.IndexOf("{{" + templateName, templateStart + templateSize);
            }
           
            return templateStrings;
        }

        public T GetTemplate<T>(string templateName) where T : WikiTemplate
        {
            var templateStrings = FindTemplates(templateName);
            return templateStrings.Count > 0 ?
                (T)Activator.CreateInstance(typeof(T), templateStrings[0], this) :
                null;
        }

        public List<T> GetTemplates<T>(string templateName) where T : WikiTemplate
        {
            var templateStrings = FindTemplates(templateName);

            return templateStrings.Count > 0 ?
                templateStrings.Select(t => (T)Activator.CreateInstance(typeof(T), t, this)).ToList() :
                null;
        }
    }
}
