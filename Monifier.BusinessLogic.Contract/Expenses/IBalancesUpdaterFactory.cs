using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IBalancesUpdaterFactory
    {
        IBalancesUpdater Create(AccountType accountType);
    }
}