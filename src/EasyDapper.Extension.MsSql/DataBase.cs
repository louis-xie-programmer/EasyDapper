﻿using System.Data;
using EasyDapper.Extension.Core.SetC;
using EasyDapper.Extension.Core.SetQ;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.MsSql
{
    public static class DataBase
    {
        public static QuerySet<T> QuerySet<T>(this IDbConnection sqlConnection)
        {
            return new QuerySet<T>(sqlConnection, new MsSqlProvider());
        }

        public static QuerySet<T> QuerySet<T>(this IDbConnection sqlConnection, IDbTransaction dbTransaction)
        {
            return new QuerySet<T>(sqlConnection, new MsSqlProvider(), dbTransaction);
        }
        
        public static CommandSet<T> CommandSet<T>(this IDbConnection sqlConnection, IDbTransaction dbTransaction)
        {
            return new CommandSet<T>(sqlConnection, new MsSqlProvider(), dbTransaction);
        }

        public static CommandSet<T> CommandSet<T>(this IDbConnection sqlConnection)
        {
            return new CommandSet<T>(sqlConnection, new MsSqlProvider());
        }

        public static void Transaction(this IDbConnection sqlConnection, Action<TransContext> action,
            Action<System.Exception> exAction = null)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                sqlConnection.Open();

            var transaction = sqlConnection.BeginTransaction();
            try
            {
                action(new TransContext(sqlConnection, transaction, new MsSqlProvider()));
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                if (exAction != null)
                    exAction(ex);
                else
                {
                    transaction.Rollback();
                }
            }
            finally
            {
                sqlConnection.Close();
            }
        }
    }
}
