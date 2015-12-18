using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using DemoGo.WebService;

namespace DemoGo.Parser
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseDemoGo();
            app.UseStatusCodePages();
        }
    }
}
