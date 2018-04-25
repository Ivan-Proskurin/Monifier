using System;
using System.Collections.Generic;
using System.Linq;

namespace Monifier.Web.Extensions
{
    public static class IdsExtension
    {
        public static string ToCsvString(this IEnumerable<int> ids)
        {
            return string.Join(',', ids.Select(x => x.ToString()));
        }

        public static List<int> ToIntList(this string ids)
        {
            var idList = new List<int>();
            if (string.IsNullOrEmpty(ids)) return idList;
            idList.AddRange(ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));
            return idList;
        }
    }
}