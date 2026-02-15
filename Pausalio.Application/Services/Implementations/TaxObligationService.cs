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

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetAllAsync()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetByYearAsync(int year)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.Year == year);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<TaxObligationToReturnDto?> GetByYearAndMonthAsync(int year, int month)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.BusinessProfileId == companyId &&
                                              x.Year == year &&
                                              x.Month == month);

            if (obligation == null)
                return null;

            return _mapper.Map<TaxObligationToReturnDto>(obligation);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GetByStatusAsync(TaxObligationStatus status)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.Status == status);

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(obligations);
        }

        public async Task<TaxObligationToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                return null;

            return _mapper.Map<TaxObligationToReturnDto>(obligation);
        }

        public async Task<IEnumerable<TaxObligationToReturnDto>> GenerateAnnualObligationsAsync(GenerateTaxObligationsDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            if (dto.Year < 2000 || dto.Year > 2100)
                throw new InvalidOperationException(_localizationHelper.InvalidYear);

            if (dto.MonthlyAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            if (dto.DueDayOfMonth < 1 || dto.DueDayOfMonth > 31)
                throw new InvalidOperationException(_localizationHelper.InvalidDueDay);

            var existingObligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.Year == dto.Year);

            if (existingObligations.Any())
                throw new InvalidOperationException(_localizationHelper.ObligationsAlreadyExistForYear);

            var generatedObligations = new List<TaxObligation>();

            for (int month = 1; month <= 12; month++)
            {
                var daysInMonth = DateTime.DaysInMonth(dto.Year, month);
                var dueDay = Math.Min(dto.DueDayOfMonth, daysInMonth); // Ako je 31. a mesec ima 30 dana

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

            foreach (var obligation in generatedObligations)
            {
                await _unitOfWork.TaxObligationRepository.AddAsync(obligation);
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<TaxObligationToReturnDto>>(generatedObligations);
        }

        public async Task<TaxObligationToReturnDto> CreateAsync(AddTaxObligationDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            if (dto.TotalAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            var year = dto.DueDate.Year;
            var month = dto.DueDate.Month;
            var existing = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.BusinessProfileId == companyId &&
                                              x.Year == year &&
                                              x.Month == month);

            if (existing != null)
                throw new InvalidOperationException(_localizationHelper.ObligationAlreadyExistsForMonth);

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
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            if (obligation.Status == TaxObligationStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidObligation);

            if (dto.TotalAmount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            _mapper.Map(dto, obligation);
            obligation.UpdatedAt = DateTime.UtcNow;

            obligation.Year = dto.DueDate.Year;
            obligation.Month = dto.DueDate.Month;

            _unitOfWork.TaxObligationRepository.Update(obligation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

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
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligation = await _unitOfWork.TaxObligationRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (obligation == null)
                throw new KeyNotFoundException(_localizationHelper.TaxObligationNotFound);

            obligation.Status = TaxObligationStatus.Paid;
            obligation.PaidDate = DateTime.UtcNow;
            obligation.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TaxObligationRepository.Update(obligation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TaxObligationSummaryDto> GetSummaryAsync(int? year)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var obligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && (!year.HasValue || x.Year == year.Value));

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