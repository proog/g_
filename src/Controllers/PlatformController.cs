using System.Collections.Generic;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}/platforms")]
    public class PlatformController : DescriptorControllerBase
    {
        private readonly IPlatformRepository platformRepository;

        public PlatformController(IPlatformRepository platformRepository, IUserRepository userRepository, IEventRepository eventRepository, IViewModelFactory vmFactory) : base(userRepository, eventRepository, vmFactory)
        {
            this.platformRepository = platformRepository;
        }

        [HttpGet(Name = Route.Platforms)]
        public ActionResult<List<DescriptorViewModel>> GetPlatforms(int userId)
        {
            return All(userId, platformRepository);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddPlatform(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm, platformRepository);
        }

        [HttpGet("{id}", Name = Route.Platform)]
        public ActionResult<DescriptorViewModel> GetPlatform(int userId, int id)
        {
            return Single(userId, id, platformRepository);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdatePlatform(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm, platformRepository);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeletePlatform(int userId, int id)
        {
            return Delete(userId, id, platformRepository);
        }
    }
}