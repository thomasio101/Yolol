﻿using YololEmulator.Execution;
using YololEmulator.Execution.Extensions;

namespace YololEmulator.Grammar.AST.Expressions.Binary
{
    public class AddExpression
        : BaseBinaryExpression
    {
        public AddExpression(BaseExpression left, BaseExpression right)
            : base(left, right)
        {
        }

        protected override Value Evaluate(string l, string r)
        {
            return new Value(l + r);
        }

        protected override Value Evaluate(decimal l, decimal r)
        {
            return new Value(l + r);
        }

        protected override Value Evaluate(string l, decimal r)
        {
            return Evaluate(l, r.Coerce());
        }

        protected override Value Evaluate(decimal l, string r)
        {
            return Evaluate(l.Coerce(), r);
        }
    }
}
