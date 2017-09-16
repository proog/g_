using Games.Models;

namespace Games.Interfaces
{
    public interface IAuthenticationService
    {
        string Authenticate(User user);

        string HashPassword(string plain);
    }
}
