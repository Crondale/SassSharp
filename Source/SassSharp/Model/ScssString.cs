using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Nodes;

namespace SassSharp.Model
{
    class ScssString
    {
        private readonly List<ValueList> _interpolations;
        private readonly string _value;

        public ScssString(string value, List<ValueList> interpolations)
        {
            _value = value;
            _interpolations = interpolations;
        }

        public string Resolve(ScopeNode scope)
        {
            object[] args = _interpolations.Select(x => x.Resolve(scope)).ToArray();
            return string.Format(_value, args);
        }

        public override string ToString()
        {
            return _value;
        }
    }
}