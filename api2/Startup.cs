using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games {
    public class Startup {
        private IConfiguration config;

        public Startup() {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public void Configure(IApplicationBuilder app) {
            var authOptions = new CookieAuthenticationOptions {
                AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ExpireTimeSpan = TimeSpan.FromHours(5),
                Events = new CookieAuthenticationEvents {
                    OnRedirectToLogin = context => {
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    }
                }
            };

            InitializeDatabase(app);
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseCookieAuthentication(authOptions)
                .UseMvc();
        }

        public void ConfigureServices(IServiceCollection services) {
            services.Configure<AppSettings>(config)
                .AddDbContext<GamesContext>()
                .AddTransient<CommonService>()
                .AddTransient<AuthenticationService>()
                .AddMvc()
                .AddJsonOptions(options => {
                    options.SerializerSettings.ReferenceLoopHandling =
                        ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        };
                });
        }

        private void InitializeDatabase(IApplicationBuilder app) {
            var scopeFactory = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope()) {
                var db = scope.ServiceProvider.GetService<GamesContext>();
                var auth = scope.ServiceProvider.GetService<AuthenticationService>();

                if (db.Database.EnsureCreated()) {
                    var user = new User {
                        Username = "Default",
                        Password = auth.HashPassword("default")
                    };
                    db.Users.Add(user);
                    db.Configs.Add(new Config { DefaultUser = user });
                    db.SaveChanges();
                }
            }
        }
    }
}