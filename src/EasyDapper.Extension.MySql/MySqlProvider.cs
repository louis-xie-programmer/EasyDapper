using EasyDapper.Extension.Exception;
using EasyDapper.Extension.Model;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Xsl;
using static Dapper.SqlMapper;

namespace EasyDapper.Extension.MySql
{
    public class MySqlProvider : SqlProvider
    {
        private const char OpenQuote = '`';
        private const char CloseQuote = '`';
        private const char ParameterPrefix = '@';
        public MySqlProvider()
        {
            ProviderOption = new ProviderOption(OpenQuote, CloseQuote, ParameterPrefix);
            ResolveExpression.InitOption(ProviderOption);
        }

        protected sealed override ProviderOption ProviderOption { get; set; }

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

        public override SqlProvider FormatCount()
        {
            var selectSql = "SELECT COUNT(1)";

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {whereSql} {groupSql}";

            return this;
        }

        public override SqlProvider FormatCreateTable<T>()
        {
            var sqlparams = FormatCreateParams<T>();

            var keyAttribute = "";

            if (!string.IsNullOrEmpty(KeyAttributeName))
                keyAttribute = $", PRIMARY KEY (`{KeyAttributeName}`)";

            SqlString = $"CREATE TABLE {FormatTableName(false)} ({sqlparams}{keyAttribute})";

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

        public override SqlProvider FormatDelete(string DeleteCondition)
        {
            var fromTableSql = FormatTableName();
            SqlString = $"DELETE {fromTableSql} {DeleteCondition}";
            return this;
        }

        public override SqlProvider FormatDropTable<T>()
        {
            var tableName = FormatTableName(false);

            SqlString = $"DROP TABLE IF EXISTS {tableName}";

            return this;
        }

        public override SqlProvider FormatExists()
        {
            var selectSql = "SELECT ";

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);
            var limitSql = ResolveExpression.ResolveLimit(1);

            SqlString = $"{selectSql} {fromTableSql} {whereSql} {groupSql} {limitSql}";

            return this;
        }

        public override SqlProvider FormatExistTable<T>()
        {
            var tableName = FormatTableName(false, false);

            SqlString = $"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

            return this;
        }

        public override SqlProvider FormatGet<T>()
        {
            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression);

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);
            var limitSql = ResolveExpression.ResolveLimit(SetContext.TopNum);

            SqlString = $"{selectSql} {fromTableSql} {whereSql} {groupSql} {orderbySql} {limitSql}";

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

        public override SqlProvider FormatSum<T>(LambdaExpression lambdaExpression)
        {
            var selectSql = ResolveExpression.ResolveSum(typeof(T).GetProperties(), lambdaExpression);

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);

            SqlString = $"{selectSql} {fromTableSql} {whereSql} {groupSql} ";

            return this;
        }

        public override SqlProvider FormatToList<T>()
        {
            var topNum = SetContext.TopNum;

            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression);

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            var groupSql = ResolveExpression.ResolveGroup(SetContext.GroupExpression);
            var limitSql = ResolveExpression.ResolveLimit(SetContext.TopNum);

            SqlString = $"{selectSql} {fromTableSql} {whereSql} {groupSql} {orderbySql} {limitSql}";

            return this;
        }

        public override SqlProvider FormatToPageList<T>(int pageIndex, int pageSize)
        {
            var orderbySql = ResolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            if (string.IsNullOrEmpty(orderbySql))
                throw new DapperExtensionException("order by takes precedence over pagelist");

            var selectSql = ResolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression);

            var fromTableSql = FormatTableName();

            var whereParams = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"SELECT COUNT(1) {fromTableSql} {whereSql};";
            SqlString += $@"{selectSql}
            FROM    ( SELECT *
                      ,ROW_NUMBER() OVER ( {orderbySql} ) AS ROWNUMBER
                      {fromTableSql}
                      {whereSql}
                    ) T
            WHERE   ROWNUMBER BETWEEN {(pageIndex - 1) * pageSize} AND {pageIndex * pageSize} {orderbySql};";

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

        public override SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator)
        {
            var update = ResolveExpression.ResolveUpdate(updator);

            var selectSql = ResolveExpression.ResolveSelectOfUpdate(typeof(T).GetProperties(), SetContext.SelectExpression);

            var where = ResolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            var topNum = SetContext.TopNum;

            var topSql = topNum.HasValue ? $" Limit ({topNum.Value})" : "";
            SqlString = $"UPDATE {FormatTableName(false)} WITH ( UPDLOCK, READPAST ) {update.SqlCmd} {selectSql} {whereSql} {topSql}";

            return this;
        }

        /// <summary>
        /// MySQL专用类型映射，支持KeyAttribute自动添加主键标识，支持StringLength、可空性、DefaultValue识别。
        /// 主键为int/long时自动设置为自增（AUTO_INCREMENT）。
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
                    sqlType = $"VARCHAR({stringLengthAttr.MaximumLength})"; // MySQL 使用 VARCHAR 而非 NVARCHAR
                else
                    sqlType = "TEXT"; // 默认使用 TEXT（适合长文本）
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
                sqlType = "TINYINT(1)"; // MySQL 用 TINYINT(1) 表示布尔值
            else if (type == typeof(decimal))
                sqlType = "DECIMAL(18,2)";
            else if (type == typeof(double))
                sqlType = "DOUBLE";
            else if (type == typeof(float))
                sqlType = "FLOAT";
            else if (type == typeof(DateTime))
                sqlType = "DATETIME";
            else if (type == typeof(Guid))
                sqlType = "CHAR(36)"; // MySQL 通常用 CHAR(36) 存储 GUID
            else
                sqlType = "TEXT"; // 未知类型默认使用 TEXT

            // 检查是否有KeyAttribute
            var isKey = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any();
            if (isKey)
            {
                // 主键为int/long时设置为自增
                if (type == typeof(int) || type == typeof(Int32) || type == typeof(long) || type == typeof(Int64))
                {
                    sqlType += " AUTO_INCREMENT";
                }
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
                defaultValueSql = " DEFAULT CURRENT_TIMESTAMP"; // MySQL 使用 CURRENT_TIMESTAMP
            }

            if (!isKey) // 主键一般不允许为NULL
            {
                sqlType += isNullable ? " NULL" : " NOT NULL";
            }
            else
            {
                sqlType += " NOT NULL"; // 主键不允许为NULL
            }

            if (defaultValueSql != null)
            {
                sqlType += defaultValueSql;
            }

            return sqlType;
        }
    }
}
