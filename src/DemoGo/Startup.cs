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

        public Startup(IHostingEnvironment env)
        {
            WebRootPath = env.WebRootPath;
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<Services.DemoService>();
            services.AddSingleton<Services.ApiAuthenticationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, Services.DemoService demoService, Services.ApiAuthenticationService apiService)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddProvider(new Logging.FileLoggerProvider());

            //app.UseDeveloperExceptionPage();

            //app.UseIISPlatformHandler();

            app.UseMvc();

            var fileProvider = new FileExtensionContentTypeProvider();
            fileProvider.Mappings.Add(".dem", "application/octet-stream");
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = fileProvider });

            demoService.Startup(1);
            apiService.Load();

            Newtonsoft.Json.JsonConvert.DefaultSettings = () =>
            {
                return new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            };
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
