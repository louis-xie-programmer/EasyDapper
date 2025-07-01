using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyDapper.Extension.Exception;
using EasyDapper.Extension.Expressions;
using EasyDapper.Extension.Extension;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.MsSql
{
    /// <summary>
    /// 表达式目录树转换成sql
    /// </summary>
    internal static class ResolveExpression
    {
        public static ProviderOption ProviderOption;

        public static void InitOption(ProviderOption providerOption)
        {
            ProviderOption = providerOption;
        }

        public static string ResolveOrderBy(Dictionary<EOrderBy, LambdaExpression> orderbyExpressionDic)
        {
            if (orderbyExpressionDic == null || !orderbyExpressionDic.Any())
                return "";

            var orderByList = orderbyExpressionDic.Select(a =>
            {
                var columnName = ((MemberExpression)a.Value.Body).Member.GetColumnAttributeName();
                return ProviderOption.CombineFieldName(columnName) + (a.Key == EOrderBy.Asc ? " ASC " : " DESC ");
            }).ToList();

            if (!orderByList.Any())
                return "";

            return "ORDER BY " + string.Join(",", orderByList);
        }

        public static WhereExpression ResolveWhere(LambdaExpression whereExpression, string prefix = null)
        {
            var where = new WhereExpression(whereExpression, prefix, ProviderOption);

            return where;
        }

        public static string ResolveGroup(List<LambdaExpression> groupExpression)
        {
            if(groupExpression == null || !groupExpression.Any())
            {
                return "";
            }

            var grouplist = groupExpression.Select(a => {
                var columnName = ((MemberExpression)a.Body).Member.GetColumnAttributeName();
                return ProviderOption.CombineFieldName(columnName);
            }).ToList();

            if (!grouplist.Any())
            {
                return "";
            }

            return "GROUP BY " + string.Join(",", grouplist);
        }

        public static UpdateEntityWhereExpression ResolveWhere(object obj)
        {
            var where = new UpdateEntityWhereExpression(obj, ProviderOption);
            where.Resolve();
            return where;
        }

        public static string ResolveSelect(PropertyInfo[] propertyInfos, LambdaExpression selector, int? topNum)
        {
            var selectFormat = topNum.HasValue ? " SELECT {1} {0} " : " SELECT {0} ";
            var selectSql = "";

            if (selector == null)
            {
                var propertyBuilder = new StringBuilder();
                foreach (var propertyInfo in propertyInfos)
                {
                    if (propertyBuilder.Length > 0)
                        propertyBuilder.Append(",");
                    propertyBuilder.AppendFormat($"{ProviderOption.CombineFieldName(propertyInfo.GetColumnAttributeName()) } AS {  ProviderOption.CombineFieldName(propertyInfo.Name)}");
                }
                selectSql = string.Format(selectFormat, propertyBuilder, $" TOP {topNum} ");
            }
            else
            {
                var select = new SelectExpression(propertyInfos, selector, topNum, ProviderOption);
                selectSql = select.SqlCmd;
                //var nodeType = selector.Body.NodeType;
                //if (nodeType == ExpressionType.MemberAccess)
                //{
                //    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                //    selectSql = string.Format(selectFormat, ProviderOption.CombineFieldName(columnName), $" TOP {topNum} ");
                //}
                //else if (nodeType == ExpressionType.MemberInit)
                //{
                //    var memberInitExpression = (MemberInitExpression)selector.Body;
                //    selectSql = string.Format(selectFormat, string.Join(",", memberInitExpression.Bindings.Select(a => ProviderOption.CombineFieldName(a.Member.GetColumnAttributeName()))), $" TOP {topNum} ");
                //}
            }

            return selectSql;
        }

        public static string ResolveSelectOfUpdate(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            var selectSql = "";

            if (selector == null)
            {
                var propertyBuilder = new StringBuilder();
                foreach (var propertyInfo in propertyInfos)
                {
                    if (propertyBuilder.Length > 0)
                        propertyBuilder.Append(",");
                    propertyBuilder.AppendFormat($"INSERTED.{ ProviderOption.CombineFieldName(propertyInfo.GetColumnAttributeName())} { ProviderOption.CombineFieldName(propertyInfo.Name)}");
                }
                selectSql = propertyBuilder.ToString();
            }
            else
            {
                var nodeType = selector.Body.NodeType;
                if (nodeType == ExpressionType.MemberAccess)
                {
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = "INSERTED." + ProviderOption.CombineFieldName(columnName);
                }
                else if (nodeType == ExpressionType.MemberInit)
                {
                    var memberInitExpression = (MemberInitExpression)selector.Body;
                    selectSql = string.Join(",", memberInitExpression.Bindings.Select(a => "INSERTED." + ProviderOption.CombineFieldName(a.Member.GetColumnAttributeName())));
                }
            }

            return "OUTPUT " + selectSql;
        }

        #region  常见聚合函数

        public static string ResolveSum(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            var nodeType = selector.Body.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = $" SELECT ISNULL(SUM({ProviderOption.CombineFieldName(columnName)}),0)  ";
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支持该表达式类型");
            }

            return selectSql;
        }


        public static string ResolveCount(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            var nodeType = selector.Body.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = $" SELECT ISNULL(Count({ProviderOption.CombineFieldName(columnName)}),0)  ";
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支持该表达式类型");
            }

            return selectSql;
        }

        public static string ResolveAvg(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            var nodeType = selector.Body.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = $" SELECT ISNULL(AVG({ProviderOption.CombineFieldName(columnName)}),0)  ";
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支持该表达式类型");
            }

            return selectSql;
        }

        public static string ResolveMin(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            var nodeType = selector.Body.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = $" SELECT ISNULL(Min({ProviderOption.CombineFieldName(columnName)}),0)  ";
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支持该表达式类型");
            }

            return selectSql;
        }

        public static string ResolveMax(PropertyInfo[] propertyInfos, LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            var nodeType = selector.Body.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    var columnName = ((MemberExpression)selector.Body).Member.GetColumnAttributeName();
                    selectSql = $" SELECT ISNULL(Max({ProviderOption.CombineFieldName(columnName)}),0)  ";
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支持该表达式类型");
            }

            return selectSql;
        }

        #endregion

        public static UpdateExpression ResolveUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            return new UpdateExpression(updateExpression, ProviderOption);
        }

        public static string ResolveWithNoLock(bool nolock)
        {
            return nolock ? "(NOLOCK)" : "";
        }
    }
}
