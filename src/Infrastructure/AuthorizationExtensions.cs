using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddRouteUserIdAuthorization(this IServiceCollection services)
        {
            return services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.SameUserPolicy, policy =>
                {
                    policy.RequireAssertion(context => RequireSameUser(context));
                });
            });
        }

        public static bool RequireSameUser(AuthorizationHandlerContext context)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            var routeUserId = filterContext?.RouteData?.Values[Constants.UserIdParameter] as string;
            return context.User.HasClaim(Constants.UserIdClaim, routeUserId);
        }
    }
}
