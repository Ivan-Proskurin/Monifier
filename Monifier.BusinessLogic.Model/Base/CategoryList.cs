using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Base
{
    public class CategoryList
    {
        public List<CategoryModel> List { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}