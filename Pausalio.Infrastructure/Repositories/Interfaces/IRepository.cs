using System.Linq.Expressions;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<int> CountAsync();
        Task<T?> FindFirstOrDefaultWithoutTrackingAsync(Expression<Func<T, bool>> predicate);
    }
}
