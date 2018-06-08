using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public AuthenticationController(IUserRepository userRepository, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.userRepository = userRepository;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpPost("token", Name = Route.Token)]
        public ActionResult<OAuthResponse> Token([FromForm] OAuthCredentials cred)
        {
            var hash = auth.HashPassword(cred.Password);
            var user = userRepository.Get(cred.Username);

            if (user == null || user.Password != hash)
                return Unauthorized();

            var jwt = auth.Authenticate(user);

            return new OAuthResponse
            {
                AccessToken = jwt,
                TokenType = "Bearer"
            };
        }
    }
}
