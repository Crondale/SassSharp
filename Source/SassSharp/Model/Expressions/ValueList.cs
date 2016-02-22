﻿using System;
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
            if(value == null)
                return;

            Add(null, value);
        }

        public void Add(string key, ExpressionNode value)
        {
            if (key != null)
            {
                _keys[key] = _items.Count;
            }

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

            valueList.SetKeys(_keys);

            return valueList;
        }

        private void SetKeys(Dictionary<string, int> keys)
        {
            _keys = keys;
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

        public ExpressionNode this[string key]
        {
            get
            {
                key = key.Trim('\"', '\'');

                return _items[_keys[key]];
            }
        }

        public static ValueList From(ExpressionNode node)  // explicit byte to digit conversion operator
        {
            if (node is ValueList)
                return node as ValueList;

            return new ValueList(node);
        }

        public void RemoveAt(int i)
        {
            _items.RemoveAt(i);
        }

    }
}
