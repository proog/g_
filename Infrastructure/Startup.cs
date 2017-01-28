using System;
using System.Threading.Tasks;
using Games.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Infrastructure {
    public class Startup {
        public void Configure(IApplicationBuilder app, CommonService service) {
            var authOptions = new CookieAuthenticationOptions {
                AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ExpireTimeSpan = TimeSpan.FromHours(5),
                Events = new CookieAuthenticationEvents {
                    OnRedirectToLogin = context => {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                }
            };

            // redirect to setup until configured
            app.MapWhen(
                ctx => ctx.Request.Path == "/" && !service.IsConfigured(),
                req => req.Run(
                    ctx => Task.Run(() => ctx.Response.Redirect("/setup"))
                )
            );
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseCookieAuthentication(authOptions)
                .UseMvc();
            CreateDatabase(app);
        }

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddDbContext<GamesContext>()
                .AddTransient<CommonService>()
                .AddTransient<AuthenticationService>()
                .AddMvc(options => {
                    options.Filters.Add(new ValidateModelFilter());
                    options.Filters.Add(new HandleExceptionFilter());
                })
                .AddJsonOptions(options => {
                    options.SerializerSettings.ReferenceLoopHandling =
                        ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        };
                });
        }

        private void CreateDatabase(IApplicationBuilder app) {
            var scopeFactory = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope()) {
                var db = scope.ServiceProvider.GetService<GamesContext>();
                db.Database.EnsureCreated();
            }
        }
    }
}
