using System.Linq;
using System.Threading.Tasks;

namespace Monifier.DataAccess.Contract
{
    public interface IQueryRepository<T> where T : class
    {
        IQueryable<T> Query { get; }
        Task<T> GetById(int id);
    }
}
