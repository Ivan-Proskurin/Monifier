using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomesQueries : IIncomesQueries
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;

        public IncomesQueries(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        private class IncomeGroup
        {
            public List<int> ItemIds { get; set; }
            public DateTime Month { get; set; }
            public decimal Sum { get; set; }
            public List<string> Types { get; set; }
        }

        public async Task<IncomesListModel> GetIncomesByMonth(DateTime dateFrom, DateTime dateTo,
            PaginationArgs paginationArgs)
        {
            dateFrom = dateFrom.StartOfTheMonth();
            dateTo = dateTo.EndOfTheMonth();
            
            var incomesQueries = _unitOfWork.GetQueryRepository<IncomeItem>();
            var typesQueries = _unitOfWork.GetQueryRepository<IncomeType>();
            var ownerId = _currentSession.UserId;

            var incomesQuery =
                from income in incomesQueries.Query
                join itype in typesQueries.Query on income.IncomeTypeId equals itype.Id
                where income.OwnerId == ownerId && income.DateTime >= dateFrom && income.DateTime < dateTo
                orderby income.Total descending 
                group new {income, itype} by new {income.DateTime.Year, income.DateTime.Month}
                into incomeGroups
                select new IncomeGroup
                {
                    ItemIds = incomeGroups.Select(x => x.income.Id).ToList(),
                    Sum = incomeGroups.Sum(x => x.income.Total),
                    Types = incomeGroups.Select(x => x.itype.Name).Distinct().ToList(),
                    Month = new DateTime(incomeGroups.Key.Year, incomeGroups.Key.Month, 1)
                };

            var incomesCount = await incomesQuery.CountAsync();
            var pagination = new PaginationInfo(paginationArgs, incomesCount);

            var incomes = await incomesQuery
                .Skip(pagination.Skipped)
                .Take(pagination.Taken)
                .ToListAsync();

            var totals = await incomesQueries.Query
                .Where(x => x.OwnerId == ownerId && x.DateTime >= dateFrom && x.DateTime < dateTo)
                .SumAsync(x => x.Total);

            return new IncomesListModel
            {
                Incomes = incomes.Select(x => new IncomesListItemModel
                {
                    ItemIds = x.ItemIds,
                    Caption = x.Month.GetMonthName(),
                    DateFrom = x.Month.ToStandardString(),
                    DateTo = x.Month.EndOfTheMonth().ToStandardString(),
                    Interval = $"{x.Month.ToStandardDateStr()} - {x.Month.EndOfTheMonth().ToStandardDateStr()}",
                    Sum = x.Sum,
                    Types = x.Types.GetLaconicString()
                }).ToList(),
                Pagination = pagination,
                Totals = new TotalsInfoModel
                {
                    Caption = $"Итого доход за период с {dateFrom.ToStandardDateStr()} по {dateTo.ToStandardDateStr()}",
                    Total = totals
                }
            };
        }

        public async Task<IncomesListModel> GetIncomesList(DateTime dateFrom, DateTime dateTo,
            PaginationArgs paginationArgs)
        {
            var incomesQueries = _unitOfWork.GetQueryRepository<IncomeItem>();
            var typeQueries = _unitOfWork.GetQueryRepository<IncomeType>();
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var ownerId = _currentSession.UserId;

            var incomesQuery =
                from income in incomesQueries.Query
                join itype in typeQueries.Query on income.IncomeTypeId equals itype.Id
                join account in accountQueries.Query on income.AccountId equals account.Id
                where income.OwnerId == ownerId && income.DateTime >= dateFrom && income.DateTime < dateTo
                orderby income.DateTime
                select new
                {
                    Income = income, 
                    IncomeType = itype.Name,
                    Account = account.Name
                };

            var incomesCount = await incomesQuery.CountAsync();
            var pagination = new PaginationInfo(paginationArgs, incomesCount);

            var incomes = await incomesQuery
                .Skip(pagination.Skipped)
                .Take(pagination.Taken)
                .ToListAsync();

            var totals = await incomesQueries.Query
                .Where(x => x.OwnerId == ownerId && x.DateTime >= dateFrom && x.DateTime < dateTo)
                .SumAsync(x => x.Total);

            var result = new IncomesListModel
            {
                Incomes = new List<IncomesListItemModel>(),
                Pagination = pagination,
                Totals = new TotalsInfoModel
                {
                    Caption = $"Итого доход за период с {dateFrom.ToStandardString()} по {dateTo.ToStandardString()}",
                    Total = totals
                }
            };

            foreach (var item in incomes)
            {
                var strDateTime = item.Income.DateTime.ToStandardString();
                result.Incomes.Add(new IncomesListItemModel
                {
                    ItemIds = new List<int> { item.Income.Id },
                    Caption = item.Account,
                    DateFrom = strDateTime,
                    DateTo = strDateTime,
                    Interval = strDateTime,
                    Sum = item.Income.Total,
                    Types = item.IncomeType
                });
            }

            return result;
        }
    }
}