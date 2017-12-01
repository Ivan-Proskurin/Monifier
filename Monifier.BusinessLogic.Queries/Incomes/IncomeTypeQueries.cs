﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeTypeQueries : IIncomeTypeQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        public IncomeTypeQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<IncomeTypeModel>> GetAll(bool includeDeleted = false)
        {
            var typesRepo = _unitOfWork.GetQueryRepository<IncomeType>();
            var items = await typesRepo.Query
                .Select(x => new IncomeTypeModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
            return items;
        }

        public async Task<IncomeTypeModel> GetById(int id)
        {
            var type = await _unitOfWork.GetQueryRepository<IncomeType>().GetById(id);
            if (type == null) return null;
            return new IncomeTypeModel
            {
                Id = type.Id,
                Name = type.Name
            };
        }

        public async Task<IncomeTypeModel> GetByName(string name, bool includeDeleted = false)
        {
            var typesRepo = _unitOfWork.GetNamedModelQueryRepository<IncomeType>();
            var type = await typesRepo.GetByName(name);
            if (type == null) return null;
            return new IncomeTypeModel
            {
                Id = type.Id,
                Name = type.Name
            };
        }

        public async Task<List<IncomeTypeModel>> GetFiltered(DateTime dateFrom, DateTime dateTo)
        {
            var typesRepo = _unitOfWork.GetQueryRepository<IncomeType>();
            var itemsRepo = _unitOfWork.GetQueryRepository<IncomeItem>();
            var query = from t in typesRepo.Query
                        join i in itemsRepo.Query on t.Id equals i.IncomeTypeId into typeItemsJoin
                        from ti in typeItemsJoin.DefaultIfEmpty()
                        where ti.DateTime >= dateFrom && ti.DateTime < dateTo
                        group new { ti.IncomeType, ti.Total } by ti.IncomeTypeId into tiGroup
                        select new IncomeTypeModel
                        {
                            Id = tiGroup.Key,
                            Name = tiGroup.FirstOrDefault().IncomeType.Name,
                            SumTotal = tiGroup.Sum(x => x.Total)
                        };
            var items = await query.ToListAsync();
            return items;
        }
    }
}