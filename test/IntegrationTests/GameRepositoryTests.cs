using System.IO;
using Games.Models;
using Games.Repositories;
using Games.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Games.IntegrationTests
{
    public class GameRepositoryTests
    {
        [Fact]
        public void DeleteImageDeletesPhysicalDirectory()
        {
            var configuration = Helper.GetConfiguration();
            var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), configuration["dataDirectory"]);
            Directory.CreateDirectory(dataDirectory);

            var fileProvider = new PhysicalFileProvider(dataDirectory);

            var contextOptions = new DbContextOptionsBuilder<GamesContext>().UseInMemoryDatabase("memory").Options;
            var context = new GamesContext(contextOptions);

            var repository = new GameRepository(context, fileProvider);

            var game = new Game { Id = 1234 };
            repository.Add(game);

            var gameDirectory = Path.Combine(dataDirectory, "images", "1234");
            Directory.CreateDirectory(gameDirectory);
            Assert.True(Directory.Exists(gameDirectory));

            repository.Delete(game);
            Assert.False(Directory.Exists(gameDirectory));
        }
    }
}