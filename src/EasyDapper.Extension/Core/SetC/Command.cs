using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using EasyDapper.Extension.Core.Interfaces;
using EasyDapper.Extension.Enum;

namespace EasyDapper.Extension.Core.SetC
{
    /// <summary>
    /// 指令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Command<T> : AbstractSet, ICommand<T>, IInsert<T>
    {
        /// <summary>
        /// 构造函数，初始化 Command，指定 SQL 提供者、数据库连接和事务。
        /// </summary>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="dbCon">数据库连接</param>
        /// <param name="dbTransaction">数据库事务</param>
        protected Command(SqlProvider sqlProvider, IDbConnection dbCon, IDbTransaction dbTransaction) : base(dbCon, sqlProvider, dbTransaction)
        {
        }

        /// <summary>
        /// 构造函数，初始化 Command，指定 SQL 提供者和数据库连接。
        /// </summary>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="dbCon">数据库连接</param>
        protected Command(SqlProvider sqlProvider, IDbConnection dbCon) : base(dbCon, sqlProvider)
        {
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <returns>任务</returns>
        public async Task CreateTable()
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatCreateTable<T>();

            // 使用 DbCon.ExecuteAsync 执行生成的 SQL 语句
            await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task DropTable()
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatDropTable<T>();
            // 使用 DbCon.ExecuteAsync 执行生成的 SQL 语句
            await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据实体更新数据。
        /// </summary>
        /// <param name="entity">要更新的实体</param>
        /// <returns>受影响的行数</returns>
        public int Update(T entity)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatUpdate(entity);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据实体异步更新数据。
        /// </summary>
        /// <param name="entity">要更新的实体</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync(T entity)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatUpdate(entity);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据表达式更新数据。
        /// </summary>
        /// <param name="updateExpression">更新表达式</param>
        /// <returns>受影响的行数</returns>
        public int Update(Expression<Func<T, T>> updateExpression)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatUpdate(updateExpression);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据表达式异步更新数据。
        /// </summary>
        /// <param name="updateExpression">更新表达式</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatUpdate(updateExpression);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 删除数据（无条件删除）。
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int Delete()
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatDelete();

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 异步删除数据（无条件删除）。
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteAsync()
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatDelete();

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据条件异步删除数据，需带 where。
        /// </summary>
        /// <param name="DeleteCondition">删除条件</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteAsync(string DeleteCondition)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatDelete(DeleteCondition);
            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 根据条件删除数据，需带 where。
        /// </summary>
        /// <param name="DeleteCondition">删除条件</param>
        /// <returns>受影响的行数</returns>
        public int Delete(string DeleteCondition)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatDelete(DeleteCondition);
            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 插入单条数据。
        /// </summary>
        /// <param name="entity">要插入的实体</param>
        /// <returns>受影响的行数</returns>
        public int Insert(T entity)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatInsert(entity);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 异步插入单条数据。
        /// </summary>
        /// <param name="entity">要插入的实体</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> InsertAsync(T entity)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatInsert(entity);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 插入单条数据并返回实体。
        /// </summary>
        /// <param name="entity">要插入的实体</param>
        /// <param name="returnTypeEnum">返回类型枚举</param>
        /// <returns>插入后的实体</returns>
        public T Insert(T entity, ReturnTypeEnum returnTypeEnum)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatInsert(entity);

            return DbCon.QueryFirst<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 异步插入单条数据并返回实体。
        /// </summary>
        /// <param name="entity">要插入的实体</param>
        /// <param name="returnTypeEnum">返回类型枚举</param>
        /// <returns>插入后的实体</returns>
        public async Task<T> InsertAsync(T entity, ReturnTypeEnum returnTypeEnum)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.FormatInsert(entity);

            return await DbCon.QueryFirstAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 批量异步插入数据。
        /// </summary>
        /// <param name="entities">要插入的实体集合</param>
        /// <param name="returnTypeEnum">返回类型枚举</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> InsertAsyncList(IEnumerable<T> entities)
        {
            if (DbCon.State == ConnectionState.Closed)
            {
                DbCon.Open();
            }

            SqlProvider.ExcuteBulkCopy(entities);
            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }
    }
}
