using Games.Models;

namespace Games.Interfaces
{
    public interface IConfigRepository
    {
        bool IsConfigured { get; }

        Config DefaultConfig { get; }

        void Configure(User defaultUser, string giantBombApiKey);
    }
}
