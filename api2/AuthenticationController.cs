using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Games {
    public class AuthenticationController : Controller {
        private GamesContext db;
        private AuthenticationService service;

        public AuthenticationController(GamesContext db, AuthenticationService service) {
            this.db = db;
            this.service = service;
        }

        [ValidateModel]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] Credentials cred) {
            var hash = service.HashPassword(cred.Password);
            var user = db.Users.SingleOrDefault(u =>
                u.Username == cred.Username && u.Password == hash
            );

            if (user == null) {
                return Unauthorized();
            }

            await service.Authenticate(user);
            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut() {
            await service.Deauthenticate();
            return Ok();
        }

        [HttpGet("login")]
        public async Task<IActionResult> GetCurrentUser() {
            var user = await service.GetCurrentUser();

            if (user != null) {
                return Ok(user);
            }

            return NotFound();
        }
    }

    public class Credentials {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}