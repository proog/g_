using System.Threading.Tasks;
using Games.Models;
using Microsoft.AspNetCore.Http;

namespace Games.Interfaces
{
    public interface IAuthenticationService
    {
        string Authenticate(User user);

        string HashPassword(string plain);
    }
}
