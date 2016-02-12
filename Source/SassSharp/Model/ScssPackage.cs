using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model
{
    class ScssPackage:ScopeNode
    {
        
        readonly Dictionary<string, MixinNode> _mixins = new Dictionary<string, MixinNode>();

        public override void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public override VariableNode GetVariable(string name)
        {
            VariableNode result = null;

            if (!_variables.TryGetValue(name, out result))
                throw new Exception($"Could not find variable: {name}");

            return result;
        }

        public override bool HasVariable(string name)
        {
            return _variables.ContainsKey(name);

        }

        public override void SetMixin(MixinNode node)
        {
            _mixins[node.Name] = node;
        }

        public override MixinNode GetMixin(string name)
        {
            MixinNode result = null;

            if (!_mixins.TryGetValue(name, out result))
                throw new Exception($"Could not find variable: {name}");
            

            return result;
        }
    }
}
