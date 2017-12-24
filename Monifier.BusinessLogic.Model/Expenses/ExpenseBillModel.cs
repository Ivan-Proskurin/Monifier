using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            UpdateState();
        }
        
        public void RemoveLastAddedItem()
        {
            if (Items == null || Items.Count == 0) return;
            var lastItem = Items.LastOrDefault();
            if (lastItem == null) return;
            
            if (IsNew || lastItem.Id <= 0)
            {
                Items.Remove(lastItem);
            }
            else
            {
                lastItem.IsDeleted = true;
            }
            
            UpdateState();
        }

        private void UpdateState()
        {
            var items = GetItems();
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
            UpdateState();
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
            UpdateState();
        }

        public void Validate()
        {
            if (Cost != Items.Sum(x => x.Cost))
                throw new ValidationException("Сумма чека должна равняться сумме всех входящих в него позиций");
        }
    }
}
