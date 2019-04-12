using System.Collections.Generic;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}/tags")]
    public class TagController : DescriptorControllerBase
    {
        private readonly ITagRepository tagRepository;

        public TagController(ITagRepository tagRepository, IUserRepository userRepository, IEventRepository eventRepository, IViewModelFactory vmFactory) : base(userRepository, eventRepository, vmFactory)
        {
            this.tagRepository = tagRepository;
        }

        [HttpGet(Name = Route.Tags)]
        public ActionResult<List<DescriptorViewModel>> GetTags(int userId)
        {
            return All(userId, tagRepository);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddTag(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm, tagRepository);
        }

        [HttpGet("{id}", Name = Route.Tag)]
        public ActionResult<DescriptorViewModel> GetTag(int userId, int id)
        {
            return Single(userId, id, tagRepository);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdateTag(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm, tagRepository);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeleteTag(int userId, int id)
        {
            return Delete(userId, id, tagRepository);
        }
    }
}