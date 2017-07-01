using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        [HttpGet("login")]
        public IActionResult GetCurrentUser()
        {
            var user = auth.GetCurrentUser(HttpContext);
            user.VerifyExists();
            return Ok(user);
        }

        [HttpPost("token")]
        public IActionResult Token([FromForm] OAuthCredentials cred)
        {
            var hash = auth.HashPassword(cred.Password);
            var user = db.Users.SingleOrDefault(
                u => u.Username == cred.Username && u.Password == hash
            );

            if (user == null)
                throw new UnauthorizedException("Invalid credentials");

            var jwt = auth.Authenticate(user);

            return Ok(new OAuthResponse
            {
                AccessToken = jwt,
                TokenType = "Bearer"
            });
        }
    }

    public class OAuthCredentials
    {
        [FromForm(Name = "grant_type"), Required, RegularExpression("password")]
        public string GrantType { get; set; }

        [FromForm(Name = "username"), Required]
        public string Username { get; set; }

        [FromForm(Name = "password"), Required]
        public string Password { get; set; }
    }

    public class OAuthResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
    }

    public class OAuthError
    {
        public string Error { get; set; }
    }
}
