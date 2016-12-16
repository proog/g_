using System.Linq;
using System.Net.Http;
using Games.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Services {
    public class CommonService {
        private GamesContext db;

        public CommonService(GamesContext db) {
            this.db = db;
        }

        public User GetUser(int id) {
            return db.Users
                .Include(u => u.Games)
                .Include(u => u.Genres)
                .Include(u => u.Platforms)
                .Include(u => u.Tags)
                .SingleOrDefault(u => u.Id == id);
        }

        public HttpClient GetHttpClient() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "User-Agent", new[] { "permortensen.com g_sharp 0.1" }
            );
            return client;
        }

        public JsonSerializerSettings GetJsonSettings() {
            return new JsonSerializerSettings {
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }
    }
}