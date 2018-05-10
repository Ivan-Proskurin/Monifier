using System;
using FluentAssertions;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Extensions;
using Xunit;

namespace Monifier.DataAccess.Model.Tests
{
    public class AccountTypeHelperTests
    {
        [Fact]
        public void GetAllHumanNames_ReturnsHumanNames()
        {
            AccountTypeHelper.GetAllHumanNames().Should().BeEquivalentTo(new[]
            {
                "Наличные",
                "Дебетовая карта",
                "Кредитная карта"
            });
        }

        [Fact]
        public void GetAllHumanNames_OrderIsImportant()
        {
            var names = AccountTypeHelper.GetAllHumanNames();
            names.Count.Should().Be(3);
            names[0].Should().BeEquivalentTo("Наличные");
            names[1].Should().BeEquivalentTo("Дебетовая карта");
            names[2].Should().BeEquivalentTo("Кредитная карта");
        }

        [Fact]
        public void GetHumanName_AllTypes_ReturnNames()
        {
            AccountType.Cash.GetNumanName().Should().BeEquivalentTo("Наличные");
            AccountType.DebitCard.GetNumanName().Should().BeEquivalentTo("Дебетовая карта");
            AccountType.CreditCard.GetNumanName().Should().BeEquivalentTo("Кредитная карта");
        }

        [Fact]
        public void GetHumanName_WrongType_ReturnsNull()
        {
            ((AccountType) 100).GetNumanName().Should().BeNull();
        }

        [Fact]
        public void ParseFromHumanName_AllNames_ReturnsAccountTypes()
        {
            "Наличные".ParseFromHumanName().Should().BeEquivalentTo(AccountType.Cash);
            "Дебетовая карта".ParseFromHumanName().Should().BeEquivalentTo(AccountType.DebitCard);
            "Кредитная карта".ParseFromHumanName().Should().BeEquivalentTo(AccountType.CreditCard);
        }

        [Fact]
        public void ParseFromHumanName_WrongName_ThrowsException()
        {
            Assert.Throws<FormatException>(() => "Неизвестный тип".ParseFromHumanName());
        }

        [Fact]
        public void ExistsAccountType_AllCorrectNames_ReturnsAllTrue()
        {
            "Наличные".ExistsAccountType().Should().BeTrue();
            "Дебетовая карта".ExistsAccountType().Should().BeTrue();
            "Кредитная карта".ExistsAccountType().Should().BeTrue();
        }

        [Fact]
        public void ExistsAccountType_WrongName_ReturnsFalse()
        {
            "Неизвестный тип".ExistsAccountType().Should().BeFalse();
            "Неизвестный тип 2".ExistsAccountType().Should().BeFalse();
            "Debit card".ExistsAccountType().Should().BeFalse();
        }
    }
}
