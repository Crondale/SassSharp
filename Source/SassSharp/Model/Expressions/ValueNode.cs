using System;
using System.Text.RegularExpressions;
using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal class ValueNode : ExpressionNode
    {
        public enum ValueNodeType
        {
            Text,
            Value,
            Number
        }

        private readonly string _textValue;

        private readonly ValueNodeType _type = ValueNodeType.Text;
        private readonly double _value;

        public ValueNode(double value, string unit)
        {
            _value = value;
            _textValue = unit;

            _type = ValueNodeType.Value;
        }

        public ValueNode(double value)
        {
            _value = value;

            _type = ValueNodeType.Number;
        }

        public ValueNode(string value)
        {
            if (value == "")
                throw new ArgumentException("Can't be empty");

            //TODO move to reader
            var m = Regex.Match(value, "^(?<value>[0-9.]+)(?<unit>[a-z%]*)$");

            if (m.Success)
            {
                if (!double.TryParse(m.Groups["value"].Value, out _value))
                    throw new Exception("Failed to parse number");

                _textValue = m.Groups["unit"].Value;

                if (_textValue == "")
                    _type = ValueNodeType.Number;
                else
                    _type = ValueNodeType.Value;
            }
            else
            {
                _textValue = value;
            }
        }

        public override string Value
        {
            get
            {
                switch (_type)
                {
                    case ValueNodeType.Number:
                        return _value.ToString();
                    case ValueNodeType.Value:
                        return $"{_value}{_textValue}";
                }
                return _textValue;
            }
        }

        public override ExpressionNode Resolve(ScopeNode scope)
        {
            return this;
        }

        public static ValueNode operator *(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a*b);
        }

        public static ValueNode operator +(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a + b);
        }

        public static ValueNode operator -(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a - b);
        }

        public static ValueNode operator /(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a/b);
        }

        public static ValueNode operator <(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a < b ? 1 : 0);
        }

        public static ValueNode operator >(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a > b ? 1 : 0);
        }

        public static ValueNode ValueEquals(ValueNode x, ValueNode y)
        {
            return checkAndCalculate(x, y, (a, b) => a == b ? 1 : 0);
        }

        private static ValueNode checkAndCalculate(ValueNode x, ValueNode y, Func<double, double, double> calculation)
        {
            if (x._type == ValueNodeType.Text)
                throw new Exception("Cannot calculate on texts");

            if (y._type == ValueNodeType.Text)
                throw new Exception("Cannot calculate on texts");

            if (x._type == ValueNodeType.Value)
            {
                return new ValueNode(calculation(x._value, y._value), x._textValue);
            }
            if (y._type == ValueNodeType.Value)
            {
                return new ValueNode(calculation(x._value, y._value), y._textValue);
            }

            return new ValueNode(calculation(x._value, y._value));
        }
    }
}