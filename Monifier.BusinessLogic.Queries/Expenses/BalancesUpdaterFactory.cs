using System;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class BalancesUpdaterFactory : IBalancesUpdaterFactory
    {
        private readonly IUnitOfWork _untOfWork;

        public BalancesUpdaterFactory(IUnitOfWork untOfWork)
        {
            _untOfWork = untOfWork;
        }

        public IBalancesUpdater Create(AccountType accountType)
        {
            switch (accountType)
            {
                case AccountType.Cash:
                case AccountType.DebitCard:
                    return new DefaultBalancesUpdater(_untOfWork);
                case AccountType.CreditCard:
                    return new CreditCardBalancesUpdater();
                default:
                    throw new ArgumentOutOfRangeException(nameof(accountType), accountType, null);
            }
        }
    }
}