using FluentAssertions;
using Xunit;

namespace Monifier.Common.Validation.Tests
{
    public class ValidatorTests
    {
        [Fact]
        public void ValidateMoney_CorrectString_ReturnsNull()
        {
            "244.65".ValidateMoney(MoneyField).Should().BeNull();
        }

        [Fact]
        public void ValidateMoney_CorrectStringWithComma_ReturnsNull()
        {
            "35,54".ValidateMoney(MoneyField).Should().BeNull();
        }

        [Fact]
        public void ValidateMoney_IncorrectString_ReturnsValidationError()
        {
            "43.45$".ValidateMoney(MoneyField).Should().BeEquivalentTo(MoneyValidationError);
        }

        [Fact]
        public void ValidateMoney_PassNull_ReturnsNull()
        {
            default(string).ValidateMoney(MoneyField).Should().BeNull();
        }

        [Fact]
        public void ValidateDateTime_CorrectString_ReturnsNull()
        {
            "2018.05.09 23:45".ValidateDateTime(DateTimeField).Should().BeNull();
        }

        [Fact]
        public void ValidateDateTime_CorrectWithoutTimePart_ReturnsValidationError()
        {
            "2018.05.09".ValidateDateTime(DateTimeField).Should().BeEquivalentTo(DateTimeValidationError);
        }

        [Fact]
        public void ValidateDateTime_IncorrectFormat_ReturnsValidationError()
        {
            "05.09.2018 12:44".ValidateDateTime(DateTimeField).Should().BeEquivalentTo(DateTimeValidationError);
        }

        [Fact]
        public void ValidateDateTime_PassNull_ReturnsNull()
        {
            default(string).ValidateDateTime(DateTimeField).Should().BeNull();
        }

        [Fact]
        public void ValidateAccountType_PassCorrectName_ReturnsNull()
        {
            "Дебетовая карта".ValidateAccountType(AccountTypeField).Should().BeNull();
        }

        [Fact]
        public void ValidateAccountType_PassIncorrectName_ReturnsValidationError()
        {
            "Карта кредитная".ValidateAccountType(AccountTypeField).Should().BeEquivalentTo(AccountTypeValidationError);
        }

        [Fact]
        public void ValidateAccountType_PassNull_ReturnsNull()
        {
            default(string).ValidateAccountType(AccountTypeField).Should().BeNull();
        }

        public static readonly string MoneyField = "Money";
        public static readonly string MoneyValidationMessage = "Введите корректное число";
        public static readonly ModelValidationResult MoneyValidationError = new ModelValidationResult(MoneyField, MoneyValidationMessage);

        public static readonly string DateTimeField = "DateTime";
        public static readonly string DateTimeValidationMessage = "Дата/время некорректна";
        public static readonly ModelValidationResult DateTimeValidationError = new ModelValidationResult(DateTimeField, DateTimeValidationMessage);

        public static readonly string AccountTypeField = "AccountType";
        public static readonly string AccountTypeValidationMessage = "Нет такого типа счета";
        public static readonly ModelValidationResult AccountTypeValidationError = new ModelValidationResult(AccountTypeField, AccountTypeValidationMessage);
    }
}
