using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Sysphera.Middleware.Drapo;
using Microsoft.Net.Http.Headers;
using gwg;

namespace WebGwg
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDrapo();
            services.AddMvc()
                  .AddJsonOptions(options =>
                  {
                      options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                  });
            services.AddSingleton<Crawler, Crawler>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "GWG",
                    Version = "v1",
                    Description = "API to be used in GWG",
                    TermsOfService = "None"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDrapo(o => { ConfigureDrapo(env, o); });
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    var headers = context.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                }
            });
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSPA API");
            });
        }

        private void ConfigureDrapo(IHostingEnvironment env, DrapoMiddlewareOptions options)
        {
            if (env.IsDevelopment())
                options.Debug = true;
            options.Config.UsePipes = false;
            options.Config.StorageErrors = "errors";
            options.Config.OnError = "";
        }
    }
}
