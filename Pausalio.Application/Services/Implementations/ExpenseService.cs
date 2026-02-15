using AutoMapper;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public ExpenseService(
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

        public async Task<IEnumerable<ExpenseToReturnDto>> GetAllAsync()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expenses = await _unitOfWork.ExpenseRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return _mapper.Map<IEnumerable<ExpenseToReturnDto>>(expenses);
        }

        public async Task<IEnumerable<ExpenseToReturnDto>> GetByStatusAsync(ExpenseStatus status)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expenses = await _unitOfWork.ExpenseRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId &&
                                   x.Status == status &&
                                   !x.IsDeleted);

            return _mapper.Map<IEnumerable<ExpenseToReturnDto>>(expenses);
        }

        public async Task<ExpenseToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expense = await _unitOfWork.ExpenseRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (expense == null)
                return null;

            return _mapper.Map<ExpenseToReturnDto>(expense);
        }

        public async Task<ExpenseToReturnDto> CreateAsync(AddExpenseDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            if (dto.Amount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            var expense = _mapper.Map<Expense>(dto);
            expense.BusinessProfileId = companyId;
            expense.Status = ExpenseStatus.Pending;
            expense.CreatedAt = DateTime.UtcNow;
            expense.ReferenceNumber = GenerateReferenceNumber();

            await _unitOfWork.ExpenseRepository.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ExpenseToReturnDto>(expense);
        }

        public async Task UpdateAsync(Guid id, UpdateExpenseDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expense = await _unitOfWork.ExpenseRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (expense == null)
                throw new KeyNotFoundException(_localizationHelper.ExpenseNotFound);

            if (expense.Status == ExpenseStatus.Paid && dto.Status != ExpenseStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotModifyPaidExpense);

            if (dto.Amount <= 0)
                throw new InvalidOperationException(_localizationHelper.AmountMustBePositive);

            _mapper.Map(dto, expense);

            _unitOfWork.ExpenseRepository.Update(expense);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expense = await _unitOfWork.ExpenseRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (expense == null)
                throw new KeyNotFoundException(_localizationHelper.ExpenseNotFound);

            if (expense.Status == ExpenseStatus.Paid)
                throw new InvalidOperationException(_localizationHelper.CannotDeletePaidExpense);

            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;

            _unitOfWork.ExpenseRepository.Update(expense);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ArchiveAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expense = await _unitOfWork.ExpenseRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (expense == null)
                throw new KeyNotFoundException(_localizationHelper.ExpenseNotFound);

            expense.Status = ExpenseStatus.Archived;

            _unitOfWork.ExpenseRepository.Update(expense);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExpenseSummaryDto> GetSummaryAsync()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var expenses = await _unitOfWork.ExpenseRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return new ExpenseSummaryDto
            {
                TotalPending = expenses.Where(x => x.Status == ExpenseStatus.Pending).Sum(x => x.Amount),
                TotalPaid = expenses.Where(x => x.Status == ExpenseStatus.Paid).Sum(x => x.Amount),
                TotalArchived = expenses.Where(x => x.Status == ExpenseStatus.Archived).Sum(x => x.Amount),
                CountPending = expenses.Count(x => x.Status == ExpenseStatus.Pending),
                CountPaid = expenses.Count(x => x.Status == ExpenseStatus.Paid),
                CountArchived = expenses.Count(x => x.Status == ExpenseStatus.Archived)
            };
        }

        private string GenerateReferenceNumber()
        {
            return $"EXP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}