using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace DemoGo.Controllers
{
    [Route("/")]
    public class MainController : Controller
    {
        private Services.DemoService DemoService { get; }

        public MainController(Services.DemoService demoService)
        {
            DemoService = demoService;
        }

        public ActionResult Index()
        {
            return Json(new { server = Environment.MachineName, apiVersion = Startup.Configuration["version"] });
        }

        [HttpGet("ondemand")]
        public ActionResult ParseDemo(string demoUrl)
        {
            var demoParser = new Parser.Parser(Guid.NewGuid(), demoUrl);
            demoParser.Parse();
            return Json(new { demoParser.Demo }, new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
        }

        [HttpGet("schedule")]
        public ActionResult ScheduleDemo(string demoUrl, string callbackUrl = null)
        {
            var demoId = DemoService.ScheduleDemoParse(demoUrl, callbackUrl);
            return Json(new { success = true, demoId, callbackUrl });
        }

        [HttpGet("demo/{demoId}")]
        public ActionResult GetDemo(Guid demoId)
        {
            var demo = DemoService.RequestDemo(demoId);
            return Json(new { demo });
        }

        [HttpGet("demo/{demoId}/progress")]
        public ActionResult GetDemoProgress(Guid demoId)
        {
            var demo = DemoService.RequestDemo(demoId);
            var schedulerPosition = DemoService.SchedulePosition(demoId);
            return Json(new { schedulerPosition, progress = demo?.ParsingProgress * 100f });
        }
    }
}
