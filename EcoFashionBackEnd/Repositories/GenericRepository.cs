using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcoFashionBackEnd.Repositories;

public class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>
where TEntity : class
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> GetAll()
        => _dbSet;

    public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate)
        => _dbSet.Where(predicate);

    public async Task<TEntity?> GetByIdAsync(TKey id)
        => await _dbSet.FindAsync(id);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var entityEntry = await _dbContext.Set<TEntity>().AddAsync(entity);
        return entityEntry.Entity;
    }
    
    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public TEntity Update(TEntity entity)
    {
        var entityEntry = _dbContext.Set<TEntity>().Update(entity);
        return entityEntry.Entity;
    }
    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        var entityEntry = _dbContext.Set<TEntity>().Update(entity);
        return Task.FromResult(entityEntry.Entity);
    }

    public TEntity Remove(TKey id)
    {
        var entity = GetByIdAsync(id).Result;
        var entityEntry = _dbContext.Set<TEntity>().Remove(entity!);
        return entityEntry.Entity;
    }

    public Task<int> Commit() => _dbContext.SaveChangesAsync();

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal>> selector)
    {
        return await _dbSet.Where(predicate).SumAsync(selector);
    }
    public void RemoveRange(IEnumerable<TEntity> entities) 
    {
        _dbSet.RemoveRange(entities);
    }
    // Bổ sung: Hàm xóa trực tiếp một thực thể
    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    // Bổ sung: Hàm xóa bất đồng bộ an toàn hơn
    public async Task RemoveAsync(TKey id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}






    //public async Task<IEnumerable<TEntity>> GetLastSevenDaysTransactionsAsync()
    //{
    //    var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);

    //    if (typeof(TEntity) == typeof(PaymentTransaction))
    //    {
    //        return await _dbSet.Cast<PaymentTransaction>()
    //                           .Where(pt => pt.CreateDate >= sevenDaysAgo)
    //                           .Cast<TEntity>()
    //                           .ToListAsync();
    //    }

    //    // If the entity is not PaymentTransaction, return an empty list
    //    return new List<TEntity>();
    //}
    //public async Task<IEnumerable<TEntity>> GetRecentUsersAsync(int count)
    //{
    //    return await _dbSet.OrderByDescending(e => EF.Property<DateTime>(e, "CreateDate"))
    //                       .Take(count)
    //                       .ToListAsync();
    //}
    //public async Task<IEnumerable<TEntity>> GetRecentTransactionsAsync(int count)
    //{
    //    return await _dbSet.OrderByDescending(e => EF.Property<DateTime>(e, "CreateDate"))
    //                       .Take(count)
    //                       .ToListAsync();
    //}
    //public async Task<IDbContextTransaction> BeginTransactionAsync()
    //{
    //    return await _dbContext.Database.BeginTransactionAsync();
    //}
    //public async Task<int> CountByStatusAsync(string status)
    //{
    //    if (typeof(TEntity) != typeof(PaymentTransaction))
    //    {
    //        throw new InvalidOperationException("This method is only valid for PaymentTransaction entities");
    //    }

    //    return await _dbSet.Cast<PaymentTransaction>()
    //        .CountAsync(pt => pt.Status.ToUpper() == status.ToUpper());
    //}
