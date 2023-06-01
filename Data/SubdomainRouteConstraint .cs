namespace SitBackTradeAPI.Data
{
    public class SubdomainRouteConstraint : IRouteConstraint
    {
        private readonly string _subdomain;

        public SubdomainRouteConstraint(string subdomain)
        {
            _subdomain = subdomain;
        }

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return httpContext!.Request.Host.Host.StartsWith(_subdomain, StringComparison.OrdinalIgnoreCase);
        }
    }
}
