﻿using System;
using JetBrains.Annotations;
using Yolol.Execution;

namespace Yolol.Grammar.AST.Expressions.Binary
{
    public class LessThan
        : BaseBinaryExpression, IEquatable<LessThan>
    {
        public override bool CanRuntimeError => Left.CanRuntimeError || Right.CanRuntimeError;

        public override bool IsBoolean => true;

        public LessThan([NotNull] BaseExpression lhs, [NotNull] BaseExpression rhs)
            : base(lhs, rhs)
        {
        }

        protected override Value Evaluate(string l, string r)
        {
            var comparison = StringComparer.OrdinalIgnoreCase.Compare(l, r);

            return new Value(comparison < 0 ? 1 : 0);
        }

        protected override Value Evaluate(Number l, Number r)
        {
            return new Value(l < r ? 1 : 0);
        }

        protected override Value Evaluate(string l, Number r)
        {
            return Evaluate(l, r.ToString());
        }

        protected override Value Evaluate(Number l, string r)
        {
            return Evaluate(l.ToString(), r);
        }

        public bool Equals(LessThan other)
        {
            return other != null
                && other.Left.Equals(Left)
                && other.Right.Equals(Right);
        }

        public override bool Equals(BaseExpression other)
        {
            return other is LessThan a
                && a.Equals(this);
        }

        public override string ToString()
        {
            return $"{Left}<{Right}";
        }
    }
}
