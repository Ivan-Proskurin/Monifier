using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.Common.Extensions;
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
                        IsExpense = true,
                        DateTime = bill3.DateTime,
                        Type = "Оплата",
                        Target = session.TechExpenseFlow.Name,
                        Total = ToMoney(-5600),
                        Balance = ToMoney(26000, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = true,
                        DateTime = bill1.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = ToMoney(-350),
                        Balance = ToMoney(31600, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = income.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = ToMoney(1450),
                        Balance = ToMoney(17450, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = transfer2DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = ToMoney(2000),
                        Balance = ToMoney(16000, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = true,
                        DateTime = transfer1DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = ToMoney(-1000),
                        Balance = ToMoney(14000, false)
                    },
                });

                transactions = await transationsQueries.GetLastTransactions(ids.CashAccountId);
                transactions.ShouldBeEquivalentTo(new[]
                    {
                        new TransactionViewModel
                        {
                            IsExpense = true,
                            DateTime = bill2.DateTime,
                            Type = "Оплата",
                            Target = session.TechExpenseFlow.Name,
                            Total = ToMoney(-15000),
                            Balance = ToMoney(14000, false)
                        },
                        new TransactionViewModel
                        {
                            IsExpense = true,
                            DateTime = transfer2DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = ToMoney(-2000),
                            Balance = ToMoney(29000, false)
                        },
                        new TransactionViewModel
                        {
                            IsExpense = false,
                            DateTime = transfer1DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = ToMoney(1000),
                            Balance = ToMoney(31000, false)
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
                        IsExpense = true,
                        DateTime = bill3.DateTime,
                        Type = "Оплата",
                        Target = session.TechExpenseFlow.Name,
                        Total = ToMoney(-5600),
                        Balance = ToMoney(21434, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = true,
                        DateTime = bill1.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = ToMoney(-350),
                        Balance = ToMoney(27034, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = income.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = ToMoney(1450),
                        Balance = ToMoney(17450, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = transfer2DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = ToMoney(2000),
                        Balance = ToMoney(16000, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = true,
                        DateTime = transfer1DateTime,
                        Type = "Перевод",
                        Target = session.CashAccount.Name,
                        Total = ToMoney(-1000),
                        Balance = ToMoney(14000, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = oldIncome.DateTime,
                        Type = "Поступление",
                        Target = session.SalaryIncome.Name,
                        Total = ToMoney(3456),
                        Balance = ToMoney(20906, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = false,
                        DateTime = veryOldIncome.DateTime,
                        Type = "Поступление",
                        Target = session.GiftsIncome.Name,
                        Total = ToMoney(6478),
                        Balance = ToMoney(27384, false)
                    },
                    new TransactionViewModel
                    {
                        IsExpense = true,
                        DateTime = oldBill.DateTime,
                        Type = "Оплата",
                        Target = session.FoodExpenseFlow.Name,
                        Total = ToMoney(-200),
                        Balance = ToMoney(21234, false)
                    }, 
                });

                var last = await transationsQueries.GetLastTransactions(ids.CashAccountId);
                last.ShouldBeEquivalentTo(new[]
                    {
                        new TransactionViewModel
                        {
                            IsExpense = true,
                            DateTime = bill2.DateTime,
                            Type = "Оплата",
                            Target = session.TechExpenseFlow.Name,
                            Total = ToMoney(-15000),
                            Balance = ToMoney(14000, false)
                        },
                        new TransactionViewModel
                        {
                            IsExpense = true,
                            DateTime = transfer2DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = ToMoney(-2000),
                            Balance = ToMoney(29000, false)
                        },
                        new TransactionViewModel
                        {
                            IsExpense = false,
                            DateTime = transfer1DateTime,
                            Type = "Перевод",
                            Target = session.DebitCardAccount.Name,
                            Total = ToMoney(1000),
                            Balance = ToMoney(31000, false)
                        },
                        new TransactionViewModel
                        {
                            IsExpense = true,
                            DateTime = veryOldBill.DateTime,
                            Type = "Оплата",
                            Target = session.FoodExpenseFlow.Name,
                            Total = ToMoney(-333),
                            Balance = ToMoney(13667, false)
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

        private static string ToMoney(int value, bool addPlusSign = true)
        {
            var result = Convert.ToDecimal(value).ToMoney();
            return value > 0 && addPlusSign ? $"+{result}" : result;
        }
    }
}