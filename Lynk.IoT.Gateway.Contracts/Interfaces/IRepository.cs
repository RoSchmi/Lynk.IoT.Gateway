using System.Linq;
using System.Threading.Tasks;

namespace Lynk.IoT.Gateway.Contracts.Interfaces
{
    public interface IRepository
    {
        Task AddEntityAsync<T>(T model) where T : class;
       
        Task UpdateEntityAsync<T>(T model) where T : class;

        Task DeleteEntityAsync<T>(T model) where T : class;

        IQueryable<T> Get<T>() where T : class;

        Task<T> Get<T>(object id) where T : class;
    }
}
