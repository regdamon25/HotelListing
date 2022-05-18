using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models.Country;
using HotelListing.Common;
using HotelListing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers
{
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("2.0")]

    public class CountriesV2Controller : ControllerBase
    {
        //Needs to be refactored from boilerplate
        //1. Don't want to call db context directly from controller...Create Repositories
        //2. Don't want to directly be sending over the data objects with our API or receiving objects of that type...Create DTOs

        private readonly ICountriesRepository _countriesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CountriesV2Controller> _logger;

        public CountriesV2Controller(ICountriesRepository countriesRepository, IMapper mapper, ILogger<CountriesV2Controller> logger)
        {
            _countriesRepository = countriesRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Countries
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDTO>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<GetCountryDTO>>(countries);
            //Select * from Countries
            return Ok(records);

        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry), id);
            }

            var results = _mapper.Map<CountryDTO>(country);

            return Ok(results);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDTO updateCountryDTO)
        {
            if (id != updateCountryDTO.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            //_context.Entry(country).State = EntityState.Modified;

            var country = await _countriesRepository.GetAsync(id); //Finding the country from the database using ID..it's being "tracked"

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountries), id);
            }

            _mapper.Map(updateCountryDTO, country); //Automatically told entity framework that I have changed this to modified and automatically assigned values from the right left to the right side

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();//This is a good response 204 No Content
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CountryDTO>> PostCountry(CreateCountryDTO createCountryDTO)
        {

            var country = await _countriesRepository.AddAsync<CreateCountryDTO, CountryDTO>(createCountryDTO);
            return CreatedAtAction(nameof(GetCountry), new {id = country.Id }, country);
            //After that is done you can get there by calling "GetCountry" url endpoint by the new id(country.id) from the country object
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountries), id);
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();//204 No Content
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
