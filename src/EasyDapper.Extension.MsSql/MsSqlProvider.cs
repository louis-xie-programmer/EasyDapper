using System.Data;
using System.Linq.Expressions;
using EasyDapper.Extension.Exception;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.MsSql
{
    internal class MsSqlProvider : SqlProvider
    {
        private const char OpenQuote = '[';
        private const char CloseQuote = ']';
        private const char ParameterPrefix = '@';

        public MsSqlProvider()
        {
            ProviderOption = new ProviderOption(OpenQuote, CloseQuote, ParameterPrefix);
            ResolveExpression.InitOption(ProviderOption);
        }

        protected sealed override ProviderOption ProviderOption { get; set; }

        public override SqlProvider FormatGet<T>()
        {
            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, 1);

            var fromTableSql = FormatTableName();

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {groupSql} {orderbySql}";

            return this;
        }

        public override SqlProvider FormatToList<T>()
        {
            var topNum = SetContext.TopNum;

            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, topNum);

            var fromTableSql = FormatTableName();

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {groupSql} {orderbySql}";

            return this;
        }

        public override SqlProvider FormatToPageList<T>(int pageIndex, int pageSize)
        {
            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            if (string.IsNullOrEmpty(orderbySql))
                throw new DapperExtensionException("order by takes precedence over pagelist");

            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, pageSize);

            var fromTableSql = FormatTableName();

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"SELECT COUNT(1) {fromTableSql} {nolockSql} {whereSql};";
            SqlString += $@"{selectSql}
            FROM    ( SELECT *
                      ,ROW_NUMBER() OVER ( {orderbySql} ) AS ROWNUMBER
                      {fromTableSql} {nolockSql}
                      {whereSql}
                    ) T
            WHERE   ROWNUMBER > {(pageIndex - 1) * pageSize}
                    AND ROWNUMBER <= {pageIndex * pageSize} {orderbySql};";

            return this;
        }

        public override SqlProvider FormatCount()
        {
            var selectSql = "SELECT COUNT(1)";

            var fromTableSql = FormatTableName();

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {groupSql}";

            return this;
        }

        public override SqlProvider FormatExists()
        {
            var selectSql = "SELECT TOP 1 1";

            var fromTableSql = FormatTableName();

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {groupSql}";

            return this;
        }

        public override SqlProvider FormatDelete()
        {
            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"DELETE {fromTableSql} {whereSql}";

            return this;
        }

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <param name="DeleteCondition">删除条件</param>
        /// <returns></returns>
        public override SqlProvider FormatDelete(string DeleteCondition)
        {
            var fromTableSql = FormatTableName();
            SqlString = $"DELETE {fromTableSql} {DeleteCondition}";
            return this;
        }


        public override SqlProvider FormatInsert<T>(T entity)
        {
            var paramsAndValuesSql = FormatInsertParamsAndValues(entity);

            if (SetContext.IfNotExistsExpression == null)
                SqlString = $"INSERT INTO {FormatTableName(false)} ({paramsAndValuesSql[0]}) VALUES({paramsAndValuesSql[1]})";
            else
            {
                var ifnotexistsWhere = ResolveExpression.ResolveWhere(SetContext.IfNotExistsExpression, "INT_");

                SqlString = string.Format(@"INSERT INTO {0}({1})  
                SELECT {2}
                WHERE NOT EXISTS(
                    SELECT 1
                    FROM {0}  
                {3}
                    ); ", FormatTableName(false), paramsAndValuesSql[0], paramsAndValuesSql[1], ifnotexistsWhere.SqlCmd);

                Params.AddDynamicParams(ifnotexistsWhere.Param);
            }

            return this;
        }

        public override SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            var update = ResolveExpression.ResolveUpdate(updateExpression);

            var where = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";

            return this;
        }

        public override SqlProvider FormatUpdate<T>(T entity)
        {
            var update = ResolveExpression.ResolveUpdate<T>(a => entity);

            var where = ResolveExpression.ResolveWhere(entity);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";

            return this;
        }


        public override SqlProvider FormatSum<T>(LambdaExpression lambdaExpression)
        {
            var selectSql = ResolveExpression.ResolveSum(typeof(T).GetProperties(), lambdaExpression);

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var nolockSql = ResolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {groupSql} ";

            return this;
        }

        public override SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator)
        {
            var update = ResolveExpression.ResolveUpdate(updator);

            var selectSql = ResolveExpression.ResolveSelectOfUpdate(typeof(T).GetProperties(), SetContext.SelectExpression);

            var where = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            var topNum = SetContext.TopNum;

            var topSql = topNum.HasValue ? $" TOP ({topNum.Value})" : "";
            SqlString = $"UPDATE {topSql} {FormatTableName(false)} WITH ( UPDLOCK, READPAST ) {update.SqlCmd} {selectSql} {whereSql}";

            return this;
        }

        /// <summary>
        /// 大批量数据插入请使用SqlBulkCopy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override SqlProvider ExcuteBulkCopy<T>(IEnumerable<T> list)
        {
            var sqlparams = FormatParams<T>();

            SqlString = $"INSERT INTO {FormatTableName(false)} ({sqlparams}) VALUES";

            var isAppend = false;
            foreach (var item in list)
            {
                var values = FormatInsertListValues<T>(item);
                if (isAppend)
                    SqlString += ",";
                SqlString += $"({values})";
                isAppend = true;
            }

            return this;
        }

        public override SqlProvider FormatCreateTable<T>()
        {
            var sqlparams = FormatCreateParams<T>();

            SqlString = $"CREATE TABLE {FormatTableName(false)} ({sqlparams})";

            return this;
        }

        /// <summary>
        /// MsSql专用类型映射，支持KeyAttribute自动添加主键标识，支持StringLength、可空性、DefaultValue识别。
        /// 主键为int/long时自动设置为自增（IDENTITY(1,1)）。
        /// </summary>
        /// <param name="type">CLR类型</param>
        /// <param name="property">属性信息</param>
        /// <returns>数据库字段类型字符串</returns>
        protected override string MapClrTypeToDbType(Type type, System.Reflection.PropertyInfo property)
        {
            string sqlType;
            // 识别 StringLength 特性
            var stringLengthAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.StringLengthAttribute), true)
                .FirstOrDefault() as System.ComponentModel.DataAnnotations.StringLengthAttribute;
            if (type == typeof(string))
            {
                if (stringLengthAttr != null && stringLengthAttr.MaximumLength > 0)
                    sqlType = $"NVARCHAR({stringLengthAttr.MaximumLength})";
                else
                    sqlType = "NVARCHAR(255)";
            }
            else if (type == typeof(int) || type == typeof(Int32))
                sqlType = "INT";
            else if (type == typeof(long) || type == typeof(Int64))
                sqlType = "BIGINT";
            else if (type == typeof(short) || type == typeof(Int16))
                sqlType = "SMALLINT";
            else if (type == typeof(byte))
                sqlType = "TINYINT";
            else if (type == typeof(bool))
                sqlType = "BIT";
            else if (type == typeof(decimal))
                sqlType = "DECIMAL(18,2)";
            else if (type == typeof(double))
                sqlType = "FLOAT";
            else if (type == typeof(float))
                sqlType = "REAL";
            else if (type == typeof(DateTime))
                sqlType = "DATETIME";
            else if (type == typeof(Guid))
                sqlType = "UNIQUEIDENTIFIER";
            else
                sqlType = "NVARCHAR(MAX)";

            // 检查是否有KeyAttribute
            var isKey = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any();
            if (isKey)
            {
                // 主键为int/long时设置为自增
                if (type == typeof(int) || type == typeof(Int32) || type == typeof(long) || type == typeof(Int64))
                {
                    sqlType += " IDENTITY(1,1)";
                }
                sqlType += " PRIMARY KEY";
                KeyAttributeName = property.Name; // 记录主键属性名
            }

            // 检查是否可空（string? 为可空，string 为不可为空）
            bool isNullable;
            if (type == typeof(string))
            {
                isNullable = property.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute")
                    || Nullable.GetUnderlyingType(property.PropertyType) != null;
            }
            else
            {
                isNullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null;
            }

            // DefaultValue 识别
            var defaultValueAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DefaultValueAttribute), true)
                .FirstOrDefault() as System.ComponentModel.DefaultValueAttribute;
            string defaultValueSql = null;
            if (defaultValueAttr != null)
            {
                if (type == typeof(string))
                    defaultValueSql = $" DEFAULT '{defaultValueAttr.Value?.ToString().Replace("'", "''")}'";
                else if (type == typeof(bool))
                    defaultValueSql = $" DEFAULT {(Convert.ToBoolean(defaultValueAttr.Value) ? 1 : 0)}";
                else if (type == typeof(DateTime))
                    defaultValueSql = $" DEFAULT '{((DateTime)defaultValueAttr.Value):yyyy-MM-dd HH:mm:ss}'";
                else
                    defaultValueSql = $" DEFAULT {defaultValueAttr.Value}";
            }
            // DateTime类型如果不为空且没有DefaultValue，自动设为当前时间
            if (type == typeof(DateTime) && !isNullable && defaultValueAttr == null)
            {
                defaultValueSql = " DEFAULT GETDATE()";
            }

            if (!isKey) // 主键一般不允许为NULL
            {
                sqlType += isNullable ? " NULL" : " NOT NULL";
            }
            else
            {
                sqlType += " NOT NULL";
            }

            if (defaultValueSql != null)
            {
                sqlType += defaultValueSql;
            }

            return sqlType;
        }

        public override SqlProvider FormatExistTable<T>()
        {
            var tableName = FormatTableName(false,false);

            SqlString = $"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

            return this;
        }

        public override SqlProvider FormatDropTable<T>()
        {
            var tableName = FormatTableName(false);

            SqlString = $"DROP TABLE IF EXISTS {tableName}";

            return this;
        }
    }
}
