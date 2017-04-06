using System.Threading.Tasks;
using Games.Models;
using Microsoft.AspNetCore.Http;

namespace Games.Services
{
    public interface IAuthenticationService
    {
        Task<User> GetCurrentUser(HttpContext ctx);

        Task<string> Authenticate(User user);

        string HashPassword(string plain);

        Task<bool> IsCurrentUser(User user, HttpContext ctx);

        Task VerifyCurrentUser(User user, HttpContext ctx);
    }
}
