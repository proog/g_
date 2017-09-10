using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Repositories;
using Games.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        private readonly IConfiguration configuration;
        private readonly string dataDirectory;
        private readonly string imageDirectory;
        private readonly string connectionString;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            this.configuration = configuration;
            dataDirectory = Path.Combine(env.ContentRootPath, configuration["dataDirectory"]);
            imageDirectory = Path.Combine(dataDirectory, "images");
            connectionString = $"Data Source={Path.Combine(dataDirectory, "games.db")}";
            Directory.CreateDirectory(imageDirectory);
        }

        public void Configure(IApplicationBuilder app)
        {
            var fileOptions = new FileServerOptions
            {
                RequestPath = "/images",
                EnableDefaultFiles = false,
                FileProvider = new PhysicalFileProvider(imageDirectory)
            };

            // redirect to setup until configured
            app.MapWhen(
                ctx =>
                {
                    var configRepository = ctx.RequestServices.GetService<IConfigRepository>();
                    return ctx.Request.Path == "/" && !configRepository.IsConfigured;
                },
                req => req.Run(
                    ctx => Task.Run(() => ctx.Response.Redirect("setup"))
                )
            );
            app.UseDefaultFiles() // serve index.html for /
                .UseStaticFiles() // serve public
                .UseFileServer(fileOptions) // serve uploaded images
                .UseAuthentication()
                .UseMvc();
            CreateDatabase(app);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions().Configure<AppSettings>(configuration);
            services.AddMvc(options =>
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["signingKey"]))
                    };
                });
            services.AddDbContext<GamesContext>(options => options.UseSqlite(connectionString))
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddTransient<IGameRepository, GameRepository>()
                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<IGenreRepository, GenreRepository>()
                .AddTransient<IPlatformRepository, PlatformRepository>()
                .AddTransient<ITagRepository, TagRepository>()
                .AddTransient<IConfigRepository, ConfigRepository>()
                .AddSingleton<HttpClient>(CreateHttpClient())
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(dataDirectory));
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
