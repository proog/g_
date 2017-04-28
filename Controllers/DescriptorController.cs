using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers
{
    [Route("api/users/{userId}")]
    public class DescriptorController : Controller
    {
        private GamesContext db;
        private ICommonService common;
        private IAuthenticationService auth;

        public DescriptorController(GamesContext db, ICommonService common, IAuthenticationService auth)
        {
            this.db = db;
            this.common = common;
            this.auth = auth;
        }

        [HttpGet("genres")]
        public IActionResult GetGenres(int userId)
        {
            return All(userId, u => u.Genres);
        }
        [HttpGet("platforms")]
        public IActionResult GetPlatforms(int userId)
        {
            return All(userId, u => u.Platforms);
        }
        [HttpGet("tags")]
        public IActionResult GetTags(int userId)
        {
            return All(userId, u => u.Tags);
        }

        [HttpGet("genres/{id}")]
        public IActionResult GetGenre(int userId, int id)
        {
            return Single(userId, id, u => u.Genres);
        }
        [HttpGet("platforms/{id}")]
        public IActionResult GetPlatform(int userId, int id)
        {
            return Single(userId, id, u => u.Platforms);
        }
        [HttpGet("tags/{id}")]
        public IActionResult GetTag(int userId, int id)
        {
            return Single(userId, id, u => u.Tags);
        }

        [HttpPost("genres"), Authorize]
        public Task<IActionResult> AddGenre(int userId, [FromBody] Genre rendition)
        {
            return Add(userId, rendition, () => db.Genres);
        }
        [HttpPost("platforms"), Authorize]
        public Task<IActionResult> AddPlatform(int userId, [FromBody] Platform rendition)
        {
            return Add(userId, rendition, () => db.Platforms);
        }
        [HttpPost("tags"), Authorize]
        public Task<IActionResult> AddTag(int userId, [FromBody] Tag rendition)
        {
            return Add(userId, rendition, () => db.Tags);
        }

        [HttpPut("genres/{id}"), Authorize]
        public Task<IActionResult> UpdateGenre(int userId, int id, [FromBody] Genre rendition)
        {
            return Update(userId, id, rendition, u => u.Genres);
        }
        [HttpPut("platforms/{id}"), Authorize]
        public Task<IActionResult> UpdatePlatform(int userId, int id, [FromBody] Platform rendition)
        {
            return Update(userId, id, rendition, u => u.Platforms);
        }
        [HttpPut("tags/{id}"), Authorize]
        public Task<IActionResult> UpdateTags(int userId, int id, [FromBody] Tag rendition)
        {
            return Update(userId, id, rendition, u => u.Tags);
        }

        [HttpDelete("genres/{id}"), Authorize]
        public Task<IActionResult> DeleteGenre(int userId, int id)
        {
            return Delete(userId, id, u => u.Genres);
        }
        [HttpDelete("platforms/{id}"), Authorize]
        public Task<IActionResult> DeletePlatform(int userId, int id)
        {
            return Delete(userId, id, u => u.Platforms);
        }
        [HttpDelete("tags/{id}"), Authorize]
        public Task<IActionResult> DeleteTag(int userId, int id)
        {
            return Delete(userId, id, u => u.Tags);
        }

        private async Task<IActionResult> Add<T>(int userId, T descriptor, Func<DbSet<T>> getter) where T : Descriptor
        {
            var user = common.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            descriptor.Id = 0;
            descriptor.User = user;
            descriptor.CreatedAt = DateTime.UtcNow;
            descriptor.UpdatedAt = DateTime.UtcNow;
            getter().Add(descriptor);
            db.SaveChanges();

            return Ok(descriptor);
        }

        private async Task<IActionResult> Update<T>(int userId, int id, T update, Func<User, IEnumerable<T>> getter) where T : Descriptor
        {
            var user = common.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var descriptor = getter(user)
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            descriptor.Name = update.Name;
            descriptor.ShortName = update.ShortName;
            descriptor.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Ok(descriptor);
        }

        private async Task<IActionResult> Delete<T>(int userId, int id, Func<User, IEnumerable<T>> getter) where T : Descriptor
        {
            var user = common.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var descriptor = getter(user)
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            db.Remove(descriptor);
            db.SaveChanges();
            return NoContent();
        }

        private IActionResult All<T>(int userId, Expression<Func<User, IEnumerable<T>>> relation) where T : DbModel
        {
            var user = common.GetUser(userId);
            user.VerifyExists();

            var list = db.Entry(user)
                .Collection(relation)
                .Query()
                .ToList();

            return Ok(list);
        }

        private IActionResult Single<T>(int userId, int id, Expression<Func<User, IEnumerable<T>>> relation) where T : DbModel
        {
            var user = common.GetUser(userId);
            user.VerifyExists();

            var descriptor = db.Entry(user)
                .Collection(relation)
                .Query()
                .SingleOrDefault(it => it.Id == id);
            descriptor.VerifyExists();

            return Ok(descriptor);
        }
    }
}
