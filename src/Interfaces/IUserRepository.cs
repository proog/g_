using System.Collections.Generic;
using Games.Models;

namespace Games.Interfaces
{
    public interface IUserRepository
    {
        IEnumerable<User> All();

        User Get(int id);

        void Add(User user);

        void Update(User user);
    }
}
