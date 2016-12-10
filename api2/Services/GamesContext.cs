using System;
using Games.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Games.Services {
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
            builder.UseMySql(string.Join(";",
                $"Server={settings.Host}",
                $"Port={settings.Port}",
                $"Database={settings.Database}",
                $"Uid={settings.Username}",
                $"Pwd={settings.Password}",
                $"CharSet={settings.Charset}",
                $"ConvertZeroDateTime=True"
            ));
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            var prefix = settings.Prefix;

            builder.Entity<Config>(it => {
                it.ToTable($"{prefix}config");
                it.Property(c => c.DefaultUserId)
                    .HasColumnName("default_user");
                it.Property(c => c.GiantBombApiKey)
                    .HasColumnName("giant_bomb_api_key");
                it.HasOne(c => c.DefaultUser)
                    .WithMany()
                    .HasForeignKey(c => c.DefaultUserId);
            });
            builder.Entity<Game>(it => {
                it.ToTable($"{prefix}games");
                it.Property(g => g.SortAs)
                    .HasColumnName("sort_as");
                it.Property(g => g.PrivateComment)
                    .HasColumnName("private_comment");
                it.Property(g => g.CurrentlyPlaying)
                    .HasColumnName("currently_playing");
                it.Property(g => g.QueuePosition)
                    .HasColumnName("queue_position");
                it.Property(g => g.WishlistPosition)
                    .HasColumnName("wishlist_position");
                it.Property(g => g.UserId)
                    .HasColumnName("user_id");
                it.Property(g => g.CreatedAt)
                    .HasColumnName("created_at");
                it.Property(g => g.UpdatedAt)
                    .HasColumnName("updated_at");
                it.Ignore(g => g.GenreIds)
                    .Ignore(g => g.PlatformIds)
                    .Ignore(g => g.TagIds);
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
                it.ToTable($"{prefix}users");
                it.Property(u => u.CreatedAt)
                    .HasColumnName("created_at");
                it.Property(u => u.UpdatedAt)
                    .HasColumnName("updated_at");
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
            builder.Entity<Genre>(it => {
                it.ToTable($"{prefix}genres");
                it.Property(g => g.CreatedAt)
                    .HasColumnName("created_at");
                it.Property(g => g.UpdatedAt)
                    .HasColumnName("updated_at");
                it.Property(g => g.ShortName)
                    .HasColumnName("short_name");
                it.Property(g => g.UserId)
                    .HasColumnName("user_id");
            });
            builder.Entity<Platform>(it => {
                it.ToTable($"{prefix}platforms");
                it.Property(p => p.CreatedAt)
                    .HasColumnName("created_at");
                it.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at");
                it.Property(p => p.ShortName)
                    .HasColumnName("short_name");
                it.Property(p => p.UserId)
                    .HasColumnName("user_id");
            });
            builder.Entity<Tag>(it => {
                it.ToTable($"{prefix}tags");
                it.Property(t => t.CreatedAt)
                    .HasColumnName("created_at");
                it.Property(t => t.UpdatedAt)
                    .HasColumnName("updated_at");
                it.Property(t => t.ShortName)
                    .HasColumnName("short_name");
                it.Property(t => t.UserId)
                    .HasColumnName("user_id");
            });
            builder.Entity<GameGenre>(it => {
                it.ToTable($"{prefix}game_genre");
                it.HasKey(gg => new { gg.GameId, gg.GenreId });
                it.Property(gg => gg.GameId)
                    .HasColumnName("game_id");
                it.Property(gg => gg.GenreId)
                    .HasColumnName("genre_id");
            });
            builder.Entity<GamePlatform>(it => {
                it.ToTable($"{prefix}game_platform");
                it.HasKey(gp => new { gp.GameId, gp.PlatformId });
                it.Property(gp => gp.GameId)
                    .HasColumnName("game_id");
                it.Property(gp => gp.PlatformId)
                    .HasColumnName("platform_id");
            });
            builder.Entity<GameTag>(it => {
                it.ToTable($"{prefix}game_tag");
                it.HasKey(gt => new { gt.GameId, gt.TagId });
                it.Property(gt => gt.GameId)
                    .HasColumnName("game_id");
                it.Property(gt => gt.TagId)
                    .HasColumnName("tag_id");
            });
        }
    }
}