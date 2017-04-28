using System.Net.Http;

namespace Games.Services
{
    public interface IHttpService
    {
        HttpClient Client { get; }
    }
}
