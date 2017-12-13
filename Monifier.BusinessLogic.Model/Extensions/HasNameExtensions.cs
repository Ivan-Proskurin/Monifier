using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.BusinessLogic.Model.Extensions
{
    public static class HasNameExtensions
    {
        private static CultureInfo Culture = new CultureInfo("ru-RU");
        
        public static T FindByName<T>(this IEnumerable<T> list, string name) where T : IHasName
        {
            return list.FirstOrDefault(x => string.Compare(x.Name, name, true, Culture) == 0);
        }
    }
}