using System;
using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    internal class FunctionNode : ScopeNode
    {
        private readonly VariableNode[] _args;

        public FunctionNode(string name, VariableNode[] args)
        {
            Name = name;
            _args = args;
        }


        public string Name { get; set; }

        public void Initialize(Expression[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                SetVariable(new VariableNode(_args[i].Name, args[i]));
            }
        }

        public override void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public override string ToString()
        {
            return $"@function {Name}";
        }

        public virtual ExpressionNode Execute(ValueList args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                SetVariable(new VariableNode(_args[i].Name, new Expression(args[i])));
            }

            foreach (var codeNode in Nodes)
            {
                if (codeNode is VariableNode)
                {
                    var n = (VariableNode) codeNode;
                    var expr = n.Expression.Resolve(this);
                    SetVariable(new VariableNode(n.Name, new Expression(expr)));
                }
                else if (codeNode is ReturnNode)
                {
                    return ((ReturnNode)codeNode).Expression.Resolve(this);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            throw new Exception("Could not find return statement");
        }
    }
}