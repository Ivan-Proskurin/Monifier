using System;
using Monifier.Common.Extensions;

namespace Monifier.Web.Models.Validation
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
            catch (FormatException exc)
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
            catch (FormatException exc)
            {
                return new ModelValidationResult(name, "Дата/время некорректна");
            }
        }
    }
}