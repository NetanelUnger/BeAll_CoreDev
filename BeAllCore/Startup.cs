using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BeAllCore.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography.Xml;
using System.Reflection.Metadata;

namespace BeAllCore
{
    public class Startup
    {

        private int? _httpsPort;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services
                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        );

                    options.AddPolicy("signalr",
                        builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()

                        .AllowCredentials()
                        .SetIsOriginAllowed(hostName => true));
                });

            services.AddMvc(opt=>
            {
                // use HTTPS
                opt.Filters.Add(typeof(RequireHttpsAttribute));
                opt.SslPort = _httpsPort;

                opt.Filters.Add(typeof(JsonExceptionFilter));

                opt.EnableEndpointRouting = false;
            } );

            services.AddRouting(opt => opt.LowercaseUrls = true);

 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Remove("X-Powered-By");
                    context.Response.Headers.Add("content-security-policy", "frame-ancestors 'none'");
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    return Task.CompletedTask;
                });

                await next();
            });

            app.UseMvc();
            app.UseCors(config => config.WithExposedHeaders().AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(origin => true));

        }
    }
}
