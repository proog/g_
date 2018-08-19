using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/assisted")]
    public class GiantBombController : ControllerBase
    {
        private readonly string apiKey;
        private readonly IUserRepository userRepository;
        private readonly IGiantBombService giantBomb;
        private readonly IViewModelFactory vmFactory;
        private readonly ApiError NoApiKeyError = new ApiError("No Giant Bomb API key specified. Please request an API key and add it in the settings dialog or database.");

        public GiantBombController(IConfigRepository configRepository, IUserRepository userRepository, IGiantBombService giantBomb, IViewModelFactory vmFactory)
        {
            this.userRepository = userRepository;
            this.giantBomb = giantBomb;
            this.vmFactory = vmFactory;
            apiKey = configRepository.DefaultConfig?.GiantBombApiKey;
        }

        [HttpGet("search", Name = Route.AssistedSearch)]
        public async Task<ActionResult<List<AssistedSearchResult>>> Search([FromQuery] string title)
        {
            if (string.IsNullOrEmpty(apiKey))
                return NotFound(NoApiKeyError);

            var results = await giantBomb.Search(title, apiKey);

            return results
                .Select(vmFactory.MakeAssistedSearchResult)
                .ToList();
        }

        [HttpGet("game/{id}", Name = Route.AssistedGame)]
        public async Task<ActionResult<AssistedGameResult>> Get(int id)
        {
            if (string.IsNullOrEmpty(apiKey))
                return NotFound(NoApiKeyError);

            var idClaim = User.FindFirst(Constants.UserIdClaim);
            var user = userRepository.Get(int.Parse(idClaim.Value));
            var gb = await giantBomb.GetGame(id, apiKey);

            return vmFactory.MakeAssistedGameResult(gb, user.Genres, user.Platforms);
        }
    }
}
