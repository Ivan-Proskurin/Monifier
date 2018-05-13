using System;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class BalancesUpdaterFactory : IBalancesUpdaterFactory
    {
        private readonly IEntityRepository _repository;

        public BalancesUpdaterFactory(IEntityRepository repository)
        {
            _repository = repository;
        }

        public IBalancesUpdater Create(AccountType accountType)
        {
            switch (accountType)
            {
                case AccountType.Cash:
                case AccountType.DebitCard:
                    return new DefaultBalancesUpdater(_repository);
                case AccountType.CreditCard:
                    return new CreditCardBalancesUpdater(_repository);
                default:
                    throw new ArgumentOutOfRangeException(nameof(accountType), accountType, null);
            }
        }
    }
}