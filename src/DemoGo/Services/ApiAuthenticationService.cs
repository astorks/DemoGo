using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DemoGo.Services
{
    public class ApiAuthenticationService
    {
        private const string AUTH_LIST_FILE = "/auth.json.private";
        private IHttpContextAccessor ContextAccessor { get; }
        private HttpContext HttpContext { get { return ContextAccessor.HttpContext; } }
        public List<ApiAuthentication> ApiAuthenticationList { get; set; } = new List<ApiAuthentication>();
        private ILogger Logger { get; }
        private static RandomNumberGenerator RNG { get; } = RandomNumberGenerator.Create();

        public ApiAuthenticationService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
        {
            ContextAccessor = contextAccessor;
            Logger = loggerFactory.CreateLogger<ApiAuthenticationService>();
        }

        public void Load()
        {
            if(File.Exists(Startup.WebRootPath + AUTH_LIST_FILE))
                ApiAuthenticationList = JsonConvert.DeserializeObject<List<ApiAuthentication>>(File.ReadAllText(Startup.WebRootPath + AUTH_LIST_FILE));
            else
            {
                var apiAuth = new ApiAuthentication
                {
                    Name = "MasterKey",
                    Permissions = new List<string> { "*" }
                };

                Logger.LogInformation("Created MasterKey: {0}", JsonConvert.SerializeObject(apiAuth));
                ApiAuthenticationList.Add(apiAuth);
                Save();
            }
        }

        public void Save()
        {
            File.WriteAllText(Startup.WebRootPath + AUTH_LIST_FILE, JsonConvert.SerializeObject(ApiAuthenticationList));
        }

        public ApiAuthentication New(string name, string permissions)
        {
            var auth = new ApiAuthentication { Name = name };
            auth.Permissions.AddRange(permissions.Split(','));
            ApiAuthenticationList.Add(auth);
            return auth;
        }

        public ApiAuthentication CurrentlyAuthenticated
        {
            get
            {
                string apiToken = null;
                if (HttpContext.Request.Query.ContainsKey("apiToken"))
                    apiToken = HttpContext.Request.Query["apiToken"];
                else if (HttpContext.Request.Headers.ContainsKey("ApiToken"))
                    apiToken = string.Join("", HttpContext.Request.Headers["ApiToken"]);

                return ApiAuthenticationList.Where(e => e.Key == apiToken).FirstOrDefault();
            }
        }

        public class ApiAuthentication
        {
            public string Name { get; set; }
            public string Key { get; set; } = RNG.GetBase64String(32);
            public List<string> Permissions { get; set; } = new List<string>();

            public bool HasPermission(string permission)
            {
                return Permissions.Contains("*") || Permissions.Contains(permission);
            }
        }
    }
}
