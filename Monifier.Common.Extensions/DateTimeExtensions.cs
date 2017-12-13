using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Monifier.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly CultureInfo Culture = new CultureInfo("ru-ru");

        public static DateTime StartOfTheYear(this DateTime value)
        {
            return new DateTime(value.Year, 1, 1);
        }

        public static DateTime EndOfTheYear(this DateTime value)
        {
            return new DateTime(value.Year, 12, 31, 23, 59, 00);
        }

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

        public static DateTime ToMinutes(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        public static string GetWeekDayName(this DateTime value)
        {
            return value.ToString("dddd", Culture);
        }

        public static string GetMonthName(this DateTime value)
        {
            return value.ToString("MMMM", Culture);
        }
    }
}