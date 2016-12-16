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
            var varchar = "varchar(255)";
            var tinyint = "int(4)";
            var text = "text";

            builder.Entity<Config>(it => {
                it.ToTable($"{prefix}config");
                it.Property(c => c.Id)
                    .HasColumnName("id");
                it.Property(c => c.DefaultUserId)
                    .HasColumnName("default_user");
                it.Property(c => c.GiantBombApiKey)
                    .HasColumnName("giant_bomb_api_key")
                    .HasColumnType(varchar);
                it.HasOne(c => c.DefaultUser)
                    .WithMany()
                    .HasForeignKey(c => c.DefaultUserId);
            });
            builder.Entity<Game>(it => {
                it.ToTable($"{prefix}games");
                it.Property(g => g.Title)
                    .HasColumnName("title")
                    .HasColumnType(varchar);
                it.Property(g => g.Developer)
                    .HasColumnName("developer")
                    .HasColumnType(varchar);
                it.Property(g => g.Publisher)
                    .HasColumnName("publisher")
                    .HasColumnType(varchar);
                it.Property(g => g.Year)
                    .HasColumnName("year");
                it.Property(g => g.Image)
                    .HasColumnName("image")
                    .HasColumnType(varchar);
                it.Property(g => g.Finished)
                    .HasColumnName("finished")
                    .HasColumnType(tinyint);
                it.Property(g => g.Comment)
                    .HasColumnName("comment")
                    .HasColumnType(text);
                it.Property(g => g.SortAs)
                    .HasColumnName("sort_as")
                    .HasColumnType(varchar);
                it.Property(g => g.PrivateComment)
                    .HasColumnName("private_comment")
                    .HasColumnType(text);
                it.Property(g => g.Playtime)
                    .HasColumnName("playtime")
                    .HasColumnType(tinyint);
                it.Property(g => g.Rating)
                    .HasColumnName("rating")
                    .HasColumnType(tinyint);
                it.Property(g => g.CurrentlyPlaying)
                    .HasColumnName("currently_playing");
                it.Property(g => g.QueuePosition)
                    .HasColumnName("queue_position");
                it.Property(g => g.Hidden)
                    .HasColumnName("hidden");
                it.Property(g => g.WishlistPosition)
                    .HasColumnName("wishlist_position");
                it.Property(g => g.UserId)
                    .HasColumnName("user_id");
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
                it.Property(u => u.Username)
                    .HasColumnName("username")
                    .HasColumnType(varchar);
                it.Property(u => u.Password)
                    .HasColumnName("password")
                    .HasColumnType(varchar);
                it.Property(u => u.View)
                    .HasColumnName("view");
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
            });
            builder.Entity<Platform>(it => {
                it.ToTable($"{prefix}platforms");
            });
            builder.Entity<Tag>(it => {
                it.ToTable($"{prefix}tags");
            });
            builder.Entity<GameGenre>(it => {
                it.ToTable($"{prefix}game_genre");
                it.HasKey(gg => new { gg.GameId, gg.GenreId });
                it.Property(gg => gg.GenreId)
                    .HasColumnName("genre_id");
            });
            builder.Entity<GamePlatform>(it => {
                it.ToTable($"{prefix}game_platform");
                it.HasKey(gp => new { gp.GameId, gp.PlatformId });
                it.Property(gp => gp.PlatformId)
                    .HasColumnName("platform_id");
            });
            builder.Entity<GameTag>(it => {
                it.ToTable($"{prefix}game_tag");
                it.HasKey(gt => new { gt.GameId, gt.TagId });
                it.Property(gt => gt.TagId)
                    .HasColumnName("tag_id");
            });
        }
    }
}