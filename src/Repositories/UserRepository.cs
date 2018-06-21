using System.Collections.Generic;
using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Services;
using Microsoft.EntityFrameworkCore;

namespace Games.Repositories
{
    public class UserRepository : IUserRepository
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

        public User Get(string username)
        {
            return Query().SingleOrDefault(u => u.Username == username);
        }

        public IEnumerable<User> All()
        {
            return Query().ToList();
        }

        public void Add(User user)
        {
            db.Users.Add(user);
            db.SaveChanges();
        }

        public void Update(User user)
        {
            db.Users.Update(user);
            db.SaveChanges();
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
