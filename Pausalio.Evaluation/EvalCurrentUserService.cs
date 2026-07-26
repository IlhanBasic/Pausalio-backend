using Pausalio.Application.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace Pausalio.Evaluation
{
    public class EvalCurrentUserService : ICurrentUserService
    {
        public static readonly Guid SeededUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        public string? GetEmail() => "eval@pausalio.rs";
        public string? GetUserId() => SeededUserId.ToString();
        public string? GetCompany() => "22222222-2222-2222-2222-222222222222";
        public IEnumerable<string> GetRoles() => new[] { "User" };
        public IEnumerable<string> GetAvailableBusinesses() => new[] { Guid.Parse("22222222-2222-2222-2222-222222222222").ToString() };
    }
}
