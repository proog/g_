using Microsoft.Extensions.Configuration;

namespace Games.IntegrationTests
{
    public static class Helper
    {
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .AddJsonFile("testsettings.Production.json", optional: true)
                .Build();
        }
    }
}
