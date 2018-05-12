using System;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.DataAccess.Model.Transactions
{
    public class Transaction : IHasId, IHasOwnerId
    {
        public int Id { get; set; }

        public int? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public DateTime DateTime { get; set; }

        public int InitiatorId { get; set; }
        [ForeignKey("InitiatorId")]
        public Account Initiator { get; set; }

        public int? ParticipantId { get; set; }
        [ForeignKey("ParticipantId")]
        public Account Participant { get; set; }

        public int? BillId { get; set; }
        [ForeignKey("BillId")]
        public ExpenseBill Bill { get; set; }

        public int? IncomeId { get; set; }
        [ForeignKey("IncomeId")]
        public IncomeItem Income { get; set; }

        public decimal? Balance { get; set; }

        public decimal Total { get; set; }

        public bool IsDeleted { get; set; }
    }
}