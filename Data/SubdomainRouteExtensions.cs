using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SitBackTradeAPI.Data
{
    public static class SubdomainRouteExtensions
    {
        public static IEndpointConventionBuilder MapSubdomainControllerRoute(
        this IEndpointRouteBuilder endpoints,
        string subdomain,
        string pattern)
        {
            var subdomainConstraint = new SubdomainRouteConstraint(subdomain);

            return endpoints.MapControllerRoute(
                name: $"subdomain_{subdomain}",
                pattern: pattern,
                defaults: null,
                constraints: new { subdomain = subdomainConstraint });
        }
    }
}
