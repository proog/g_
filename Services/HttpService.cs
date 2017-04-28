using System.Net.Http;

namespace Games.Services
{
    class HttpService : IHttpService
    {
        public HttpClient Client { get; }

        public HttpService()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add(
                "User-Agent", new[] { "permortensen.com g_sharp 0.1" }
            );
        }
    }
}
