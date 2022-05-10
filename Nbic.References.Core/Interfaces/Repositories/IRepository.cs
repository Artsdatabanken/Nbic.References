namespace Nbic.References.Core.Interfaces.Repositories;

public interface IRepository<T>
{
    Task<int> CountAsync();
}