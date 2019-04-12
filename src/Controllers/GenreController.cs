using System.Collections.Generic;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}/genres")]
    public class GenreController : DescriptorControllerBase
    {
        private readonly IGenreRepository genreRepository;

        public GenreController(IGenreRepository genreRepository, IUserRepository userRepository, IEventRepository eventRepository, IViewModelFactory vmFactory) : base(userRepository, eventRepository, vmFactory)
        {
            this.genreRepository = genreRepository;
        }

        [HttpGet(Name = Route.Genres)]
        public ActionResult<List<DescriptorViewModel>> GetGenres(int userId)
        {
            return All(userId, genreRepository);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddGenre(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm, genreRepository);
        }

        [HttpGet("{id}", Name = Route.Genre)]
        public ActionResult<DescriptorViewModel> GetGenre(int userId, int id)
        {
            return Single(userId, id, genreRepository);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdateGenre(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm, genreRepository);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeleteGenre(int userId, int id)
        {
            return Delete(userId, id, genreRepository);
        }
    }
}