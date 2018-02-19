using System;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

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
            CurrentUser = CreateUser("Евгений", "evgen", "password", true, uof);
            
            SalaryIncome = CreateIncomeType("Зарплата", uof);
            
            FoodCategory = CreateCategory("Продукты", uof);
            TechCategory = CreateCategory("Техника", uof);

            Bread = CreateProduct(FoodCategory.Id, "Хлеб", uof);
            Meat = CreateProduct(FoodCategory.Id, "Мясо", uof);
            Tv = CreateProduct(TechCategory.Id, "Телевизор", uof);

            FoodExpenseFlow = CreateExpenseFlow("Продукты питания", 1000, DateTime.Today, 1, uof);
            
            DebitCardAccount = CreateAccount("Дебетовая карта", 15000, DateTime.Today, uof);
            CashAccount = CreateAccount("Наличные", 30000, DateTime.Today, uof);
        }
        
        protected IncomeType SalaryIncome { get; private set; }
        
        protected Category FoodCategory { get; private set; }
        protected Category TechCategory { get; private set; }
        
        protected Product Bread { get; private set; }
        protected Product Meat { get; private set; }
        protected Product Tv { get; private set; }
        
        protected ExpenseFlow FoodExpenseFlow { get; private set; }
        
        protected Account DebitCardAccount { get; private set; }
        protected Account CashAccount { get; private set; }
    }
}