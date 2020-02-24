using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Interfaces;
using Games.Services;
using Moq;
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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            giantBomb = new GiantBombService(httpClientFactoryMock.Object);
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
            Assert.Equal("1997-10-31", result.OriginalReleaseDate);
            Assert.Single(result.Developers);
            Assert.Single(result.Publishers);
            Assert.Single(result.Genres);
            Assert.Single(result.Platforms);
        }
    }
}
