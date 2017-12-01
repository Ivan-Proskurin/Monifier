using System;
using System.Globalization;

namespace Monifier.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly CultureInfo Culture = new CultureInfo("ru-ru");

        public static DateTime StartOfTheMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static DateTime EndOfTheMonth(this DateTime value)
        {
            return StartOfTheMonth(value).AddMonths(1).AddSeconds(-1);
        }

        public static DateTime StartOfTheWeek(this DateTime value)
        {
            var monday = value;
            while (monday.DayOfWeek != DayOfWeek.Monday)
                monday = monday.AddDays(-1);
            return new DateTime(monday.Year, monday.Month, monday.Day);
        }

        public static DateTime EndOfTheWeek(this DateTime value)
        {
            var sunday = value;
            while (sunday.DayOfWeek != DayOfWeek.Sunday)
                sunday = sunday.AddDays(1);
            return new DateTime(sunday.Year, sunday.Month, sunday.Day).AddDays(1).AddSeconds(-1);
        }

        public static string ToStandardString(this DateTime value, bool toMinutes = true)
        {
            return value.ToString(toMinutes ? "yyyy.MM.dd HH:mm" : "yyyy.MM.dd HH:00");
        }

        public static string ToStandardDateStr(this DateTime value)
        {
            return value.ToString("yyyy.MM.dd");
        }

        public static DateTime ParseDtFromStandardString(this string value)
        {
            return DateTime.ParseExact(value, "yyyy.MM.dd HH:mm", Culture);
        }
    }
}