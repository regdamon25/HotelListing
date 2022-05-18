using HotelListing.API.Core.Models.Country;
using HotelListing.Data;

namespace HotelListing.API.Core.Contracts
{
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<CountryDTO> GetDetails(int id);
    }
}
