using System.Collections.Generic;

namespace LightsBuilder.Core.Repositories
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        void Add(T entity);
        void AddAll(IEnumerable<T> entities);
        void Remove(T entity);
        void Update(T entity);
        void AddOrUpdate(T entity);
    }
}
