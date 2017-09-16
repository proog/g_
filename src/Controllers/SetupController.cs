using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("setup")]
    public class SetupController : Controller
    {
        private readonly IConfigRepository configRepository;
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;

        public SetupController(IConfigRepository configRepository, IUserRepository userRepository, IAuthenticationService auth)
        {
            this.configRepository = configRepository;
            this.userRepository = userRepository;
            this.auth = auth;
        }

        [HttpGet]
        public ViewResult Show()
        {
            return Render(new SetupViewModel
            {
                Success = configRepository.IsConfigured
            });
        }

        [HttpPost]
        public ViewResult Do([FromForm] SetupViewModel vm)
        {
            if (configRepository.IsConfigured)
            {
                vm.Success = true;
                return Render(vm);
            }

            if (new[] { vm.Username, vm.Password }.Any(string.IsNullOrWhiteSpace))
                vm.UserError = "Username or password invalid. Please enter a valid username and password.";
            else if (vm.Password == "1234")
                vm.UserError = "I said <em>NOT 1234</em>.";

            if (!string.IsNullOrEmpty(vm.ApiKey) && vm.ApiKey.Length != 40)
                vm.OtherError = "The Giant Bomb API key you entered doesn't look like a valid key.";

            if (vm.UserError != null || vm.OtherError != null)
            {
                vm.Success = false;
                return Render(vm);
            }

            var defaultUser = new User
            {
                Username = vm.Username.Trim(),
                Password = auth.HashPassword(vm.Password)
            };

            userRepository.Add(defaultUser);
            configRepository.Configure(defaultUser, vm.ApiKey?.Trim());

            vm.Success = true;
            return Render(vm);
        }

        private ViewResult Render(SetupViewModel vm)
        {
            return View("~/Views/Setup.cshtml", vm);
        }
    }
}
