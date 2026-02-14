using Pausalio.Application.Services.Interfaces;

namespace Pausalio.API.Middlewares
{
    public class BusinessContextMiddleware
    {
        private readonly RequestDelegate _next;

        public BusinessContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
        {
            var businessId = context.Request.Headers["X-Business-Context"].FirstOrDefault();

            if (!string.IsNullOrEmpty(businessId))
            {
                var availableBusinesses = currentUserService.GetAvailableBusinesses();

                if (!availableBusinesses.Contains(businessId))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Nemate pristup ovoj firmi"
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
    
}
