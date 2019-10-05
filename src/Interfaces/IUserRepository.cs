using System.Collections.Generic;
using Games.Models;

namespace Games.Interfaces
{
    public interface IUserRepository
    {
        List<User> All();

        User Get(int id);

        User Get(string username);

        void Add(User user);

        void Update(User user);
    }
}
