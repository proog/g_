using Games.Models;
using Microsoft.EntityFrameworkCore;

namespace Games.Services
{
    public class GamesContext : DbContext
    {
        public DbSet<Game> Games { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<Platform> Platforms { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<Event> Events { get; set; }

        public GamesContext(DbContextOptions<GamesContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(it =>
            {
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
            modelBuilder.Entity<User>(it =>
            {
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
            modelBuilder.Entity<Config>()
                .HasOne(c => c.DefaultUser)
                .WithMany()
                .HasForeignKey(c => c.DefaultUserId);
            modelBuilder.Entity<GameGenre>()
                .HasKey(gg => new { gg.GameId, gg.GenreId });
            modelBuilder.Entity<GamePlatform>()
                .HasKey(gp => new { gp.GameId, gp.PlatformId });
            modelBuilder.Entity<GameTag>()
                .HasKey(gt => new { gt.GameId, gt.TagId });
        }
    }
}
