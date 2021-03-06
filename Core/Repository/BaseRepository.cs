using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Repository
{
    public abstract class BaseRepository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : struct, IEquatable<TId>
    {
        protected readonly DbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        protected BaseRepository(DbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
        }

        public async Task<bool> Contains(TId id)
        {
            return await DbSet.AnyAsync(e => e.Id.Equals(id));
        }

        public async Task<int> Count()
        {
            return await DbSet.CountAsync();
        }

        public virtual async Task<TEntity> Get(TId id, QueryInject<TEntity> queryInject = null)
        {
            return await GetModifiedQuery(queryInject).FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual async Task<List<TEntity>> GetAll(QueryInject<TEntity> queryInject = null)
        {
            return await GetModifiedQuery(queryInject).ToListAsync();
        }

        public virtual Task Post(TEntity entity)
        {
            DbSet.Add(entity);
            return Task.CompletedTask;
        }

        public virtual async Task Put(TEntity entity)
        {
            var existingEntity = await Get(entity.Id);
            if (existingEntity == null)
                throw new InvalidOperationException(
                    "Invalid PUT operation, no entity exists in the database with the specified ID.");
            DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        public virtual async Task<TEntity> Delete(TId id)
        {
            var existingEntity = await Get(id);
            if (existingEntity == null)
                throw new InvalidOperationException(
                    "Invalid DELETE operation, no entity exists in the database with the specified ID.");
            DbSet.Remove(existingEntity);
            return existingEntity;
        }

        public async Task Save()
        {
            await DbContext.SaveChangesAsync();
        }

        protected IQueryable<TEntity> GetModifiedQuery(QueryInject<TEntity> queryInject)
        {
            return queryInject != null ? queryInject(DbSet) : DbSet;
        }
    }
}