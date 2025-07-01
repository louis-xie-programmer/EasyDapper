using System.Data;
using System.Linq.Expressions;

namespace EasyDapper.Extension.Core.Interfaces
{
    public interface ICommandSet<T>
    {
        ICommand<T> Where(Expression<Func<T, bool>> predicate);

        IInsert<T> IfNotExists(Expression<Func<T, bool>> predicate);

        void BatchInsert(IEnumerable<T> entities, int timeout);

        void BatchInsert(DataTable dt, int timeout);
    }
}
