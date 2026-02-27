using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IExchangeRateService _exchangeRateService;

        public InvoiceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationHelper localizationHelper,
            ICurrentUserService currentUserService,
            IExchangeRateService exchangeRateService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
            _currentUserService = currentUserService;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<IEnumerable<InvoiceToReturnDto>> GetAllAsync()
        {
            var companyId = GetCurrentCompanyId();

            var invoices = await _unitOfWork.InvoiceRepository
                
                .FindInvoicesWithEntities(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return _mapper.Map<IEnumerable<InvoiceToReturnDto>>(invoices);
        }

        public async Task<IEnumerable<InvoiceToReturnDto>> GetByStatusAsync(InvoiceStatus status)
        {
            var companyId = GetCurrentCompanyId();

            var invoices = await _unitOfWork.InvoiceRepository
                .FindInvoicesWithEntities(x => x.BusinessProfileId == companyId &&
                                   x.InvoiceStatus == status &&
                                   !x.IsDeleted);

            return _mapper.Map<IEnumerable<InvoiceToReturnDto>>(invoices);
        }

        public async Task<IEnumerable<InvoiceToReturnDto>> GetByPaymentStatusAsync(PaymentStatus paymentStatus)
        {
            var companyId = GetCurrentCompanyId();

            var invoices = await _unitOfWork.InvoiceRepository
                .FindInvoicesWithEntities(x => x.BusinessProfileId == companyId &&
                                   x.PaymentStatus == paymentStatus &&
                                   !x.IsDeleted);

            return _mapper.Map<IEnumerable<InvoiceToReturnDto>>(invoices);
        }

        public async Task<InvoiceToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var invoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithEntities(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoice == null)
                return null;

            return _mapper.Map<InvoiceToReturnDto>(invoice);
        }

        public async Task<InvoiceToReturnDto> CreateAsync(AddInvoiceDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == dto.ClientId &&
                                              x.BusinessProfileId == companyId);

            if (client == null)
                throw new KeyNotFoundException(_localizationHelper.ClientNotFound);

            if (dto.Items == null || !dto.Items.Any())
                throw new InvalidOperationException(_localizationHelper.InvoiceMustHaveItems);

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0 || item.UnitPrice <= 0)
                    throw new InvalidOperationException(_localizationHelper.InvalidInvoiceItemValues);
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = companyId,
                ClientId = dto.ClientId,
                IssueDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Currency = dto.Currency,
                InvoiceStatus = InvoiceStatus.Draft,
                PaymentStatus = PaymentStatus.Unpaid,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                ReferenceNumber = GenerateReferenceNumber()
            };

            if (dto.Currency != Currency.RSD)
            {
                var currentRate = await _exchangeRateService.GetExchangeRateAsync(dto.Currency);

                if (currentRate == null)
                    throw new InvalidOperationException("Kurs nije dostupan za odabranu valutu");

                invoice.ExchangeRate = currentRate.Value;
            }
            else
            {
                invoice.ExchangeRate = 1;
            }

            foreach (var itemDto in dto.Items)
            {
                var invoiceItem = new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Name = itemDto.Name,
                    Description = itemDto.Description,
                    ItemType = itemDto.ItemType,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.Quantity * itemDto.UnitPrice
                };

                invoice.Items.Add(invoiceItem);
            }

            CalculateInvoiceTotals(invoice);

            await _unitOfWork.InvoiceRepository.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            var createdInvoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithEntities(x => x.Id == invoice.Id);

            return _mapper.Map<InvoiceToReturnDto>(createdInvoice);
        }

        public async Task UpdateAsync(Guid id, UpdateInvoiceDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var invoiceExists = await _unitOfWork.InvoiceRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoiceExists == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            if (invoiceExists.PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidInvoice);

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == dto.ClientId &&
                                              x.BusinessProfileId == companyId);

            if (client == null)
                throw new KeyNotFoundException(_localizationHelper.ClientNotFound);

            if (dto.Items == null || !dto.Items.Any())
                throw new InvalidOperationException(_localizationHelper.InvoiceMustHaveItems);

            decimal exchangeRate = 1;
            if (dto.Currency != Currency.RSD)
            {
                var currentRate = await _exchangeRateService.GetExchangeRateAsync(dto.Currency);

                if (currentRate == null)
                    throw new InvalidOperationException("Exchange rate unavailable");

                exchangeRate = currentRate.Value;
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var context = _unitOfWork.GetContext();

                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM InvoiceItems WHERE InvoiceId = {0}",
                    id
                );

                await context.Database.ExecuteSqlRawAsync(@"
            UPDATE Invoices 
            SET ClientId = {0}, 
                DueDate = {1}, 
                Currency = {2}, 
                InvoiceStatus = {3}, 
                PaymentStatus = {4}, 
                Notes = {5}, 
                ExchangeRate = {6},
                UpdatedAt = {7}
            WHERE Id = {8}",
                    dto.ClientId,
                    dto.DueDate.HasValue ? (object)dto.DueDate.Value : DBNull.Value,
                    (int)dto.Currency,
                    (int)dto.InvoiceStatus,
                    (int)dto.PaymentStatus,
                    dto.Notes ?? (object)DBNull.Value,
                    exchangeRate,
                    DateTime.UtcNow,
                    id
                );

                decimal totalAmount = 0;

                foreach (var itemDto in dto.Items)
                {
                    var itemId = Guid.NewGuid();
                    var totalPrice = itemDto.Quantity * itemDto.UnitPrice;
                    totalAmount += totalPrice;

                    await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO InvoiceItems (Id, InvoiceId, Name, Description, ItemType, Quantity, UnitPrice, TotalPrice)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                        itemId,
                        id,
                        itemDto.Name,
                        itemDto.Description ?? (object)DBNull.Value,
                        (int)itemDto.ItemType,
                        itemDto.Quantity,
                        itemDto.UnitPrice,
                        totalPrice
                    );
                }

                var totalAmountRsd = dto.Currency == Currency.RSD ? totalAmount : totalAmount * exchangeRate;

                await context.Database.ExecuteSqlRawAsync(@"
            UPDATE Invoices 
            SET TotalAmount = {0}, 
                TotalAmountRSD = {1},
                AmountToPay = {0}
            WHERE Id = {2}",
                    totalAmount,
                    totalAmountRsd,
                    id
                );

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var invoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithEntities(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoice == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            invoice.IsDeleted = true;
            invoice.DeletedAt = DateTime.UtcNow;

            _unitOfWork.InvoiceRepository.Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<InvoiceSummaryDto> GetSummaryAsync()
        {
            var companyId = GetCurrentCompanyId();

            var invoices = await _unitOfWork.InvoiceRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return new InvoiceSummaryDto
            {
                TotalDraft = invoices.Where(x => x.InvoiceStatus == InvoiceStatus.Draft).Sum(x => x.TotalAmountRSD),
                TotalSent = invoices.Where(x => x.InvoiceStatus == InvoiceStatus.Sent).Sum(x => x.TotalAmountRSD),
                TotalPaid = invoices.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.TotalAmountRSD),
                TotalUnpaid = invoices.Where(x => x.PaymentStatus == PaymentStatus.Unpaid).Sum(x => x.TotalAmountRSD),
                TotalOverdue = invoices.Where(x => x.PaymentStatus == PaymentStatus.Unpaid &&
                                                    x.DueDate.HasValue &&
                                                    x.DueDate < DateTime.UtcNow).Sum(x => x.TotalAmountRSD),
                CountDraft = invoices.Count(x => x.InvoiceStatus == InvoiceStatus.Draft),
                CountSent = invoices.Count(x => x.InvoiceStatus == InvoiceStatus.Sent),
                CountPaid = invoices.Count(x => x.PaymentStatus == PaymentStatus.Paid),
                CountUnpaid = invoices.Count(x => x.PaymentStatus == PaymentStatus.Unpaid),
                CountOverdue = invoices.Count(x => x.PaymentStatus == PaymentStatus.Unpaid &&
                                                    x.DueDate.HasValue &&
                                                    x.DueDate < DateTime.UtcNow)
            };
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var companyId = GetCurrentCompanyId();
            var year = DateTime.UtcNow.Year;

            var lastInvoice = await _unitOfWork.InvoiceRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId &&
                                   x.IssueDate.Year == year);

            var count = lastInvoice.Count() + 1;

            return $"{year}-{count:D4}";
        }

        private void CalculateInvoiceTotals(Invoice invoice)
        {
            invoice.TotalAmount = invoice.Items.Sum(x => x.TotalPrice);

            if (invoice.Currency == Currency.RSD)
            {
                invoice.TotalAmountRSD = invoice.TotalAmount;
            }
            else
            {
                invoice.TotalAmountRSD = invoice.TotalAmount * invoice.ExchangeRate;
            }

            var totalPaid = invoice.Payments?.Sum(p => p.Amount) ?? 0;
            invoice.AmountToPay = invoice.TotalAmount - totalPaid;
        }

        private string GenerateReferenceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private Guid GetCurrentCompanyId()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            return companyId;
        }

        public async Task ArchiveInvoice(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var invoiceExists = await _unitOfWork.InvoiceRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoiceExists == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            if (invoiceExists.InvoiceStatus != InvoiceStatus.Finished)
                throw new InvalidOperationException(_localizationHelper.CannotArchiveUnfinishedInvoice);

            if (invoiceExists.PaymentStatus != PaymentStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidInvoice);
            invoiceExists.InvoiceStatus = InvoiceStatus.Archived;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelInvoice(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var invoiceExists = await _unitOfWork.InvoiceRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoiceExists == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            if (invoiceExists.InvoiceStatus == InvoiceStatus.Finished)
                throw new InvalidOperationException(_localizationHelper.CannotCancelFinishedInvoice);

            if (invoiceExists.PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidInvoice);
            invoiceExists.InvoiceStatus = InvoiceStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}