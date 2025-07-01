using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using EasyDapper.Extension.Extension;
using Microsoft.Data.SqlClient;

namespace EasyDapper.Extension.MsSql.Helper
{
    internal static class SqlHelper
    {
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="list">源数据</param>
        public static void BulkCopy<T>(IDbConnection conn, IEnumerable<T> list, int timeout = 240)
        {
            var dt = list.ToDataTable();

            if (conn.State == ConnectionState.Closed)
                conn.Open();

            using (var sqlbulkcopy = new SqlBulkCopy((SqlConnection)conn))
            {
                sqlbulkcopy.DestinationTableName = dt.TableName;
                sqlbulkcopy.BulkCopyTimeout = timeout;

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                }
                sqlbulkcopy.WriteToServer(dt);
            }
        }

        /// <summary>
        /// 批量插入（DataTable）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt">源数据</param>
        public static void BulkCopy(IDbConnection conn, DataTable dt, int timeout = 240)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            using (var sqlbulkcopy = new SqlBulkCopy((SqlConnection)conn))
            {
                sqlbulkcopy.DestinationTableName = dt.TableName;
                sqlbulkcopy.BulkCopyTimeout = timeout;

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                }
                sqlbulkcopy.WriteToServer(dt);
            }
        }

    }
}
