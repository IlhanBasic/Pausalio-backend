using AutoMapper;
using Pausalio.Application.DTOs.Country;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;

        public CountryService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationHelper localizationHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
        }

        public async Task<List<CountryToReturnDto>> GetAllCountries()
        {
            var countries = await _unitOfWork.CountryRepository.GetAllAsync();
            return _mapper.Map<List<CountryToReturnDto>>(countries);
        }

        public async Task<CountryToReturnDto?> GetCountryById(Guid id)
        {
            var country = await _unitOfWork.CountryRepository.GetByIdAsync(id);

            if (country == null)
                return null;

            return _mapper.Map<CountryToReturnDto>(country);
        }

        public async Task CreateCountry(AddCountryDto dto)
        {
            var country = _mapper.Map<Country>(dto);

            await _unitOfWork.CountryRepository.AddAsync(country);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCountry(Guid id, UpdateCountyDto dto)
        {
            var country = await _unitOfWork.CountryRepository.GetByIdAsync(id);

            if (country == null)
                throw new KeyNotFoundException(_localizationHelper.CountryNotFound);

            _mapper.Map(dto, country);

            _unitOfWork.CountryRepository.Update(country);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCountry(Guid id)
        {
            var country = await _unitOfWork.CountryRepository.GetByIdAsync(id);

            if (country == null)
                throw new KeyNotFoundException(_localizationHelper.CountryNotFound);

            _unitOfWork.CountryRepository.Remove(country);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}