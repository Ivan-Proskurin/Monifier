using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class DatabaseRelatedEntitiesTest : DatabaseRelatedBlankTest
    {
        public DatabaseRelatedEntitiesTest()
        {
            UseUnitOfWork(uow =>
            {
                UserSveta = CreateUser("Света", "svetika", "pass", false, uow);
                CurrentUser = UserSveta;
                CreateEntities(uow);
                UserEvgeny = CreateUser("Евгений", "evgen", "password", true, uow);
                CurrentUser = UserEvgeny;
                CreateEntities(uow);
                uow.SaveChanges();
            });
        }

        private void CreateEntities(IUnitOfWork uow)
        {
            SalaryIncome = CreateIncomeType("Зарплата", uow);
            GiftsIncome = CreateIncomeType("Подарки", uow);
            
            FoodCategory = CreateCategory("Продукты", uow);
            TechCategory = CreateCategory("Техника", uow);

            Bread = CreateProduct(FoodCategory.Id, "Хлеб", uow);
            Meat = CreateProduct(FoodCategory.Id, "Мясо", uow);
            Tv = CreateProduct(TechCategory.Id, "Телевизор", uow);

            FoodExpenseFlow = CreateExpenseFlow("Продукты питания", 1000, DateTime.Today, 1, uow);
            
            DebitCardAccount = CreateAccount("Дебетовая карта", 15000, DateTime.Today, uow);
            CashAccount = CreateAccount("Наличные", 30000, DateTime.Today, uow);

            Incomes = new []
            {
                CreateIncome(SalaryIncome.Id, new DateTime(2018, 01, 31), 100000, DebitCardAccount.Id, uow),
                CreateIncome(GiftsIncome.Id, new DateTime(2018, 03, 08), 8000, CashAccount.Id, uow)
            }
                .OrderBy(x => x.DateTime).ToList();
        }

        public User UserSveta { get; private set; }
        public User UserEvgeny { get; private set; }
        
        protected IncomeType SalaryIncome { get; private set; }
        protected IncomeType GiftsIncome { get; private set; }
        
        protected Category FoodCategory { get; private set; }
        protected Category TechCategory { get; private set; }
        
        protected Product Bread { get; private set; }
        protected Product Meat { get; private set; }
        protected Product Tv { get; private set; }
        
        protected ExpenseFlow FoodExpenseFlow { get; private set; }
        
        protected Account DebitCardAccount { get; private set; }
        protected Account CashAccount { get; private set; }

        protected List<IncomeItem> Incomes { get; private set; }
    }
}