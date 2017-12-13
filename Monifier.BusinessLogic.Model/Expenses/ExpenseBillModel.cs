using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.Common.Extensions;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseBillModel
    {
        public ExpenseBillModel()
        {
            DateTime = DateTime.Now.ToMinutes();
            Items = new List<ExpenseItemModel>();
        }
        
        public int Id { get; set; }
        public int ExpenseFlowId { get; set; }
        public DateTime DateTime { get; set; }
        public string Category { get; set; }
        public decimal Cost { get; set; }
        public List<ExpenseItemModel> Items { get; set; }

        public bool IsNew => Id <= 0;

        public void AddItem(ExpenseItemModel item)
        {
            if (Items == null) Items = new List<ExpenseItemModel>();
            Items.Add(item);
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            var items = GetItems();
            DateTime = items.Count > 0 ? items.Min(x => x.DateTime) : DateTime.Today;
            Cost = items.Sum(x => x.Cost);
        }

        public void DeleteItems(int[] itemIndicies)
        {
            if (Items == null) return;
            if (IsNew)
            {
                var itemsToRemove = new List<ExpenseItemModel>();
                foreach (var index in itemIndicies)
                {
                    if (index < Items.Count)
                        itemsToRemove.Add(Items[index]);
                }
                foreach (var item in itemsToRemove)
                    Items.Remove(item);
            }
            else
            {
                foreach (var index in itemIndicies)
                {
                    if (index < Items.Count)
                        Items[index].IsDeleted = true;
                }
                var i = 0;
                while (i < Items.Count)
                {
                    var item = Items[i];
                    if (item.Id < 0 && item.IsDeleted)
                        Items.Remove(item);
                    else
                        i++;
                }
            }
            UpdateProperties();
        }

        public List<ExpenseItemModel> GetItems()
        {
            if (Items == null) Items = new List<ExpenseItemModel>();
            return Items.Where(x => !x.IsDeleted).ToList();
        }

        public ExpenseBillModel Clone()
        {
            var clone = new ExpenseBillModel
            {
                Id = Id,
                DateTime = DateTime,
                Category = Category,
                Cost = Cost
            };
            if (Items == null) return clone;
            clone.Items = new List<ExpenseItemModel>();
            foreach (var item in Items)
            {
                clone.Items.Add(new ExpenseItemModel
                {
                    Id = item.Id,
                    DateTime = item.DateTime,
                    Category = item.Category,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    Product = item.Product,
                    Cost = item.Cost,
                    Quantity = item.Quantity,
                    Comment = item.Comment,
                    IsDeleted = item.IsDeleted,
                    IsModified = item.IsModified
                });
            }
            return clone;
        }

        public ExpenseItemModel GetItem(int id)
        {
            return Items?.FirstOrDefault(x => x.Id == id);
        }

        public void SetItem(ExpenseItemModel item)
        {
            if (Items == null) return;
            var index = Items.FindIndex(x => x.Id == item.Id);
            if (index < 0) return;
            Items[index] = item;
            UpdateProperties();
        }
    }
}
