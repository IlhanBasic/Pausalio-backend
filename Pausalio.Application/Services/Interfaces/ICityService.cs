using Pausalio.Application.DTOs.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface ICityService
    {
        Task<List<CityToReturnDto>> GetAllCities();
        Task<CityToReturnDto?> GetCityById(Guid id);
        Task CreateCity(AddCityDto dto);
        Task DeleteCity(Guid id);
        Task UpdateCity(Guid id, UpdateCityDto dto);
    }
}
