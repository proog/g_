using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public AuthenticationController(GamesContext dbContext, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpPost("token", Name = Route.Token)]
        public ActionResult<OAuthResponse> Token([FromForm] OAuthCredentials cred)
        {
            var hash = auth.HashPassword(cred.Password);
            var user = dbContext.Users.FirstOrDefault(u => u.Username == cred.Username && u.Password == hash);

            if (user == null)
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
