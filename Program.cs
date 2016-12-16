using Microsoft.AspNetCore.Hosting;
using System.IO;
using Games.Infrastructure;

namespace Games {
    public class Program {
        public static void Main(string[] args) {
            var cwd = Directory.GetCurrentDirectory();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(cwd)
                .UseWebRoot(Path.Combine(cwd, "public"))
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
