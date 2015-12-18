using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DemoGo.Services
{
    public class DemoService
    {
        private bool Started { get; set; }
        private List<Parser.Demo> ParsedDemos { get; } = new List<Parser.Demo>();
        private Queue<DemoQueue> QueuedDemos { get; } = new Queue<DemoQueue>();
        private List<Thread> DemoParserThreads { get; } = new List<Thread>();
        private List<int> DemoParserThreadsShutdown { get; } = new List<int>();

        public void Startup(int threadCount)
        {
            Started = true;

            for (var i = 0; i < threadCount; i++)
                StartNewThread();
        }

        public void StartNewThread()
        {
            var demoParserThread = new Thread(new ThreadStart(DemoParserRun));
            demoParserThread.Start();
            DemoParserThreads.Add(demoParserThread);
        }

        public void Shutdown()
        {
            Started = false;

            foreach (var demoParserThread in DemoParserThreads)
                DemoParserThreadsShutdown.Add(demoParserThread.ManagedThreadId);
        }

        private async void DemoParserRun()
        {
            while(true)
            {
                if (DemoParserThreadsShutdown.Contains(Thread.CurrentThread.ManagedThreadId))
                    break;

                if (QueuedDemos.Count == 0)
                    Thread.Sleep(100);
                else
                {
                    var demoQueue = QueuedDemos.Dequeue();
                    var demoParser = new Parser.Parser(demoQueue);
                    Console.WriteLine("[DemoService] Thread {0} starting demo parsing for demo {1}", Thread.CurrentThread.ManagedThreadId, demoParser.DemoId);
                    demoParser.Parse();
                    Console.WriteLine("[DemoService] Thread {0} finished demo parsing for demo {1}", Thread.CurrentThread.ManagedThreadId, demoParser.DemoId);

                    if(demoQueue.CallbackUrl != null)
                    {
                        Console.WriteLine("[DemoService] Callback: {0} for demo {1}", demoQueue.CallbackUrl, demoParser.DemoId);
                        using (var httpClient = new HttpClient())
                        {
                            try
                            {
                                var response = await httpClient.PostAsync(demoQueue.CallbackUrl, new StringContent(JsonConvert.SerializeObject(demoParser.Demo, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() })));
                                Console.WriteLine("[DemoService] Callback success: {0} for demo {1}", response.StatusCode, demoParser.DemoId);
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("[DemoService] Callback failed: {0} for demo {1}", e.Message, demoParser.DemoId);
                            }
                        }
                    }
                    else
                        ParsedDemos.Add(demoParser.Demo);
                }
            }

            DemoParserThreadsShutdown.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public Parser.Demo RequestDemo(Guid demoId)
        {
            var demo = ParsedDemos.Where(e => e.Id == demoId).FirstOrDefault();
            if (demo != null)
                ParsedDemos.Remove(demo);
            return demo;
        }

        public Guid ScheduleDemoParse(string demoFile, string callbackUrl)
        {
            var demoId = Guid.NewGuid();
            var demoQueue = new DemoQueue { DemoId = demoId, DemoFile = demoFile, CallbackUrl = callbackUrl };
            QueuedDemos.Enqueue(demoQueue);
            return demoId;
        }

        public class DemoQueue
        {
            public Guid DemoId { get; set; }
            public string DemoFile { get; set; }
            public string CallbackUrl { get; set; }
        }
    }
}
