using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OlawaleFiledApp.Api.Validation;
using OlawaleFiledApp.Core;

namespace OlawaleFiledApp.Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(opt =>
            {
                opt.AddSimpleConsole(c =>
                {
                    c.ColorBehavior = LoggerColorBehavior.Enabled;
                    c.UseUtcTimestamp = true;
                    c.IncludeScopes = false;
                    c.TimestampFormat = "[yyyy-MM-dd-HH-mm-ss] ";
                });
            });
            
            services.AddControllers(
                options =>
                {
                    options.Filters.Add(typeof(PayloadValidationActionFilter));
                });
            
            services.Configure<ApiBehaviorOptions>(
                options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });
            
            services.InitCoreServicesAndRepositories();
            services.AddRouting(x => x.LowercaseUrls = true);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/home", async (context) =>
                {
                    await context.Response.WriteAsync($"Hello World!");
                });
                // endpoints.MapControllers();
            });
        }
    }
}