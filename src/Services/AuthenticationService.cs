using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Games.Services
{
    class AuthenticationService : IAuthenticationService
    {
        private readonly GamesContext db;
        private readonly string signingKey;
        private const string claimType = "id";
        private const string claimValueType = ClaimValueTypes.Integer;
        private const string authenticationType = "Password";

        public AuthenticationService(GamesContext db, IOptions<AppSettings> appSettings)
        {
            this.db = db;
            this.signingKey = appSettings.Value.SigningKey;
        }

        public User GetCurrentUser(HttpContext ctx)
        {
            var idClaim = ctx.User.Claims.FirstOrDefault(
                c => c.Type == claimType && c.ValueType == claimValueType
            );
            return idClaim != null
                ? db.GetUser(int.Parse(idClaim.Value))
                : null;
        }

        public string Authenticate(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity(
                new[] { new Claim(claimType, user.Id.ToString(), claimValueType) }
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

        public string HashPassword(string plain)
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hashBytes = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLower();
        }

        public bool IsCurrentUser(User user, HttpContext ctx)
        {
            var currentUser = GetCurrentUser(ctx);
            return currentUser != null && user.Id == currentUser.Id;
        }

        public void VerifyCurrentUser(User user, HttpContext ctx)
        {
            user.VerifyExists("The user does not exist.");

            if (!IsCurrentUser(user, ctx))
            {
                throw new UnauthorizedException(
                    "The specified user is not the current user."
                );
            }
        }
    }
}
