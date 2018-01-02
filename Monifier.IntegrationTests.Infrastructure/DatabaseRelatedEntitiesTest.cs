using System;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class DatabaseRelatedEntitiesTest : DatabaseRelatedBlankTest
    {
        public DatabaseRelatedEntitiesTest()
        {
            UseUnitOfWork(uof =>
            {
                CreateEntities(uof);
                uof.SaveChanges();
            });
        }

        private void CreateEntities(IUnitOfWork uof)
        {
            FoodCategory = CreateCategory("Продукты", uof);
            TechCategory = CreateCategory("Техника", uof);

            Bread = CreateProduct(FoodCategory.Id, "Хлеб", uof);
            Meat = CreateProduct(FoodCategory.Id, "Мясо", uof);
            Tv = CreateProduct(TechCategory.Id, "Телевизор", uof);

            FoodExpenseFlow = CreateExpenseFlow("Продукты питания", 1000, DateTime.Today, 1, uof);
        }
        
        protected Category FoodCategory { get; private set; }
        protected Category TechCategory { get; private set; }
        
        protected Product Bread { get; private set; }
        protected Product Meat { get; private set; }
        protected Product Tv { get; private set; }
        
        protected ExpenseFlow FoodExpenseFlow { get; private set; }
    }
}