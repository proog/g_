using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Models;
using Xunit;

namespace Games.UnitTests
{
    public class ViewModelFactoryTests
    {
        [Fact]
        public void UsesExistingGenreRelations()
        {
            var ids = new List<int> { 1, 2 };
            var all = new List<Genre>
            {
                new Genre { Id = 1 },
                new Genre { Id = 2 }
            };
            var game = new Game
            {
                GameGenres = new List<GameGenre>
                {
                    new GameGenre { GenreId = 1 }
                }
            };

            var factory = new ViewModelFactory(null);
            var result = factory.MakeGameGenres(game, ids, all);
            Assert.Equal(2, result.Count);
            Assert.Contains(game.GameGenres.First(), result);
        }

        [Fact]
        public void UsesExistingPlatformRelations()
        {
            var ids = new List<int> { 1, 2 };
            var all = new List<Platform>
            {
                new Platform { Id = 1 },
                new Platform { Id = 2 }
            };
            var game = new Game
            {
                GamePlatforms = new List<GamePlatform>
                {
                    new GamePlatform { PlatformId = 1 }
                }
            };

            var factory = new ViewModelFactory(null);
            var result = factory.MakeGamePlatforms(game, ids, all);
            Assert.Equal(2, result.Count);
            Assert.Contains(game.GamePlatforms.First(), result);
        }

        [Fact]
        public void UsesExistingTagRelations()
        {
            var ids = new List<int> { 1, 2 };
            var all = new List<Tag>
            {
                new Tag { Id = 1 },
                new Tag { Id = 2 }
            };
            var game = new Game
            {
                GameTags = new List<GameTag>
                {
                    new GameTag { TagId = 1 }
                }
            };

            var factory = new ViewModelFactory(null);
            var result = factory.MakeGameTags(game, ids, all);
            Assert.Equal(2, result.Count);
            Assert.Contains(game.GameTags.First(), result);
        }
    }
}
