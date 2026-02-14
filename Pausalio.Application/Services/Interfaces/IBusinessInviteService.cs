using Pausalio.Application.DTOs.BusinessInvite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IBusinessInviteService
    {
        Task<BusinessInviteToReturnDto> SendInvite(AddBusinessInviteDto invite, Guid ownerId, Guid businessId);
        Task<BusinessInviteToReturnDto?> GetBusinessInviteByEmailAndCompany(string email, Guid companyId);
        Task<BusinessInviteToReturnDto?> GetBusinessInviteByEmail(string email);
        Task RemoveInvite(Guid id);
    }
}
