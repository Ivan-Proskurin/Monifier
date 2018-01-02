using System;
using System.Collections.Generic;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseFlowModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Balance { get; set; }
        public int Version { get; set; }
        public List<int> Categories { get; set; }
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