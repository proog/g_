using System.Collections.Generic;
using Games.Models;

namespace Games.Interfaces
{
    public interface IGameRepository
    {
        List<Game> All(User user);

        Game Get(User user, int id);

        void Add(Game game);

        void Update(Game game);

        void Delete(Game game);
    }
}
