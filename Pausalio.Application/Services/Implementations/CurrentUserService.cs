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
            var companiesClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("BusinessProfileId")?.Value;


            return companiesClaim;
        }
    }
}
