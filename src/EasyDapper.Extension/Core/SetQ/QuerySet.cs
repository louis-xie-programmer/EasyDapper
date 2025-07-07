using System.Data;
using System.Linq.Expressions;
using EasyDapper.Extension.Core.Interfaces;
using EasyDapper.Extension.Helper;

namespace EasyDapper.Extension.Core.SetQ
{
    /// <summary>
    /// 查询集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuerySet<T> : Aggregation<T>, IQuerySet<T>
    {
        /// <summary>
        /// 构造函数，初始化 QuerySet，指定数据库连接和 SQL 提供者。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        public QuerySet(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        /// <summary>
        /// 构造函数，初始化 QuerySet，指定数据库连接、SQL 提供者和事务。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="dbTransaction">数据库事务</param>
        public QuerySet(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        /// <summary>
        /// 内部构造函数，支持自定义表类型和事务。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="tableType">表类型</param>
        /// <param name="dbTransaction">数据库事务</param>
        internal QuerySet(IDbConnection conn, SqlProvider sqlProvider, Type tableType, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {
            SqlProvider.SetContext.TableType = tableType;
        }

        /// <summary>
        /// 指定查询返回的最大行数（Top N），支持链式调用。
        /// </summary>
        /// <param name="count">返回的最大行数</param>
        /// <returns>QuerySet&lt;T&gt; 实例，支持链式调用</returns>
        public QuerySet<T> Take(int count)
        {
            SqlProvider.SetContext.TopNum = count;
            return this;
        }

        /// <summary>
        /// 添加 Where 条件，支持链式调用，多个条件自动 And。
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns>QuerySet&lt;T&gt; 实例，支持链式调用</returns>
        public QuerySet<T> Where(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.WhereExpression = SqlProvider.SetContext.WhereExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.WhereExpression).And(predicate);

            return this;
        }

        /// <summary>
        /// 查询时添加 WITH(NOLOCK) 选项，支持链式调用。
        /// </summary>
        /// <returns>QuerySet&lt;T&gt; 实例，支持链式调用</returns>
        public QuerySet<T> WithNoLock()
        {
            SqlProvider.SetContext.NoLock = true;
            return this;
        }
    }
}
