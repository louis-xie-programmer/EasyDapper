namespace EasyDapper.Extension.Core.Interfaces
{
    public interface IInsert<T>
    {
        int Insert(T entity);

        Task<int> InsertAsync(T entity);
    }
}
