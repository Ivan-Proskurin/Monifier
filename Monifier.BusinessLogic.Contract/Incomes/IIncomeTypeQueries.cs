using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Incomes;

namespace Monifier.BusinessLogic.Contract.Incomes
{
    public interface IIncomeTypeQueries : ICommonModelQueries<IncomeTypeModel>
    {
        Task<List<IncomeTypeModel>> GetFiltered(DateTime dateFrom, DateTime dateTo);
    }
}