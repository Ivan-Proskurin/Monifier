using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Base
{
    public class ProductList
    {
        public List<ProductModel> Products { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}