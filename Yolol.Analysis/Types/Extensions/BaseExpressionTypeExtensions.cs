﻿using JetBrains.Annotations;
using Yolol.Grammar.AST.Expressions;

using Type = Yolol.Execution.Type;

namespace Yolol.Analysis.Types.Extensions
{
    public static class BaseExpressionTypeExtensions
    {
        public static Type InferType([NotNull] this BaseExpression expr, ITypeAssignments types)
        {
            return new ExpressionTypeInference(types).Visit(expr);
        }
    }
}
