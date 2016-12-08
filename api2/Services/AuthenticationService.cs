using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Games.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Games {
    public class AuthenticationService {
        private GamesContext db;
        private HttpContext httpContext;
        private const string claimType = "id";
        private const string claimValueType = ClaimValueTypes.Integer;
        private const string authenticationType = "Password";

        public AuthenticationService(GamesContext db, IHttpContextAccessor ctx) {
            this.db = db;
            this.httpContext = ctx.HttpContext;
        }

        public async Task<User> GetCurrentUser() {
            var auth = await httpContext.Authentication.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            if (auth != null && auth.Identity.IsAuthenticated) {
                var claim = auth.Claims.Single(c =>
                    c.Type == claimType && c.ValueType == claimValueType
                );
                var id = int.Parse(claim.Value);
                return db.Users.Find(id);
            }

            return null;
        }

        public async Task Authenticate(User user) {
            var identity = new ClaimsIdentity(
                new [] { new Claim(claimType, user.Id.ToString(), claimValueType) },
                authenticationType
            );
            var principal = new ClaimsPrincipal(identity);
            await httpContext.Authentication.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }

        public async Task Deauthenticate() {
            await httpContext.Authentication.SignOutAsync(
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

        public async Task<bool> IsCurrentUser(User user) {
            var currentUser = await GetCurrentUser();
            return currentUser != null && user.Id == currentUser.Id;
        }
    }
}