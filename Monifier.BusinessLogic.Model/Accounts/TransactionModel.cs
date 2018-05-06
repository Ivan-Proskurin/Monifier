﻿using System;
using Monifier.DataAccess.Model.Accounts;

namespace Monifier.BusinessLogic.Model.Accounts
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public int? OwnerId { get; set; }
        public DateTime DateTime { get; set; }
        public int InitiatorId { get; set; }
        public int? ParticipantId { get; set; }
        public int? BillId { get; set; }
        public int? IncomeId { get; set; }
        public decimal Total { get; set; }
        public bool IsDeleted { get; set; }
    }

    public static class TransactionExtensions
    {
        public static TransactionModel ToModel(this Transaction transaction)
        {
            return new TransactionModel
            {
                Id = transaction.Id,
                OwnerId = transaction.OwnerId,
                DateTime = transaction.DateTime,
                InitiatorId = transaction.InitiatorId,
                ParticipantId = transaction.ParticipantId,
                BillId = transaction.BillId,
                IncomeId = transaction.IncomeId,
                IsDeleted = transaction.IsDeleted,
                Total = transaction.Total
            };
        }
    }
}