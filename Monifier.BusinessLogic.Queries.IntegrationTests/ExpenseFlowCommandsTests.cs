using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class ExpenseFlowCommandsTests : QueryTestBase
    {
        private ExpenseFlowModel _model;
        
        public ExpenseFlowCommandsTests()
        {
            _model = new ExpenseFlowModel
            {
                Id = -1,
                Name = "Путешествия",
                Number = 1,
                Balance = 100.50m,
                DateCreated = DateTime.Today.AddHours(3).AddMinutes(45), 
            };
        }
        
        [Fact]
        public async void Update_NewFlow_VersionEqualsOne()
        {
            using (var session = await CreateDefaultSession())
            {
                var queries = session.UnitOfWork.GetQueryRepository<ExpenseFlow>();
                var efc = session.CreateExpenseFlowCommands();
                _model = await efc.Update(_model);
                Assert.True(_model.Id > 0);

                var flow = await queries.GetById(_model.Id);

                flow.Should().NotBeNull();
                flow.Name.ShouldBeEquivalentTo(_model.Name);
                flow.Balance.ShouldBeEquivalentTo(_model.Balance);
                flow.DateCreated.ShouldBeEquivalentTo(_model.DateCreated);
                flow.Version.ShouldBeEquivalentTo(1);
            }
        }

        [Fact]
        public async void Update_UpdateFlow_VersionIncreased()
        {
            using (var session = await CreateDefaultSession())
            {
                var efc = session.CreateExpenseFlowCommands();
                _model.DateCreated = DateTime.Today;
                _model = await efc.Update(_model); // created first
                _model.Id.Should().BeGreaterThan(0);
            }

            using (var session = await CreateDefaultSession())
            {
                var efc = session.CreateExpenseFlowCommands();
                var queries = session.UnitOfWork.GetQueryRepository<ExpenseFlow>();
                _model.Balance -= 50;
                await efc.Update(_model); // flow updated
                var flow = await queries.GetById(_model.Id);
                flow.Version.ShouldBeEquivalentTo(2);
            }
        }

        [Fact]
        public async void Update_UpdateFlowBalance_UpdatedSuccessfully()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var queries = session.UnitOfWork.GetQueryRepository<ExpenseFlow>();
                var efc = session.CreateExpenseFlowCommands();
                var model = session.FoodExpenseFlow.ToModel();
                model.Balance -= 100;
                await efc.Update(model);
                var flow = await queries.GetById(model.Id);
                flow.Balance.ShouldBeEquivalentTo(model.Balance);
            }
        }

        [Fact]
        public async void AddExpense_DecreasedFlowBalanceIncreasedVersion()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var expense = new ExpenseFlowExpense
                {
                    Correcting = false,
                    Account = session.DebitCardAccount.Name,
                    ExpenseFlowId = session.FoodExpenseFlow.Id,
                    Category = session.FoodCategory.Name,
                    Cost = 25.60m,
                    DateCreated = DateTime.Today,
                    Product = session.Meat.Name,
                };

                var now = DateTime.Now;
                var efc = session.CreateExpenseFlowCommands();
                await efc.AddExpense(expense);
                var flow = await session.UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance - expense.Cost);
                flow.Version.ShouldBeEquivalentTo(session.FoodExpenseFlow.Version + 1);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance - expense.Cost);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
                account.LastWithdraw.Should().NotBeNull();
                account.LastWithdraw?.Should().BeOnOrAfter(now);
            }
        }

        [Fact]
        public async void AddExpense_Correcting_OnlyFlowBalanceChanged()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var expense = new ExpenseFlowExpense
                {
                    Correcting = true,
                    Account = session.DebitCardAccount.Name,
                    ExpenseFlowId = session.FoodExpenseFlow.Id,
                    Category = session.FoodCategory.Name,
                    Cost = 25.60m,
                    DateCreated = DateTime.Today,
                    Product = session.Meat.Name,
                };

                var efc = session.CreateExpenseFlowCommands();
                await efc.AddExpense(expense);
                var flow = await session.UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance - expense.Cost);
                flow.Version.ShouldBeEquivalentTo(session.FoodExpenseFlow.Version + 1);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
                account.LastWithdraw.ShouldBeEquivalentTo(session.DebitCardAccount.LastWithdraw);
            }
        }
    }
}