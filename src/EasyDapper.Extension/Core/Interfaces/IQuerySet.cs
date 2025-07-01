using System.Linq.Expressions;
using EasyDapper.Extension.Core.SetQ;

namespace EasyDapper.Extension.Core.Interfaces
{
    public interface IQuerySet<T>
    {
        QuerySet<T> Where(Expression<Func<T, bool>> predicate);

        QuerySet<T> WithNoLock();

        QuerySet<T> Take(int count);
    }
}
