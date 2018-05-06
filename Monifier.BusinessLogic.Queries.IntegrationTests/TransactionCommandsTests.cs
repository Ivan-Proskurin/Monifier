using System;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Queries.Transactions;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class TransactionCommandsTests : QueryTestBase
    {
        [Fact]
        public async void SameInitiatorAndPariticioantTransfer_ShouldThrowExpception()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var transaction = new TransactionModel
                {
                    OwnerId = session.UserSession.UserId,
                    DateTime = DateTime.UtcNow,
                    Total = 1000,
                    InitiatorId = ids.CashAccountId,
                    ParticipantId = ids.CashAccountId,
                };
                var commands = new TransactionCommands(session.UnitOfWork, session.UserSession);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await commands.Update(transaction));
            }
        }

        [Fact]
        public async void SeveralTargets_ShoulsThrowException()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var bill = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.UtcNow, session.Meat, 2500);
                var transaction = new TransactionModel
                {
                    OwnerId = session.UserSession.UserId,
                    DateTime = DateTime.UtcNow,
                    Total = 1000,
                    InitiatorId = ids.CashAccountId,
                    BillId = bill.Id,
                    IncomeId = ids.SalaryIncomeId
                };
                var commands = new TransactionCommands(session.UnitOfWork, session.UserSession);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await commands.Update(transaction));
            }
        }

        [Fact]
        public async void NoTargetsAissigned_ShouldThrowException()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var transaction = new TransactionModel
                {
                    OwnerId = session.UserSession.UserId,
                    DateTime = DateTime.UtcNow,
                    Total = 1000,
                };
                var commands = new TransactionCommands(session.UnitOfWork, session.UserSession);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await commands.Update(transaction));
            }
        }
    }
}