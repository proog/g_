using System.Collections.Generic;
using Games.Models;

namespace Games.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> All();

        User Get(int id);
    }
}
