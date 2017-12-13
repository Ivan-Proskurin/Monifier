using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public ExpensesQueries(IUnitOfWork unitOfWork, IUserSettings userSettings)
        {
            _unitOfWork = unitOfWork;
            _userSettings = userSettings;
        }

        private class BillGoodsGroup
        {
            public List<int> BillIds { get; set; }
            public decimal Sum { get; set; }
            public DateTime DateTime { get; set; }
            public List<BillGood> Goods { get; set; }
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

        private static string GetLaconicGoodString(ICollection<string> goods)
        {
            var firstTwo = goods.Take(2);
            var result = string.Join(", ", firstTwo);
            if (goods.Count > 2)
            {
                result += $" ... ({goods.Count})";
            }
            return result;
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
            DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            var expenseQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var goodsGroupQuery = expenseQueries.Query
                .Where(x => x.DateTime >= dateFrom && x.DateTime < dateTo)
                .OrderBy(x => x.DateTime)
                .GroupBy(x => x.DateTime.Date)
                .Select(x => new BillGoodsGroup
                {
                    BillIds = x.Select(v => v.Id).ToList(),
                    Sum = x.Sum(v => v.SumPrice),
                    DateTime = x.Key
                });

            var totalCount = goodsGroupQuery.Count();
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
            DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            var expenseQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var goodsGroupQuery = expenseQueries.Query
                .Where(x => x.DateTime >= dateFrom && x.DateTime < dateTo)
                .OrderBy(x => x.DateTime)
                .GroupBy(x => new { x.DateTime.Year, x.DateTime.Month })
                .Select(x => new BillGoodsGroup
                {
                    BillIds = x.Select(v => v.Id).ToList(),
                    Sum = x.Sum(v => v.SumPrice),
                    DateTime = new DateTime(x.Key.Year, x.Key.Month, 1)
                });

            var totalCount = goodsGroupQuery.Count();
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

        private async Task GroupAndSortBillsGoodsAsync(List<BillGoodsGroup> goodsGroups)
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
            DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs, bool byMonth)
        {
            // отбираем нужные счета и группируем их по дате
            var goodsGroupsWithPagination = byMonth 
                ? await GetGoodsGroupsByMonthAsync(dateFrom, dateTo, paginationArgs)
                : await GetGoodsGroupsByDayAsync(dateFrom, dateTo, paginationArgs);
            var goodsGroups = goodsGroupsWithPagination.GoodsGrops;

            // группируем все товары и категории и сортируем их в порядке уменьшение общей стоимости в каждой группе
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
                    
                    Interval = byMonth 
                        ? $"{x.DateTime.StartOfTheMonth().ToStandardDateStr()} - {x.DateTime.EndOfTheMonth().ToStandardDateStr()}" 
                        : x.DateTime.ToStandardDateStr(),
                    
                    Caption = byMonth ? x.DateTime.GetMonthName() : x.DateTime.GetWeekDayName().Capitalize(),
                    Goods = GetLaconicGoodString(x.Goods.Select(g => g.Name).ToList()),
                    IsDangerExpense = x.Sum >= _userSettings.DangerExpense(byMonth)
                }).ToList(),
                Pagination = goodsGroupsWithPagination.Pagination
            };
            
            // считаем тоталы
            expenses.Totals = new TotalsInfoModel
            {
                Caption = $"Итого за период с {dateFrom.ToStandardString()} по {dateTo.ToStandardString()}",
                Total = await _unitOfWork.GetQueryRepository<ExpenseBill>().Query
                    .Where(x => x.DateTime >= dateFrom && x.DateTime < dateTo).SumAsync(x => x.SumPrice)
            };
            
            return expenses;
        }
        
        public async Task<ExpensesListModel> GetExpensesByDay(DateTime dateFrom, DateTime dateTo,
            PaginationArgs paginationArgs)
        {
            return await GetExpenses(dateFrom, dateTo, paginationArgs, byMonth: false);
        }

        public async Task<ExpensesListModel> GetExpensesByMonth(DateTime dateFrom, DateTime dateTo,
            PaginationArgs paginationArgs)
        {
            return await GetExpenses(dateFrom, dateTo, paginationArgs, byMonth: true);
        }

        public async Task<ExpensesListModel> GetExpensesForDay(DateTime day)
        {
            // отбираем нужные счета          
            var expensesQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var today = day.Date;
            var tomorrow = today.AddDays(1);
            var goodsGroups = await expensesQueries.Query
                .Where(x => x.DateTime >= today && x.DateTime < tomorrow)
                .OrderBy(x => x.DateTime)
                .Select(x => new BillGoodsGroup
                {
                    BillIds = new List<int> { x.Id },
                    DateTime = x.DateTime,
                    Sum = x.SumPrice
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
                    Interval = string.Empty, 
                    Caption = x.DateTime.ToStandardString(),
                    Goods = GetLaconicGoodString(x.Goods.Select(g => g.Name).ToList()),
                    IsDangerExpense = false
                }).ToList(),
                Pagination = null
            };
            
            // считаем тоталы
            expenses.Totals = new TotalsInfoModel
            {
                Caption = $"Итого за день с {today.ToStandardDateStr()} по {tomorrow.ToStandardDateStr()}",
                Total = await _unitOfWork.GetQueryRepository<ExpenseBill>().Query
                    .Where(x => x.DateTime >= today && x.DateTime < tomorrow).SumAsync(x => x.SumPrice)
            };

            return expenses;
        }
    }
}