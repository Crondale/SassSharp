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


        public ValueList()
        {
        }

        public ValueList(ExpressionNode node)
        {
            Add(node);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool PreferComma { get; set; }

        public void Add(ExpressionNode value)
        {
            _items.Add(value);
        }

        public void Set(string key, ExpressionNode value)
        {
            _items.Add(value);
        }

        public override ExpressionNode Resolve(ScopeNode scope)
        {
            if (Count == 1)
                return _items[0].Resolve(scope);

            ValueList valueList = new ValueList();
            valueList.PreferComma = PreferComma;

            foreach (var expressionNode in _items)
            {
                valueList.Add(expressionNode.Resolve(scope));
            }

            return valueList;
        }

        public override string Value
        {
            get
            {
                string seperator = PreferComma ? ", " : " ";
                return String.Join(seperator, _items.Select(x => x.Value));
            }
        }

        public ExpressionNode this[int i]
        {
            get { return _items[i]; }
        }

        public static ValueList From(ExpressionNode node)  // explicit byte to digit conversion operator
        {
            if (node is ValueList)
                return node as ValueList;

            return new ValueList(node);
        }
    }
}
