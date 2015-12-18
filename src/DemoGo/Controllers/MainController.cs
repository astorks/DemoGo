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

        [HttpGet("force-parse")]
        public ActionResult ForceParse(string demoFile)
        {
            var demoParser = new Parser.Parser(Guid.NewGuid(), demoFile);
            demoParser.Parse();
            return Json(new { demoParser.Demo }, new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
        }

        [HttpGet("schedule")]
        public ActionResult ScheduleDemoParse(string apiKey, string demoFile, string callbackUrl = null)
        {
            //if (apiKey != "cupcake")
            //    return HttpBadRequest();

            var demoId = DemoService.ScheduleDemoParse(demoFile, callbackUrl);
            return Json(new { demoId, callbackUrl });
        }

        [HttpGet("request")]
        public ActionResult RequestDemo(string apiKey, Guid demoId)
        {
            //if (apiKey != "cupcake")
            //    return HttpBadRequest();

            var demo = DemoService.RequestDemo(demoId);
            return Json(new { demo });
        }

        [HttpPost("test")]
        public ActionResult TestCallback()
        {
            return Json(new { success = true });
        }
    }
}
