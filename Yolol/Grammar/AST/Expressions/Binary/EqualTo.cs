﻿using System;
using JetBrains.Annotations;
using Yolol.Execution;

namespace Yolol.Grammar.AST.Expressions.Binary
{
    public class EqualTo
        : BaseBinaryExpression
    {
        public override bool CanRuntimeError => Left.CanRuntimeError || Right.CanRuntimeError;

        public override bool IsBoolean => true;

        public EqualTo([NotNull] BaseExpression lhs, [NotNull] BaseExpression rhs)
            : base(lhs, rhs)
        {
        }

        protected override Value Evaluate(string l, string r)
        {
            return new Value(l.Equals(r, StringComparison.OrdinalIgnoreCase) ? 1 : 0);
        }

        protected override Value Evaluate(Number l, Number r)
        {
            return new Value(l == r ? 1 : 0);
        }

        protected override Value Evaluate(string l, Number r)
        {
            return 0;
        }

        protected override Value Evaluate(Number l, string r)
        {
            return 0;
        }

        public override string ToString()
        {
            return $"{Left}=={Right}";
        }
    }
}
