using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games
{
    public class Startup
    {
        private string dataDirectory;
        private IConfigurationRoot configuration;

        public Startup(IHostingEnvironment env)
        {
            dataDirectory = Path.Combine(env.ContentRootPath, "data");
            configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();
            CreateDirectories();
        }

        public void Configure(IApplicationBuilder app, GamesContext db, IAuthenticationService auth, IOptions<AppSettings> appSettings)
        {
            var authOptions = new JwtBearerOptions
            {
                AuthenticationScheme = "Bearer",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.SigningKey))
                }
            };
            var fileOptions = new FileServerOptions
            {
                RequestPath = "/images",
                EnableDefaultFiles = false,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(dataDirectory, "images")
                )
            };

            // redirect to setup until configured
            app.MapWhen(
                ctx => ctx.Request.Path == "/" && !db.IsConfigured,
                req => req.Run(
                    ctx => Task.Run(() => ctx.Response.Redirect("/setup"))
                )
            );
            app.UseDefaultFiles() // serve index.html for /
                .UseStaticFiles() // serve public
                .UseFileServer(fileOptions) // serve data/images
                .UseJwtBearerAuthentication(authOptions)
                .UseMvc();
            CreateDatabase(app);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<AppSettings>(configuration)
                .AddOptions()
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddSingleton<HttpClient>(CreateHttpClient())
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(dataDirectory))
                .AddDbContext<GamesContext>(ConfigureDatabase)
                .AddMvc(options =>
                {
                    options.Filters.Add(new ValidateModelFilter());
                    options.Filters.Add(new HandleExceptionFilter());
                })
                .AddJsonOptions(options =>
                {
                    var settings = options.SerializerSettings;
                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    settings.Converters.Add(new UnixDateTimeConverter());
                    settings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };
                });
        }

        private void CreateDirectories()
        {
            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(Path.Combine(dataDirectory, "images"));
        }

        private void ConfigureDatabase(DbContextOptionsBuilder builder)
        {
            var path = Path.Combine(dataDirectory, "games.db");
            builder.UseSqlite($"Data Source={path}");
        }

        private void CreateDatabase(IApplicationBuilder app)
        {
            var scopeFactory = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<GamesContext>();
                db.Database.EnsureCreated();
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "User-Agent", new[] { "permortensen.com g_sharp 0.1" }
            );
            return client;
        }
    }
}
