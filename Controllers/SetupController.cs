using System.Linq;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("setup")]
    public partial class SetupController : Controller
    {
        private GamesContext db;
        private ICommonService common;
        private IAuthenticationService auth;

        public SetupController(GamesContext db, ICommonService common, IAuthenticationService auth)
        {
            this.db = db;
            this.common = common;
            this.auth = auth;
        }

        [HttpGet]
        public IActionResult Show()
        {
            return Render(new SetupViewModel
            {
                Success = common.IsConfigured
            });
        }

        [HttpPost]
        public IActionResult Do([FromForm] SetupViewModel vm)
        {
            if (common.IsConfigured)
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

            db.Configs.Add(new Config
            {
                GiantBombApiKey = vm.ApiKey?.Trim(),
                DefaultUser = new User
                {
                    Username = vm.Username.Trim(),
                    Password = auth.HashPassword(vm.Password)
                }
            });

            db.SaveChanges();
            vm.Success = true;
            return Render(vm);
        }

        private IActionResult Render(SetupViewModel vm)
        {
            return View("~/Views/Setup.cshtml", vm);
        }
    }
}
