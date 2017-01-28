using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers {
    [Route("api")]
    public class SettingsController : Controller {
        private GamesContext db;
        private CommonService common;
        private AuthenticationService auth;

        public SettingsController(GamesContext db, CommonService common, AuthenticationService auth) {
            this.db = db;
            this.common = common;
            this.auth = auth;
        }

        [HttpGet("config")]
        public IActionResult GetConfig() {
            var config = db.Configs
                .Include(c => c.DefaultUser)
                .SingleOrDefault();
            common.VerifyExists(config);
            return Ok(config);
        }

        [HttpGet("settings"), Authorize]
        public IActionResult GetSettings() {
            var config = db.Configs.SingleOrDefault();
            var settings = new AuthorizedSettings {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
            return Ok(settings);
        }

        [HttpPut("settings"), Authorize]
        public async Task<IActionResult> UpdateSettings([FromBody] AuthorizedSettingsInput settings) {
            var user = await auth.GetCurrentUser(HttpContext);
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password) {
                throw new UnauthorizedException();
            }

            var defaultUser = common.GetUser(settings.DefaultUserId);

            if (defaultUser == null) {
                throw new BadRequestException();
            }

            var config = db.Configs.Single();
            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = settings.GiantBombApiKey;

            if (!string.IsNullOrEmpty(settings.NewPassword)) {
                user.Password = auth.HashPassword(settings.NewPassword);
            }

            db.SaveChanges();
            return GetSettings();
        }

        public class AuthorizedSettings {
            [Required]
            public int DefaultUserId { get; set; }
            public string GiantBombApiKey { get; set; }
        }

        public class AuthorizedSettingsInput : AuthorizedSettings {
            [Required]
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
