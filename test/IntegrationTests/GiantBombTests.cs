using System;
using System.Linq;
using System.Net.Http;
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
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine($"Skipping {nameof(CanSearchGiantBomb)}");
                return;
            }

            var result = (await giantBomb.Search("Steel Panthers III", apiKey)).ToList();
            Assert.Single(result);
            Assert.Equal(73, result.Single().Id);
        }

        [Fact]
        public async Task CanFetchGiantBombGame()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine($"Skipping {nameof(CanFetchGiantBombGame)}");
                return;
            }

            var result = await giantBomb.GetGame(73, apiKey);
            Assert.NotNull(result);
            Assert.Equal("Steel Panthers III: Brigade Command (1939-1999)", result.Name);
            Assert.Equal("1997-10-31 00:00:00", result.OriginalReleaseDate);
            Assert.Single(result.Developers);
            Assert.Single(result.Publishers);
            Assert.Single(result.Genres);
            Assert.Single(result.Platforms);
        }
    }
}
