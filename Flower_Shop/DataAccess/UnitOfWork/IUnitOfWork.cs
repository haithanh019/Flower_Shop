namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
    }
}
