using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> AddIncludes<T>(this IQueryable<T> source)
        {
            switch (source)
            {
                case IQueryable<BusinessProfile> businessProfiles:
                    return (IQueryable<T>)businessProfiles.Include(x=>x.ActivityCode);  
                case IQueryable<Client> clients:
                    return (IQueryable<T>)clients.Include(x => x.Country);
                case IQueryable<Invoice> invoices:
                    return (IQueryable<T>)invoices
                        .Include(x => x.Client)
                            .ThenInclude(c => c.Country)
                        .Include(x => x.Items)
                        .Include(x=> x.Payments)
                        .Include(x => x.BusinessProfile);
                case IQueryable<Payment> payments:
                    return (IQueryable<T>)payments
                        .Include(x => x.Invoice)
                            .ThenInclude(i => i!.Client)
                        .Include(x => x.Invoice)
                            .ThenInclude(i => i!.Items)
                        .Include(p => p.TaxObligation)
                        .Include(p => p.Expense)
                        .Include(p => p.BankAccount);
                case IQueryable<UserProfile> userProfiles:
                    return (IQueryable<T>)userProfiles
                        .Include(u => u.UserBusinessProfiles)
                            .ThenInclude(u => u.BusinessProfile);
            }
            return source;
        }
    }
}
