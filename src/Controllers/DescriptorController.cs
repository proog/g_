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
    [Route("api/users/{userId}")]
    public class DescriptorController : Controller
    {
        private readonly GamesContext db;
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService auth;

        public DescriptorController(GamesContext db, IUserRepository userRepository, IAuthenticationService auth)
        {
            this.db = db;
            this.userRepository = userRepository;
            this.auth = auth;
        }

        [HttpGet("genres")]
        public List<DescriptorViewModel> GetGenres(int userId)
        {
            return All(userId, u => u.Genres);
        }
        [HttpGet("platforms")]
        public List<DescriptorViewModel> GetPlatforms(int userId)
        {
            return All(userId, u => u.Platforms);
        }
        [HttpGet("tags")]
        public List<DescriptorViewModel> GetTags(int userId)
        {
            return All(userId, u => u.Tags);
        }

        [HttpGet("genres/{id}")]
        public DescriptorViewModel GetGenre(int userId, int id)
        {
            return Single(userId, id, u => u.Genres);
        }
        [HttpGet("platforms/{id}")]
        public DescriptorViewModel GetPlatform(int userId, int id)
        {
            return Single(userId, id, u => u.Platforms);
        }
        [HttpGet("tags/{id}")]
        public DescriptorViewModel GetTag(int userId, int id)
        {
            return Single(userId, id, u => u.Tags);
        }

        [HttpPost("genres"), Authorize]
        public DescriptorViewModel AddGenre(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, () => db.Genres);
        }
        [HttpPost("platforms"), Authorize]
        public DescriptorViewModel AddPlatform(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, () => db.Platforms);
        }
        [HttpPost("tags"), Authorize]
        public DescriptorViewModel AddTag(int userId, [FromBody] DescriptorViewModel rendition)
        {
            return Add(userId, rendition, () => db.Tags);
        }

        [HttpPut("genres/{id}"), Authorize]
        public DescriptorViewModel UpdateGenre(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, u => u.Genres);
        }
        [HttpPut("platforms/{id}"), Authorize]
        public DescriptorViewModel UpdatePlatform(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, u => u.Platforms);
        }
        [HttpPut("tags/{id}"), Authorize]
        public DescriptorViewModel UpdateTag(int userId, int id, [FromBody] DescriptorViewModel rendition)
        {
            return Update(userId, id, rendition, u => u.Tags);
        }

        [HttpDelete("genres/{id}"), Authorize]
        public IActionResult DeleteGenre(int userId, int id)
        {
            return Delete(userId, id, u => u.Genres);
        }
        [HttpDelete("platforms/{id}"), Authorize]
        public IActionResult DeletePlatform(int userId, int id)
        {
            return Delete(userId, id, u => u.Platforms);
        }
        [HttpDelete("tags/{id}"), Authorize]
        public IActionResult DeleteTag(int userId, int id)
        {
            return Delete(userId, id, u => u.Tags);
        }

        private DescriptorViewModel Add<T>(int userId, DescriptorViewModel vm, Func<DbSet<T>> getter) where T : Descriptor, new()
        {
            var user = userRepository.Get(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var descriptor = new T
            {
                Name = vm.Name,
                ShortName = vm.ShortName,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            getter().Add(descriptor);
            db.SaveChanges();

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }

        private DescriptorViewModel Update<T>(int userId, int id, DescriptorViewModel vm, Func<User, IEnumerable<T>> getter) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var descriptor = getter(user)
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            descriptor.Name = vm.Name;
            descriptor.ShortName = vm.ShortName;
            descriptor.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }

        private IActionResult Delete<T>(int userId, int id, Func<User, IEnumerable<T>> getter) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var descriptor = getter(user)
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            db.Remove(descriptor);
            db.SaveChanges();
            return NoContent();
        }

        private List<DescriptorViewModel> All<T>(int userId, Expression<Func<User, IEnumerable<T>>> relation) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            user.VerifyExists();

            return db.Entry(user)
                .Collection(relation)
                .Query()
                .Select(ViewModelFactory.MakeDescriptorViewModel)
                .ToList();
        }

        private DescriptorViewModel Single<T>(int userId, int id, Expression<Func<User, IEnumerable<T>>> relation) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            user.VerifyExists();

            var descriptor = db.Entry(user)
                .Collection(relation)
                .Query()
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            return ViewModelFactory.MakeDescriptorViewModel(descriptor);
        }
    }
}
