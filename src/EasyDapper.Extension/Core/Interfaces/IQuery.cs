using EasyDapper.Extension.Core.SetQ;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.Core.Interfaces
{
    public interface IQuery<T>
    {
        T Get();

        T FirstOrDefault();

        Task<T> GetAsync();

        Task<T> FirstOrDefaultAsync();

        List<T> ToList();

        Task<List<T>> ToListAsync();

        PageList<T> PageList(int pageIndex, int pageSize);

    }
}
