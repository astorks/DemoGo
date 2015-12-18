using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoGo.Parser
{
    public class Program
    {
        private IServiceProvider ServiceProvider { get; }

        public Program(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public Task<int> Main(string[] args)
        {
            Console.WriteLine("Hello Async");
            Console.ReadLine();
            return Task.FromResult(0);
        }

        //private static int roundNumber = 0;

        //public static async void Main(string[] args)
        //{
        //    //MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;userid=csgodemos;password=hNY8uwVwpavSINxk8yKv;database=csgodemos;default command timeout=10;connection timeout=5;charset=utf8");
        //    //conn.Open();

        //    var start = DateTime.UtcNow;
        //    using (var demo = new DemoInfo.DemoParser(File.OpenRead("./fuck.dem")))
        //    {
        //        var header = await demo.ParseHeaderAsync();
        //        Console.WriteLine("Tick rate: {0}", demo.TickRate);
        //        //parser.PlayerKilled += Parser_PlayerKilled;
        //        //parser.PlayerBind += Parser_PlayerBind;
        //        //parser.RoundStart += Parser_RoundStart;
        //        //parser.ParseHeader();
        //        //Console.WriteLine("{0}, {1}", parser.TickRate, parser.Header);
        //        //parser.ParseToEnd();
        //    }
        //    var end = DateTime.UtcNow;

        //    Console.WriteLine("Finished in {0}", end - start);
        //    Console.ReadLine();
        //}

        //private static void Parser_RoundStart(object sender, DemoInfo.RoundStartedEventArgs e)
        //{
        //    roundNumber++;
        //    //Console.WriteLine("Round {0} started", roundNumber);
        //}

        //private static void Parser_PlayerBind(object sender, DemoInfo.PlayerBindEventArgs e)
        //{
        //    //Console.WriteLine("{0} connected", e.Player.Name);
        //}

        //private static void Parser_PlayerKilled(object sender, DemoInfo.PlayerKilledEventArgs e)
        //{
        //    //Console.WriteLine("{0} killed {1} with {2}", e.Killer?.Name ?? "Bot", e.Victim?.Name ?? "Bot", e.Weapon?.Weapon);
        //}
    }
}
