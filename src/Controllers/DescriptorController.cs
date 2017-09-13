using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Repositories;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers
{
    [Route("api/users/{" + Constants.UserIdParameter + "}")]
    public class DescriptorController : Controller
    {
        private readonly IUserRepository users;
        private readonly IGenreRepository genres;
        private readonly IPlatformRepository platforms;
        private readonly ITagRepository tags;
        private readonly IAuthenticationService auth;

        public DescriptorController(IUserRepository users, IGenreRepository genres, IPlatformRepository platforms, ITagRepository tags, IAuthenticationService auth)
        {
            this.users = users;
            this.genres = genres;
            this.platforms = platforms;
            this.tags = tags;
            this.auth = auth;
        }

        [HttpGet("genres")]
        public List<DescriptorViewModel> GetGenres(int userId)
        {
            return All(userId, genres);
        }
        [HttpGet("platforms")]
        public List<DescriptorViewModel> GetPlatforms(int userId)
        {
            return All(userId, platforms);
        }
        [HttpGet("tags")]
        public List<DescriptorViewModel> GetTags(int userId)
        {
            return All(userId, tags);
        }

        [HttpGet("genres/{id}")]
        public DescriptorViewModel GetGenre(int userId, int id)
        {
            return Single(userId, id, genres);
        }
        [HttpGet("platforms/{id}")]
        public DescriptorViewModel GetPlatform(int userId, int id)
        {
            return Single(userId, id, platforms);
        }
        [HttpGet("tags/{id}")]
        public DescriptorViewModel GetTag(int userId, int id)
        {
            return Single(userId, id, tags);
        }

        [HttpPost("genres"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel AddGenre(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, genres);
        }
        [HttpPost("platforms"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel AddPlatform(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, platforms);
        }
        [HttpPost("tags"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel AddTag(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, tags);
        }

        [HttpPut("genres/{id}"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel UpdateGenre(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, genres);
        }
        [HttpPut("platforms/{id}"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel UpdatePlatform(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, platforms);
        }
        [HttpPut("tags/{id}"), Authorize(Constants.SameUserPolicy)]
        public DescriptorViewModel UpdateTag(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, tags);
        }

        [HttpDelete("genres/{id}"), Authorize(Constants.SameUserPolicy)]
        public IActionResult DeleteGenre(int userId, int id)
        {
            return Delete(userId, id, genres);
        }
        [HttpDelete("platforms/{id}"), Authorize(Constants.SameUserPolicy)]
        public IActionResult DeletePlatform(int userId, int id)
        {
            return Delete(userId, id, platforms);
        }
        [HttpDelete("tags/{id}"), Authorize(Constants.SameUserPolicy)]
        public IActionResult DeleteTag(int userId, int id)
        {
            return Delete(userId, id, tags);
        }

        private DescriptorViewModel Add<T>(int userId, DescriptorViewModel vm, IDescriptorRepository<T> repository) where T : Descriptor, new()
        {
            var user = users.Get(userId);
            var descriptor = new T
            {
                Name = vm.Name,
                ShortName = vm.ShortName,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            repository.Add(descriptor);

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }

        private DescriptorViewModel Update<T>(int userId, int id, DescriptorViewModel vm, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = users.Get(userId);
            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                throw new NotFoundException();

            descriptor.Name = vm.Name;
            descriptor.ShortName = vm.ShortName;
            descriptor.UpdatedAt = DateTime.UtcNow;
            repository.Update(descriptor);

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }

        private IActionResult Delete<T>(int userId, int id, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = users.Get(userId);
            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                throw new NotFoundException();

            repository.Delete(descriptor);
            return NoContent();
        }

        private List<DescriptorViewModel> All<T>(int userId, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = users.Get(userId);

            if (user == null)
                throw new NotFoundException();

            return repository.All(user)
                .Select(ViewModelFactory.MakeDescriptorViewModel)
                .ToList();
        }

        private DescriptorViewModel Single<T>(int userId, int id, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = users.Get(userId);

            if (user == null)
                throw new NotFoundException();

            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                throw new NotFoundException();

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }
    }
}
