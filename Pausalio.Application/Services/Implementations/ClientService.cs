using AutoMapper;
using Pausalio.Application.DTOs.Client;
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
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public ClientService(
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

        public async Task<IEnumerable<ClientToReturnDto>> GetAllAsync()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var clients = await _unitOfWork.ClientRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            return _mapper.Map<IEnumerable<ClientToReturnDto>>(clients);
        }

        public async Task<IEnumerable<ClientToReturnDto>> GetByTypeAsync(ClientType clientType)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var clients = await _unitOfWork.ClientRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.ClientType == clientType);

            return _mapper.Map<IEnumerable<ClientToReturnDto>>(clients);
        }

        public async Task<ClientToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (client == null)
                return null;

            return _mapper.Map<ClientToReturnDto>(client);
        }

        public async Task CreateAsync(AddClientDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            if (dto.ClientType != ClientType.Individual && string.IsNullOrEmpty(dto.PIB))
                throw new InvalidOperationException(_localizationHelper.LegalEntityMustHavePIB);

            if (!string.IsNullOrEmpty(dto.PIB))
            {
                var existingClientWithPIB = await _unitOfWork.ClientRepository
                    .FindFirstOrDefaultAsync(x => x.PIB == dto.PIB && x.BusinessProfileId == companyId);

                if (existingClientWithPIB != null)
                    throw new InvalidOperationException(_localizationHelper.ClientWithPIBAlreadyExists);
            }

            var existingClientWithEmail = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Email == dto.Email && x.BusinessProfileId == companyId);

            if (existingClientWithEmail != null)
                throw new InvalidOperationException(_localizationHelper.ClientEmailAlreadyExists);

            var client = _mapper.Map<Client>(dto);
            client.BusinessProfileId = companyId;
            client.CreatedAt = DateTime.UtcNow;

            if (dto.ClientType == ClientType.Domestic)
            {
                var serbia = await _unitOfWork.CountryRepository
                    .FindFirstOrDefaultAsync(x => x.Code == "RS" || x.Name == "Srbija");

                if (serbia == null)
                    throw new InvalidOperationException(_localizationHelper.CountrySerbiaNotFound);

                client.CountryId = serbia.Id;
            }
            else if (dto.ClientType == ClientType.Foreign)
            {
                if (dto.CountryId == null)
                    throw new InvalidOperationException(_localizationHelper.ForeignClientMustHaveCountry);

                client.CountryId = dto.CountryId;
            }
            else if (dto.ClientType == ClientType.Individual)
            {
                if (dto.CountryId == null || dto.CountryId == Guid.Empty)
                {
                    var serbia = await _unitOfWork.CountryRepository
                        .FindFirstOrDefaultAsync(x => x.Code == "RS" || x.Name == "Srbija");

                    if (serbia != null)
                    {
                        client.CountryId = serbia.Id;
                    }
                }
                else
                {
                    client.CountryId = dto.CountryId;
                }
            }

            await _unitOfWork.ClientRepository.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateClientDto dto)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (client == null)
                throw new KeyNotFoundException(_localizationHelper.ClientNotFound);

            if (dto.ClientType != ClientType.Individual && string.IsNullOrEmpty(dto.PIB))
                throw new InvalidOperationException(_localizationHelper.LegalEntityMustHavePIB);

            if (!string.IsNullOrEmpty(dto.PIB) && client.PIB != dto.PIB)
            {
                var existingClientWithPIB = await _unitOfWork.ClientRepository
                    .FindFirstOrDefaultAsync(x => x.PIB == dto.PIB &&
                                                   x.BusinessProfileId == companyId &&
                                                   x.Id != id);

                if (existingClientWithPIB != null)
                    throw new InvalidOperationException(_localizationHelper.ClientWithPIBAlreadyExists);
            }

            if (client.Email != dto.Email)
            {
                var existingClientWithEmail = await _unitOfWork.ClientRepository
                    .FindFirstOrDefaultAsync(x => x.Email == dto.Email &&
                                                   x.BusinessProfileId == companyId &&
                                                   x.Id != id);

                if (existingClientWithEmail != null)
                    throw new InvalidOperationException(_localizationHelper.ClientEmailAlreadyExists);
            }

            _mapper.Map(dto, client);
            client.UpdatedAt = DateTime.UtcNow;

            if (dto.ClientType == ClientType.Domestic)
            {
                var serbia = await _unitOfWork.CountryRepository
                    .FindFirstOrDefaultAsync(x => x.Code == "RS" || x.Name == "Srbija");

                if (serbia == null)
                    throw new InvalidOperationException(_localizationHelper.CountrySerbiaNotFound);

                client.CountryId = serbia.Id;
            }
            else if (dto.ClientType == ClientType.Foreign)
            {
                if (dto.CountryId == null)
                    throw new InvalidOperationException(_localizationHelper.ForeignClientMustHaveCountry);

                client.CountryId = dto.CountryId;
            }

            _unitOfWork.ClientRepository.Update(client);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (client == null)
                throw new KeyNotFoundException(_localizationHelper.ClientNotFound);

            _unitOfWork.ClientRepository.Remove(client);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task ActivateAsync(Guid id)
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var client = await _unitOfWork.ClientRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (client == null)
                throw new KeyNotFoundException(_localizationHelper.ClientNotFound);

            client.IsActive = true;
            client.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ClientRepository.Update(client);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}