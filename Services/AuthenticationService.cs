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
using Microsoft.IdentityModel.Tokens;

namespace Games.Services
{
    class AuthenticationService : IAuthenticationService
    {
        private readonly GamesContext db;
        private const string claimType = "id";
        private const string claimValueType = ClaimValueTypes.Integer;
        private const string authenticationType = "Password";

        public AuthenticationService(GamesContext db)
        {
            this.db = db;
        }

        public async Task<User> GetCurrentUser(HttpContext ctx)
        {
            var idClaim = ctx.User.Claims.FirstOrDefault(
                c => c.Type == claimType && c.ValueType == claimValueType
            );
            return idClaim != null
                ? db.GetUser(int.Parse(idClaim.Value))
                : null;
        }

        public async Task<string> Authenticate(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(
                subject: new ClaimsIdentity(
                    new[] { new Claim(claimType, user.Id.ToString(), claimValueType) },
                    authenticationType
                ),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("hejsasdifosdfnsdiofnsdifosdnfiosdfndsio")),
                    SecurityAlgorithms.HmacSha256Signature
                )
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

        public async Task<bool> IsCurrentUser(User user, HttpContext ctx)
        {
            var currentUser = await GetCurrentUser(ctx);
            return currentUser != null && user.Id == currentUser.Id;
        }

        public async Task VerifyCurrentUser(User user, HttpContext ctx)
        {
            user.VerifyExists("The user does not exist.");

            if (!await IsCurrentUser(user, ctx))
            {
                throw new UnauthorizedException(
                    "The specified user is not the current user."
                );
            }
        }
    }
}
