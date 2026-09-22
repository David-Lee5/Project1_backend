using RepairSystem.Data.Repository.IRepository;

namespace RepairSystem.Data.Repository.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<string, object> _repositories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<string, object>();
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var key = typeof(T).Name;
        if (!_repositories.ContainsKey(key))
        {
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(T));
            var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
            _repositories.Add(key, repositoryInstance!);
        }
        return (IGenericRepository<T>)_repositories[key];
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
