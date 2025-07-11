﻿using System.Data;
using System.Linq.Expressions;
using Dapper;
using EasyDapper.Extension.Core.Interfaces;

namespace EasyDapper.Extension.Core.SetQ
{
    /// <summary>
    /// 聚合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Aggregation<T> : Order<T>, IAggregation<T>
    {
        protected Aggregation(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {

        }

        protected Aggregation(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {

        }

        /// <inheritdoc />
        public int Count()
        {
            SqlProvider.FormatCount();

            return DbCon.QuerySingle<int>(SqlProvider.SqlString, SqlProvider.Params);
        }

        public virtual Aggregation<T> GroupBy<TResult>(Expression<Func<T, TResult>> group)
        {
            if (group != null)
            {
                if (SqlProvider.SetContext.GroupExpression == null)
                    SqlProvider.SetContext.GroupExpression = new List<LambdaExpression>();
                SqlProvider.SetContext.GroupExpression.Add(group);
            }
            return this;
        }

        /// <inheritdoc />
        public TResult Sum<TResult>(Expression<Func<T, TResult>> sumExpression)
        {
            SqlProvider.FormatSum<TResult>(sumExpression);

            return DbCon.QuerySingle<TResult>(SqlProvider.SqlString, SqlProvider.Params);
        }

        /// <inheritdoc />
        public bool Exists()
        {
            SqlProvider.FormatExists();

            return DbCon.QuerySingle<int>(SqlProvider.SqlString, SqlProvider.Params) == 1;
        }
    }
}
