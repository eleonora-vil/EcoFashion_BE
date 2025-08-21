using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace EcoFashionBackEnd.Repositories;

public interface IRepository<TEntity, in TKey>
{
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity> AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    void RemoveRange(IEnumerable<TEntity> entities);
    TEntity Update(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    TEntity Remove(TKey id);
    Task<int> Commit();
    Task<int> CountAsync();
    Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal>> selector);
    void Remove(TEntity entity);
    Task RemoveAsync(TKey id);



    //Task<IEnumerable<TEntity>> GetLastSevenDaysTransactionsAsync();
    //Task<IEnumerable<TEntity>> GetRecentUsersAsync(int count);
    //Task<IEnumerable<TEntity>> GetRecentTransactionsAsync(int count);
    //Task<IDbContextTransaction> BeginTransactionAsync();
    //Task<int> CountByStatusAsync(string status);


}