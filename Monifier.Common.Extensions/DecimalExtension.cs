using System.Globalization;

namespace Monifier.Common.Extensions
{
    public static class DecimalExtension
    {
        private static readonly CultureInfo Culture = new CultureInfo("ru-ru");

        public static decimal ParseMoneyInvariant(this string value)
        {
            return decimal.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        public static string ToStandardString(this decimal value)
        {
            return value.ToString(Culture);
        }

        public static string ToMoney(this decimal value)
        {
            return value.ToString("c", Culture);
        }
    }
}
