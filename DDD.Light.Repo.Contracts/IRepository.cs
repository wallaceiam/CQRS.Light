using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDD.Light.Repo.Contracts
{
    public interface IRepository<TId, TAggregate>
    {
        TAggregate GetById(TId id);
        Task<IEnumerable<TAggregate>> GetAll();
        IQueryable<TAggregate> Get();
        void Save(TAggregate item);
        void SaveAll(IEnumerable<TAggregate> items);
        void Delete(TId id);
        void Delete(TAggregate item);
        void DeleteAll();
        long Count();
    }
}
