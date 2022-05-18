using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models.Country;
using HotelListing.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Core.Repository
{
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public CountriesRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CountryDTO> GetDetails(int id)
        {
            var country = await _context.Countries.Include(q => q.Hotels) //Get the countries, include the hotels
                .ProjectTo<CountryDTO>(_mapper.ConfigurationProvider) // Use ProjectTo to map country object to the countryDTO
                .FirstOrDefaultAsync(Queryable => Queryable.Id == id); //Find the country with the matching Id

            if (country == null)
            { 
                throw new NotFoundException(nameof(GetDetails), id);
            }

            return country;
            
        }
    }
}
