using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Games.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string signingKey;

        public AuthenticationService(IOptions<AppSettings> appSettings)
        {
            signingKey = appSettings.Value.SigningKey;
        }

        public string Authenticate(User user)
        {
            var claims = new[]
            {
                new Claim(Constants.UserIdClaim, user.Id.ToString(), ClaimValueTypes.Integer)
            };
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256Signature
            );
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(
                subject: new ClaimsIdentity(claims),
                signingCredentials: signingCredentials,
                expires: DateTime.Now.AddHours(6)
            );
            return handler.WriteToken(token);
        }

        public string HashPassword(string plain)
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hashBytes = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLower();
        }
    }
}
