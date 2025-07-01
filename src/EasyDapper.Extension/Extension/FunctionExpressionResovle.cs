using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyDapper.Extension.Extension
{
    public class FunctionExpressionResovle: ExpressionVisitor
    {
        private readonly char _closeQuote;

        private readonly char _openQuote;

        public FunctionExpressionResovle(Expression expression, char openQuote, char closeQuote)
        {
            _expression = expression;

            _openQuote = openQuote;
            _closeQuote = closeQuote;
        }

        protected readonly Expression _expression = null;

        protected readonly StringBuilder _textBuilder = new StringBuilder();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _textBuilder.Append(node.Method.Name.ToUpper());
            _textBuilder.Append("(");
            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var item = node.Arguments[i];
                Visit(item);
            }
            if (_textBuilder[_textBuilder.Length - 1] == ',')
            {
                _textBuilder.Remove(_textBuilder.Length - 1, 1);
            }
            _textBuilder.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = VisitConstantValue(node);
            if (value == null)
            {
                value = "NULL";
            }
            else if (value is string)
            {
                value = $"'{value}'";
            }
            else if (value is bool)
            {
                value = Convert.ToBoolean(value) ? 1 : 0;
            }
            _textBuilder.Append($"{value},");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = node.Member.Name;
            _textBuilder.Append($"{_openQuote}{name}{_closeQuote},");
            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            foreach (var item in node.Expressions)
            {
                Visit(item);
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.Operand != null)
            {
                Visit(node.Operand);
            }
            return node;
        }

        /// <summary>
        /// 解析表达式参数
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object VisitConstantValue(Expression expression)
        {
            var names = new Stack<string>();
            var exps = new Stack<Expression>();
            var mifs = new Stack<MemberInfo>();
            if (expression is ConstantExpression constant)
                return constant.Value;
            else if (expression is MemberExpression)
            {
                var temp = expression;
                object value = null;
                while (temp is MemberExpression memberExpression)
                {
                    names.Push(memberExpression.Member.Name);
                    exps.Push(memberExpression.Expression);
                    mifs.Push(memberExpression.Member);
                    temp = memberExpression.Expression;
                }
                foreach (var name in names)
                {
                    var exp = exps.Pop();
                    var mif = mifs.Pop();
                    if (exp is ConstantExpression cex)
                        value = cex.Value;
                    if (mif is PropertyInfo pif)
                        value = pif.GetValue(value);
                    else if (mif is FieldInfo fif)
                        value = fif.GetValue(value);
                }
                return value;
            }
            else
            {
                return Expression.Lambda(expression).Compile().DynamicInvoke();
            }
        }

        public virtual string Resovle()
        {
            Visit(_expression);
            return _textBuilder.ToString();
        }
    }
}
