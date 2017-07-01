using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;

namespace Games.Controllers
{
    [Route("api/users/{userId}/games/{id}/image"), Authorize]
    public class ImageController : Controller
    {
        private readonly GamesContext db;
        private readonly HttpClient httpClient;
        private readonly IAuthenticationService auth;
        private readonly IFileProvider data;

        public ImageController(GamesContext db, IAuthenticationService auth, IFileProvider data, HttpClient httpClient)
        {
            this.db = db;
            this.auth = auth;
            this.data = data;
            this.httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int userId, int id)
        {
            var user = db.GetUser(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var game = user.Games.SingleOrDefault(g => g.Id == id);
            game.VerifyExists();
            Stream imageStream;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files["image"];

                if (file == null)
                    throw new BadRequestException("No image supplied");

                imageStream = file.OpenReadStream();
            }
            else
            {
                string url;
                using (var reader = new StreamReader(Request.Body))
                {
                    var json = reader.ReadToEnd();
                    url = (string)JObject.Parse(json)["image_url"];
                }

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    throw new BadRequestException("Not a valid url");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Giant Bomb returned non-success status code");

                imageStream = await response.Content.ReadAsStreamAsync();
            }

            var path = $"images/{game.Id}/image.jpg";
            var fileInfo = data.GetFileInfo(path);

            using (imageStream)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath));

                using (var stream = new FileStream(fileInfo.PhysicalPath, FileMode.Create))
                {
                    imageStream.CopyTo(stream);
                }
            }

            game.Image = path;
            db.SaveChanges();
            return Ok(game);
        }

        [HttpDelete]
        public IActionResult DeleteImage(int userId, int id)
        {
            var user = db.GetUser(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var game = user.Games.SingleOrDefault(g => g.Id == id);
            game.VerifyExists();

            game.Image = null;
            db.SaveChanges();

            var imageDir = data.GetFileInfo($"images/{game.Id}");
            Directory.Delete(imageDir.PhysicalPath, true);

            return NoContent();
        }
    }
}
