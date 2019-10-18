using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Games.Controllers
{
    [ApiController]
    [Authorize(Constants.SameUserPolicy)]
    [Route("api/users/{" + Constants.UserIdParameter + "}/games/{id}/image", Name = Route.Image)]
    public class ImageController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly HttpClient httpClient;
        private readonly IViewModelFactory vmFactory;
        private readonly IImageService imageService;

        public ImageController(GamesContext dbContext, IHttpClientFactory httpClientFactory, IViewModelFactory vmFactory, IImageService imageService)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClientFactory.CreateClient();
            this.vmFactory = vmFactory;
            this.imageService = imageService;
        }

        [HttpPost]
        public async Task<ActionResult<GameViewModel>> UploadImage(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);
            var game = dbContext.Entry(user).Collection(u => u.Games).Query().FirstOrDefault(g => g.Id == id);

            if (game == null)
                return NotFound();

            string imagePath;
            try
            {
                var imageStream = Request.HasFormContentType
                    ? await ReadFormImage()
                    : await ReadGiantBombImage();

                using (imageStream)
                    imagePath = await imageService.CreateImage(game, imageStream);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ApiError(e.Message));
            }

            game.Image = imagePath;
            game.UpdatedAt = DateTime.UtcNow;

            dbContext.Events.Add(new Event("ImageUpdated", new { game.Id, game.Image }, user));
            dbContext.SaveChanges();

            return vmFactory.MakeGameViewModel(game);
        }

        [HttpDelete]
        public ActionResult DeleteImage(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);
            var game = dbContext.Entry(user).Collection(u => u.Games).Query().FirstOrDefault(g => g.Id == id);

            if (game == null)
                return NotFound();

            imageService.DeleteImage(game);
            game.Image = null;
            game.UpdatedAt = DateTime.UtcNow;

            dbContext.Events.Add(new Event("ImageDeleted", new { game.Id }, user));
            dbContext.SaveChanges();

            return NoContent();
        }

        private async Task<Stream> ReadFormImage()
        {
            var form = await Request.ReadFormAsync();
            var file = form.Files["image"];

            if (file == null)
                throw new ArgumentException("No image supplied");

            return file.OpenReadStream();
        }

        private async Task<Stream> ReadGiantBombImage()
        {
            string url;
            using (var reader = new StreamReader(Request.Body))
            {
                var json = reader.ReadToEnd();
                url = (string)JObject.Parse(json)["image_url"];
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("Not a valid url");

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Giant Bomb returned non-success status code");

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
