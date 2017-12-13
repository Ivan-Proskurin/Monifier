using System.Globalization;

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
    }
}