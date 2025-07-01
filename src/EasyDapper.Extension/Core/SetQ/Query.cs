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
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider) : base(dbCon, sqlProvider)
        {
        }
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(dbCon, sqlProvider, dbTransaction)
        {
        }

        public T Get()
        {
            SqlProvider.FormatGet<T>();

            return DbCon.QueryFirstOrDefault<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<T> GetAsync()
        {
            SqlProvider.FormatGet<T>();

            return await DbCon.QueryFirstOrDefaultAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public T FirstOrDefault()
        {
            return this.Get();
        }

        public Task<T> FirstOrDefaultAsync()
        {
            return this.GetAsync();
        }

        public List<T> ToList()
        {
            SqlProvider.FormatToList<T>();

            return DbCon.Query<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction).ToList();
        }

        public async Task<List<T>> ToListAsync()
        {
            SqlProvider.FormatToList<T>();
            var asyncResult = await DbCon.QueryAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
            return asyncResult.ToList();
        }

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
