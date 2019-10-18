using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IViewModelFactory vmFactory;

        public UserController(GamesContext dbContext, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.vmFactory = vmFactory;
        }

        [HttpGet(Name = Route.Users)]
        public List<UserViewModel> GetUsers()
        {
            return dbContext.Users
                .Select(vmFactory.MakeUserViewModel)
                .ToList();
        }

        [HttpGet("{id}", Name = Route.User)]
        public ActionResult<UserViewModel> GetUser(int id)
        {
            var user = dbContext.Users.Find(id);

            if (user == null)
                return NotFound();

            return vmFactory.MakeUserViewModel(user);
        }
    }
}
