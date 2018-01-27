using System.Threading.Tasks;
using Monifier.BusinessLogic.Model.Inventorization;

namespace Monifier.BusinessLogic.Contract.Inventorization
{
    public interface IInventorizationQueries
    {
        Task<BalanceState> GetBalanceState();
    }
}