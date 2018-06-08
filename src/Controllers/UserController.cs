using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IViewModelFactory vmFactory;

        public UserController(IUserRepository userRepository, IViewModelFactory vmFactory)
        {
            this.userRepository = userRepository;
            this.vmFactory = vmFactory;
        }

        [HttpGet(Name = Route.Users)]
        public List<UserViewModel> GetUsers()
        {
            return userRepository.All()
                .Select(vmFactory.MakeUserViewModel)
                .ToList();
        }

        [HttpGet("{id}", Name = Route.User)]
        public ActionResult<UserViewModel> GetUser(int id)
        {
            var user = userRepository.Get(id);

            if (user == null)
                return NotFound();

            return vmFactory.MakeUserViewModel(user);
        }
    }
}
