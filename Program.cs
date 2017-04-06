using Microsoft.AspNetCore.Hosting;
using System.IO;
using Games.Infrastructure;

namespace Games {
    public class Program {
        public static void Main(string[] args) {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseWebRoot("public")
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
