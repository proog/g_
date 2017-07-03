using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Models;
using Games.Models.ViewModels;
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
        public List<UserViewModel> GetUsers()
        {
            return db.Users
                .Select(ViewModelFactory.MakeUserViewModel)
                .ToList();
        }

        [HttpGet("users/{id}")]
        public UserViewModel GetUser(int id)
        {
            var user = db.GetUser(id);
            user.VerifyExists();
            return ViewModelFactory.MakeUserViewModel(user);
        }
    }
}
