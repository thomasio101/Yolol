﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Yolol.Analysis.ControlFlowGraph;
using Yolol.Analysis.ControlFlowGraph.AST;
using Yolol.Analysis.ControlFlowGraph.Extensions;
using Yolol.Analysis.TreeVisitor;
using Yolol.Analysis.TreeVisitor.Inspection;
using Yolol.Grammar;
using Yolol.Grammar.AST.Statements;

namespace Yolol.Analysis.Types
{
    public static class FlowTypingExtensions
    {
        [NotNull] public static IControlFlowGraph FlowTypingAssignment(
            [NotNull] this IControlFlowGraph graph,
            [NotNull] ISingleStaticAssignmentTable ssa,
            [NotNull] out ITypeAssignments types,
            [NotNull] params (string, Execution.Type)[] hints)
        {
            var typesMut = new TypeAssignmentTable();
            types = typesMut;

            // Unconditionally assign hints
            foreach (var (name, type) in hints)
                typesMut.Assign(name, type);

            // Assign all variables which are never assigned the default type (number)
            foreach (var unassReads in UnassignedReadNames(graph))
                typesMut.Assign(unassReads.Name, Execution.Type.Number);

            // Keep assigning types until we no longer find any additional types
            var output = graph;
            int modified;
            do
            {
                var assigner = new AssignTypes(typesMut);
                output = output.Modify((a, b) => {
                    foreach (var statement in a.Statements)
                        b.Add(assigner.Visit(statement));
                });
                modified = assigner.Modified;
            } while (modified > 0);

            return output;
        }

        [NotNull] private static IEnumerable<VariableName> UnassignedReadNames([NotNull] IControlFlowGraph graph)
        {
            var assignedVars = new FindAssignedVariables();
            var readVars = new FindReadVariables();
            foreach (var vertex in graph.Vertices)
            foreach (var statement in vertex.Statements)
            {
                assignedVars.Visit(statement);
                readVars.Visit(statement);
            }

            return readVars.Names.Where(a => !a.IsExternal).Except(assignedVars.Names);
        }

        private class AssignTypes
            : BaseStatementVisitor<BaseStatement>
        {
            private readonly TypeAssignmentTable _types;

            public int Modified { get; private set; }

            public AssignTypes(TypeAssignmentTable types)
            {
                _types = types;
            }

            protected override BaseStatement VisitUnknown(BaseStatement statement)
            {
                if (statement is Conditional)
                    return statement;

                return base.VisitUnknown(statement);
            }

            protected override BaseStatement Visit(EmptyStatement empty)
            {
                return empty;
            }

            protected override BaseStatement Visit(StatementList list)
            {
                return new StatementList(list.Statements.Select(Visit));
            }

            protected override BaseStatement Visit(CompoundAssignment compAss)
            {
                throw new NotSupportedException("Cannot flow type CFG with compound expression (decompose to simpler form first)");
            }

            protected override BaseStatement Visit(Assignment ass)
            {
                // Check type of the right hand side
                var type = new ExpressionTypeInference(_types).Visit(ass.Right);

                // If we've already assigned this type, just keep that as is
                if (ass is TypedAssignment tass && tass.Type == type)
                    return ass;

                // If type is unassigned then we can't type this yet
                if (type == Execution.Type.Unassigned)
                    return ass;

                // We found some new type information
                Modified++;
                _types.Assign(ass.Left.Name, type);
                return new TypedAssignment(type, ass.Left, ass.Right);
            }

            protected override BaseStatement Visit(ExpressionWrapper expr)
            {
                throw new NotImplementedException();
            }

            protected override BaseStatement Visit(Goto @goto)
            {
                return @goto;
            }

            protected override BaseStatement Visit(If @if)
            {
                var l = (StatementList)Visit(@if.TrueBranch);
                var r = (StatementList)Visit(@if.FalseBranch);
                return new If(@if.Condition, l, r);
            }
        }

        private class TypeAssignmentTable
            : ITypeAssignments
        {
            private readonly Dictionary<string, Execution.Type> _types = new Dictionary<string, Execution.Type>();

            public void Assign(string varName, Execution.Type type)
            {
                if (_types.ContainsKey(varName))
                    throw new ArgumentException("type already assigned to {varName}", nameof(varName));
                _types[varName] = type & ~Execution.Type.Error;
            }

            
            public Execution.Type? TypeOf(string varName)
            {
                if (_types.TryGetValue(varName, out var type))
                    return type;
                return null;
            }
        }
    }
}
