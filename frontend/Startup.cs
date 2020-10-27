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
            services.AddControllers();

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .WithOrigins(new[]{ "http://localhost:4200" })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

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

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("CorsPolicy");

                app.UseDeveloperExceptionPage();
            } 

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (env.IsDevelopment())
            {
                //fallback su applicazione angular in serve su localhost:4200
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path.Value== "/")
                    {
                        context.Response.Redirect("http://localhost:4200/");
                        return;
                    }
                    else 
                    {
                        await next();
                    }
                });
            } 
            else 
            {
                app.Use(async (context, next) =>
                {
                    if(context.Request.Path.Value == "/")
                    {
                        await SelectPhase(context, next);
                    }
                    else
                    {
                        await next();
                    }
                });

                //TODO: abililtare per esegure il challenge su tutte le risorse statiche
                // app.Use(async (context, next) =>
                // {
                //     if(!context.User.Identity.IsAuthenticated)
                //     {
                //          await context.ChallengeAsync("oidc");
                //     }
                //     else
                //     {
                //         await next();
                //     }
                // });
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {                   
                    if (context.File.Name == "index.html" ) {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                }
            });
            
            if (env.IsDevelopment())
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("/index.html");
                });
            }
        }

        private async Task SelectPhase(HttpContext context, Func<Task> next)
        {
            var needPhase = NeedPhase(context);
            if(needPhase)
            {
                context.Response.Redirect("/phased/");
            }
            else
            {
                await next();
            }
        }

        private bool NeedPhase(HttpContext context)
        {
            if(!context.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var name = context.User.FindFirst("name")?.Value;
            if(name.ToLower().StartsWith("alice"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
