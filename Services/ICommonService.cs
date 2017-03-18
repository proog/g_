using System.Net.Http;
using Games.Models;
using Newtonsoft.Json;

namespace Games.Services {
    public interface ICommonService {
        HttpClient HttpClient { get; }
        JsonSerializerSettings JsonSettings { get; }
        bool IsConfigured { get; }
        User GetUser(int id);
        void DeleteImageDirectory(Game game);
        void VerifyExists<T>(T value, string message = "Not found.");
    }
}
