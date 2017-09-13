using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Repositories;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers
{
    [Route("api")]
    public class SettingsController : Controller
    {
        private readonly IConfigRepository configRepository;
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;

        public SettingsController(IConfigRepository configRepository, IUserRepository userRepository, IAuthenticationService auth)
        {
            this.configRepository = configRepository;
            this.userRepository = userRepository;
            this.auth = auth;
        }

        [HttpGet("config")]
        public ConfigViewModel GetConfig()
        {
            var config = configRepository.DefaultConfig;
            config.VerifyExists();
            return ViewModelFactory.MakeConfigViewModel(config);
        }

        [HttpGet("settings"), Authorize]
        public AuthorizedSettings GetSettings()
        {
            var config = configRepository.DefaultConfig;
            return new AuthorizedSettings
            {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
        }

        [HttpPut("settings"), Authorize]
        public AuthorizedSettings UpdateSettings([FromBody] AuthorizedSettingsInput settings)
        {
            var idClaim = User.FindFirst(Constants.UserIdClaim);
            var user = userRepository.Get(int.Parse(idClaim.Value));
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password)
                throw new UnauthorizedException();

            var defaultUser = userRepository.Get(settings.DefaultUserId);

            if (defaultUser == null)
                throw new BadRequestException();

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
