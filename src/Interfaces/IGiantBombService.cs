using System.Collections.Generic;
using System.Threading.Tasks;
using Games.Models.GiantBomb;

namespace Games.Interfaces
{
    public interface IGiantBombService
    {
        Task<List<GBSearchResult>> Search(string title, string apiKey);

        Task<GBGame> GetGame(int id, string apiKey);
    }
}
