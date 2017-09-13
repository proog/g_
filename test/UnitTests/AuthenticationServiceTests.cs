using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Games.UnitTests
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public void SignsJwtCorrectly()
        {
            var options = Options.Create<AppSettings>(new AppSettings
            {
                SigningKey = Guid.NewGuid().ToString()
            });
            var user = new User
            {
                Id = new Random().Next()
            };
            var auth = new AuthenticationService(options);
            var jwt = auth.Authenticate(user);

            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey))
            }, out SecurityToken validated);

            Assert.Equal(user.Id, int.Parse(claims.FindFirst(Constants.UserIdClaim).Value));
        }
    }
}
