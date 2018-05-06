using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class TransactionQueriesTests : QueryTestBase
    {
        [Fact]
        public async void GetLastTransactions_MainTest()
        {
            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                var accountCommands = session.CreateAccountCommands();
                await accountCommands.Transfer(ids.DebitCardAccountId, ids.CashAccountId, 1000);
                await accountCommands.Transfer(ids.CashAccountId, ids.DebitCardAccountId, 2000);
                var incomeCommands = session.CreateIncomeCommands();
                await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow,
                    Total = 1450,
                    IncomeTypeId = ids.GiftsIncomeId,
                });
                await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow.AddDays(-100),
                    Total = 14500,
                    IncomeTypeId = ids.SalaryIncomeId,
                });
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.Now, session.Meat, 350);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 15000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 5600);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var accountCommands = session.CreateAccountCommands();
                var transfer1DateTime = await accountCommands.Transfer(ids.DebitCardAccountId, ids.CashAccountId, 1000);
                var transfer2DateTime = await accountCommands.Transfer(ids.CashAccountId, ids.DebitCardAccountId, 2000);
                var incomeCommands = session.CreateIncomeCommands();
                var income = await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow,
                    Total = 1450,
                    IncomeTypeId = ids.GiftsIncomeId,
                });
                await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow.AddDays(-100),
                    Total = 14500,
                    IncomeTypeId = ids.SalaryIncomeId,
                });
                var bill1 = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.Now, session.Meat, 350);
                var bill2 = await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 15000);
                var bill3 = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 5600);
                var transationsQueries = session.CreateTransactionQueries();
                var transactions = await transationsQueries.GetLastTransactions(ids.DebitCardAccountId);
                transactions.ShouldBeEquivalentTo(new[]
                {
                    new TransactionViewModel
                    {
                        DateTime = bill3.DateTime,
                        Type = "Оплата",
                        Target = session.TechExpenseFlow.Name,
                        Total = -5600
                    },
                    new TransactionViewModel
                    {
                        DateTime = bill1.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = -350,
                    },
                    new TransactionViewModel
                    {
                        DateTime = income.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = 1450
                    },
                    new TransactionViewModel
                    {
                        DateTime = transfer2DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = 2000
                    },
                    new TransactionViewModel
                    {
                        DateTime = transfer1DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = -1000
                    },
                });

                transactions = await transationsQueries.GetLastTransactions(ids.CashAccountId);
                transactions.ShouldBeEquivalentTo(new[]
                    {
                        new TransactionViewModel
                        {
                            DateTime = bill2.DateTime,
                            Type = "Оплата",
                            Target = session.TechExpenseFlow.Name,
                            Total = -15000
                        },
                        new TransactionViewModel
                        {
                            DateTime = transfer2DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = -2000
                        },
                        new TransactionViewModel
                        {
                            DateTime = transfer1DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = 1000
                        },
                    }
                );

                transactions = await transationsQueries.GetLastTransactions(ids.DebitCardAccountId, 3);
                transactions.Count.ShouldBeEquivalentTo(3);
            }
        }

        [Fact]
        public async void GetAllTransactions_MainTest()
        {
            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                var accountCommands = session.CreateAccountCommands();
                await accountCommands.Transfer(ids.DebitCardAccountId, ids.CashAccountId, 1000);
                await accountCommands.Transfer(ids.CashAccountId, ids.DebitCardAccountId, 2000);
                var incomeCommands = session.CreateIncomeCommands();
                await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow,
                    Total = 1450,
                    IncomeTypeId = ids.GiftsIncomeId,
                });
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.Now, session.Meat, 350);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 15000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 5600);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var accountCommands = session.CreateAccountCommands();
                var transfer1DateTime = await accountCommands.Transfer(ids.DebitCardAccountId, ids.CashAccountId, 1000);
                var transfer2DateTime = await accountCommands.Transfer(ids.CashAccountId, ids.DebitCardAccountId, 2000);
                var incomeCommands = session.CreateIncomeCommands();
                var income = await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow,
                    Total = 1450,
                    IncomeTypeId = ids.GiftsIncomeId,
                });
                var oldIncome = await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow.AddDays(-100),
                    Total = 3456,
                    IncomeTypeId = ids.SalaryIncomeId,
                });
                var veryOldIncome = await incomeCommands.Update(new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = DateTime.UtcNow.AddDays(-1000),
                    Total = 6478,
                    IncomeTypeId = ids.GiftsIncomeId,
                });
                var bill1 = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.Now, session.Meat, 350);
                var bill2 = await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 15000);
                var bill3 = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId,
                    DateTime.Now, session.Tv, 5600);
                var oldBill = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId,
                    DateTime.UtcNow.AddDays(-1010), session.Bread, 200);
                var veryOldBill = await session.CreateExpenseBill(ids.CashAccountId, ids.FoodExpenseFlowId,
                    DateTime.UtcNow.AddDays(-500), session.Bread, 333);
                var transationsQueries = session.CreateTransactionQueries();
                var paginationArgs = new PaginationArgs
                {
                    PageNumber = 1,
                    ItemsPerPage = int.MaxValue,
                    IncludeDeleted = true
                };
                var transactionsPaged = await transationsQueries.GetAllTransactions(new TransactionFilter { AccountId = ids.DebitCardAccountId}, paginationArgs);
                transactionsPaged.Transactions.ShouldBeEquivalentTo(new[]
                {
                    new TransactionViewModel
                    {
                        DateTime = bill3.DateTime,
                        Type = "Оплата",
                        Target = session.TechExpenseFlow.Name,
                        Total = -5600
                    },
                    new TransactionViewModel
                    {
                        DateTime = bill1.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = -350,
                    },
                    new TransactionViewModel
                    {
                        DateTime = income.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = 1450
                    },
                    new TransactionViewModel
                    {
                        DateTime = transfer2DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = 2000
                    },
                    new TransactionViewModel
                    {
                        DateTime = transfer1DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = -1000
                    },
                    new TransactionViewModel
                    {
                        DateTime = oldIncome.DateTime,
                        Type = "Поступление",
                        Target = session.SalaryIncome.Name,
                        Total = 3456
                    },
                    new TransactionViewModel
                    {
                        DateTime = veryOldIncome.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = 6478
                    },
                    new TransactionViewModel
                    {
                        DateTime = oldBill.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = -200
                    }, 
                });

                var last = await transationsQueries.GetLastTransactions(ids.CashAccountId);
                last.ShouldBeEquivalentTo(new[]
                    {
                        new TransactionViewModel
                        {
                            DateTime = bill2.DateTime,
                            Type = "Оплата",
                            Target = session.TechExpenseFlow.Name,
                            Total = -15000
                        },
                        new TransactionViewModel
                        {
                            DateTime = transfer2DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = -2000
                        },
                        new TransactionViewModel
                        {
                            DateTime = transfer1DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = 1000
                        },
                        new TransactionViewModel
                        {
                            DateTime = veryOldBill.DateTime,
                            Type = "Оплата",
                            Target = session.FoodExpenseFlow.Name,
                            Total = -333
                        }, 
                    }
                );
            }
        }

        [Fact]
        public async void GetLastTransactions_LimitIsOver100_ShouldTrhowException()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var transactions = session.CreateTransactionQueries();
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                    await transactions.GetLastTransactions(ids.CashAccountId, 101));
            }
        }
    }
}