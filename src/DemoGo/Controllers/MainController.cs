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
            return Json(new { });
        }

        [HttpGet("parse")]
        public ActionResult ForceParse(string demoUrl)
        {
            var demoParser = new Parser.Parser(Guid.NewGuid(), demoUrl);
            demoParser.Parse();
            return Json(new { demoParser.Demo }, new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });

        }

        [HttpGet("schedule")]
        public ActionResult ScheduleDemoParse(string demoUrl, string callbackUrl = null)
        {
            var demoId = DemoService.ScheduleDemoParse(demoUrl, callbackUrl);
            return Json(new { success = true, demoId, callbackUrl });
        }

        [HttpGet("request")]
        public ActionResult RequestDemo(string apiKey, Guid demoId)
        {
            var demo = DemoService.RequestDemo(demoId);
            return Json(new { demo });
        }

        [HttpPost("test")]
        public ActionResult TestCallback([FromBody]dynamic model)
        {
            return Json(model);
        }
    }
}
