﻿using System;
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

namespace ResxFileSample
{
    public class Startup
    {


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddFileLocalization(options => { options.ResourcesPath = "Resources"; });
            services.AddMvcCore()
            //    .AddRazorViewEngine(option =>
            //{
            //    option.ViewLocationFormats.Clear();
            //    option.ViewLocationFormats.Add("/Views1/{1}/{0}" + RazorViewEngine.ViewExtension);
            //    option.ViewLocationFormats.Add("/Views1/Shared/{0}" + RazorViewEngine.ViewExtension);

            //    option.AreaViewLocationFormats.Clear();
            //    option.AreaViewLocationFormats.Add("/Areas/{2}/Views1/{1}/{0}" + RazorViewEngine.ViewExtension);
            //    option.AreaViewLocationFormats.Add("/Areas/{2}/Views1/Shared/{0}" + RazorViewEngine.ViewExtension);
            //    option.AreaViewLocationFormats.Add("/Views1/Shared/{0}" + RazorViewEngine.ViewExtension);
            //})
                    .AddJsonFormatters()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();

            // services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
            // services.AddTransient(typeof(IUrlHelper), typeof(UrlLocalHelper));

            services.AddLocalUrl();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug();
            var vSupCulture = new List<CultureInfo>()
            {
                new CultureInfo("zh-CN"),
                new CultureInfo("fr-FR"),
                new CultureInfo("en-US")
            };


            app.UseLocalRequestLocalization(new LocalRequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture("zh-CN"),
                SupportedCultures = vSupCulture,
                SupportedUICultures = vSupCulture,
                SupportedAliasUICultures = new List<KeyValuePair<string, string>>()
                {
                new KeyValuePair<string,string>("china","zh-CN"),
                new KeyValuePair<string,string>("fr","fr-FR"),
                new KeyValuePair<string,string>("usa","en-US")
                }
            });

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
