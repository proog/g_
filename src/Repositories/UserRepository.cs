using System.Collections.Generic;
using System.Linq;
using Games.Models;
using Games.Services;
using Microsoft.EntityFrameworkCore;

namespace Games.Repositories
{
    class UserRepository : IUserRepository
    {
        private readonly GamesContext db;

        public UserRepository(GamesContext db)
        {
            this.db = db;
        }

        public User Get(int id)
        {
            return Query().SingleOrDefault(u => u.Id == id);
        }

        public IEnumerable<User> All()
        {
            return Query().ToList();
        }

        private IQueryable<User> Query()
        {
            return db.Users
                .Include(u => u.Games)
                .Include(u => u.Genres)
                .Include(u => u.Platforms)
                .Include(u => u.Tags);
        }
    }
}
