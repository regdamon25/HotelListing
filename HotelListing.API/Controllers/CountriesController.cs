using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using AutoMapper;
using HotelListing.API.Contracts;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        //Needs to be refactored from boilerplate
        //1. Don't want to call db context directly from controller...Create Repositories
        //2. Don't want to directly be sending over the data objects with our API or receiving objects of that type...Create DTOs
        
        private readonly ICountriesRepository _countriesRepository;
        private readonly IMapper _mapper;

        public CountriesController(ICountriesRepository countriesRepository, IMapper mapper)
        {
            _countriesRepository = countriesRepository;
            _mapper = mapper;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDTO>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            var results = _mapper.Map<List<GetCountryDTO>>(countries);
            //Select * from Countries
            return Ok(results);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                return NotFound();
            }

            var results = _mapper.Map<CountryDTO>(country);

            return Ok(results);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDTO updateCountryDTO)
        {
            if (id != updateCountryDTO.Id)
            {
                return BadRequest();
            }

            //_context.Entry(country).State = EntityState.Modified;

            var country = await _countriesRepository.GetAsync(id); //Finding the country from the database using ID..it's being "tracked"

            if (country == null)
            {
                return NotFound();
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
        public async Task<ActionResult<Country /*Response*/>> PostCountry(/*Request*/ CreateCountryDTO createCountryDTO)
        {

            var country = _mapper.Map<Country>(createCountryDTO);

            await _countriesRepository.AddAsync(country);//after you do that, save changes

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
            //After that is done you can get there by calling "GetCountry" url endpoint by the new id(country.id) from the country object
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound();
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
