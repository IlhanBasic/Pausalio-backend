using Pausalio.Application.DTOs.Country;

namespace Pausalio.Application.Services.Interfaces
{
    public interface ICountryService
    {
        Task<List<CountryToReturnDto>> GetAllCountries();
        Task<CountryToReturnDto?> GetCountryById(Guid id);
        Task CreateCountry(AddCountryDto dto);
        Task UpdateCountry(Guid id, UpdateCountyDto dto);
        Task DeleteCountry(Guid id);
    }
}