using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
            app.UseDeveloperExceptionPage();
            var authOptions = new CookieAuthenticationOptions() {
                AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ExpireTimeSpan = TimeSpan.FromHours(5),
                Events = new CookieAuthenticationEvents() {
                    OnRedirectToLogin = context => {
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    }
                }
            };
            app.UseCookieAuthentication(authOptions);
            app.UseMvc();
            app.UseStaticFiles();
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling =
                    ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver =
                    new DefaultContractResolver {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };
            });
            services.AddDbContext<GamesContext>();
            services.AddTransient<GameService>();
            services.AddTransient<AuthenticationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<AppSettings>(config);
        }
    }
}