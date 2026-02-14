using AutoMapper;
using Pausalio.Application.DTOs.City;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class CityService : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;

        public CityService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationHelper localizationHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
        }

        public async Task<List<CityToReturnDto>> GetAllCities()
        {
            var cities = await _unitOfWork.CityRepository.GetAllAsync();
            return _mapper.Map<List<CityToReturnDto>>(cities);
        }

        public async Task<CityToReturnDto?> GetCityById(Guid id)
        {
            var city = await _unitOfWork.CityRepository.GetByIdAsync(id);

            if (city == null)
                return null;

            return _mapper.Map<CityToReturnDto>(city);
        }

        public async Task CreateCity(AddCityDto dto)
        {
            var city = _mapper.Map<City>(dto);

            await _unitOfWork.CityRepository.AddAsync(city);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCity(Guid id, UpdateCityDto dto)
        {
            var city = await _unitOfWork.CityRepository.GetByIdAsync(id);

            if (city == null)
                throw new KeyNotFoundException(_localizationHelper.CityNotFound);

            _mapper.Map(dto, city);

            _unitOfWork.CityRepository.Update(city);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCity(Guid id)
        {
            var city = await _unitOfWork.CityRepository.GetByIdAsync(id);

            if (city == null)
                throw new KeyNotFoundException(_localizationHelper.CityNotFound);

            _unitOfWork.CityRepository.Remove(city);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}