﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface ICategoriesQueries : ICommonModelQueries<CategoryModel>
    {
        Task<List<ProductModel>> GetProductsByCategoryName(string categoryName, bool includeDeleted = false);
        Task<CategoryList> GetList(PaginationArgs args);
        Task<List<CategoryModel>> GetFlowCategories(int flowId);
        Task<CategoryModel> GetFlowCategoryByName(int flowId, string category);
        Task<CategoryModel> GetFlowCategoryByProductName(int flowId, string product);
    }
}
