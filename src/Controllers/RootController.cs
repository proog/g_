using Games.Infrastructure;
using Games.Interfaces;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api", Name = Route.Root)]
    public class RootController : Controller
    {
        private readonly IConfigRepository configRepository;
        private readonly IViewModelFactory vmFactory;

        public RootController(IConfigRepository configRepository, IViewModelFactory vmFactory)
        {
            this.configRepository = configRepository;
            this.vmFactory = vmFactory;
        }

        [HttpGet]
        public Root GetRoot()
        {
            var config = configRepository.DefaultConfig;

            if (config == null)
                throw new NotFoundException();

            return vmFactory.MakeRoot(config);
        }
    }
}