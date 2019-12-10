using CizimApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CizimApp.Data
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private readonly AppDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        protected GenericRepository(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
            this._dbSet = _dbContext.Set<T>();
        }

        public Guid Save(T entity)
        {
            entity.Id = Guid.NewGuid();
            _dbSet.Add(entity);

            return entity.Id;
        }

        public T Get(Guid id)
        {
            return _dbSet.Find(id);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(Guid id)
        {
            var entity = Get(id);
            _dbSet.Remove(entity);
        }


        public IQueryable<T> All()
        {
            return _dbSet.AsNoTracking();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<Guid> SaveAsync(T entity)
        {
            entity.Id = Guid.NewGuid();
            await _dbSet.AddAsync(entity);

            return await Task.FromResult(entity.Id);
        }

      
    }
}
