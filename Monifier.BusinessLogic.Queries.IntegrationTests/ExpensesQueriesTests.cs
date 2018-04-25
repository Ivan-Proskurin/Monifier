﻿using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.BusinessLogic.Queries.Settings;
using Monifier.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Agregation;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class ExpensesQueriesTests : QueryTestBase
    {
        [Fact]
        public async void GetExpensesByFlowsTests()
        {
            var date1 = new DateTime(2018, 01, 15);
            var date2 = new DateTime(2018, 01, 31);
            var date3 = new DateTime(2018, 02, 01);

            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date2, session.Bread, 46);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId, date3, session.Tv, 10000);

                var queries = new ExpensesQueries(session.UnitOfWork, new UserSettings(), session.UserSession);
                var report = await queries.GetExpensesByFlows(new DateTime(2017, 12, 30), date3, new PaginationArgs
                {
                    PageNumber = 1,
                    ItemsPerPage = 10,
                });
                report.Items.Count.ShouldBeEquivalentTo(2);
                report.Items[0].ShouldBeEquivalentTo(new ExpenseByFlowsItemModel
                {
                    FlowId = ids.TechExpenseFlowId,
                    Flow = "Техника",
                    LastBill = date2,
                    Total = 20000
                });
                report.Items[1].ShouldBeEquivalentTo(new ExpenseByFlowsItemModel
                {
                    FlowId = ids.FoodExpenseFlowId,
                    Flow = "Продукты питания",
                    LastBill = date2,
                    Total = 391.45m
                });
                report.Totals.Total.ShouldBeEquivalentTo(20391.45m);
            }
        }

        [Fact]
        public async void GetExpensesByDaysTest()
        {
            var date1 = new DateTime(2018, 01, 15);
            var date2 = new DateTime(2018, 01, 31);
            var date3 = new DateTime(2018, 02, 01);

            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var bill1Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                var bill2Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date2, session.Bread, 46);
                var bill3Id = await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId, date3, session.Tv, 10000);

                var queries = new ExpensesQueries(session.UnitOfWork, new UserSettings(), session.UserSession);
                var report = await queries.GetExpensesByDays(new ExpensesFilter
                {
                    DateFrom = new DateTime(2017, 12, 30),
                    DateTo = date3
                }, 
                new PaginationArgs
                {
                    PageNumber = 1,
                    ItemsPerPage = 10,
                });

                report.Expenses.ShouldBeEquivalentTo(new[] 
                {
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill1Id},
                        Sum = 345.45m,
                        Caption = "Понедельник",
                        DateFrom = "2018.01.15 00:00",
                        DateTo = "2018.01.15 23:59",
                        Period = "2018.01.15",
                        Goods = "Продукты, Мясо",
                        IsDangerExpense = false,
                    },
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill2Id, bill3Id},
                        Sum = 20046,
                        Caption = "Среда",
                        DateFrom = "2018.01.31 00:00",
                        DateTo = "2018.01.31 23:59",
                        Period = "2018.01.31",
                        Goods = "Техника, Телевизор ... (4)",
                        IsDangerExpense = true,
                    },
                });
                report.Totals.ShouldBeEquivalentTo(new TotalsInfoModel
                {
                    Caption = "Итого за период с 2017.12.30 по 2018.02.01",
                    Total = 20391.45m
                });
            }
        }

        [Fact]
        public async void GetExpensesByMonthTest()
        {
            var date1 = new DateTime(2018, 01, 15);
            var date2 = new DateTime(2018, 02, 01);
            var date3 = new DateTime(2018, 02, 05);

            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var bill1Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                var bill2Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date2, session.Bread, 46);
                var bill3Id = await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
                var bill4Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId, date3, session.Tv, 10000);

                var queries = new ExpensesQueries(session.UnitOfWork, new UserSettings(), session.UserSession);
                var report = await queries.GetExpensesByMonth(new ExpensesFilter
                {
                    DateFrom = new DateTime(2017, 12, 30),
                    DateTo = date3
                },
                new PaginationArgs
                {
                    PageNumber = 1,
                    ItemsPerPage = 10,
                });

                report.Expenses.ShouldBeEquivalentTo(new[]
                {
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill1Id},
                        Sum = 345.45m,
                        Caption = "Январь",
                        DateFrom = "2018.01.01 00:00",
                        DateTo = "2018.01.31 23:59",
                        Period = "2018.01.01 - 2018.01.31",
                        Goods = "Продукты, Мясо",
                        IsDangerExpense = false,
                    },
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill2Id, bill3Id, bill4Id},
                        Sum = 30046,
                        Caption = "Февраль",
                        DateFrom = "2018.02.01 00:00",
                        DateTo = "2018.02.28 23:59",
                        Period = "2018.02.01 - 2018.02.28",
                        Goods = "Техника, Телевизор ... (4)",
                        IsDangerExpense = false,
                    },
                });
                report.Totals.ShouldBeEquivalentTo(new TotalsInfoModel
                {
                    Caption = "Итого за период с 2017.12.01 по 2018.02.28",
                    Total = 30391.45m
                });
            }
        }

        [Fact]
        public async void GetExpensesForDayTest()
        {
            var date1 = new DateTime(2018, 01, 15);
            var date2 = new DateTime(2018, 01, 31);
            var date3 = new DateTime(2018, 02, 01);

            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                var bill2Id = await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date2, session.Bread, 46);
                var bill3Id = await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId, date3, session.Tv, 10000);

                var queries = new ExpensesQueries(session.UnitOfWork, new UserSettings(), session.UserSession);
                var report = await queries.GetExpensesForDay(new ExpensesFilter
                {
                    DateFrom = date2,
                    DateTo = date3
                });

                report.Expenses.ShouldBeEquivalentTo(new[]
                {
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill2Id},
                        Sum = 46,
                        Caption = "2018.01.31 00:00",
                        DateFrom = "2018.01.31 00:00",
                        DateTo = "2018.02.01 00:00",
                        Period = "Продукты питания",
                        Goods = "Продукты, Хлеб",
                        IsDangerExpense = false,
                    },
                    new ExpensesListItemModel
                    {
                        BillIds = new List<int> {bill3Id},
                        Sum = 20000,
                        Caption = "2018.01.31 00:00",
                        DateFrom = "2018.01.31 00:00",
                        DateTo = "2018.02.01 00:00",
                        Period = "Техника",
                        Goods = "Техника, Телевизор",
                        IsDangerExpense = false,
                    },
                });
                report.Totals.ShouldBeEquivalentTo(new TotalsInfoModel
                {
                    Caption = "Итого за день с 2018.01.31 по 2018.02.01",
                    Total = 20046
                });
            }
        }
    }
}
