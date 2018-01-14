using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Contract;
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

        public void Topup(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Topup amount must be positive");
            if (amount == 0) return;

            if (Balance < AvailBalance)
            {
                Balance += amount;
                if (Balance > AvailBalance) AvailBalance = Balance;
            }
            else
            {
                Balance += amount;
                AvailBalance += amount;
            }
        }

        public void Withdraw(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Withdraw amount must be positive");
            if (amount == 0) return;

            if (AvailBalance < Balance)
            {
                Balance -= amount;
                if (Balance < AvailBalance) AvailBalance = Balance;
                if (AvailBalance < 0) AvailBalance = 0;
            }
            else
            {
                Balance -= amount;
                if (AvailBalance > amount)
                    AvailBalance -= amount;
                else
                    AvailBalance = 0;
            }

            LastWithdraw = DateTime.Now;
        }
        
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
                LastWithdraw = account.LastWithdraw
            };
        }
        
        public static void Withdraw(this Account account, decimal amount, IUnitOfWork unitOfWork)
        {
            var accountCommands = unitOfWork.GetCommandRepository<Account>();
            var accountModel = account.ToModel();
            accountModel.Withdraw(amount);
            account.Balance = accountModel.Balance;
            account.AvailBalance = accountModel.AvailBalance;
            account.LastWithdraw = accountModel.LastWithdraw;
            accountCommands.Update(account);
        }

        public static void Topup(this Account account, decimal amount, IUnitOfWork unitOfWork)
        {
            var accountCommands = unitOfWork.GetCommandRepository<Account>();
            var accountModel = account.ToModel();
            accountModel.Topup(amount);
            account.Balance = accountModel.Balance;
            account.AvailBalance = accountModel.AvailBalance;
            accountCommands.Update(account);
        }
        
        public static AccountModel GetLastUsedAccount(this IEnumerable<AccountModel> accounts)
        {
            return accounts.OrderByDescending(x => x.LastWithdraw).FirstOrDefault();
        }
    }
}