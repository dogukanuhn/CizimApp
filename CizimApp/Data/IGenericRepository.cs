using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CizimApp.Data
{
    public interface IGenericRepository<T> where T : class, IEntity
    {
        Guid Save(T entity);
        T Get(Guid id);
        void Update(T entity);
        void Delete(Guid id);
        IQueryable<T> All();
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);

       
    }
}
