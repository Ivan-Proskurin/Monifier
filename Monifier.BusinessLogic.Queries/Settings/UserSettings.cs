using Monifier.BusinessLogic.Contract.Settings;

namespace Monifier.BusinessLogic.Queries.Settings
{
    public class UserSettings : IUserSettings
    {
        public int ItemsPerPage { get; set; } = 2;

        public decimal DangerExpense(bool monthPeriod)
        {
            return monthPeriod ? 50000m : 6000m;
        }
    }
}