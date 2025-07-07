using System.Data;
using System.Linq.Expressions;
using EasyDapper.Extension.Core.Interfaces;
using EasyDapper.Extension.Helper;

namespace EasyDapper.Extension.Core.SetC
{
    /// <summary>
    /// 指令集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandSet<T> : Command<T>, ICommandSet<T>
    {
        /// <summary>
        /// 构造函数，初始化 CommandSet，指定数据库连接和 SQL 提供者。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        public CommandSet(IDbConnection conn, SqlProvider sqlProvider) : base(sqlProvider, conn)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        /// <summary>
        /// 构造函数，初始化 CommandSet，指定数据库连接、SQL 提供者和事务。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="dbTransaction">数据库事务</param>
        public CommandSet(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(sqlProvider, conn, dbTransaction)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        /// <summary>
        /// 内部构造函数，支持自定义表类型和条件表达式。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="tableType">表类型</param>
        /// <param name="whereExpression">条件表达式</param>
        internal CommandSet(IDbConnection conn, SqlProvider sqlProvider, Type tableType, LambdaExpression whereExpression) : base(sqlProvider, conn)
        {
            SqlProvider.SetContext.TableType = tableType;
            SqlProvider.SetContext.WhereExpression = whereExpression;
        }

        /// <summary>
        /// 添加 Where 条件，支持链式调用，多个条件自动 And。
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns>ICommand&lt;T&gt; 实例，支持链式调用</returns>
        public ICommand<T> Where(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.WhereExpression = SqlProvider.SetContext.WhereExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.WhereExpression).And(predicate);

            return this;
        }

        /// <summary>
        /// 指定插入前的唯一性条件（如存在则不插入），支持链式调用。
        /// </summary>
        /// <param name="predicate">唯一性条件表达式</param>
        /// <returns>IInsert&lt;T&gt; 实例，支持链式调用</returns>
        public IInsert<T> IfNotExists(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.IfNotExistsExpression = SqlProvider.SetContext.IfNotExistsExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.IfNotExistsExpression).And(predicate);

            return this;
        }
    }
}
