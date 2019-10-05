using System.Collections.Generic;
using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Services;

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
            return db.Users.SingleOrDefault(u => u.Id == id);
        }

        public User Get(string username)
        {
            return db.Users.SingleOrDefault(u => u.Username == username);
        }

        public List<User> All()
        {
            return db.Users.ToList();
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
    }
}
