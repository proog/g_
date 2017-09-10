using System.Threading.Tasks;
using Games.Models;
using Microsoft.AspNetCore.Http;

namespace Games.Interfaces
{
    public interface IAuthenticationService
    {
        User GetCurrentUser(HttpContext ctx);

        string Authenticate(User user);

        string HashPassword(string plain);

        bool IsCurrentUser(User user, HttpContext ctx);

        void VerifyCurrentUser(User user, HttpContext ctx);
    }
}
