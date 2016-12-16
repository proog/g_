using System.ComponentModel.DataAnnotations;
using System.Linq;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers {
    [Route("api/setup")]
    public class SetupController : Controller {
        private GamesContext db;

        public SetupController(GamesContext db) {
            this.db = db;
        }

        [HttpGet]
        public IActionResult Show() {
            return Render(new ViewModel {
                Success = IsConfigured()
            });
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult Do([FromForm] ViewModel vm) {
            if (IsConfigured()) {
                vm.Success = true;
                return Render(vm);
            }

            if (new[] { vm.DbName, vm.DbUser, vm.DbPass }.Any(string.IsNullOrEmpty)) {
                vm.DbError = "You must specify a database name, user, and password.";
            }

            if (new[] { vm.Username, vm.Password }.Any(string.IsNullOrEmpty)) {
                vm.UserError = "Username or password invalid. Please enter a valid username and password.";
            }
            else if (vm.Password == "1234") {
                vm.UserError = "I said <em>NOT 1234</em>.";
            }

            if (!string.IsNullOrEmpty(vm.ApiKey) && vm.ApiKey.Length != 40) {
                vm.OtherError = "The Giant Bomb API key you entered doesn't look like a valid key.";
            }

            if (vm.DbError != null || vm.UserError != null || vm.OtherError != null) {
                vm.Success = false;
                return Render(vm);
            }

            // builds database tables from model
            db.Database.EnsureCreated();

            vm.Success = true;
            return Render(vm);
        }

        private bool IsConfigured() {
            return false;
        }

        private IActionResult Render(ViewModel vm) {
            return View("~/Views/Setup.cshtml", vm);
        }

        public class ViewModel {
            public bool Success { get; set; }
            public string DbError { get; set; }
            public string UserError { get; set; }
            public string OtherError { get; set; }
            [Required]
            public string DbHost { get; set; } = "localhost";
            [Required]
            public int DbPort { get; set; } = 3306;
            [Required]
            public string DbName { get; set; }
            [Required]
            public string DbUser { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string DbPass { get; set; }
            [Required]
            public string DbPrefix { get; set; } = "g_";
            [Required]
            public string Username { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            public string ApiKey { get; set; }
        }
    }
}