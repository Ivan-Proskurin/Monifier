using System.Collections.Generic;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class EntityIdSet
    {
        public int SalaryIncomeId { get; set; }
        public int GiftsIncomeId { get; set; }

        public int FoodCategoryId { get; set; }
        public int TechCategoryId { get; set; }

        public int BreadId { get; set; }
        public int MeatId { get; set; }
        public int TvId { get; set; }

        public int FoodExpenseFlowId { get; set; }
        public int TechExpenseFlowId { get; set; }

        public int DebitCardAccountId { get; set; }
        public int CashAccountId { get; set; }

        public List<int> IncomeIds { get; set; }
    }
}