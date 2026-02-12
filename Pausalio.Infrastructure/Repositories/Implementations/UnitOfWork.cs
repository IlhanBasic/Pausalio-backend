using Microsoft.EntityFrameworkCore.Storage;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Interfaces;


namespace Pausalio.Infrastructure.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PausalioDbContext _context;

        public IActivityCodeRepository ActivityCodeRepository { get; }
        public IBankAccountRepository BankAccountRepository { get; }
        public IBusinessProfileRepository BusinessProfileRepository { get; }
        public IUserBusinessProfileRepository UserBusinessProfileRepository { get; }
        public ICityRepository CityRepository { get; }
        public IClientRepository ClientRepository { get; }
        public ICountryRepository CountryRepository { get; }
        public IDocumentRepository DocumentRepository { get; }
        public IExpenseRepository ExpenseRepository { get; }
        public IInvoiceRepository InvoiceRepository { get; }
        public IInvoiceItemRepository InvoiceItemRepository { get; }
        public IItemRepository ItemRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IReminderRepository ReminderRepository { get; }
        public ITaxObligationRepository TaxObligationRepository { get; }
        public IUserProfileRepository UserProfileRepository { get; }
        public IBusinessInviteRepository BusinessInviteRepository { get; }

        public UnitOfWork(
            PausalioDbContext context,
            IActivityCodeRepository activityCodeRepository,
            IBankAccountRepository bankAccountRepository,
            IBusinessProfileRepository businessProfileRepository,
            IUserBusinessProfileRepository userBusinessProfileRepository,
            ICityRepository cityRepository,
            IClientRepository clientRepository,
            ICountryRepository countryRepository,
            IDocumentRepository documentRepository,
            IExpenseRepository expenseRepository,
            IInvoiceRepository invoiceRepository,
            IInvoiceItemRepository invoiceItemRepository,
            IItemRepository itemRepository,
            IPaymentRepository paymentRepository,
            IReminderRepository reminderRepository,
            ITaxObligationRepository taxObligationRepository,
            IUserProfileRepository userProfileRepository,
            IBusinessInviteRepository businessInviteRepository
        )
        {
            _context = context;

            ActivityCodeRepository = activityCodeRepository;
            BankAccountRepository = bankAccountRepository;
            BusinessProfileRepository = businessProfileRepository;
            UserBusinessProfileRepository = userBusinessProfileRepository;
            CityRepository = cityRepository;
            ClientRepository = clientRepository;
            CountryRepository = countryRepository;
            DocumentRepository = documentRepository;
            ExpenseRepository = expenseRepository;
            InvoiceRepository = invoiceRepository;
            InvoiceItemRepository = invoiceItemRepository;
            ItemRepository = itemRepository;
            PaymentRepository = paymentRepository;
            ReminderRepository = reminderRepository;
            TaxObligationRepository = taxObligationRepository;
            UserProfileRepository = userProfileRepository;
            BusinessInviteRepository = businessInviteRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
