﻿using System.Linq.Expressions;

namespace EasyDapper.Extension.Model
{
    internal class SetContext
    {
        public SetContext()
        {
            OrderbyExpressionList = new Dictionary<EOrderBy, LambdaExpression>();
        }

        public Type TableType { get; internal set; }

        public LambdaExpression WhereExpression { get; internal set; }

        public List<LambdaExpression> GroupExpression { get; internal set; }

        public LambdaExpression IfNotExistsExpression { get; internal set; }

        public Dictionary<EOrderBy, LambdaExpression> OrderbyExpressionList { get; internal set; }

        public LambdaExpression SelectExpression { get; internal set; }

        public int? TopNum { get; internal set; }

        public bool NoLock { get; internal set; }
    }
}
