using AutoMapper;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class TaxObligationService : ITaxObligationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public TaxObligationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationHelper localizationHelper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
            _currentUserService = currentUserService;
        }

        private Guid GetCompanyIdOrThrow()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            return companyId;
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetAllAsync()
        {
            var companyId = GetCompanyIdOrThrow();

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetByYearAsync(int year)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.Year == year);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetByYearAndMonthAsync(int year, int month)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId &&
                                   x.Year == year &&
                                   x.Month == month);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetByStatusAsync(TaxObligationStatus status)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.Status == status);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<TaxObligationToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                return null;

            return _mapper.Map<TaxObligationToReturnDto>(obligation);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GenerateAnnualObligationsAsync(GenerateTaxObligationsDto dto)
        {
            var companyId = GetCompanyIdOrThrow();
            int currentYear = DateTime.UtcNow.Year;
            if (dto.Year < currentYear || dto.Year > currentYear)
                dto.Year = currentYear;

            if (dto.MonthlyAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            if (dto.DueDayOfMonth < 1 || dto.DueDayOfMonth > 31)
                throw new InvalidOperationException(_localizationHelper.InvalidDueDay);

            var existingObligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId &&
                                   x.Year == dto.Year &&
                                   x.Type == dto.Type);

            var existingMonths = existingObligations.Select(x => x.Month).ToHashSet();

            var generatedObligations = new List<TaxObligation>();

            for (int month = 1; month <= 12; month++)
            {
                if (existingMonths.Contains(month))
                    continue;

                var daysInMonth = DateTime.DaysInMonth(dto.Year, month);
                var dueDay = Math.Min(dto.DueDayOfMonth, daysInMonth);
                var dueDate = new DateTime(dto.Year, month, dueDay);

                var obligation = new TaxObligation
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = companyId,
                    Year = dto.Year,
                    Month = month,
                    Status = TaxObligationStatus.Pending,
                    DueDate = dueDate,
                    ReferenceNumber = GenerateReferenceNumber(dto.Year, month),
                    Type = dto.Type,
                    TotalAmount = dto.MonthlyAmount,
                    CreatedAt = DateTime.UtcNow
                };

                generatedObligations.Add(obligation);
            }

            if (!generatedObligations.Any())
                throw new InvalidOperationException(_localizationHelper.ObligationsAlreadyExistForYearAndType);

            foreach (var obligation in generatedObligations)
                await _unitOfWork.TaxObligationRepository.AddAsync(obligation);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(generatedObligations);
        }

        public async Task<TaxObligationToReturnDto> CreateAsync(AddTaxObligationDto dto)
        {
            var companyId = GetCompanyIdOrThrow();

            if (dto.TotalAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            var year = DateTime.UtcNow.Year;
            var month = dto.DueDate.Month;

            var existing = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.BusinessProfileId == companyId &&
                                              x.Year == year &&
                                              x.Month == month &&
                                              x.Type == dto.Type);

            if (existing != null)
                throw new InvalidOperationException(_localizationHelper.ObligationAlreadyExistsForMonthAndType);

            var obligation = new TaxObligation
            {
                BusinessProfileId = companyId,
                Year = year,
                Month = month,
                Status = TaxObligationStatus.Pending,
                DueDate = dto.DueDate,
                ReferenceNumber = GenerateReferenceNumber(year, month),
                Type = dto.Type,
                TotalAmount = dto.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TaxObligationRepository.AddAsync(obligation);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaxObligationToReturnDto>(obligation);
        }

        public async Task UpdateAsync(Guid id, UpdateTaxObligationDto dto)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            if (obligation.Status == TaxObligationStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidObligation);

            if (dto.TotalAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            var newYear = dto.DueDate.Year;
            var newMonth = dto.DueDate.Month;

            var duplicate = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.BusinessProfileId == companyId &&
                                              x.Year == newYear &&
                                              x.Month == newMonth &&
                                              x.Type == dto.Type &&
                                              x.Id != id);

            if (duplicate != null)
                throw new InvalidOperationException(_localizationHelper.ObligationAlreadyExistsForMonthAndType);

            _mapper.Map(dto, obligation);
            obligation.Year = newYear;
            obligation.Month = newMonth;
            obligation.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TaxObligationRepository.Update(obligation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            if (obligation.Status == TaxObligationStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotDeletePaidObligation);

            _unitOfWork.TaxObligationRepository.Remove(obligation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAsPaidAsync(Guid id)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            if (obligation.Status == TaxObligationStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.ObligationAlreadyPaid);

            var obligationPeriod = new DateTime(obligation.Year, obligation.Month, 1);
            var currentPeriod = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            if (obligationPeriod > currentPeriod)
                throw new InvalidOperationException(_localizationHelper.CannotMarkFutureObligationAsPaid);

            obligation.Status = TaxObligationStatus.Paid;
            obligation.PaidDate = DateTime.UtcNow;
            obligation.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TaxObligationRepository.Update(obligation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TaxObligationSummaryDto> GetSummaryAsync(int? year)
        {
            var companyId = GetCompanyIdOrThrow();

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId &&
                                   (!year.HasValue || x.Year == year.Value));

            var now = DateTime.UtcNow;

            return new TaxObligationSummaryDto
            {
                TotalPending = obligations.Where(x => x.Status == TaxObligationStatus.Pending).Sum(x => x.TotalAmount),
                TotalPaid = obligations.Where(x => x.Status == TaxObligationStatus.Paid).Sum(x => x.TotalAmount),
                TotalOverdue = obligations.Where(x => x.Status == TaxObligationStatus.Pending && x.DueDate < now).Sum(x => x.TotalAmount),
                CountPending = obligations.Count(x => x.Status == TaxObligationStatus.Pending),
                CountPaid = obligations.Count(x => x.Status == TaxObligationStatus.Paid),
                CountOverdue = obligations.Count(x => x.Status == TaxObligationStatus.Pending && x.DueDate < now)
            };
        }

        private string GenerateReferenceNumber(int year, int month)
        {
            return $"TAX-{year}{month:D2}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
    }
}