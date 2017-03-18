using System.IO;
using System.Linq;
using System.Net.Http;
using Games.Infrastructure;
using Games.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Services {
    class CommonService : ICommonService {
        private GamesContext db;
        private IFileProvider data;
        private static HttpClient httpClient;

        public HttpClient HttpClient => httpClient;
        public bool IsConfigured => db.Configs.Count() > 0;
        public JsonSerializerSettings JsonSettings => new JsonSerializerSettings {
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        static CommonService() {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent", new[] { "permortensen.com g_sharp 0.1" }
            );
        }

        public CommonService(GamesContext db, IFileProvider fileProvider) {
            this.db = db;
            this.data = fileProvider;
        }

        public User GetUser(int id) {
            return db.Users
                .Include(u => u.Games)
                .Include(u => u.Genres)
                .Include(u => u.Platforms)
                .Include(u => u.Tags)
                .SingleOrDefault(u => u.Id == id);
        }

        public void DeleteImageDirectory(Game game) {
            var file = data.GetFileInfo($"images/{game.Id}");
            Directory.Delete(file.PhysicalPath, true);
        }

        public void VerifyExists<T>(T value, string message) {
            if (value == null) {
                throw new NotFoundException(message);
            }
        }
    }
}
