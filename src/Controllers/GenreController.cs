using System.Collections.Generic;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}/genres")]
    public class GenreController : DescriptorControllerBase<Genre>
    {
        public GenreController(GamesContext dbContext, IViewModelFactory vmFactory) : base(dbContext, vmFactory)
        { }

        [HttpGet(Name = Route.Genres)]
        public ActionResult<List<DescriptorViewModel>> GetGenres(int userId)
        {
            return All(userId);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddGenre(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm);
        }

        [HttpGet("{id}", Name = Route.Genre)]
        public ActionResult<DescriptorViewModel> GetGenre(int userId, int id)
        {
            return Single(userId, id);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdateGenre(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeleteGenre(int userId, int id)
        {
            return Delete(userId, id);
        }
    }
}