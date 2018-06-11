using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Model.Base
{
    public class AccountModel : IFlowEndPoint
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailBalance { get; set; }
        public DateTime? LastWithdraw { get; set; }
        public bool IsDefault { get; set; }
        public AccountType AccountType { get; set; }
        
        #region IFlowEndPoint

        decimal IFlowEndPoint.Balance => AvailBalance;

        DistributionFlowRule IFlowEndPoint.FlowRule { get; set; }

        void IFlowEndPoint.Topup(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative decimal");
            if (amount == 0) return;

            AvailBalance += amount;
        }

        void IFlowEndPoint.Withdraw(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative decimal");
            if (amount == 0) return;
            
            if (amount > AvailBalance)
                throw new InvalidOperationException("Can't withdraw amount greater than available balance");

            AvailBalance -= amount;
        }
        
        #endregion
    }

    public static class AccountExtensions
    {
        public static AccountModel ToModel(this Account account)
        {
            if (account == null) return null;
            return new AccountModel
            {
                Id = account.Id,
                Number = account.Number,
                DateCreated = account.DateCreated,
                Name = account.Name,
                Balance = account.Balance,
                AvailBalance = account.AvailBalance,
                LastWithdraw = account.LastWithdraw,
                IsDefault = account.IsDefault,
                AccountType = account.AccountType
            };
        }

        public static Account ToEntity(this AccountModel model, int ownerId)
        {
            return new Account
            {
                Id = model.Id,
                Number = model.Number,
                DateCreated = model.DateCreated,
                Name = model.Name,
                Balance = model.Balance,
                AvailBalance = model.AvailBalance,
                LastWithdraw = model.LastWithdraw,
                IsDefault = model.IsDefault,
                AccountType = model.AccountType,
                OwnerId = ownerId
            };
        }
        
        public static AccountModel GetDefaultAccount(this IEnumerable<AccountModel> accounts)
        {
            return accounts
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.LastWithdraw).FirstOrDefault();
        }
    }
}