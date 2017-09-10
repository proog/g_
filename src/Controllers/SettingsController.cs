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
        private readonly GamesContext db;
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;

        public SettingsController(GamesContext db, IUserRepository userRepository, IAuthenticationService auth)
        {
            this.db = db;
            this.userRepository = userRepository;
            this.auth = auth;
        }

        [HttpGet("config")]
        public ConfigViewModel GetConfig()
        {
            var config = db.Configs
                .Include(c => c.DefaultUser)
                .SingleOrDefault();
            config.VerifyExists();
            return ViewModelFactory.MakeConfigViewModel(config);
        }

        [HttpGet("settings"), Authorize]
        public AuthorizedSettings GetSettings()
        {
            var config = db.Configs.SingleOrDefault();
            return new AuthorizedSettings
            {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
        }

        [HttpPut("settings"), Authorize]
        public AuthorizedSettings UpdateSettings([FromBody] AuthorizedSettingsInput settings)
        {
            var user = auth.GetCurrentUser(HttpContext);
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password)
                throw new UnauthorizedException();

            var defaultUser = userRepository.Get(settings.DefaultUserId);

            if (defaultUser == null)
                throw new BadRequestException();

            var config = db.Configs.Single();
            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = settings.GiantBombApiKey;

            if (!string.IsNullOrEmpty(settings.NewPassword))
                user.Password = auth.HashPassword(settings.NewPassword);

            db.SaveChanges();
            return GetSettings();
        }
    }
}
