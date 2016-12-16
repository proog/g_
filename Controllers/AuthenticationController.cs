using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers {
    [Route("api")]
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

            await service.Authenticate(user, HttpContext);
            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut() {
            await service.Deauthenticate(HttpContext);
            return Ok();
        }

        [HttpGet("login")]
        public async Task<IActionResult> GetCurrentUser() {
            var user = await service.GetCurrentUser(HttpContext);

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