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
    [Route("api/users/{" + Constants.UserIdParameter + "}/tags")]
    public class TagController : DescriptorControllerBase<Tag>
    {
        public TagController(GamesContext dbContext, IViewModelFactory vmFactory) : base(dbContext, vmFactory)
        { }

        [HttpGet(Name = Route.Tags)]
        public ActionResult<List<DescriptorViewModel>> GetTags(int userId)
        {
            return All(userId);
        }

        [HttpPost, Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> AddTag(int userId, [FromBody] DescriptorViewModel vm)
        {
            return Add(userId, vm);
        }

        [HttpGet("{id}", Name = Route.Tag)]
        public ActionResult<DescriptorViewModel> GetTag(int userId, int id)
        {
            return Single(userId, id);
        }

        [HttpPut("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult<DescriptorViewModel> UpdateTag(int userId, int id, [FromBody] DescriptorViewModel vm)
        {
            return Update(userId, id, vm);
        }

        [HttpDelete("{id}"), Authorize(Constants.SameUserPolicy)]
        public ActionResult DeleteTag(int userId, int id)
        {
            return Delete(userId, id);
        }
    }
}