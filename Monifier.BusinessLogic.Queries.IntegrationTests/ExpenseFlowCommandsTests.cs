using System;
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
        public async void Update_NewFlow_LastUpdatedEqualsDateCreated()
        {
            var queries = UnitOfWork.GetQueryRepository<ExpenseFlow>();
            var efc = new ExpenseFlowCommands(UnitOfWork);
            _model = await efc.Update(_model);
            Assert.True(_model.Id > 0);

            var flow = await queries.GetById(_model.Id);
            Assert.NotNull(flow);
            Assert.Equal(_model.Name, flow.Name);
            Assert.Equal(_model.Balance, flow.Balance);
            Assert.Equal(_model.DateCreated, flow.DateCreated);
            Assert.Equal(1, flow.Version);
        }

        [Fact]
        public async void Update_UpdateFlow_LastUpdatedIncreased()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                var efc = new ExpenseFlowCommands(uof);
                _model.DateCreated = DateTime.Today;
                _model = await efc.Update(_model); // created first
                Assert.True(_model.Id > 0);
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var efc = new ExpenseFlowCommands(uof);
                var queries = uof.GetQueryRepository<ExpenseFlow>();
                _model.Balance -= 50;
                await efc.Update(_model); // flow updated
                var flow = await queries.GetById(_model.Id);
                Assert.Equal(2, flow.Version);
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
            Assert.Equal(model.Balance, flow.Balance);
        }

        [Fact]
        public async void AddExpense_DecreasedFlowBalanceIncreasedVersion()
        {
            var expense = new ExpenseFlowExpense
            {
                ExpenseFlowId = FoodExpenseFlow.Id,
                Category = FoodCategory.Name,
                Cost = 25.60m,
                DateCreated = DateTime.Today,
                Product = Meat.Name,
            };

            var efc = new ExpenseFlowCommands(UnitOfWork);
            await efc.AddExpense(expense);
            var flow = await UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(FoodExpenseFlow.Id);
            Assert.Equal(FoodExpenseFlow.Balance - expense.Cost, flow.Balance);
            Assert.Equal(FoodExpenseFlow.Version + 1, flow.Version);
        }
    }
}