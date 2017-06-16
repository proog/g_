using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api")]
    public class AuthenticationController : Controller
    {
        private readonly GamesContext db;
        private readonly IAuthenticationService auth;

        public AuthenticationController(GamesContext db, IAuthenticationService auth)
        {
            this.db = db;
            this.auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] Credentials cred)
        {
            var hash = auth.HashPassword(cred.Password);
            var user = db.Users.SingleOrDefault(u =>
                u.Username == cred.Username && u.Password == hash
            );

            if (user == null)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            await auth.Authenticate(user, HttpContext);
            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await auth.Deauthenticate(HttpContext);
            return Ok();
        }

        [HttpGet("login")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await auth.GetCurrentUser(HttpContext);
            user.VerifyExists();
            return Ok(user);
        }
    }
}
