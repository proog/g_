using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Games.Controllers {
    [Route("api/users/{userId}/games/{id}/image")]
    public class ImageController : Controller {
        private GamesContext db;
        private GameService service;
        private AuthenticationService auth;
        private IHostingEnvironment environment;

        public ImageController(GamesContext db, GameService service, AuthenticationService auth, IHostingEnvironment env) {
            this.db = db;
            this.service = service;
            this.auth = auth;
            this.environment = env;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadImage(int userId, int id) {
            var user = service.GetUser(userId);
            var invalid = await auth.VerifyUserIsCurrent(user, HttpContext);

            if (invalid != null) {
                return invalid;
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            Stream imageStream;

            if (Request.HasFormContentType) {
                var form = await Request.ReadFormAsync();
                var file = form.Files["image"];

                if (file == null) {
                    return BadRequest("No image supplied");
                }

                imageStream = file.OpenReadStream();
            }
            else {
                string url;
                using (var reader = new StreamReader(Request.Body)) {
                    var json = reader.ReadToEnd();
                    url = (string) JObject.Parse(json)["image_url"];
                }

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                    return BadRequest("Not a valid url");
                }

                var client = service.GetHttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode) {
                    return BadRequest("Giant Bomb returned non-success status code");
                }

                imageStream = await response.Content.ReadAsStreamAsync();
            }

            var path = $"images/{game.Id}/image.jpg";
            var absPath = Path.Combine(environment.WebRootPath, path);

            using (imageStream) {
                Directory.CreateDirectory(Path.GetDirectoryName(absPath));

                using (var stream = new FileStream(absPath, FileMode.Create)) {
                    imageStream.CopyTo(stream);
                }
            }

            game.Image = path;
            db.SaveChanges();

            return Ok(game);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int userId, int id) {
            var user = service.GetUser(userId);
            var invalid = await auth.VerifyUserIsCurrent(user, HttpContext);

            if (invalid != null) {
                return invalid;
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            var path = Path.Combine(environment.WebRootPath, $"images/{game.Id}");
            Directory.Delete(path, true);

            game.Image = null;
            db.SaveChanges();

            return NoContent();
        }
    }
}