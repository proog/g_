using Games.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Games {
    public class GamesContext : DbContext {
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Config> Configs { get; set; }
        private AppSettings settings;

        public GamesContext(IOptions<AppSettings> options) {
            settings = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder) {
            var host = settings.Host;
            var db = settings.Database;
            var user = settings.Username;
            var pass = settings.Password;
            builder.UseMySql($"Server={host};Database={db};Uid={user};Pwd={pass};ConvertZeroDateTime=True;");
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.Entity<GameGenre>()
                .HasKey(it => new { it.GameId, it.GenreId });
            builder.Entity<GamePlatform>()
                .HasKey(it => new { it.GameId, it.PlatformId });
            builder.Entity<GameTag>()
                .HasKey(it => new { it.GameId, it.TagId });
        }
    }
}