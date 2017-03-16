using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Games.Services {
    class AuthenticationService : IAuthenticationService {
        private ICommonService common;
        private const string claimType = "id";
        private const string claimValueType = ClaimValueTypes.Integer;
        private const string authenticationType = "Password";

        public AuthenticationService(ICommonService common) {
            this.common = common;
        }

        public async Task<User> GetCurrentUser(HttpContext ctx) {
            var auth = await ctx.Authentication.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            if (auth != null && auth.Identity.IsAuthenticated) {
                var claim = auth.Claims.Single(c =>
                    c.Type == claimType && c.ValueType == claimValueType
                );
                var id = int.Parse(claim.Value);
                return common.GetUser(id);
            }

            return null;
        }

        public async Task Authenticate(User user, HttpContext ctx) {
            var identity = new ClaimsIdentity(
                new [] { new Claim(claimType, user.Id.ToString(), claimValueType) },
                authenticationType
            );
            var principal = new ClaimsPrincipal(identity);
            await ctx.Authentication.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }

        public async Task Deauthenticate(HttpContext ctx) {
            await ctx.Authentication.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }

        public string HashPassword(string plain) {
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hashBytes = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLower();
        }

        public async Task<bool> IsCurrentUser(User user, HttpContext ctx) {
            var currentUser = await GetCurrentUser(ctx);
            return currentUser != null && user.Id == currentUser.Id;
        }

        public async Task VerifyCurrentUser(User user, HttpContext ctx) {
            common.VerifyExists(user, "The user does not exist.");

            if (!await IsCurrentUser(user, ctx)) {
                throw new UnauthorizedException(
                    "The specified user is not the current user."
                );
            }
        }
    }
}
