using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api")]
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;

        public AuthenticationController(IUserRepository userRepository, IAuthenticationService auth)
        {
            this.userRepository = userRepository;
            this.auth = auth;
        }

        [HttpGet("login"), Authorize]
        public UserViewModel GetCurrentUser()
        {
            var user = auth.GetCurrentUser(HttpContext);
            user.VerifyExists();
            return ViewModelFactory.MakeUserViewModel(user);
        }

        [HttpPost("token")]
        public OAuthResponse Token([FromForm] OAuthCredentials cred)
        {
            var hash = auth.HashPassword(cred.Password);
            var user = userRepository.Get(cred.Username);

            if (user == null || user.Password != hash)
                throw new UnauthorizedException("Invalid credentials");

            var jwt = auth.Authenticate(user);

            return new OAuthResponse
            {
                AccessToken = jwt,
                TokenType = "Bearer"
            };
        }
    }
}
