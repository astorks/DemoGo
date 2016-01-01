using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.Diagnostics;

namespace DemoGo
{
    public class Startup
    {
        public static string WebRootPath { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            WebRootPath = env.WebRootPath;
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("project.json")
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<Services.DemoService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, Services.DemoService demoService)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //app.UseDeveloperExceptionPage();

            //app.UseIISPlatformHandler();

            app.UseMvc();

            var fileProvider = new FileExtensionContentTypeProvider();
            fileProvider.Mappings.Add(".dem", "application/octet-stream");
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = fileProvider });

            demoService.Startup(1);

            Newtonsoft.Json.JsonConvert.DefaultSettings = () =>
            {
                return new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            };
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
