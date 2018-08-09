using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace gmod_typescript
{
    public class WikiObject
    {
        protected Dictionary<string, string> escapeTypeDict = new Dictionary<string, string>
        {
            {"function", "Function"},
            {"vararg", "any[]"}
        };

        protected Dictionary<string, string> escapeNameDict = new Dictionary<string, string>
        {
            {"function", "func"},
            {"string", "str"}
        };

        public string EscapeType(string type)
        {
            if (escapeTypeDict.ContainsKey(type))
            {
                return escapeTypeDict[type];
            }
            return type;
        }

        public string EscapeName(string name)
        {
            if (escapeNameDict.ContainsKey(name))
            {
                return escapeNameDict[name];
            }
            return name;
        }

        public string WikiRequest(string url) {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string raw = string.Empty;
            using (StreamReader sr = new StreamReader(data))
            {
                raw = sr.ReadToEnd();
            }
            return raw;
        }

        public IEnumerable<string> GetPagesInCategory(string category)
        {
            string url = $"https://wiki.garrysmod.com/api.php?action=query&list=categorymembers&cmtitle=Category:{category}&cmlimit=1000&format=json";
            string raw = WikiRequest(url);
            var cats = JObject.Parse(raw)["query"]["categorymembers"]
                              .ToArray()
                              .Select(cat => cat["title"].ToString().Replace(' ', '_'))
                              .ToList();
            return cats;
        }

        public ILookup<string, string> GetGmodFunctionsByCategory(string category)
        {
            var topCat = GetPagesInCategory(category);
            var res = topCat.ToLookup(f => f.Substring(0, f.LastIndexOf('/')));
            return res;
        }

        public virtual string ToDocString()
        {
            return "";
        }
   }
}
