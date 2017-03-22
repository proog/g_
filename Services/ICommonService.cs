using Games.Models;
using Newtonsoft.Json;

namespace Games.Services {
    public interface ICommonService {
        JsonSerializerSettings JsonSettings { get; }
        bool IsConfigured { get; }
        User GetUser(int id);
        void DeleteImageDirectory(Game game);
    }
}
