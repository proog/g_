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

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        public List<UserViewModel> GetUsers()
        {
            return userRepository.All()
                .Select(ViewModelFactory.MakeUserViewModel)
                .ToList();
        }

        [HttpGet("{id}")]
        public UserViewModel GetUser(int id)
        {
            var user = userRepository.Get(id);

            if (user == null)
                throw new NotFoundException();

            return ViewModelFactory.MakeUserViewModel(user);
        }
    }
}
