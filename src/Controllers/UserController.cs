using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api")]
    public class UserController : Controller
    {
        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet("users")]
        public List<UserViewModel> GetUsers()
        {
            return userRepository.All()
                .Select(ViewModelFactory.MakeUserViewModel)
                .ToList();
        }

        [HttpGet("users/{id}")]
        public UserViewModel GetUser(int id)
        {
            var user = userRepository.Get(id);

            if (user == null)
                throw new NotFoundException();

            return ViewModelFactory.MakeUserViewModel(user);
        }
    }
}
