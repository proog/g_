using System.IO;
using Games.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Games.Services {
    public class GamesContext : DbContext {
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Config> Configs { get; set; }
        private IHostingEnvironment environment;

        public GamesContext(IHostingEnvironment environment) {
            this.environment = environment;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder) {
            var path = Path.Combine(environment.ContentRootPath, "data/games.db");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            builder.UseSqlite($"Data Source={path}");
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.Entity<Game>(it => {
                it.HasMany(g => g.GameGenres)
                    .WithOne(gg => gg.Game)
                    .HasForeignKey(gg => gg.GameId);
                it.HasMany(g => g.GamePlatforms)
                    .WithOne(gp => gp.Game)
                    .HasForeignKey(gp => gp.GameId);
                it.HasMany(g => g.GameTags)
                    .WithOne(gt => gt.Game)
                    .HasForeignKey(gt => gt.GameId);
            });
            builder.Entity<User>(it => {
                it.HasMany(u => u.Games)
                    .WithOne(g => g.User)
                    .HasForeignKey(g => g.UserId);
                it.HasMany(u => u.Genres)
                    .WithOne(g => g.User)
                    .HasForeignKey(g => g.UserId);
                it.HasMany(u => u.Platforms)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);
                it.HasMany(u => u.Tags)
                    .WithOne(t => t.User)
                    .HasForeignKey(t => t.UserId);
            });
            builder.Entity<Config>()
                .HasOne(c => c.DefaultUser)
                .WithMany()
                .HasForeignKey(c => c.DefaultUserId);
            builder.Entity<GameGenre>()
                .HasKey(gg => new { gg.GameId, gg.GenreId });
            builder.Entity<GamePlatform>()
                .HasKey(gp => new { gp.GameId, gp.PlatformId });
            builder.Entity<GameTag>()
                .HasKey(gt => new { gt.GameId, gt.TagId });
        }
    }
}