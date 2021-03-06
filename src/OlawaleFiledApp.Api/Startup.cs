using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OlawaleFiledApp.Api.Middlewares;
using OlawaleFiledApp.Api.Validation;
using OlawaleFiledApp.Core;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Models.Constants;

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
            services.AddDbContext<AppDbContext>(x =>
            {
                var connection = new SqliteConnection($"Data Source={StringConstants.ConnectionString};Cache=Shared;Mode=Memory");
                connection.Open();
                x.UseSqlite(connection, b => b.MigrationsAssembly("OlawaleFiledApp.Api"));
            });
            services.AddRouting(x => x.LowercaseUrls = true);
            
            services.AddSwaggerGen(c =>
            {
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext appDbContext, ILoggerFactory loggerFactory)
        {
            appDbContext.Database.EnsureCreated(); //Hack to get Sqlite to create DB
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }   
            else
            {
                app.UseMiddleware<ErrorHandlerMiddleware>(); //Handle Exceptions Gracefully 
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.RoutePrefix = string.Empty;
                });

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (env.IsProduction())
                loggerFactory.AddFile("/logs/PaymentProcessing_{DateTime.Now:yy_MM_dd}.txt");
        }
    }
}