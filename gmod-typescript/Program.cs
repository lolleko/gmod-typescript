using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace gmod_typescript
{
    class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;


            Console.WriteLine("Fetching stuff");

            Directory.CreateDirectory("../out");
            Directory.CreateDirectory("../wikiData");

            WriteResults();

            Console.WriteLine("done");
        }

        public static void WriteResults()
        {
            var funcCollection = new FunctionCollection();
            var taskList = new List<Task>();

            taskList.Add(funcCollection.AddEnumerations());
            taskList.Add(funcCollection.AddStructures());
            taskList.Add(funcCollection.AddPanels());

            (string, CategoryType)[] categories = 
            {
                ("Class_Functions", CategoryType.Class),
                ("Library_Functions", CategoryType.Library),
                ("Hooks", CategoryType.Class),
                ("Global", CategoryType.Global)
            };

            foreach (var cat in categories) {
                taskList.Add(funcCollection.AddFunctionsFromCategory(cat.Item1, cat.Item2));
            }

           Task.WhenAll(taskList.ToArray()).Wait();

            string extraData =
@"interface table {
    [key: string]: any;
}
type thread = any;
type pixelvis_handle_t = any;
type sensor = any;
//type Vec = number;
//type Ang = number;
type userdata = any;
type SERVER = boolean;
type CLIENT = boolean;
";
            File.WriteAllText("../out/index.d.ts", extraData + funcCollection.EnumsToString() + funcCollection.StructuresToString() + funcCollection.ClassesToString());
        }
    }
}
