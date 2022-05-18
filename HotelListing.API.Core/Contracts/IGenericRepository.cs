using HotelListing.API.Core.Models;
using static HotelListing.API.Core.Models.QueryParameters;

namespace HotelListing.API.Core.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id);
        Task<TResult?> GetAsync<TResult>(int? id);
        //Task<List<T>> GetAllAsync();
        Task<List<TResult>> GetAllAsync<TResult>();
        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters); //This will do the mapping within in the query to reduce the mapping thats done in the Repository...Results in less lines of code
        //Task<T> AddAsync(T entity);
        Task<TResult> AddAsync<TSource, TResult>(TSource source);
        Task DeleteAsync(int id);
        //Task UpdateAsync(T entity);
        Task UpdateAsync<TSource>(int id, TSource source) where TSource : IBaseDTO;
        Task<bool> Exists(int id);
    }
}
