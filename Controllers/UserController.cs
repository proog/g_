using System.Collections.Generic;
using System.Linq;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers {
    [Route("api")]
    public class UserController : Controller {
        private GamesContext db;
        private CommonService service;

        public UserController(GamesContext db, CommonService service) {
            this.db = db;
            this.service = service;
        }

        [HttpGet("users")]
        public List<User> GetUsers() {
            return db.Users.ToList();
        }

        [HttpGet("users/{id}")]
        public User GetUser(int id) {
            return service.GetUser(id);
        }
    }
}