using Microsoft.EntityFrameworkCore;
using SitBackTradeAPI.Data;

namespace SitBackTradeAPI.MiddleWare
{
    public class StoreResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public StoreResolverMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var hostParts = context.Request.Host.Host.Split('.');
            if (hostParts.Length > 2)
            {
                var subdomain = hostParts[0];
                var store = await dbContext?.Stores?.FirstOrDefaultAsync(s => s.StoreName == subdomain)!;

                if (store != null)
                {
                    context.Items["CurrentStore"] = store;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Store not found.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
