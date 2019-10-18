using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/settings", Name = Route.Settings)]
    public class SettingsController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public SettingsController(GamesContext dbContext, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpGet]
        public AuthorizedSettings GetSettings()
        {
            var config = dbContext.Configs.FirstOrDefault();
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
            var userId = int.Parse(idClaim.Value);
            var passwordHash = auth.HashPassword(settings.OldPassword);

            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId && u.Password == passwordHash);
            if (user == null)
                return Unauthorized();

            var defaultUser = dbContext.Users.Find(settings.DefaultUserId);
            if (defaultUser == null)
                return BadRequest();

            var config = dbContext.Configs.FirstOrDefault();
            if (config == null)
            {
                config = new Config();
                dbContext.Configs.Add(config);
            }

            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = settings.GiantBombApiKey;

            if (!string.IsNullOrEmpty(settings.NewPassword))
                user.Password = auth.HashPassword(settings.NewPassword);

            dbContext.SaveChanges();

            return new AuthorizedSettings
            {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
        }
    }
}
