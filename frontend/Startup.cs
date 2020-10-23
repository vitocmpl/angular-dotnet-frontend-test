using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;

namespace frontend
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";

                    options.ClientId = "frontend";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.SaveTokens = true;

                    options.Scope.Add("api1");
                    options.Scope.Add("profile");
                    options.GetClaimsFromUserInfoEndpoint = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                if(!context.User.Identity.IsAuthenticated)
                {
                     await context.ChallengeAsync("oidc");
                }
                else
                {
                    await next();
                }
            });

            app.Use(async (context, next) =>
            {
                if(context.Request.Path.Value == "/index.html")
                {
                    context.Response.Headers.Add("Cache-Control", "no-store,no-cache");
                    context.Response.Headers.Add("Pragma", "no-cache");

                    await SelectPhase(context);
                }
                else
                {
                    await next();
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallback(async context => 
                {
                   context.Response.Redirect("/index.html");
                   await Task.CompletedTask;
                });
            });
        }

        private async Task SelectPhase(HttpContext context)
        {
            var name = context.User.FindFirst("name")?.Value;
            if(name.ToLower().StartsWith("alice"))
            {
                context.Response.Redirect("/phased/index.html");
            }
            else
            {
                context.Response.Redirect("/index.html");
            }

            await Task.CompletedTask;
        }
    }
}
