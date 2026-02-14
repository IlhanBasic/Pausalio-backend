using AutoMapper;
using Pausalio.Application.DTOs.Document;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public DocumentService(
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

        public async Task<List<DocumentToReturnDto>> GetAllAsync()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var documents = await _unitOfWork.DocumentRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return _mapper.Map<List<DocumentToReturnDto>>(documents);
        }

        public async Task<DocumentToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var document = await _unitOfWork.DocumentRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (document == null)
                return null;

            return _mapper.Map<DocumentToReturnDto>(document);
        }

        public async Task CreateAsync(AddDocumentDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var entity = _mapper.Map<Document>(dto);
            entity.BusinessProfileId = companyId;
            entity.UploadedAt = DateTime.UtcNow;

            await _unitOfWork.DocumentRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateDocumentDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var document = await _unitOfWork.DocumentRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException(_localizationHelper.DocumentNotFound);

            _mapper.Map(dto, document);

            _unitOfWork.DocumentRepository.Update(document);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var document = await _unitOfWork.DocumentRepository
                .FindFirstOrDefaultAsync(x => x.Id == id &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException(_localizationHelper.DocumentNotFound);

            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;

            _unitOfWork.DocumentRepository.Update(document);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}