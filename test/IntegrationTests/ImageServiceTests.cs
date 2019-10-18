using System.IO;
using Games.Models;
using Games.Services;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Games.IntegrationTests
{
    public class ImageServiceTests
    {
        [Fact]
        public void DeleteImageDeletesPhysicalDirectory()
        {
            var configuration = Helper.GetConfiguration();
            var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), configuration["dataDirectory"]);
            Directory.CreateDirectory(dataDirectory);

            var fileProvider = new PhysicalFileProvider(dataDirectory);
            var imageService = new ImageService(fileProvider);

            var game = new Game { Id = 1234 };
            var gameDirectory = Path.Combine(dataDirectory, "images", "1234");
            Directory.CreateDirectory(gameDirectory);

            Assert.True(Directory.Exists(gameDirectory));
            imageService.DeleteImage(game);
            Assert.False(Directory.Exists(gameDirectory));
        }
    }
}