﻿using System.Data;
using System.Linq.Expressions;
using System.Text;
using Dapper;
using EasyDapper.Extension.Extension;
using EasyDapper.Extension.Helper;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension
{
    public abstract class SqlProvider
    {
        internal SetContext SetContext { get; set; }

        protected SqlProvider()
        {
            Params = new DynamicParameters();
            SetContext = new SetContext();
        }

        protected abstract ProviderOption ProviderOption { get; set; }

        public string SqlString { get; set; }

        public DynamicParameters Params { get; set; }

        public abstract SqlProvider FormatGet<T>();

        public abstract SqlProvider FormatToList<T>();

        public abstract SqlProvider FormatToPageList<T>(int pageIndex, int pageSize);

        public abstract SqlProvider FormatCount();

        public abstract SqlProvider FormatExists();

        public abstract SqlProvider FormatDelete();

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <param name="DeleteCondition">删除条件</param>
        /// <returns></returns>
        public abstract SqlProvider FormatDelete(string DeleteCondition);

        public abstract SqlProvider FormatInsert<T>(T entity);

        public abstract SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression);

        public abstract SqlProvider FormatUpdate<T>(T entity);

        public abstract SqlProvider FormatSum<T>(LambdaExpression lambdaExpression);

        public abstract SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator);

        public abstract SqlProvider ExcuteBulkCopy<T>(IDbConnection conn, IEnumerable<T> list, int timeout);

        public abstract SqlProvider ExcuteBulkCopy(IDbConnection conn, DataTable dt, int timeout);

        protected string FormatTableName(bool isNeedFrom = true)
        {
            var typeOfTableClass = SetContext.TableType;

            var tableName = typeOfTableClass.GetTableAttributeName();

            SqlString = $" {ProviderOption.CombineFieldName(tableName)} ";
            if (isNeedFrom)
                SqlString = " FROM " + SqlString;

            return SqlString;
        }

        protected string[] FormatInsertParamsAndValues<T>(T entity)
        {
            var paramSqlBuilder = new StringBuilder(64);
            var valueSqlBuilder = new StringBuilder(64);

            var properties = entity.GetProperties();

            var isAppend = false;
            foreach (var propertiy in properties)
            {
                if (isAppend)
                {
                    paramSqlBuilder.Append(",");
                    valueSqlBuilder.Append(",");
                }

                var columnName = propertiy.GetColumnAttributeName();
                var paramterName = ProviderOption.ParameterPrefix + columnName;
                paramSqlBuilder.Append(ProviderOption.CombineFieldName(columnName));
                valueSqlBuilder.Append(paramterName);

                Params.Add(paramterName, propertiy.GetValue(entity));

                isAppend = true;
            }

            return new[] { paramSqlBuilder.ToString(), valueSqlBuilder.ToString() };
        }
    }
}
