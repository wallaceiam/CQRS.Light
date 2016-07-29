using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IRepository<TAggregate>
    {
        Task<TAggregate> GetByIdAsync(Guid id);
        Task<IEnumerable<TAggregate>> GetAllAsync();
        Task<IQueryable<TAggregate>> GetAsync();
        Task SaveAsync(TAggregate item);
        Task SaveAllAsync(IEnumerable<TAggregate> items);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(TAggregate item);
        Task DeleteAllAsync();
        Task<long> CountAsync();
    }
}
