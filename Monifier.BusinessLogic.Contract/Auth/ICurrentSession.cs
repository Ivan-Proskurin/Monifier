namespace Monifier.BusinessLogic.Contract.Auth
{
    public interface ICurrentSession
    {
        bool IsAuthenticated { get; }
        int UserId { get; }
    }
}