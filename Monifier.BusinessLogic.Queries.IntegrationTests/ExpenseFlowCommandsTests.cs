using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class ExpenseFlowCommandsTests : DatabaseRelatedEntitiesTest
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
            var queries = UnitOfWork.GetQueryRepository<ExpenseFlow>();
            var efc = new ExpenseFlowCommands(UnitOfWork);
            _model = await efc.Update(_model);
            Assert.True(_model.Id > 0);

            var flow = await queries.GetById(_model.Id);
            
            flow.Should().NotBeNull();
            flow.Name.ShouldBeEquivalentTo(_model.Name);
            flow.Balance.ShouldBeEquivalentTo(_model.Balance);
            flow.DateCreated.ShouldBeEquivalentTo(_model.DateCreated);
            flow.Version.ShouldBeEquivalentTo(1);
        }

        [Fact]
        public async void Update_UpdateFlow_VersionIncreased()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                var efc = new ExpenseFlowCommands(uof);
                _model.DateCreated = DateTime.Today;
                _model = await efc.Update(_model); // created first
                _model.Id.Should().BeGreaterThan(0);
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var efc = new ExpenseFlowCommands(uof);
                var queries = uof.GetQueryRepository<ExpenseFlow>();
                _model.Balance -= 50;
                await efc.Update(_model); // flow updated
                var flow = await queries.GetById(_model.Id);
                flow.Version.ShouldBeEquivalentTo(2);
            });
        }

        [Fact]
        public async void Update_UpdateFlowBalance_UpdatedSuccessfully()
        {
            var queries = UnitOfWork.GetQueryRepository<ExpenseFlow>();
            var efc = new ExpenseFlowCommands(UnitOfWork);
            var model = FoodExpenseFlow.ToModel();
            model.Balance -= 100;
            await efc.Update(model);
            var flow = await queries.GetById(model.Id);
            flow.Balance.ShouldBeEquivalentTo(model.Balance);
        }

        [Fact]
        public async void AddExpense_DecreasedFlowBalanceIncreasedVersion()
        {
            var expense = new ExpenseFlowExpense
            {
                Account = DebitCardAccount.Name,
                ExpenseFlowId = FoodExpenseFlow.Id,
                Category = FoodCategory.Name,
                Cost = 25.60m,
                DateCreated = DateTime.Today,
                Product = Meat.Name,
            };

            var efc = new ExpenseFlowCommands(UnitOfWork);
            await efc.AddExpense(expense);
            var flow = await UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(FoodExpenseFlow.Id);
            flow.Balance.ShouldBeEquivalentTo(FoodExpenseFlow.Balance - expense.Cost);
            flow.Version.ShouldBeEquivalentTo(FoodExpenseFlow.Version + 1);
        }
    }
}