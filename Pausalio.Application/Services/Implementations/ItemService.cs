using AutoMapper;
using Pausalio.Application.DTOs.Item;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public ItemService(
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

        public async Task<List<ItemToReturnDto>> GetAllAsync()
        {
            var companyId = GetCurrentCompanyId();

            var items = await _unitOfWork.ItemRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<List<ItemToReturnDto>>(items);
        }

        public async Task<ItemToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var item = await _unitOfWork.ItemRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId);

            if (item == null)
                return null;

            return _mapper.Map<ItemToReturnDto>(item);
        }

        public async Task CreateAsync(AddItemDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var entity = _mapper.Map<Item>(dto);
            entity.BusinessProfileId = companyId;

            await _unitOfWork.ItemRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateItemDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var item = await _unitOfWork.ItemRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId);

            if (item == null)
                throw new KeyNotFoundException(_localizationHelper.ItemNotFound);

            _mapper.Map(dto, item);

            _unitOfWork.ItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var item = await _unitOfWork.ItemRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId);

            if (item == null)
                throw new KeyNotFoundException(_localizationHelper.ItemNotFound);

            _unitOfWork.ItemRepository.Remove(item);
            await _unitOfWork.SaveChangesAsync();
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