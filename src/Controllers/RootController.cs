using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api", Name = Route.Root)]
    public class RootController : ControllerBase
    {
        private readonly IConfigRepository configRepository;
        private readonly IViewModelFactory vmFactory;

        public RootController(IConfigRepository configRepository, IViewModelFactory vmFactory)
        {
            this.configRepository = configRepository;
            this.vmFactory = vmFactory;
        }

        [HttpGet]
        public ActionResult<Root> GetRoot()
        {
            var config = configRepository.DefaultConfig;

            if (config == null)
                return NotFound();

            return vmFactory.MakeRoot(config);
        }
    }
}