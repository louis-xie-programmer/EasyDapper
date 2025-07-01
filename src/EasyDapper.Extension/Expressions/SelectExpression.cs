using EasyDapper.Extension.Extension;
using EasyDapper.Extension.Helper;
using EasyDapper.Extension.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace EasyDapper.Extension.Expressions
{
    /// <summary>
    /// 查询表达书目录树
    /// </summary>
    public sealed class SelectExpression : ExpressionVisitor
    {
        private readonly StringBuilder _sqlCmd;

        private readonly char _closeQuote;

        private readonly char _openQuote;

        private readonly PropertyInfo[] _propertyInfos;

        private string _topString;
        /// <summary>
        /// sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.Length > 0 ? $" SELECT {_topString} {_sqlCmd} " : "";

        public SelectExpression(PropertyInfo[] propertyInfos, LambdaExpression selector, int? topNum, ProviderOption providerOption)
        {
            _sqlCmd = new StringBuilder(100);
            _topString = topNum.HasValue ? $"TOP {topNum}" : "";
            _closeQuote = providerOption.CloseQuote;
            _openQuote = providerOption.OpenQuote;
            _propertyInfos = propertyInfos;

            var exp = TrimExpression.Trim(selector);
            Visit(exp);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = node.Member.Name;
            _sqlCmd.Append($"{_openQuote}{name}{_closeQuote}");
            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            for (int i = 0; i < node.Bindings.Count; i++)
            {
                var item = node.Bindings[i] as MemberAssignment;

                if (item.Expression is MemberExpression member)
                {
                    var name = member.Member.Name;
                    _sqlCmd.Append($"{_openQuote}{name}{_closeQuote} AS {item.Member.Name}");
                }
                else if (item.Expression is MethodCallExpression)
                {
                    var expression = new FunctionExpressionResovle(item.Expression,_openQuote,_closeQuote).Resovle();
                    _sqlCmd.Append($"{expression} AS {item.Member.Name}");
                }
                else if (item.Expression is ConstantExpression)
                {
                    var expression = item.Expression as ConstantExpression;
                    switch (item.Expression.Type.Name)
                    {
                        case "DateTime":
                        case "String":
                            _sqlCmd.Append($"'{expression.Value}' AS {item.Member.Name}");
                            break;
                        case "Int32":
                            _sqlCmd.Append($"{expression.Value} AS {item.Member.Name}");
                            break;
                    }
                }
                else if (item.Expression is BinaryExpression)
                {
                    var binaryExpression = item.Expression as BinaryExpression;
                    if (binaryExpression.NodeType == ExpressionType.Add)
                    {
                        if (binaryExpression.Left is ConstantExpression)
                        {
                            var l = binaryExpression.Left as ConstantExpression;
                            switch (l.Type.Name)
                            {
                                case "Int32":
                                    _sqlCmd.Append(l.Value);
                                    break;
                                default:
                                    _sqlCmd.Append($"'{l.Value}'");
                                    break;
                            }
                        }
                        else
                        {
                            var l = binaryExpression.Left as MemberExpression;
                            _sqlCmd.Append($"{l.Member.Name}");
                        }

                        _sqlCmd.Append("+");

                        if (binaryExpression.Right is ConstantExpression)
                        {
                            var r = binaryExpression.Right as ConstantExpression;
                            switch (r.Type.Name)
                            {
                                case "Int32":
                                    _sqlCmd.Append(r.Value);
                                    break;
                                default:
                                    _sqlCmd.Append($"'{r.Value}'");
                                    break;
                            }
                        }
                        else
                        {
                            var r = binaryExpression.Right as MemberExpression;
                            _sqlCmd.Append($"{r.Member.Name}");
                        }
                        _sqlCmd.Append($" AS {item.Member.Name}");
                    }
                }
                if (i != node.Bindings.Count - 1)
                {
                    _sqlCmd.Append(",");
                }
            }
            return node;
        }


        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var item = node.Arguments[i];
                var column = node.Members[i].Name;
                if (item is MemberExpression member)
                {
                }
                else if (item is MethodCallExpression)
                {
                }
                if (i != node.Arguments.Count - 1)
                {
                }
            }
            return node;
        }

        /// <inheritdoc />
        /// <summary>
        /// 访问方法表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var result = new FunctionExpressionResovle(node, _openQuote, _closeQuote).Resovle();

            _sqlCmd.Append($"{result}");

            return node;
        }
    }
}
