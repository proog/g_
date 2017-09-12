using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Games.Interfaces;
using Games.Services;
using Xunit;

namespace Games.IntegrationTests
{
    public class GiantBombTests
    {
        private readonly IGiantBombService giantBomb;
        private readonly string apiKey;

        public GiantBombTests()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", new[] { Guid.NewGuid().ToString() });

            giantBomb = new GiantBombService(httpClient);
            apiKey = Helper.GetConfiguration()["giantBombApiKey"];
        }

        [Fact]
        public async Task CanSearchGiantBomb()
        {
            if (apiKey == null)
            {
                Console.WriteLine($"Skipping {nameof(CanSearchGiantBomb)}");
                return;
            }

            var result = (await giantBomb.Search("Steel Panthers III", apiKey)).ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal(73, result.Single().Id);
        }

        [Fact]
        public async Task CanFetchGiantBombGame()
        {
            if (apiKey == null)
            {
                Console.WriteLine($"Skipping {nameof(CanFetchGiantBombGame)}");
                return;
            }

            var result = await giantBomb.GetGame(73, apiKey);
            Assert.NotNull(result);
            Assert.Equal("Steel Panthers III: Brigade Command (1939-1999)", result.Name);
            Assert.Equal("1997-10-31 00:00:00", result.OriginalReleaseDate);
            Assert.Equal(1, result.Developers.Count);
            Assert.Equal(1, result.Publishers.Count);
            Assert.Equal(1, result.Genres.Count);
            Assert.Equal(1, result.Platforms.Count);
        }
    }
}
