using System.Threading.Tasks;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Contract.Processing
{
    public interface ICreditCardProcessing
    {
        Task<int> ProcessReducingBalanceAsCreditFees(Account account, decimal reduceAmount);
    }
}