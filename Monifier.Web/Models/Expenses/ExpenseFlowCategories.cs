using System;
using System.Collections.Generic;
using System.Linq;

namespace Monifier.Web.Models.Expenses
{
    public static class ExpenseFlowCategoriesMapper
    {
        public static string ToCsvString(this IEnumerable<int> ids)
        {
            return string.Join(',', ids.Select(x => x.ToString()));
        }

        public static List<int> ToIntList(this string ids)
        {
            var _ids = new List<int>();
            if (string.IsNullOrEmpty(ids)) return _ids;
            _ids.AddRange(ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));
            return _ids;
        }
    }
}