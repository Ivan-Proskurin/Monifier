using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Monifier.Common.Extensions
{
    public static class StringExtensions
    {
        private static CultureInfo Culture = new CultureInfo("ru-RU");
        
        public static string Capitalize(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Substring(0, 1).ToUpper(Culture) + value.Substring(1);
        }
        
        public static string GetLaconicString(this ICollection<string> values)
        {
            var firstTwo = values.Take(2);
            var result = string.Join(", ", firstTwo);
            if (values.Count > 2)
            {
                result += $" ... ({values.Count})";
            }
            return result;
        }
    }
}