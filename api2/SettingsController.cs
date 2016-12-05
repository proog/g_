using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games {
    public class SettingsController : Controller {
        private GamesContext db;
        private AuthenticationService auth;

        public SettingsController(GamesContext db, AuthenticationService auth) {
            this.db = db;
            this.auth = auth;
        }

        [HttpGet("config")]
        public Config GetConfig() {
            return db.Configs.Single();
        }

        [Authorize]
        [HttpGet("settings")]
        public AuthorizedSettings GetSettings() {
            var config = GetConfig();
            return new AuthorizedSettings {
                DefaultUserId = config.DefaultUserId,
                GiantBombApiKey = config.GiantBombApiKey
            };
        }

        [Authorize]
        [ValidateModel]
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] AuthorizedSettingsInput settings) {
            var user = await auth.GetCurrentUser();
            var hash = auth.HashPassword(settings.OldPassword);

            if (hash != user.Password) {
                return Unauthorized();
            }

            var defaultUser = db.Users.Find(settings.DefaultUserId);

            if (defaultUser == null) {
                return BadRequest();
            }

            var config = GetConfig();
            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = settings.GiantBombApiKey;

            if (!string.IsNullOrEmpty(settings.NewPassword)) {
                user.Password = auth.HashPassword(settings.NewPassword);
            }

            db.SaveChanges();
            return Ok(GetSettings());
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