using System;
using System.Collections.Generic;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseFlowModel : IFlowEndPoint
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Balance { get; set; }        
        public int Version { get; set; }
        public List<int> Categories { get; set; }
        
        #region IFlowEndPoint
        
        public void Topup(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative decimal");
            if (amount == 0) return;
            
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative decimal");
            if (amount == 0) return;

            Balance -= amount;
        }

        public DistributionFlowRule FlowRule { get; set; }
        
        #endregion
    }
    
    public static class ExpenseFlowExtensions 
    {
        public static ExpenseFlowModel ToModel(this ExpenseFlow entity)
        {
            if (entity == null) return null;
            return new ExpenseFlowModel
            {
                Id = entity.Id,
                Number = entity.Number,
                Name = entity.Name,
                DateCreated = entity.DateCreated,
                Balance = entity.Balance,
                Version = entity.Version,
            };
        }
    }
}