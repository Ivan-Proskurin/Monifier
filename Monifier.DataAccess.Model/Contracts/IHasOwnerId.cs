namespace Monifier.DataAccess.Model.Contracts
{
    public interface IHasOwnerId
    {
        int? OwnerId { get; set; }
    }
}