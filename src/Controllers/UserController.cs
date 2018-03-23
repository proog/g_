using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
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
        public UserViewModel GetUser(int id)
        {
            var user = userRepository.Get(id);

            if (user == null)
                throw new NotFoundException();

            return vmFactory.MakeUserViewModel(user);
        }
    }
}
