using System.Collections.Generic;
using System.Linq;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers {
    [Route("api")]
    public class UserController : Controller {
        private GamesContext db;
        private ICommonService common;

        public UserController(GamesContext db, ICommonService common) {
            this.db = db;
            this.common = common;
        }

        [HttpGet("users")]
        public List<User> GetUsers() {
            return db.Users.ToList();
        }

        [HttpGet("users/{id}")]
        public User GetUser(int id) {
            return common.GetUser(id);
        }
    }
}
