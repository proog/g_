using System;
using System.Collections.Generic;
using System.Linq;
using Games.Interfaces;
using Games.Models;

namespace Games.Services
{
    public class SuggestionService : ISuggestionService
    {
        private readonly IGameRepository gameRepository;

        public SuggestionService(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }

        public List<Suggestion> GetSuggestions(User user, bool includeHidden = false)
        {
            var visibleGames = gameRepository.All(user)
                .Where(game => includeHidden || !game.Hidden)
                .ToList();

            var topGames = visibleGames
                .OrderByDescending(g => g.Rating)
                .OrderByDescending(g => g.Playtime)
                .Take(visibleGames.Count / 10) // top 10% of all games
                .ToList();

            // collect genres from top 10 and the occurrences of each
            var topGenres = topGames
                .SelectMany(g => g.GameGenres)
                .Select(g => g.GenreId)
                .GroupBy(id => id)
                .ToDictionary(it => it.Key, it => it.Count());

            var random = new Random();
            return visibleGames
                .Where(g => g.Finished == Completion.NotFinished)
                .Where(g => g.Rating == null)
                .Where(g => g.Playtime == null)
                .Where(g => g.QueuePosition == null)
                .Where(g => !g.CurrentlyPlaying)
                .Where(g => g.WishlistPosition == null)
                .Select(game =>
                {
                    var score = 0;

                    // how similar are the genres to top 10's genres?
                    foreach (var genre in game.GameGenres)
                    {
                        var id = genre.GenreId;

                        if (topGenres.ContainsKey(id))
                            score += topGenres[id];
                    }

                    // how similar is the title on average to top 10
                    var titleSimilarity = 0;
                    foreach (var topGame in topGames)
                    {
                        // TODO similar_text
                        titleSimilarity += 0;
                    }
                    score += topGames.Count > 0
                        ? titleSimilarity / topGames.Count
                        : 0;

                    // 30% chance of getting score boosted by 33%
                    if (random.Next(10) > 7)
                        score += score / 3;

                    return new Suggestion(game, score);
                })
                .OrderByDescending(it => it.Score)
                .ToList();
        }
    }
}