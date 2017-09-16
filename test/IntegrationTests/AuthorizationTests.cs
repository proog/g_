using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Games.IntegrationTests
{
    public class AuthorizationTests
    {
        private readonly HttpClient client;
        private readonly User user;
        private readonly string jwt;

        public AuthorizationTests()
        {
            var configuration = Helper.GetConfiguration();

            if (Directory.Exists(configuration["dataDirectory"]))
                Directory.Delete(configuration["dataDirectory"], true);

            var hostBuilder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
            var testServer = new TestServer(hostBuilder);
            client = testServer.CreateClient();

            user = CreateUser(testServer);
            jwt = CreateJwt(user.Id, configuration["signingKey"]);
        }

        [Fact]
        public async void ReturnsNonAuthorizationCodeForSameUser()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{user.Id}/games")
            {
                Content = new StringContent(""),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", jwt)
                }
            };
            var response = await client.SendAsync(request);
            Assert.Equal(415, (int)response.StatusCode);
        }

        [Fact]
        public async void ReturnsUnauthorizedWithoutValidJwt()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{user.Id}/games")
            {
                Content = new StringContent(""),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", jwt + "a")
                }
            };
            var response = await client.SendAsync(request);
            Assert.Equal(401, (int)response.StatusCode);
        }

        [Fact]
        public async void ReturnsForbiddenForDifferentUser()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{user.Id + 1}/games")
            {
                Content = new StringContent(""),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", jwt)
                }
            };
            var response = await client.SendAsync(request);
            Assert.Equal(403, (int)response.StatusCode);
        }

        private User CreateUser(TestServer testServer)
        {
            var gamesContext = testServer.Host.Services.GetService<GamesContext>();
            var user = new User
            {
                Username = "testuser",
                Password = "9f735e0df9a1ddc702bf0a1a7b83033f9f7153a00c29de82cedadc9957289b05"
            };
            gamesContext.Users.Add(user);
            gamesContext.SaveChanges();
            return user;
        }

        private string CreateJwt(int userId, string signingKey)
        {
            var handler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity(
                new[] { new Claim("id", userId.ToString(), ClaimValueTypes.Integer) }
            );
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256Signature
            );
            var token = handler.CreateJwtSecurityToken(
                subject: identity,
                signingCredentials: signingCredentials,
                expires: DateTime.Now.AddHours(6)
            );
            return handler.WriteToken(token);
        }
    }
}
