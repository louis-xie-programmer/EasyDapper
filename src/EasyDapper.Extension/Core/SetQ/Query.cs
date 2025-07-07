using System.Data;
using Dapper;
using EasyDapper.Extension.Core.Interfaces;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.Core.SetQ
{
    /// <summary>
    /// 查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Query<T> : AbstractSet, IQuery<T>
    {
        /// <summary>
        /// 构造函数，初始化 Query，指定数据库连接和 SQL 提供者。
        /// </summary>
        /// <param name="dbCon">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider) : base(dbCon, sqlProvider)
        {
        }

        /// <summary>
        /// 构造函数，初始化 Query，指定数据库连接、SQL 提供者和事务。
        /// </summary>
        /// <param name="dbCon">数据库连接</param>
        /// <param name="sqlProvider">SQL 提供者</param>
        /// <param name="dbTransaction">数据库事务</param>
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(dbCon, sqlProvider, dbTransaction)
        {
        }

        public bool ExistTable()
        {
            SqlProvider.FormatExistTable<T>();
            var result = DbCon.ExecuteScalar<int>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
            return result > 0;
        }

        /// <summary>
        /// 获取单条数据（如无数据返回默认值）。
        /// </summary>
        /// <returns>实体对象或默认值</returns>
        public T Get()
        {
            SqlProvider.FormatGet<T>();

            return DbCon.QueryFirstOrDefault<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 异步获取单条数据（如无数据返回默认值）。
        /// </summary>
        /// <returns>实体对象或默认值</returns>
        public async Task<T> GetAsync()
        {
            SqlProvider.FormatGet<T>();

            return await DbCon.QueryFirstOrDefaultAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        /// <summary>
        /// 获取单条数据（如无数据返回默认值，等价于 Get）。
        /// </summary>
        /// <returns>实体对象或默认值</returns>
        public T FirstOrDefault()
        {
            return this.Get();
        }

        /// <summary>
        /// 异步获取单条数据（如无数据返回默认值，等价于 GetAsync）。
        /// </summary>
        /// <returns>实体对象或默认值</returns>
        public Task<T> FirstOrDefaultAsync()
        {
            return this.GetAsync();
        }

        /// <summary>
        /// 查询所有数据并返回列表。
        /// </summary>
        /// <returns>实体列表</returns>
        public List<T> ToList()
        {
            SqlProvider.FormatToList<T>();

            return DbCon.Query<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction).ToList();
        }

        /// <summary>
        /// 异步查询所有数据并返回列表。
        /// </summary>
        /// <returns>实体列表</returns>
        public async Task<List<T>> ToListAsync()
        {
            SqlProvider.FormatToList<T>();
            var asyncResult = await DbCon.QueryAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
            return asyncResult.ToList();
        }

        /// <summary>
        /// 分页查询，返回分页结果。
        /// </summary>
        /// <param name="pageIndex">页码（从 1 开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>分页结果 PageList&lt;T&gt;</returns>
        public PageList<T> PageList(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);

            using (var queryResult = DbCon.QueryMultiple(SqlProvider.SqlString, SqlProvider.Params, DbTransaction))
            {
                var pageTotal = queryResult.ReadFirst<int>();

                var itemList = queryResult.Read<T>();

                return new PageList<T>(pageIndex, pageSize, pageTotal, itemList.ToList());
            }
        }
    }
}
