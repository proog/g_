using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/settings", Name = Route.Settings)]
    public class SettingsController : ControllerBase
    {
        private readonly IConfigRepository configRepository;
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public SettingsController(IConfigRepository configRepository, IUserRepository userRepository, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.configRepository = configRepository;
            this.userRepository = userRepository;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpGet]
        public AuthorizedSettings GetSettings()
        {
            var config = configRepository.DefaultConfig;
            return new AuthorizedSettings
            {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
        }

        [HttpPut]
        public ActionResult<AuthorizedSettings> UpdateSettings([FromBody] AuthorizedSettingsInput settings)
        {
            var idClaim = User.FindFirst(Constants.UserIdClaim);
            var user = userRepository.Get(int.Parse(idClaim.Value));
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password)
                return Unauthorized();

            var defaultUser = userRepository.Get(settings.DefaultUserId);

            if (defaultUser == null)
                return BadRequest();

            configRepository.Configure(defaultUser, settings.GiantBombApiKey);

            if (!string.IsNullOrEmpty(settings.NewPassword))
            {
                user.Password = auth.HashPassword(settings.NewPassword);
                userRepository.Update(user);
            }

            return GetSettings();
        }
    }
}
