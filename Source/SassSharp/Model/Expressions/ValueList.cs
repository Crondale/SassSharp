using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp.Model
{
    class ValueList: ExpressionNode
    {
        private List<ExpressionNode> _items = new List<ExpressionNode>();
        private Dictionary<string, int> _keys = new Dictionary<string, int>();

        public int Count
        {
            get { return _items.Count; }
        }


        public void Add(ExpressionNode value)
        {
            _items.Add(value);
        }

        public void Set(string key, ExpressionNode value)
        {
            _items.Add(value);
        }

        public override ValueNode Resolve(ScopeNode scope)
        {
            return new ValueNode(String.Join(" ", _items.Select(x => x.Resolve(scope).Value)));
        }
    }
}
