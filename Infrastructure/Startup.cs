using System;
using System.IO;
using System.Threading.Tasks;
using Games.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Infrastructure {
    public class Startup {
        private string dataDirectory;

        public Startup(IHostingEnvironment env) {
            dataDirectory = Path.Combine(env.ContentRootPath, "data");
            CreateDirectories();
        }

        public void Configure(IApplicationBuilder app, ICommonService common) {
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
            var fileOptions = new FileServerOptions {
                RequestPath = "/images",
                EnableDefaultFiles = false,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(dataDirectory, "images")
                )
            };

            // redirect to setup until configured
            app.MapWhen(
                ctx => ctx.Request.Path == "/" && !common.IsConfigured,
                req => req.Run(
                    ctx => Task.Run(() => ctx.Response.Redirect("/setup"))
                )
            );
            app.UseDefaultFiles() // serve index.html for /
                .UseStaticFiles() // serve public
                .UseFileServer(fileOptions) // serve data/images
                .UseCookieAuthentication(authOptions)
                .UseMvc();
            CreateDatabase(app);
        }

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddTransient<ICommonService, CommonService>()
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddSingleton<IHttpService, HttpService>()
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(dataDirectory))
                .AddDbContext<GamesContext>(ConfigureDatabase)
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

        private void CreateDirectories() {
            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(Path.Combine(dataDirectory, "images"));
        }

        private void ConfigureDatabase(DbContextOptionsBuilder builder) {
            var path = Path.Combine(dataDirectory, "games.db");
            builder.UseSqlite($"Data Source={path}");
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
