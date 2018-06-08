using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;

namespace Games.Controllers
{
    [ApiController]
    [Authorize(Constants.SameUserPolicy)]
    [Route("api/users/{" + Constants.UserIdParameter + "}/games/{id}/image", Name = Route.Image)]
    public class ImageController : ControllerBase
    {
        private readonly IGameRepository gameRepository;
        private readonly IUserRepository userRepository;
        private readonly HttpClient httpClient;
        private readonly IViewModelFactory vmFactory;
        private readonly IAuthenticationService auth;
        private readonly IFileProvider data;

        public ImageController(IGameRepository gameRepository, IUserRepository userRepository, IAuthenticationService auth, IFileProvider data, HttpClient httpClient, IViewModelFactory vmFactory)
        {
            this.gameRepository = gameRepository;
            this.userRepository = userRepository;
            this.auth = auth;
            this.data = data;
            this.httpClient = httpClient;
            this.vmFactory = vmFactory;
        }

        [HttpPost]
        public async Task<ActionResult<GameViewModel>> UploadImage(int userId, int id)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                return NotFound();

            var path = $"images/{game.Id}/image.jpg";
            var fileInfo = data.GetFileInfo(path);

            try {
                var imageStream = Request.HasFormContentType
                ? await ReadFormImage()
                : await ReadGiantBombImage();

                using (imageStream)
                    await WriteToFile(imageStream, fileInfo.PhysicalPath);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ApiError(e.Message));
            }

            game.Image = path;
            gameRepository.Update(game);

            return vmFactory.MakeGameViewModel(game);
        }

        [HttpDelete]
        public ActionResult DeleteImage(int userId, int id)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                return NotFound();

            game.Image = null;
            gameRepository.Update(game);
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

        private async Task WriteToFile(Stream stream, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var file = new FileStream(path, FileMode.Create))
                await stream.CopyToAsync(file);
        }
    }
}
