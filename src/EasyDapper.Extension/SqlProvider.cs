using System.Data;
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

        public string KeyAttributeName { get; set; }

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

        public abstract SqlProvider FormatCreateTable<T>();

        public abstract SqlProvider FormatInsert<T>(T entity);

        public abstract SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression);

        public abstract SqlProvider FormatUpdate<T>(T entity);

        public abstract SqlProvider FormatSum<T>(LambdaExpression lambdaExpression);

        public abstract SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator);

        public abstract SqlProvider ExcuteBulkCopy<T>(IEnumerable<T> list);

        public abstract SqlProvider FormatExistTable<T>();

        public abstract SqlProvider FormatDropTable<T>();

        protected string FormatTableName(bool isNeedFrom = true, bool isNeedOpenClose = true)
        {
            var typeOfTableClass = SetContext.TableType;

            var tableName = typeOfTableClass.GetTableAttributeName();

            if (!isNeedFrom && !isNeedOpenClose)
            {
                return tableName;
            }

            SqlString = $" {ProviderOption.CombineFieldName(tableName)} ";
            if (isNeedFrom)
                SqlString = " FROM " + SqlString;

            return SqlString;
        }

        protected string FormatParams<T>()
        {
            var paramSqlBuilder = new StringBuilder(64);
            var properties = typeof(T).GetProperties();
            var isAppend = false;
            foreach (var propertiy in properties)
            {
                // 跳过主键（KeyAttribute）
                // 跳过自增主键（KeyAttribute 且 int/long 类型）
                var isKey = propertiy.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any();
                var type = Nullable.GetUnderlyingType(propertiy.PropertyType) ?? propertiy.PropertyType;
                bool isIdentity = isKey && (type == typeof(int) || type == typeof(long));
                if (isIdentity)
                {
                    continue;
                }
                if (isAppend)
                {
                    paramSqlBuilder.Append(",");
                }
                var columnName = propertiy.GetColumnAttributeName();
                paramSqlBuilder.Append(ProviderOption.CombineFieldName(columnName));
                isAppend = true;
            }
            return paramSqlBuilder.ToString();
        }

        /// <summary>
        /// 通过反射获取实体类的属性，并格式化成Create Table语句的参数部分。
        /// 支持后续扩展多数据库类型（如MsSql、MySql）。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected string FormatCreateParams<T>()
        {
            var paramSqlBuilder = new StringBuilder(128);
            var properties = typeof(T).GetProperties();
            var isAppend = false;
            foreach (var property in properties)
            {
                if (isAppend)
                {
                    paramSqlBuilder.Append(", ");
                }
                var columnName = property.GetColumnAttributeName();
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                string sqlType = MapClrTypeToDbType(type, property);
                paramSqlBuilder.Append($"{ProviderOption.CombineFieldName(columnName)} {sqlType}");
                isAppend = true;
            }
            return paramSqlBuilder.ToString();
        }

        /// <summary>
        /// CLR类型到数据库类型的映射，便于多数据库扩展。
        /// 子类可重写以适配不同数据库（如MsSql、MySql）。
        /// </summary>
        /// <param name="type">CLR类型</param>
        /// <returns>数据库字段类型字符串</returns>
        protected virtual string MapClrTypeToDbType(Type type, System.Reflection.PropertyInfo property)
        {
            // 默认MsSql类型映射，可由子类重写
            if (type == typeof(int) || type == typeof(Int32))
                return "INT";
            if (type == typeof(long) || type == typeof(Int64))
                return "BIGINT";
            if (type == typeof(short) || type == typeof(Int16))
                return "SMALLINT";
            if (type == typeof(byte))
                return "TINYINT";
            if (type == typeof(bool))
                return "BIT";
            if (type == typeof(decimal))
                return "DECIMAL(18,2)";
            if (type == typeof(double))
                return "FLOAT";
            if (type == typeof(float))
                return "REAL";
            if (type == typeof(DateTime))
                return "DATETIME";
            if (type == typeof(Guid))
                return "UNIQUEIDENTIFIER";
            if (type == typeof(string))
                return "NVARCHAR(255)";
            return "NVARCHAR(MAX)";
        }

        protected string FormatInsertListValues<T>(T entity)
        {
            var valueBuilder = new StringBuilder();

            var properties = entity.GetProperties();

            foreach (var property in properties)
            {
                // 跳过自增主键（KeyAttribute 且 int/long 类型）
                var isKey = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any();
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                bool isIdentity = isKey && (type == typeof(int) || type == typeof(long));
                if (isIdentity)
                {
                    continue;
                }
                var value = property.GetValue(entity);
                object finalValue = value;
                // 检查 DefaultValue
                var defaultValueAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DefaultValueAttribute), true)
                    .FirstOrDefault() as System.ComponentModel.DefaultValueAttribute;
                if (defaultValueAttr != null && value == null)
                {
                    finalValue = defaultValueAttr.Value;
                }
                else if (type == typeof(DateTime))
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        // 可空DateTime，设为SQL Server允许的最小值
                        finalValue = new DateTime(1753, 1, 1);
                    }
                    else
                    {
                        // 不可空DateTime，设为当前时间
                        finalValue = DateTime.Now;
                    }
                }
                else if (Nullable.GetUnderlyingType(property.PropertyType) != null && value == null)
                {
                    // 其他可空类型，保持为null
                    finalValue = null;
                }
                else if (type.IsValueType && value == null)
                {
                    // 其他不可空值类型，赋默认值
                    finalValue = Activator.CreateInstance(type);
                }

                if (valueBuilder.Length > 0)
                {
                    valueBuilder.Append(",");
                }

                // SQL值转化
                if (finalValue == null)
                {
                    valueBuilder.Append("NULL");
                }
                else if (type == typeof(string) || type == typeof(char))
                {
                    valueBuilder.Append($"'{finalValue.ToString().Replace("'", "''")}'");
                }
                else if (type == typeof(DateTime))
                {
                    valueBuilder.Append($"'{((DateTime)finalValue):yyyy-MM-dd HH:mm:ss}'");
                }
                else if (type == typeof(bool))
                {
                    valueBuilder.Append((bool)finalValue ? "1" : "0");
                }
                else if (type == typeof(Guid))
                {
                    valueBuilder.Append($"'{finalValue}'");
                }
                else
                {
                    valueBuilder.Append(finalValue);
                }
            }

            return valueBuilder.ToString();
        }

        protected string[] FormatInsertParamsAndValues<T>(T entity)
        {
            var paramSqlBuilder = new StringBuilder(64);
            var valueSqlBuilder = new StringBuilder(64);

            var properties = entity.GetProperties();

            foreach (var property in properties)
            {
                // 跳过自增主键（KeyAttribute 且 int/long 类型）
                var isKey = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any();
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                bool isIdentity = isKey && (type == typeof(int) || type == typeof(long));
                if (isIdentity)
                {
                    continue;
                }

                var value = property.GetValue(entity);
                object finalValue = value;

                // 检查 DefaultValue
                var defaultValueAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DefaultValueAttribute), true)
                    .FirstOrDefault() as System.ComponentModel.DefaultValueAttribute;
                if (defaultValueAttr != null)
                {
                    finalValue = defaultValueAttr.Value;
                }
                else if (type == typeof(DateTime))
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        // 可空DateTime，设为SQL Server允许的最小值
                        finalValue = new DateTime(1753, 1, 1);
                    }
                    else
                    {
                        // 不可空DateTime，设为当前时间
                        finalValue = DateTime.Now;
                    }
                }
                else if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    // 其他可空类型，保持为null
                    finalValue = null;
                }
                else if (type.IsValueType)
                {
                    // 其他不可空值类型，赋默认值
                    finalValue = Activator.CreateInstance(type);
                }

                if (paramSqlBuilder.Length > 0)
                {
                    paramSqlBuilder.Append(",");
                    valueSqlBuilder.Append(",");
                }

                var columnName = property.GetColumnAttributeName();
                var paramterName = ProviderOption.ParameterPrefix + columnName;
                paramSqlBuilder.Append(ProviderOption.CombineFieldName(columnName));
                valueSqlBuilder.Append(paramterName);

                Params.Add(paramterName, finalValue);
            }

            return new[] { paramSqlBuilder.ToString(), valueSqlBuilder.ToString() };
        }
    }
}
