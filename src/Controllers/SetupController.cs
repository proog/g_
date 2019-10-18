using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/setup", Name = Route.Setup)]
    public class SetupController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public SetupController(GamesContext dbContext, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpPost]
        public ActionResult<Root> Configure([FromBody] SetupViewModel vm)
        {
            var config = dbContext.Configs.FirstOrDefault();

            if (config != null)
                return BadRequest(new ApiError("Already configured"));

            var defaultUser = new User
            {
                Username = vm.Username.Trim(),
                Password = auth.HashPassword(vm.Password)
            };
            config = new Config();
            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = vm.ApiKey?.Trim();

            dbContext.Users.Add(defaultUser);
            dbContext.Configs.Add(config);
            dbContext.SaveChanges();

            var eventPayload = new { defaultUserId = defaultUser.Id, defaultUsername = defaultUser.Username };
            dbContext.Events.Add(new Event("SetupCompleted", eventPayload, null));
            dbContext.SaveChanges();

            return vmFactory.MakeRoot(config);
        }
    }
}
