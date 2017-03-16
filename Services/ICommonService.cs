using System.Net.Http;
using Games.Models;
using Newtonsoft.Json;

namespace Games.Services {
    public interface ICommonService {
        User GetUser(int id);
        HttpClient GetHttpClient();
        JsonSerializerSettings GetJsonSettings();
        bool IsConfigured();
        void DeleteImageDirectory(Game game);
        void VerifyExists<T>(T value, string message = "Not found.");
    }
}
