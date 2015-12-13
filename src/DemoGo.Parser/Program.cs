using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoGo.Parser
{
    public class Program
    {
        private static int roundNumber = 0;

        public static void Main(string[] args)
        {
            DemoInfo.DemoParser parser = new DemoInfo.DemoParser(File.OpenRead("./fuck.dem"));
            parser.PlayerKilled += Parser_PlayerKilled;
            parser.PlayerBind += Parser_PlayerBind;
            parser.RoundStart += Parser_RoundStart;
            parser.ParseHeader();
            Console.WriteLine("{0}, {1}", parser.TickRate, parser.Header);
            parser.ParseToEnd();
            Console.ReadLine();
        }

        private static void Parser_RoundStart(object sender, DemoInfo.RoundStartedEventArgs e)
        {
            roundNumber++;
            Console.WriteLine("Round {0} started", roundNumber);
        }

        private static void Parser_PlayerBind(object sender, DemoInfo.PlayerBindEventArgs e)
        {
            Console.WriteLine("{0} connected", e.Player.Name);
        }

        private static void Parser_PlayerKilled(object sender, DemoInfo.PlayerKilledEventArgs e)
        {
            Console.WriteLine("{0} killed {1} with {2}", e.Killer?.Name ?? "Bot", e.Victim?.Name ?? "Bot", e.Weapon?.Weapon);
        }
    }
}
