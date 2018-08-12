using System;
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
            ServicePointManager.DefaultConnectionLimit = 50000;
            System.Net.ServicePointManager.MaxServicePointIdleTime = 2000;
            System.Net.ServicePointManager.MaxServicePoints = 1000;
            System.Net.ServicePointManager.SetTcpKeepAlive(false, 0, 0);

            File.WriteAllText("index.d.ts", "");
            Console.WriteLine("Fetching stuff");

            Console.WriteLine(new StructureArticle("Structures/GM"));

            WriteResults().Wait();

            Console.WriteLine("done");
            Console.ReadKey();
        }

        public static async Task WriteResults()
        {
            var enumsTask = Task.Run(() => new EnumerationsCategory());
            var classTask = Task.Run(() => new ClassAndHooksFunctionsCategory());
            var libTask = Task.Run(() => new LibraryCategory());
            var globalTask = Task.Run(() => new GlobalCategory());

            EnumerationsCategory enums = await enumsTask;
            ClassAndHooksFunctionsCategory classes = await classTask;
            LibraryCategory libs = await libTask;
            GlobalCategory globals = await globalTask;

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
";

            File.WriteAllText("index.d.ts", extraData + enums + libs + globals + classes);
        }
    }
}
