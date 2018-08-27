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

            var x = new Scrapper();
            var trans = new TransformerTypescript(false);
            var seri = new SerializerTypescript();

            File.WriteAllText("../out/index.json", JsonType.Serialize.ToJson(trans.Transform(x.Data)));



            string defaults = @"interface table {
[key: string]: any;
}
type thread = any;
type pixelvis_handle_t = any;
type sensor = any;
//type Vec = number;
//type Ang = number;
type userdata = any;
declare var SERVER: boolean;
declare var CLIENT: boolean;
declare var GM: Gamemode;
declare var GAMEMODE: Gamemode;
";

            File.WriteAllText("../out/index.d.ts", defaults + seri.Serialize(x.Data, trans));

            Console.WriteLine("done");
        }
    }
}
