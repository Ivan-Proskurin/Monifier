namespace Monifier.DataAccess.Contract
{
    public interface ICommandRepository<in T> where T : class
    {
        void Create(T model);
        void Update(T model);
        void Delete(T model);
        void Detach(T model);
    }
}
