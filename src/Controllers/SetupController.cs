using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/setup", Name = Route.Setup)]
    public class SetupController : ControllerBase
    {
        private readonly IConfigRepository configRepository;
        private readonly IUserRepository userRepository;
        private readonly IEventRepository eventRepository;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public SetupController(IConfigRepository configRepository, IUserRepository userRepository, IEventRepository eventRepository, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.configRepository = configRepository;
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpPost]
        public ActionResult<Root> Configure([FromBody] SetupViewModel vm)
        {
            if (configRepository.IsConfigured)
                return BadRequest(new ApiError("Already configured"));

            var defaultUser = new User
            {
                Username = vm.Username.Trim(),
                Password = auth.HashPassword(vm.Password)
            };

            userRepository.Add(defaultUser);
            configRepository.Configure(defaultUser, vm.ApiKey?.Trim());

            var eventPayload = new { defaultUserId = defaultUser.Id, defaultUsername = defaultUser.Username };
            eventRepository.Add(new Event("SetupCompleted", eventPayload, null));

            return vmFactory.MakeRoot(configRepository.DefaultConfig);
        }
    }
}
