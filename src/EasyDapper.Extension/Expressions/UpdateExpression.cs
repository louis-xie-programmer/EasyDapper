using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using Dapper;
using EasyDapper.Extension.Extension;
using EasyDapper.Extension.Helper;
using EasyDapper.Extension.Model;

namespace EasyDapper.Extension.Expressions
{
    public sealed class UpdateExpression : ExpressionVisitor
    {
        #region sql指令

        private readonly StringBuilder _sqlCmd;

        private const string Prefix = "UPDATE_";

        /// <summary>
        /// sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.Length > 0 ? $" SET {_sqlCmd} " : "";

        private readonly ProviderOption _providerOption;

        private readonly char _parameterPrefix;

        public DynamicParameters Param { get; }

        #endregion

        #region 执行解析

        /// <inheritdoc />
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public UpdateExpression(LambdaExpression expression, ProviderOption providerOption)
        {
            _sqlCmd = new StringBuilder(100);
            _providerOption = providerOption;
            _parameterPrefix = _providerOption.ParameterPrefix;
            Param = new DynamicParameters();

            Visit(expression);
        }

        #endregion

        protected override Expression VisitMember(MemberExpression node)
        {
            var memberInitExpression = node;

            var entity = ((ConstantExpression)TrimExpression.Trim(memberInitExpression)).Value;

            // 获取表达式中实际被赋值的字段名集合
            var assignedMembers = new HashSet<string>();
            if (memberInitExpression.Expression is MemberInitExpression memberInit)
            {
                foreach (var binding in memberInit.Bindings)
                {
                    assignedMembers.Add(binding.Member.Name);
                }
            }


            var properties = memberInitExpression.Type.GetProperties();

            foreach (var item in properties)
            {
                if (item.CustomAttributes.Any(b => b.AttributeType == typeof(KeyAttribute)))
                    continue;

                // 只处理表达式中手动赋值的字段
                if (!assignedMembers.Contains(item.Name))
                    continue;

                if (_sqlCmd.Length > 0)
                    _sqlCmd.Append(",");

                var paramName = item.Name;
                var value = item.GetValue(entity);
                var fieldName = _providerOption.CombineFieldName(item.GetColumnAttributeName());
                SetParam(fieldName, paramName, value);
            }

            return node;
        }


        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var memberInitExpression = node;

            foreach (var item in memberInitExpression.Bindings)
            {
                var memberAssignment = (MemberAssignment)item;

                if (_sqlCmd.Length > 0)
                    _sqlCmd.Append(",");

                var paramName = memberAssignment.Member.Name;
                var fieldName = _providerOption.CombineFieldName(memberAssignment.Member.GetColumnAttributeName());
                switch (memberAssignment.Expression.NodeType)
                {
                    case ExpressionType.Add:
                        // Handle addition operation
                        if (memberAssignment.Expression is BinaryExpression binaryExpr &&
                            binaryExpr.NodeType == ExpressionType.Add)
                        {
                            // Get the left and right parts of the addition
                            var left = binaryExpr.Left;
                            var right = binaryExpr.Right;

                            // The left part should be a member access (the field we're updating)
                            if (left is MemberExpression leftMemberExpr)
                            {
                                // Get the value of the right part (the value to add)
                                object rightValue = GetValue(right);

                                // Format the SQL for addition: fieldName = fieldName + value
                                _sqlCmd.AppendFormat(" {0}=CONCAT({0},{1}) ", fieldName, _parameterPrefix + Prefix + paramName);

                                // Add the parameter for the value to add
                                Param.Add(_parameterPrefix + Prefix + paramName, rightValue);
                            }
                            else
                            {
                                throw new NotSupportedException("Only member access expressions are supported on the left side of an addition operation in an update.");
                            }
                        }
                        break;
                    case ExpressionType.Constant:
                        var constantExpression = (ConstantExpression)memberAssignment.Expression;
                        SetParam(fieldName, paramName, constantExpression.Value);
                        break;
                    case ExpressionType.MemberAccess:
                        var constantValue = ((MemberExpression)memberAssignment.Expression).MemberToValue();
                        SetParam(fieldName, paramName, constantValue);
                        break; ;
                }

            }

            return node;
        }

        private void SetParam(string fieldName, string paramName, object value)
        {
            var n = $"{_parameterPrefix}{Prefix}{paramName}";
            _sqlCmd.AppendFormat(" {0}={1} ", fieldName, n);
            Param.Add(n, value);
        }

        private object GetValue(Expression expression)
        {
            if (expression is ConstantExpression constantExpr)
            {
                return constantExpr.Value;
            }
            else if (expression is MemberExpression memberExpr)
            {
                // If it's a member expression, we need to get the value from the entity
                // This is more complex and might require accessing the entity instance
                // For simplicity, we'll assume this is not needed for the addition case
                // and that the right side of the addition is a constant or a simple value
                throw new NotSupportedException("Member expressions are not supported on the right side of an addition operation in an update.");
            }
            else
            {
                throw new NotSupportedException($"Expression type {expression.NodeType} is not supported for getting a value.");
            }
        }
    }
}
