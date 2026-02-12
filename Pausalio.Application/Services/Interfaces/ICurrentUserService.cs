using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string? GetEmail();
        string? GetUserId();
        string? GetCompany();
        IEnumerable<string> GetRoles();
    }
}
