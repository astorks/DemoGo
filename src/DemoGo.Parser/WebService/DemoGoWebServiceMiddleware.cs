using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net;

namespace DemoGo.WebService
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class DemoGoWebServiceMiddleware
    {
        private readonly RequestDelegate _next;

        public DemoGoWebServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Method == "GET")
            {
                return Task.Run(async () =>
                {
                    httpContext.Response.ContentType = "application/json; charset=UTF-8";
                    string url = WebUtility.UrlDecode(httpContext.Request.Path.ToString().Substring(1));
                    //await httpContext.Response.WriteAsync("DemoGo Remote: " + url);
                    var demo = await DemoInfo.DemoParser.ParseAsync(url);
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(demo));
                });
            }
            else if (httpContext.Request.Method == "POST")
                return Task.Run(async () => { await httpContext.Response.WriteAsync("DemoGo Upload: " + httpContext.Request.Body.Length); });

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DemoGoWebServiceMiddlewareExtensions
    {
        public static IApplicationBuilder UseDemoGo(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DemoGoWebServiceMiddleware>();
        }
    }
}
