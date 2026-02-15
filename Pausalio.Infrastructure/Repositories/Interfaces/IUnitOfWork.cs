using Microsoft.EntityFrameworkCore.Storage;
using Pausalio.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        PausalioDbContext GetContext();
        IActivityCodeRepository ActivityCodeRepository { get; }
        IBankAccountRepository BankAccountRepository { get; }
        IBusinessProfileRepository BusinessProfileRepository { get; }
        IUserBusinessProfileRepository UserBusinessProfileRepository { get; }
        ICityRepository CityRepository { get; }
        IClientRepository ClientRepository { get; } 
        ICountryRepository CountryRepository { get; }
        IDocumentRepository DocumentRepository { get; }
        IExpenseRepository ExpenseRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }
        IInvoiceItemRepository InvoiceItemRepository { get; }
        IItemRepository ItemRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IReminderRepository ReminderRepository { get; }
        ITaxObligationRepository TaxObligationRepository { get; }
        IUserProfileRepository UserProfileRepository { get; }
        IBusinessInviteRepository BusinessInviteRepository { get; }
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }   
}
