using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xakep.Extensions.Localization;
using Xakep.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Routing;
using ResxFileSample.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;

namespace ResxFileSample
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(env.ContentRootPath)
                 .AddJsonFile("resxlocalization.json", true, true);

            //if (env.IsDevelopment())
            //{
            //    builder.AddUserSecrets();
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddFileLocalization(options => { options.ResourcesPath = "Resources"; });
            services.AddMvcCore()
                    .AddJsonFormatters()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();
            services.AddLocalRequestLocalization(Configuration);
          
            
            //services.AddLocalRequestLocalization(options =>
            //{
            //    var vSupCulture = new List<CultureInfo>()
            //    {
            //    new CultureInfo("zh-CN"),
            //    new CultureInfo("fr-FR"),
            //    new CultureInfo("en-US")
            //    };
            //    options.DefaultRequestCulture = new RequestCulture("zh-CN");
            //    options.SupportedCultures = vSupCulture;
            //    options.SupportedUICultures = vSupCulture;
            //    options.SupportedAliasUICultures = new List<KeyValuePair<string, string>>()
            //    {
            //    new KeyValuePair<string,string>("china","zh-CN"),
            //    new KeyValuePair<string,string>("fr","fr-FR"),
            //    new KeyValuePair<string,string>("usa","en-US")
            //    };
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug();
            //启用Resx的本地化
            app.UseLocalRequestLocalization();

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

}
