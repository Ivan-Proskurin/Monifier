using System;
using System.Collections.Generic;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.Model.Extensions
{
    public static class AccountTypeHelper
    {
        public static List<string> GetAllHumanNames()
        {
            return new List<string>
            {
                CashName,
                DebitCardName,
                CreditCardName
            };
        }

        public static string GetNumanName(this AccountType accountType)
        {
            return TypeToNames.GetOrDefault(accountType);
        }

        public static AccountType ParseFromHumanName(this string name)
        {
            var accountType = NameToTypes.GetOrDefault(name);
            if (accountType == null)
                throw new FormatException("Unknown account type human name");
            return accountType.Value;
        }

        public static bool ExistsAccountType(this string humanName)
        {
            return NameToTypes.GetOrDefault(humanName) != null;
        }

        public static readonly string CashName = "Наличные";
        public static readonly string DebitCardName = "Дебетовая карта";
        public static readonly string CreditCardName = "Кредитная карта";

        private static readonly Dictionary<AccountType, string> TypeToNames = new Dictionary<AccountType, string>
        {
            {AccountType.Cash, CashName},
            {AccountType.DebitCard, DebitCardName},
            {AccountType.CreditCard, CreditCardName},
        };

        private static readonly Dictionary<string, AccountType?> NameToTypes = new Dictionary<string, AccountType?>
        {
            {CashName, AccountType.Cash},
            {DebitCardName, AccountType.DebitCard},
            {CreditCardName, AccountType.CreditCard}
        };
    }
}