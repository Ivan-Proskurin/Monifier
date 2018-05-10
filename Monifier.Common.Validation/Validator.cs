using System;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Model.Extensions;

namespace Monifier.Common.Validation
{
    public static class Validator
    {
        public static ModelValidationResult ValidateMoney(this string value, string name)
        {
            try
            {
                value?.ParseMoneyInvariant();
                return null;
            }
            catch (FormatException)
            {
                return new ModelValidationResult(name, "Введите корректное число");
            }
        }

        public static ModelValidationResult ValidateDateTime(this string value, string name)
        {
            try
            {
                value?.ParseDtFromStandardString();
                return null;
            }
            catch (FormatException)
            {
                return new ModelValidationResult(name, "Дата/время некорректна");
            }
        }

        public static ModelValidationResult ValidateAccountType(this string value, string name)
        {
            return (!value?.ExistsAccountType() ?? false) ? new ModelValidationResult(name, "Нет такого типа счета") : null;
        }
    }
}