using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoGo.Parser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DemoInfo.DemoParser parser = new DemoInfo.DemoParser(File.OpenRead("./fuck.dem"));
            parser.PlayerKilled += Parser_PlayerKilled;
            parser.ParseHeader();
            parser.ParseToEnd();
            Console.WriteLine(parser.TickRate);
            Console.ReadLine();
        }

        private static void Parser_PlayerKilled(object sender, DemoInfo.PlayerKilledEventArgs e)
        {
            Console.WriteLine("{0} killed {1} with {2}", e.Killer.Name, e.Victim.Name, e.Weapon.Weapon);
        }
    }
}
