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
        private Services.ApiAuthenticationService AuthService { get; }

        public MainController(Services.DemoService demoService, Services.ApiAuthenticationService authService)
        {
            DemoService = demoService;
            AuthService = authService;
        }

        public ActionResult Index()
        {
            return Json(new { AuthService.CurrentlyAuthenticated?.Name });
        }

        [HttpGet("apitoken/list")]
        public ActionResult ListTokens()
        {
            return Json(AuthService.ApiAuthenticationList);
        }


        [HttpGet("apitoken/new")]
        public ActionResult NewToken(string name, string permissions = "schedule,parse")
        {
            if (AuthService.CurrentlyAuthenticated?.HasPermission("management.newtoken") == true)
            {
                var auth = AuthService.New(name, permissions);
                return Json(auth);
            }

            return Json(new { success = false, message = "Invalid api token" });
        }

        [HttpGet("parse")]
        public ActionResult ForceParse(string demoUrl)
        {
            if (AuthService.CurrentlyAuthenticated?.HasPermission("parse") == true)
            {
                var demoParser = new Parser.Parser(Guid.NewGuid(), demoUrl);
                demoParser.Parse();
                return Json(new { demoParser.Demo }, new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
            }

            return Json(new { success = false, message = "Invalid api token" });
        }

        [HttpGet("schedule")]
        public ActionResult ScheduleDemoParse(string demoUrl, string callbackUrl = null)
        {
            if (AuthService.CurrentlyAuthenticated?.HasPermission("schedule") == true)
            {
                var demoId = DemoService.ScheduleDemoParse(demoUrl, callbackUrl);
                return Json(new { success = true, demoId, callbackUrl });
            }

            return Json(new { success = false, message = "Invalid api token" });
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
