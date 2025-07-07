using System.Linq.Expressions;
using Dapper;
using EasyDapper.Extension.Core.SetQ;

namespace EasyDapper.Extension.MySql.Extension
{
    public static class QueryExtension
    {
        /// <summary>
        /// 提交修改查出来的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="updator"></param>
        /// <returns></returns>
        public static List<T> UpdateSelect<T>(this Query<T> query, Expression<Func<T, T>> updator)
        {
            var sqlProvider = query.SqlProvider;
            var dbCon = query.DbCon;
            var dbTransaction = query.DbTransaction;
            sqlProvider.FormatUpdateSelect(updator);

            return dbCon.Query<T>(sqlProvider.SqlString, sqlProvider.Params, dbTransaction).ToList();
        }

        /// <summary>
        /// 提交修改查出来的集合--异步方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="updator"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> UpdateSelectAsync<T>(this Query<T> query, Expression<Func<T, T>> updator)
        {
            var sqlProvider = query.SqlProvider;
            var dbCon = query.DbCon;
            var dbTransaction = query.DbTransaction;
            sqlProvider.FormatUpdateSelect(updator);

            return await dbCon.QueryAsync<T>(sqlProvider.SqlString, sqlProvider.Params, dbTransaction);
        }
    }
}
