using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.Repo
{
    public interface IRepository<TAggregate>
    {
        Task<TAggregate> GetById(Guid id);
        Task<IEnumerable<TAggregate>> GetAll();
        Task<IQueryable<TAggregate>> Get();
        Task Save(TAggregate item);
        void SaveAll(IEnumerable<TAggregate> items);
        void Delete(Guid id);
        void Delete(TAggregate item);
        void DeleteAll();
        long Count();
    }
}
