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
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;


            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("resxlocalization.json", true, true);
            builder.AddEnvironmentVariables();
            ResxConfiguration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IConfiguration ResxConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddFileLocalization(options => { options.ResourcesPath = "Resources"; });

            services.AddMvcCore()
                    .AddJsonFormatters()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();

            services.AddLocalRequestLocalization(ResxConfiguration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           
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
