using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;

namespace Games.Controllers {
    [Route("api/users/{userId}/games/{id}/image"), Authorize]
    public class ImageController : Controller {
        private GamesContext db;
        private CommonService common;
        private AuthenticationService auth;
        private IFileProvider data;

        public ImageController(GamesContext db, CommonService common, AuthenticationService auth, IFileProvider fileProvider) {
            this.db = db;
            this.common = common;
            this.auth = auth;
            this.data = fileProvider;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int userId, int id) {
            var user = common.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var game = user.Games.SingleOrDefault(g => g.Id == id);
            common.VerifyExists(game);
            Stream imageStream;

            if (Request.HasFormContentType) {
                var form = await Request.ReadFormAsync();
                var file = form.Files["image"];

                if (file == null) {
                    throw new BadRequestException("No image supplied");
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
                    throw new BadRequestException("Not a valid url");
                }

                var client = common.GetHttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode) {
                    throw new Exception("Giant Bomb returned non-success status code");
                }

                imageStream = await response.Content.ReadAsStreamAsync();
            }

            var path = $"images/{game.Id}/image.jpg";
            var fileInfo = data.GetFileInfo(path);

            using (imageStream) {
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath));

                using (var stream = new FileStream(fileInfo.PhysicalPath, FileMode.Create)) {
                    imageStream.CopyTo(stream);
                }
            }

            game.Image = path;
            db.SaveChanges();
            return Ok(game);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int userId, int id) {
            var user = common.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var game = user.Games.SingleOrDefault(g => g.Id == id);
            common.VerifyExists(game);

            common.DeleteImageDirectory(game);
            game.Image = null;
            db.SaveChanges();
            return NoContent();
        }
    }
}
