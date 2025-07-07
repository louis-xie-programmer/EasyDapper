using EasyDapper.Extension.Core.SetC;
using EasyDapper.Extension.Extension;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;

namespace EasyDapper.Extension.MsSql.Extension
{
    public static class CommandExtension
    {
        public static int BulkCopy<T>(this Command<T> command, IEnumerable<T> list, int timeOut)
        {
            if (command.DbCon.State == ConnectionState.Closed)
            {
                command.DbCon.Open();
            }
            var dt = list.ToDataTable();

            // 修复：检查所有 DateTime 列，确保值在 SQL Server 支持的范围内
            foreach (DataColumn col in dt.Columns)
            {
                if (col.DataType == typeof(DateTime))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[col] != DBNull.Value)
                        {
                            DateTime value = (DateTime)row[col];
                            if (value < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ||
                                value > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                            {
                                // 你可以选择抛出异常，或将其设置为 SqlDateTime.MinValue
                                row[col] = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                            }
                        }
                    }
                }
            }

            using (var sqlbulkcopy = new SqlBulkCopy((SqlConnection)command.DbCon))
            {
                sqlbulkcopy.DestinationTableName = dt.TableName;
                sqlbulkcopy.BulkCopyTimeout = timeOut;

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                }
                sqlbulkcopy.WriteToServer(dt);
            }
            return dt.Rows.Count;
        }
    }
}
