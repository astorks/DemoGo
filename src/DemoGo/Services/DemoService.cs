using Microsoft.Extensions.Logging;
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
        private ILogger Logger { get; }

        public DemoService(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<DemoService>();
        }

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

        private async Task PostCallback(string callbackUrl, CallbackModel callbackModel)
        {
            Logger.LogInformation("Callback: {0} for demo {1}", callbackUrl, callbackModel.DemoId);
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var callbackContent = new StringContent(JsonConvert.SerializeObject(callbackModel, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }));
                    callbackContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var response = await httpClient.PostAsync(callbackUrl, callbackContent);
                    Logger.LogInformation("Callback: {0} for demo {1}", response.StatusCode, callbackModel.DemoId);
                }
                catch (Exception e)
                {
                    Logger.LogError("Callback: Failed reason={0} for demo {1}", e.Message, callbackModel.DemoId);
                }
            }
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

                    try
                    {
                        var demoParser = new Parser.Parser(demoQueue);
                        Logger.LogInformation("Thread {0} starting demo parsing for demo {1}", Thread.CurrentThread.ManagedThreadId, demoParser.DemoId);
                        demoParser.Parse();
                        Logger.LogInformation("Thread {0} finished demo parsing for demo {1}", Thread.CurrentThread.ManagedThreadId, demoParser.DemoId);

                        if (demoQueue.CallbackUrl != null)
                            await PostCallback(demoQueue.CallbackUrl, new CallbackModel { Success = true, DemoId = demoQueue.DemoId, Demo = demoParser.Demo });
                        else
                            ParsedDemos.Add(demoParser.Demo);
                    }
                    catch(Exception e)
                    {
                        Logger.LogError("Thread {0} encountered an error parsing demo {1}", Thread.CurrentThread.ManagedThreadId, demoQueue.DemoId);
                        await PostCallback(demoQueue.CallbackUrl, new CallbackModel { Success = false, Message = e.Message, DemoId = demoQueue.DemoId });
                    }
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

        public Guid ScheduleDemoParse(string demoUrl, string callbackUrl)
        {
            var demoId = Guid.NewGuid();
            var demoQueue = new DemoQueue { DemoId = demoId, DemoUrl = demoUrl, CallbackUrl = callbackUrl };
            QueuedDemos.Enqueue(demoQueue);
            return demoId;
        }

        public class DemoQueue
        {
            public Guid DemoId { get; set; }
            public string DemoUrl { get; set; }
            public string CallbackUrl { get; set; }
        }

        public class CallbackModel
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Guid DemoId { get; set; }
            public Parser.Demo Demo { get; set; }
        }
    }
}
