using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers {
    [Route("api")]
    public class SettingsController : Controller {
        private GamesContext db;
        private CommonService service;
        private AuthenticationService auth;

        public SettingsController(GamesContext db, CommonService service, AuthenticationService auth) {
            this.db = db;
            this.service = service;
            this.auth = auth;
        }

        [HttpGet("config")]
        public IActionResult GetConfig() {
            var config = db.Configs
                .Include(c => c.DefaultUser)
                .SingleOrDefault();

            if (config == null) {
                return NotFound();
            }

            return Ok(config);
        }

        [Authorize]
        [HttpGet("settings")]
        public IActionResult GetSettings() {
            var config = db.Configs.SingleOrDefault();
            var settings = new AuthorizedSettings {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
            return Ok(settings);
        }

        [Authorize]
        [ValidateModel]
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] AuthorizedSettingsInput settings) {
            var user = await auth.GetCurrentUser(HttpContext);
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password) {
                return Unauthorized();
            }

            var defaultUser = service.GetUser(settings.DefaultUserId);

            if (defaultUser == null) {
                return BadRequest();
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