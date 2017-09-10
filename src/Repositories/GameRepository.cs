using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Games.Repositories
{
    class GameRepository : IGameRepository
    {
        private readonly GamesContext db;
        private readonly IFileProvider files;

        public GameRepository(GamesContext db, IFileProvider files)
        {
            this.db = db;
            this.files = files;
        }

        public IEnumerable<Game> All(User user)
        {
            return Query(user);
        }

        public Game Get(User user, int id)
        {
            return Query(user).SingleOrDefault(x => x.Id == id);
        }

        public void Add(Game game)
        {
            game.CreatedAt = DateTime.UtcNow;
            game.UpdatedAt = DateTime.UtcNow;

            db.Games.Add(game);
            db.SaveChanges();
        }

        public void Update(Game game)
        {
            game.UpdatedAt = DateTime.UtcNow;

            if (game.Image == null)
                DeleteImage(game);

            db.Games.Update(game);
            db.SaveChanges();
        }

        public void Delete(Game game)
        {
            DeleteImage(game);

            db.Games.Remove(game);
            db.SaveChanges();
        }

        private IQueryable<Game> Query(User user)
        {
            return db.Entry(user)
                .Collection(u => u.Games)
                .Query()
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameTags);
        }

        private void DeleteImage(Game game)
        {
            var imageDir = files.GetFileInfo($"images/{game.Id}");

            if (imageDir.Exists)
                Directory.Delete(imageDir.PhysicalPath, true);
        }
    }
}
