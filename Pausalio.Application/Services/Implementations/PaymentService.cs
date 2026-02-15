using AutoMapper;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IExchangeRateService _exchangeRateService;

        public PaymentService(
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

        public async Task<IEnumerable<PaymentToReturnDto>> GetAllAsync()
        {
            var companyId = GetCurrentCompanyId();

            var payments = await _unitOfWork.PaymentRepository
                .FindPaymentsWithEntitiesAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<IEnumerable<PaymentToReturnDto>>(payments);
        }

        public async Task<IEnumerable<PaymentToReturnDto>> GetByTypeAsync(PaymentType paymentType)
        {
            var companyId = GetCurrentCompanyId();

            var payments = await _unitOfWork.PaymentRepository
                .FindPaymentsWithEntitiesAsync(x => x.BusinessProfileId == companyId &&
                                                    x.PaymentType == paymentType);

            return _mapper.Map<IEnumerable<PaymentToReturnDto>>(payments);
        }

        public async Task<PaymentToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var payment = await _unitOfWork.PaymentRepository
                .FindPaymentWithEntitiesAsync(x => x.Id == id &&
                                                   x.BusinessProfileId == companyId);

            if (payment == null)
                return null;

            return _mapper.Map<PaymentToReturnDto>(payment);
        }

        public async Task<PaymentToReturnDto> CreateAsync(AddPaymentDto dto)
        {
            var companyId = GetCurrentCompanyId();

            if (dto.Amount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            if (dto.EntityId == Guid.Empty)
                throw new InvalidOperationException(_localizationHelper.EntityIdRequired);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = companyId,
                PaymentType = dto.PaymentType,
                Amount = dto.Amount,
                Currency = dto.Currency,
                ReferenceNumber = dto.ReferenceNumber ?? GenerateReferenceNumber(dto.PaymentType),
                Description = dto.Description,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Currency != Currency.RSD)
            {
                var currentRate = await _exchangeRateService.GetExchangeRateAsync(dto.Currency);
                payment.ExchangeRate = currentRate ?? dto.ExchangeRate ?? 1;
            }
            else
            {
                payment.ExchangeRate = 1;
            }

            payment.AmountRSD = payment.Currency == Currency.RSD
                ? payment.Amount
                : payment.Amount * (payment.ExchangeRate ?? 1);

            switch (dto.PaymentType)
            {
                case PaymentType.InvoicePayment:
                    await ProcessInvoicePaymentAsync(payment, dto.EntityId, companyId);
                    break;

                case PaymentType.TaxPayment:
                    await ProcessTaxPaymentAsync(payment, dto.EntityId, companyId);
                    break;

                case PaymentType.ExpensePayment:
                    await ProcessExpensePaymentAsync(payment, dto.EntityId, companyId);
                    break;

                default:
                    throw new InvalidOperationException(_localizationHelper.InvalidPaymentType);
            }

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PaymentToReturnDto>(payment);
        }

        public async Task UpdateAsync(Guid id, UpdatePaymentDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var payment = await _unitOfWork.PaymentRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId);

            if (payment == null)
                throw new KeyNotFoundException(_localizationHelper.PaymentNotFound);

            payment.ReferenceNumber = dto.ReferenceNumber ?? payment.ReferenceNumber;
            payment.Description = dto.Description;

            _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var payment = await _unitOfWork.PaymentRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId);

            if (payment == null)
                throw new KeyNotFoundException(_localizationHelper.PaymentNotFound);

            await RevertPaymentAsync(payment);

            _unitOfWork.PaymentRepository.Remove(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaymentSummaryDto> GetSummaryAsync()
        {
            var companyId = GetCurrentCompanyId();

            var payments = await _unitOfWork.PaymentRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return new PaymentSummaryDto
            {
                TotalInvoicePayments = payments.Where(x => x.PaymentType == PaymentType.InvoicePayment).Sum(x => x.AmountRSD),
                TotalTaxPayments = payments.Where(x => x.PaymentType == PaymentType.TaxPayment).Sum(x => x.AmountRSD),
                TotalExpensePayments = payments.Where(x => x.PaymentType == PaymentType.ExpensePayment).Sum(x => x.AmountRSD),
                CountInvoicePayments = payments.Count(x => x.PaymentType == PaymentType.InvoicePayment),
                CountTaxPayments = payments.Count(x => x.PaymentType == PaymentType.TaxPayment),
                CountExpensePayments = payments.Count(x => x.PaymentType == PaymentType.ExpensePayment)
            };
        }

        private async Task ProcessInvoicePaymentAsync(Payment payment, Guid invoiceId, Guid companyId)
        {
            var invoice = await _unitOfWork.InvoiceRepository
                .FindFirstOrDefaultAsync(x => x.Id == invoiceId &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoice == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            if (invoice.PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.InvoiceAlreadyPaid);

            if (payment.AmountRSD > invoice.AmountToPay)
                throw new InvalidOperationException(_localizationHelper.PaymentExceedsRemainingAmount);

            payment.InvoiceId = invoiceId;

            invoice.AmountToPay -= payment.AmountRSD;

            if (invoice.AmountToPay <= 0)
            {
                invoice.PaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            _unitOfWork.InvoiceRepository.Update(invoice);
        }

        private async Task ProcessTaxPaymentAsync(Payment payment, Guid taxObligationId, Guid companyId)
        {
            var taxObligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == taxObligationId &&
                                              x.BusinessProfileId == companyId);

            if (taxObligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            if (taxObligation.Status == TaxObligationStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.TaxObligationAlreadyPaid);

            if (payment.AmountRSD != taxObligation.TotalAmount)
                throw new InvalidOperationException(_localizationHelper.TaxObligationMustBePaidInFull);

            payment.TaxObligationId = taxObligationId;

            taxObligation.Status = TaxObligationStatus.Paid;
            taxObligation.PaidDate = DateTime.UtcNow;

            _unitOfWork.TaxObligationRepository.Update(taxObligation);
        }

        private async Task ProcessExpensePaymentAsync(Payment payment, Guid expenseId, Guid companyId)
        {
            var expense = await _unitOfWork.ExpenseRepository
                .FindFirstOrDefaultAsync(x => x.Id == expenseId &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (expense == null)
                throw new KeyNotFoundException(_localizationHelper.ExpenseNotFound);

            if (expense.Status == ExpenseStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.ExpenseAlreadyPaid);

            if (payment.AmountRSD != expense.Amount)
                throw new InvalidOperationException(_localizationHelper.ExpenseMustBePaidInFull);

            payment.ExpenseId = expenseId;

            expense.Status = ExpenseStatus.Paid;

            _unitOfWork.ExpenseRepository.Update(expense);
        }

        private async Task RevertPaymentAsync(Payment payment)
        {
            switch (payment.PaymentType)
            {
                case PaymentType.InvoicePayment:
                    if (payment.InvoiceId.HasValue)
                    {
                        var invoice = await _unitOfWork.InvoiceRepository.GetByIdAsync(payment.InvoiceId.Value);
                        if (invoice != null)
                        {
                            invoice.AmountToPay += payment.AmountRSD;

                            if (invoice.AmountToPay >= invoice.TotalAmountRSD)
                            {
                                invoice.PaymentStatus = PaymentStatus.Unpaid;
                            }
                            else
                            {
                                invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
                            }

                            _unitOfWork.InvoiceRepository.Update(invoice);
                        }
                    }
                    break;

                case PaymentType.TaxPayment:
                    if (payment.TaxObligationId.HasValue)
                    {
                        var taxObligation = await _unitOfWork.TaxObligationRepository.GetByIdAsync(payment.TaxObligationId.Value);
                        if (taxObligation != null)
                        {
                            taxObligation.Status = TaxObligationStatus.Pending;
                            taxObligation.PaidDate = null;
                            _unitOfWork.TaxObligationRepository.Update(taxObligation);
                        }
                    }
                    break;

                case PaymentType.ExpensePayment:
                    if (payment.ExpenseId.HasValue)
                    {
                        var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(payment.ExpenseId.Value);
                        if (expense != null)
                        {
                            expense.Status = ExpenseStatus.Pending;
                            _unitOfWork.ExpenseRepository.Update(expense);
                        }
                    }
                    break;
            }
        }

        private string GenerateReferenceNumber(PaymentType paymentType)
        {
            var prefix = paymentType switch
            {
                PaymentType.InvoicePayment => "PAY-INV",
                PaymentType.TaxPayment => "PAY-TAX",
                PaymentType.ExpensePayment => "PAY-EXP",
                _ => "PAY"
            };

            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private Guid GetCurrentCompanyId()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            return companyId;
        }
    }
}
