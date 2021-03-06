﻿using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal class ReferenceNode : ExpressionNode
    {
        public ReferenceNode(string variableName)
        {
            VariableName = variableName;
        }

        public string VariableName { get; set; }

        public override string Value
        {
            get { return $"${VariableName}"; }
        }

        public override ExpressionNode Resolve(ScopeNode scope)
        {
            return scope.GetVariable(VariableName).Expression.Resolve(scope);
        }
    }
}