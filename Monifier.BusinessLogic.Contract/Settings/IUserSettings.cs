namespace Monifier.BusinessLogic.Contract.Settings
{
    public interface IUserSettings
    {
        int ItemsPerPage { get; set; }
        decimal DangerExpense(bool monthPeriod);
    }
}