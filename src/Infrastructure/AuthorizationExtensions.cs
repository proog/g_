using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddRouteUserIdAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, RequireSameUserAuthorizationHandler>();
            services.AddHttpContextAccessor();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.SameUserPolicy, policy =>
                {
                    policy.AddRequirements(new SameUserRequirement());
                });
            });
            return services;
        }

        private class SameUserRequirement : IAuthorizationRequirement { }

        private class RequireSameUserAuthorizationHandler : AuthorizationHandler<SameUserRequirement>
        {
            private readonly IHttpContextAccessor httpContextAccessor;

            public RequireSameUserAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }

            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserRequirement requirement)
            {
                var routeUserId = httpContextAccessor.HttpContext?.Request.RouteValues[Constants.UserIdParameter] as string;

                if (routeUserId != null && context.User.HasClaim(Constants.UserIdClaim, routeUserId))
                    context.Succeed(requirement);

                return Task.CompletedTask;
            }
        }
    }
}
