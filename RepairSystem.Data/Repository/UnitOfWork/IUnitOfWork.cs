using RepairSystem.Data.Repository.IRepository;

namespace RepairSystem.Data.Repository.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task SaveAsync();
}
