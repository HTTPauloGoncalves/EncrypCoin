namespace EncrypCoin.API.Repository.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task <List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task<T?> UpdateAsync(T entity);
        Task DeleteAllAsync();
        Task DeleteByIdAsync(Guid id);
    }
}
