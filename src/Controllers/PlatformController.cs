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
    [Route("api/users/{" + Constants.UserIdParameter + "}/platforms")]
    public class PlatformController : DescriptorControllerBase<Platform>
    {
        public PlatformController(GamesContext dbContext, IViewModelFactory vmFactory) : base(dbContext, vmFactory)
        { }

        [HttpGet(Name = Route.Platforms)]
        public ActionResult<List<DescriptorViewModel>> GetPlatforms(int userId)
        {
            return All(userId);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddPlatform(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm);
        }

        [HttpGet("{id}", Name = Route.Platform)]
        public ActionResult<DescriptorViewModel> GetPlatform(int userId, int id)
        {
            return Single(userId, id);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdatePlatform(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeletePlatform(int userId, int id)
        {
            return Delete(userId, id);
        }
    }
}