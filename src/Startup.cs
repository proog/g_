using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            dataDirectory = Path.Combine(env.ContentRootPath, configuration["dataDirectory"]);
            imageDirectory = Path.Combine(dataDirectory, "images");
            Directory.CreateDirectory(imageDirectory);
        }

        public void Configure(IApplicationBuilder app)
        {
            var headerOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            // allow reverse proxy on other addresses than localhost
            headerOptions.KnownNetworks.Clear();
            headerOptions.KnownProxies.Clear();

            var fileOptions = new FileServerOptions
            {
                RequestPath = "/images",
                EnableDefaultFiles = false,
                FileProvider = new PhysicalFileProvider(imageDirectory)
            };

            app.UseForwardedHeaders(headerOptions) // trust reverse proxy
                .UseDefaultFiles() // serve index.html for /
                .UseStaticFiles() // serve public
                .UseFileServer(fileOptions) // serve uploaded images
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers());
            CreateDatabase(app);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions().Configure<AppSettings>(configuration);
            services.AddRouting();
            services.AddControllers().AddNewtonsoftJson(ConfigureJson);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(ConfigureJwtBearer);
            services.AddRouteUserIdAuthorization();

            services.AddHttpClient<HttpClient>(ConfigureHttpClient);
            services.AddDbContextPool<GamesContext>(ConfigureDatabase)
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddTransient<IGiantBombService, GiantBombService>()
                .AddTransient<ISuggestionService, SuggestionService>()
                .AddTransient<IImageService, ImageService>()
                .AddTransient<IViewModelFactory, ViewModelFactory>()
                .AddScoped<IUrlHelper>(CreateUrlHelper)
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
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

        private void ConfigureJson(MvcNewtonsoftJsonOptions options)
        {
            var settings = options.SerializerSettings;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Converters.Add(new UnixDateTimeConverter());
            settings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
        }

        private void ConfigureJwtBearer(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["signingKey"]))
            };
        }

        private void ConfigureDatabase(DbContextOptionsBuilder options)
        {
            var useMySql = configuration["database"]?.ToLower() == "mysql";
            var usePostgres = configuration["database"]?.ToLower() == "postgres";
            var connectionString = configuration["connectionString"];

            if (useMySql)
                options.UseMySql(connectionString);
            else if (usePostgres)
                options.UseNpgsql(connectionString);
            else
                options.UseSqlite(connectionString);
        }

        private void ConfigureHttpClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add(
                "User-Agent", new[] { "permortensen.com g_sharp 0.1" }
            );
        }

        private IUrlHelper CreateUrlHelper(IServiceProvider serviceProvider)
        {
            var actionContext = serviceProvider.GetRequiredService<IActionContextAccessor>().ActionContext;
            var factory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext);
        }
    }
}
