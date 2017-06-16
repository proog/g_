using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api")]
    public class UserController : Controller
    {
        private readonly GamesContext db;

        public UserController(GamesContext db)
        {
            this.db = db;
        }

        [HttpGet("users")]
        public List<User> GetUsers()
        {
            return db.Users.ToList();
        }

        [HttpGet("users/{id}")]
        public User GetUser(int id)
        {
            var user = db.GetUser(id);
            user.VerifyExists();
            return user;
        }
    }
}
