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
                "��������",
                "��������� �����",
                "��������� �����"
            });
        }

        [Fact]
        public void GetAllHumanNames_OrderIsImportant()
        {
            var names = AccountTypeHelper.GetAllHumanNames();
            names.Count.Should().Be(3);
            names[0].Should().BeEquivalentTo("��������");
            names[1].Should().BeEquivalentTo("��������� �����");
            names[2].Should().BeEquivalentTo("��������� �����");
        }

        [Fact]
        public void GetHumanName_AllTypes_ReturnNames()
        {
            AccountType.Cash.GetNumanName().Should().BeEquivalentTo("��������");
            AccountType.DebitCard.GetNumanName().Should().BeEquivalentTo("��������� �����");
            AccountType.CreditCard.GetNumanName().Should().BeEquivalentTo("��������� �����");
        }

        [Fact]
        public void GetHumanName_WrongType_ReturnsNull()
        {
            ((AccountType) 100).GetNumanName().Should().BeNull();
        }

        [Fact]
        public void ParseFromHumanName_AllNames_ReturnsAccountTypes()
        {
            "��������".ParseFromHumanName().Should().BeEquivalentTo(AccountType.Cash);
            "��������� �����".ParseFromHumanName().Should().BeEquivalentTo(AccountType.DebitCard);
            "��������� �����".ParseFromHumanName().Should().BeEquivalentTo(AccountType.CreditCard);
        }

        [Fact]
        public void ParseFromHumanName_WrongName_ThrowsException()
        {
            Assert.Throws<FormatException>(() => "����������� ���".ParseFromHumanName());
        }

        [Fact]
        public void ExistsAccountType_AllCorrectNames_ReturnsAllTrue()
        {
            "��������".ExistsAccountType().Should().BeTrue();
            "��������� �����".ExistsAccountType().Should().BeTrue();
            "��������� �����".ExistsAccountType().Should().BeTrue();
        }

        [Fact]
        public void ExistsAccountType_WrongName_ReturnsFalse()
        {
            "����������� ���".ExistsAccountType().Should().BeFalse();
            "����������� ��� 2".ExistsAccountType().Should().BeFalse();
            "Debit card".ExistsAccountType().Should().BeFalse();
        }
    }
}
