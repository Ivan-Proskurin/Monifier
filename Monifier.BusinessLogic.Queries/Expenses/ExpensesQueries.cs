using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesQueries : IExpensesQueries
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSettings _userSettings;
        private readonly ICurrentSession _currentSession;

        public ExpensesQueries(IUnitOfWork unitOfWork, IUserSettings userSettings, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _userSettings = userSettings;
            _currentSession = currentSession;
        }

        private class BillGoodsGroup
        {
            public List<int> BillIds { get; set; }
            public decimal Sum { get; set; }
            public DateTime DateTime { get; set; }
            public List<BillGood> Goods { get; set; }
        }

        private class FlowBillGoodsGroup : BillGoodsGroup
        {
            public string FlowName { get; set; }
        }

        private class BillGoodsGroupWithPagination
        {
            public List<BillGoodsGroup> GoodsGrops { get; set; }
            public PaginationInfo Pagination { get; set; }
        }
        
        private class BillGood
        {
            public decimal Sum { get; set; }
            public string Name { get; set; }
            public bool IsCategory { get; set; }
        }

        private class BillGoodComparison : IComparer<BillGood>
        {
            public int Compare(BillGood x, BillGood y)
            {
                var result = -x.Sum.CompareTo(y.Sum);
                if (result == 0) result = -x.IsCategory.CompareTo(y.IsCategory);
                return result;
            }
        }

        private async Task<BillGoodsGroupWithPagination> GetGoodsGroupsByDayAsync(
            int? flowId, DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            var expenseQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var ownerId = _currentSession.UserId;
            var goodsGroupQuery = expenseQueries.Query
                .Where(x => x.OwnerId == ownerId && (flowId == null || x.ExpenseFlowId == flowId)
                                                 && x.DateTime >= dateFrom && x.DateTime < dateTo)
                .OrderBy(x => x.DateTime)
                .GroupBy(x => x.DateTime.Date)
                .Select(x => new BillGoodsGroup
                {
                    BillIds = x.Select(v => v.Id).ToList(),
                    Sum = x.Sum(v => v.SumPrice),
                    DateTime = x.Key
                });

            var totalCount = await goodsGroupQuery.CountAsync();
            var pagination = new PaginationInfo(paginationArgs, totalCount);
            var goodsGroups = await goodsGroupQuery
                .Skip(pagination.Skipped).Take(pagination.Taken)
                .ToListAsync();
            
            return new BillGoodsGroupWithPagination
            {
                GoodsGrops = goodsGroups,
                Pagination = pagination
            };
        }
        
        private async Task<BillGoodsGroupWithPagination> GetGoodsGroupsByMonthAsync(
            int? flowId, DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            var expenseQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var ownerId = _currentSession.UserId;
            var goodsGroupQuery = expenseQueries.Query
                .Where(x => x.OwnerId == ownerId && (flowId == null || x.ExpenseFlowId == flowId) 
                                                 && x.DateTime >= dateFrom && x.DateTime < dateTo)
                .OrderBy(x => x.DateTime)
                .GroupBy(x => new { x.DateTime.Year, x.DateTime.Month })
                .Select(x => new BillGoodsGroup
                {
                    BillIds = x.Select(v => v.Id).ToList(),
                    Sum = x.Sum(v => v.SumPrice),
                    DateTime = new DateTime(x.Key.Year, x.Key.Month, 1)
                });

            var totalCount = await goodsGroupQuery.CountAsync();
            var pagination = new PaginationInfo(paginationArgs, totalCount);
            var goodsGroups = await goodsGroupQuery
                .Skip(pagination.Skipped).Take(pagination.Taken)
                .ToListAsync();
            
            return new BillGoodsGroupWithPagination
            {
                GoodsGrops = goodsGroups,
                Pagination = pagination
            };
        }

        private async Task GroupAndSortBillsGoodsAsync(IReadOnlyCollection<BillGoodsGroup> goodsGroups)
        {
            // ищем категории, принадлежащие отобранным счетам и группируем их
            var billItemsQuery = _unitOfWork.GetQueryRepository<ExpenseItem>();
            var categoryQuery = _unitOfWork.GetQueryRepository<Category>();
            foreach (var goodGroup in goodsGroups)
            {
                var billIds = goodGroup.BillIds;
                var billCatsQuery = 
                    from item in billItemsQuery.Query
                    join cat in categoryQuery.Query on item.CategoryId equals cat.Id
                    where billIds.Contains(item.BillId)
                    group item by cat.Name into itemGroup
                    select new BillGood
                    {
                        Sum = itemGroup.Sum(v => v.Price),
                        Name = itemGroup.Key,
                        IsCategory = true
                    };
                goodGroup.Goods = await billCatsQuery.ToListAsync();
            }
            
            // ищем продукты, принадлежащие отобранным счетам и группируем их
            var productQueries = _unitOfWork.GetQueryRepository<Product>();
            foreach (var goodGroup in goodsGroups)
            {
                var billIds = goodGroup.BillIds;
                var billProdQuery =
                    from item in billItemsQuery.Query
                    join prod in productQueries.Query on item.ProductId equals prod.Id
                    where billIds.Contains(item.BillId)
                    group item by prod.Name into itemGroup
                    select new BillGood
                    {
                        Sum = itemGroup.Sum(v => v.Price),
                        Name = itemGroup.Key,
                        IsCategory = false
                    };
                goodGroup.Goods.AddRange(billProdQuery);
            }
            
            // по каждой группе отобранных товаров, сортируем их так,
            // чтобы первыми шли самые дорогие, если цена совпадает, то сначала категории, а потом продукты
            var comparer = new BillGoodComparison();
            foreach (var goodGroup in goodsGroups)
            {
                goodGroup.Goods.Sort(comparer);
            }

        }

        private async Task<ExpensesListModel> GetExpenses(
            ExpensesFilter filter, PaginationArgs paginationArgs, bool byMonth)
        {
            var flowId = filter.FlowId;
            var dateFrom = filter.DateFrom;
            var dateTo = filter.DateTo;
            var userId = _currentSession.UserId;

            // отбираем нужные счета и группируем их по дате
            var goodsGroupsWithPagination = byMonth 
                ? await GetGoodsGroupsByMonthAsync(flowId, dateFrom, dateTo, paginationArgs)
                : await GetGoodsGroupsByDayAsync(flowId, dateFrom, dateTo, paginationArgs);
            var goodsGroups = goodsGroupsWithPagination.GoodsGrops;

            // группируем все товары и категории и сортируем их в порядке уменьшения общей стоимости в каждой группе
            await GroupAndSortBillsGoodsAsync(goodsGroups);
            
            // финальное преобразование из внутренних моделей
            var expenses = new ExpensesListModel
            {
                Expenses = goodsGroups.Select(x => new ExpensesListItemModel
                {
                    BillIds = x.BillIds,
                    Sum = x.Sum,

                    DateFrom = byMonth
                        ? x.DateTime.StartOfTheMonth().ToStandardString()
                        : x.DateTime.Date.ToStandardString(),

                    DateTo = byMonth
                        ? x.DateTime.EndOfTheMonth().ToStandardString()
                        : x.DateTime.AddDays(1).AddMinutes(-1).ToStandardString(),

                    Period = byMonth
                        ? $"{x.DateTime.StartOfTheMonth().ToStandardDateStr()} - {x.DateTime.EndOfTheMonth().ToStandardDateStr()}"
                        : x.DateTime.ToStandardDateStr(),

                    Caption = byMonth ? x.DateTime.GetMonthName() : x.DateTime.GetWeekDayName().Capitalize(),
                    Goods = x.Goods.Select(g => g.Name).ToList().GetLaconicString(),
                    IsDangerExpense = x.Sum >= _userSettings.DangerExpense(byMonth)
                }).ToList(),
                Pagination = goodsGroupsWithPagination.Pagination,
                
                // считаем тоталы
                Totals = new TotalsInfoModel
                {
                    Caption = $"Итого за период с {dateFrom.ToStandardString()} по {dateTo.ToStandardString()}",
                    Total = await _unitOfWork.GetQueryRepository<ExpenseBill>().Query
                        .Where(x => x.OwnerId == userId && (flowId == null || x.ExpenseFlowId == flowId) 
                                                        && x.DateTime >= dateFrom && x.DateTime < dateTo)
                        .SumAsync(x => x.SumPrice)
                }
            };

            return expenses;
        }
        
        public async Task<ExpensesListModel> GetExpensesByDay(ExpensesFilter filter, PaginationArgs paginationArgs)
        {
            return await GetExpenses(filter, paginationArgs, false);
        }

        public async Task<ExpensesListModel> GetExpensesByMonth(ExpensesFilter filter, PaginationArgs paginationArgs)
        {
            filter.DateFrom = filter.DateFrom.StartOfTheMonth();
            filter.DateTo = filter.DateTo.EndOfTheMonth();
            return await GetExpenses(filter, paginationArgs, true);
        }

        public async Task<ExpensesListModel> GetExpensesForDay(ExpensesFilter filter)
        {
            // отбираем нужные счета          
            var expensesQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();

            var flowId = filter.FlowId;
            var today = filter.DateFrom.Date;
            var tomorrow = today.AddDays(1);
            var ownerId = _currentSession.UserId;

            var goodsGroups = await expensesQueries.Query
                .Where(x => x.OwnerId == ownerId && (flowId == null || x.ExpenseFlowId == flowId)
                                                 && x.DateTime >= today && x.DateTime < tomorrow)
                .OrderBy(x => x.DateTime)
                .Select(x => new FlowBillGoodsGroup
                {
                    BillIds = new List<int> { x.Id },
                    DateTime = x.DateTime,
                    Sum = x.SumPrice,
                    FlowName = x.ExpenseFlow.Name
                }).ToListAsync();

            // группируем категории и товары с сортировкой
            await GroupAndSortBillsGoodsAsync(goodsGroups);
            
            // финальное преобразование из внутренних моделей
            var expenses = new ExpensesListModel
            {
                Expenses = goodsGroups.Select(x => new ExpensesListItemModel
                {
                    BillIds = x.BillIds,
                    Sum = x.Sum,
                    DateFrom = today.ToStandardString(),
                    DateTo = tomorrow.ToStandardString(),
                    Period = x.FlowName,
                    Caption = x.DateTime.ToStandardString(),
                    Goods = x.Goods.Select(g => g.Name).ToList().GetLaconicString(),
                    IsDangerExpense = false
                }).ToList(),
                Pagination = null,

                // считаем тоталы
                Totals = new TotalsInfoModel
                {
                    Caption = $"Итого за день с {today.ToStandardDateStr()} по {tomorrow.ToStandardDateStr()}",
                    Total = await _unitOfWork.GetQueryRepository<ExpenseBill>().Query
                        .Where(x => x.OwnerId == ownerId && (flowId == null || x.ExpenseFlowId == flowId) 
                                                         && x.DateTime >= today && x.DateTime < tomorrow)
                        .SumAsync(x => x.SumPrice)
                }
            };

            return expenses;
        }

        public async Task<ExpensesByFlowsModel> GetExpensesByFlows(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            // отбираем нужные счета          
            var expensesQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();

            var ownerId = _currentSession.UserId;

            var billByFlowsQuery = expensesQueries.Query
                .Where(x => x.OwnerId == ownerId && x.DateTime >= dateFrom && x.DateTime < dateTo)
                .GroupBy(x => x.ExpenseFlowId)
                .Select(x => new ExpenseByFlowsItemModel
                {
                    FlowId = x.Key,
                    Total = x.Sum(e => e.SumPrice),
                    Flow = x.First().ExpenseFlow.Name,
                    LastBill = x.Max(e => e.DateTime),
                    IsDangerExpense = false
                })
                .OrderByDescending(x => x.Total);

            var totalCount = await billByFlowsQuery.CountAsync();
            var pagination = new PaginationInfo(paginationArgs, totalCount);

            // финальное преобразование из внутренних моделей
            var expenses = new ExpensesByFlowsModel
            {
                Items = await billByFlowsQuery
                    .Skip(pagination.Skipped)
                    .Take(pagination.Taken)
                    .ToListAsync(),

                // считаем тоталы
                Totals = new TotalsInfoModel
                {
                    Caption = $"Итого за период с {dateFrom.ToStandardString()} по {dateTo.ToStandardString()}",
                    Total = await expensesQueries.Query
                        .Where(x => x.OwnerId == ownerId && x.DateTime >= dateFrom && x.DateTime < dateTo)
                        .SumAsync(x => x.SumPrice)
                },

                Pagination = pagination
            };

            return expenses;
        }
    }
}