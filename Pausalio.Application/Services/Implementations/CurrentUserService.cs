using Microsoft.AspNetCore.Http;
using Pausalio.Application.Services.Interfaces;
using System.Security.Claims;

namespace Pausalio.Application.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public IEnumerable<string> GetRoles()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public string? GetCompany()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var headerValue = context.Request.Headers["X-Business-Context"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue))
            {
                var availableBusinesses = GetAvailableBusinesses();
                if (availableBusinesses.Contains(headerValue))
                {
                    return headerValue;
                }

            }

            var availableClaim = context.User?.FindFirst("AvailableBusinesses")?.Value;
            return availableClaim?.Split(',').FirstOrDefault();
        }

        public IEnumerable<string> GetAvailableBusinesses()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("AvailableBusinesses")?.Value;
            if (string.IsNullOrEmpty(claim))
                return Enumerable.Empty<string>();

            return claim.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}