using AutoMapper;
using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public BankAccountService(
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

        public async Task<List<BankAccountToReturnDto>> GetAllAsync()
        {
            var companyId = GetCurrentCompanyId();

            var accounts = await _unitOfWork.BankAccountRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<List<BankAccountToReturnDto>>(accounts);
        }

        public async Task<BankAccountToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var account = await _unitOfWork.BankAccountRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (account == null)
                return null;

            return _mapper.Map<BankAccountToReturnDto>(account);
        }

        public async Task CreateAsync(AddBankAccountDto dto)
        {
            var companyId = GetCurrentCompanyId();

            ValidateForeignCurrencyFields(dto.Currency, dto.IBAN, dto.SWIFT);

            var entity = _mapper.Map<BankAccount>(dto);
            entity.BusinessProfileId = companyId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.BankAccountRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateBankAccountDto dto)
        {
            var companyId = GetCurrentCompanyId();
            var account = await _unitOfWork.BankAccountRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);
            if (account == null)
                throw new KeyNotFoundException(_localizationHelper.BankAccountNotFound);

            if (account.Currency != dto.Currency)
                throw new InvalidOperationException(_localizationHelper.CurrencyCannotBeChanged);

            bool existingIsForeign = account.Currency != Currency.RSD;
            bool newIsForeign = dto.Currency != Currency.RSD;
            if (existingIsForeign != newIsForeign)
                throw new InvalidOperationException(_localizationHelper.CurrencyTypeCannotBeChanged);

            ValidateForeignCurrencyFields(dto.Currency, dto.IBAN, dto.SWIFT);
            _mapper.Map(dto, account);
            account.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.BankAccountRepository.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var account = await _unitOfWork.BankAccountRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (account == null)
                throw new KeyNotFoundException(_localizationHelper.BankAccountNotFound);

            _unitOfWork.BankAccountRepository.Remove(account);
            await _unitOfWork.SaveChangesAsync();
        }

        private Guid GetCurrentCompanyId()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            return companyId;
        }

        private void ValidateForeignCurrencyFields(Currency currency, string? iban, string? swift)
        {
            if (currency != Currency.RSD)
            {
                if (string.IsNullOrWhiteSpace(iban))
                    throw new InvalidOperationException(_localizationHelper.IBANRequiredForForeignCurrency);

                if (string.IsNullOrWhiteSpace(swift))
                    throw new InvalidOperationException(_localizationHelper.SWIFTRequiredForForeignCurrency);
            }
        }
    }
}