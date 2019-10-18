using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers
{
    [ApiController]
    [Route("api", Name = Route.Root)]
    public class RootController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IViewModelFactory vmFactory;

        public RootController(GamesContext dbContext, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.vmFactory = vmFactory;
        }

        [HttpGet]
        public ActionResult<Root> GetRoot()
        {
            var config = dbContext.Configs.Include(x => x.DefaultUser).FirstOrDefault();
            return vmFactory.MakeRoot(config);
        }
    }
}