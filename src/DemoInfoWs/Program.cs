using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace DemoInfoWs
{
    public class Program
    {
        public static HttpServer HttpServer { get; private set; }

        public static void Main(string[] args)
        {
            HttpServer = new HttpServer(8086);
        }
    }
}
