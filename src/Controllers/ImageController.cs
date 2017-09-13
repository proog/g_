using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Games.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;

namespace Games.Controllers
{
    [Route("api/users/{" + Constants.UserIdParameter + "}/games/{id}/image"), Authorize(Constants.SameUserPolicy)]
    public class ImageController : Controller
    {
        private readonly IGameRepository gameRepository;
        private readonly IUserRepository userRepository;
        private readonly HttpClient httpClient;
        private readonly IAuthenticationService auth;
        private readonly IFileProvider data;

        public ImageController(IGameRepository gameRepository, IUserRepository userRepository, IAuthenticationService auth, IFileProvider data, HttpClient httpClient)
        {
            this.gameRepository = gameRepository;
            this.userRepository = userRepository;
            this.auth = auth;
            this.data = data;
            this.httpClient = httpClient;
        }

        [HttpPost]
        public async Task<GameViewModel> UploadImage(int userId, int id)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                throw new NotFoundException();

            var path = $"images/{game.Id}/image.jpg";
            var fileInfo = data.GetFileInfo(path);
            var imageStream = Request.HasFormContentType
                ? await ReadFormImage(Request)
                : await ReadGiantBombImage(Request);

            using (imageStream)
                await WriteToFile(imageStream, fileInfo.PhysicalPath);

            game.Image = path;
            gameRepository.Update(game);

            return ViewModelFactory.MakeGameViewModel(game);
        }

        [HttpDelete]
        public IActionResult DeleteImage(int userId, int id)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                throw new NotFoundException();

            game.Image = null;
            gameRepository.Update(game);
            return NoContent();
        }

        private async Task<Stream> ReadFormImage(HttpRequest request)
        {
            var form = await request.ReadFormAsync();
            var file = form.Files["image"];

            if (file == null)
                throw new BadRequestException("No image supplied");

            return file.OpenReadStream();
        }

        private async Task<Stream> ReadGiantBombImage(HttpRequest request)
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
