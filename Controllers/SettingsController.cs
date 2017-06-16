using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
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
        private readonly IAuthenticationService auth;

        public SettingsController(GamesContext db, IAuthenticationService auth)
        {
            this.db = db;
            this.auth = auth;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var config = db.Configs
                .Include(c => c.DefaultUser)
                .SingleOrDefault();
            config.VerifyExists();
            return Ok(config);
        }

        [HttpGet("settings"), Authorize]
        public IActionResult GetSettings()
        {
            var config = db.Configs.SingleOrDefault();
            var settings = new AuthorizedSettings
            {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
            return Ok(settings);
        }

        [HttpPut("settings"), Authorize]
        public async Task<IActionResult> UpdateSettings([FromBody] AuthorizedSettingsInput settings)
        {
            var user = await auth.GetCurrentUser(HttpContext);
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password)
                throw new UnauthorizedException();

            var defaultUser = db.GetUser(settings.DefaultUserId);

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
