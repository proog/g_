using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Games.Controllers {
    [Route("api/assisted")]
    public class GiantBombController : Controller {
        private Config config;
        private CommonService service;
        private AuthenticationService auth;
        private JsonSerializerSettings jsonSettings;
        private const string NotFoundMessage = "No Giant Bomb API key specified. Please request an API key and update the GiantBombApiKey attribute in the Configs table.";

        public GiantBombController(GamesContext db, CommonService service, AuthenticationService auth) {
            this.service = service;
            this.auth = auth;
            config = db.Configs.SingleOrDefault();
            jsonSettings = service.GetJsonSettings();
            // sometimes GB returns results as an object instead of an
            // array on error conditions. We just squelch errors about
            // those since the response will be unusable anyway
            jsonSettings.Error = (sender, args) => {
                if (args.ErrorContext.Path == "results") {
                    args.ErrorContext.Handled = true;
                }
            };
        }

        [Authorize]
        [HttpGet("search/{title}")]
        public async Task<IActionResult> Search(string title) {
            if (config == null) {
                return NotFound(NotFoundMessage);
            }

            var uri = GetUri("games", new Dictionary<string, string> {
                { "filter", "name:" + Uri.EscapeDataString(title) },
                { "field_list", "name,original_release_date,id" },
                { "limit", "20" }
            });
            var json = await service.GetHttpClient().GetStringAsync(uri);
            var response = JsonConvert
                .DeserializeObject<GBResponse<List<GBSearchResult>>>(json, jsonSettings);

            if (!response.IsSuccess) {
                return this.BadRequest(response.ErrorMessage);
            }

            var results = response.Results
                .Select(it => new SearchResult {
                    Id = it.Id,
                    Title = it.Name
                });
            return Ok(results);
        }

        [Authorize]
        [HttpGet("game/{id}")]
        public async Task<IActionResult> Get(int id) {
            if (config == null) {
                return NotFound(NotFoundMessage);
            }

            var user = await auth.GetCurrentUser(HttpContext);
            var uri = GetUri($"game/{id}", new Dictionary<string, string> {
                { "field_list", "name,original_release_date,genres,platforms,image,developers,publishers" }
            });
            var json = await service.GetHttpClient().GetStringAsync(uri);
            var response = JsonConvert
                .DeserializeObject<GBResponse<GBGameResult>>(json, jsonSettings);

            if (!response.IsSuccess) {
                return this.BadRequest(response.ErrorMessage);
            }

            var gb = response.Results;
            var result = new GameResult {
                Title = gb.Name,
                Developer = gb.Developers?.Select(it => it.Name).FirstOrDefault(),
                Publisher = gb.Publishers?.Select(it => it.Name).FirstOrDefault(),
                ImageUrl = gb.Image.AnyUrl,
                Year = null,
                GenreIds = new List<int>(),
                PlatformIds = new List<int>()
            };

            if (!string.IsNullOrEmpty(gb.OriginalReleaseDate)) {
                var dt = default(DateTime);
                if (DateTime.TryParse(gb.OriginalReleaseDate, out dt)) {
                    result.Year = dt.Year;
                }
            }

            if (gb.Genres != null) {
                result.GenreIds = user.Genres
                    .Where(it => gb.Genres.Exists(gbGenre =>
                        MatchesDescriptor(gbGenre, it)
                    ))
                    .Select(it => it.Id)
                    .ToList();
            }

            if (gb.Platforms != null) {
                result.PlatformIds = user.Platforms
                    .Where(it => gb.Platforms.Exists(gbPlatform =>
                        MatchesDescriptor(gbPlatform, it)
                    ))
                    .Select(it => it.Id)
                    .ToList();
            }

            // if we found only one platform match, use that. Otherwise, use nothing,
            // because odds are it should only be added for one platform anyway
            if (result.PlatformIds.Count > 1) {
                result.PlatformIds.Clear();
            }

            return Ok(result);
        }

        private bool MatchesDescriptor(GBDescriptor gb, Descriptor d) {
            var gbName = gb.Name.Replace(" ", "").ToLower();
            var name = d.Name.Replace(" ", "").ToLower();
            var shortName = d.ShortName.Replace(" ", "").ToLower();

            return gbName.Contains(name)
                || gbName.Contains(shortName)
                || name.Contains(gbName);
        }

        private Uri GetUri(string resource, IDictionary<string, string> queries) {
            return new UriBuilder {
                Scheme = "http",
                Host = "www.giantbomb.com",
                Path = $"api/{resource}",
                Query = string.Join("&",
                    queries.Concat(new [] {
                        new KeyValuePair<string, string>("format", "json"),
                        new KeyValuePair<string, string>("api_key", config.GiantBombApiKey)
                    })
                    .Select(it => $"{it.Key}={it.Value}")
                )
            }.Uri;
        }

        public class GBResponse<T> {
            public string Error;
            public int StatusCode;
            public T Results;
            public bool IsSuccess => StatusCode == 1;
            public string ErrorMessage => $"Error {StatusCode}: {Error}";
        }

        public class GBSearchResult {
            public int Id;
            public string Name;
        }

        public class GBGameResult {
            public string Name;
            public string OriginalReleaseDate;
            public GBImage Image;
            public List<GBDescriptor> Developers;
            public List<GBDescriptor> Publishers;
            public List<GBDescriptor> Genres;
            public List<GBPlatform> Platforms;
        }

        public class GBDescriptor {
            public int Id;
            public string Name;
        }

        public class GBPlatform : GBDescriptor {
            public string Abbreviation;
        }

        public class GBImage {
            public string IconUrl;
            public string SmallUrl;
            public string MediumUrl;
            public string ScreenUrl;
            public string SuperUrl;
            public string ThumbUrl;
            public string TinyUrl;
            public string AnyUrl => new List<string> {
                SuperUrl, MediumUrl, SmallUrl,
                TinyUrl, ThumbUrl, ScreenUrl, IconUrl
            }.FirstOrDefault(it => it != null);
        }

        public class SearchResult {
            public int Id;
            public string Title;
        }

        public class GameResult {
            public string Title;
            public int? Year;
            public string Developer;
            public string Publisher;
            public string ImageUrl;
            public List<int> GenreIds;
            public List<int> PlatformIds;
        }
    }
}