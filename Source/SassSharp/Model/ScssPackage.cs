using System;
using System.Collections.Generic;
using SassSharp.IO;
using SassSharp.Model.Nodes;
using SassSharp.Model.Nodes.Functions;

namespace SassSharp.Model
{
    internal class ScssPackage : ScopeNode
    {
        private readonly Dictionary<string, MixinNode> _mixins = new Dictionary<string, MixinNode>();
        private readonly Dictionary<string, FunctionNode> _functions = new Dictionary<string, FunctionNode>();

        public ScssPackage(PathFile file)
        {
            File = file;
            
        }

        public void LoadBuiltInFunctions()
        {
            this.SetFunction(new IfFunction());
        }

        public PathFile File { get; set; }

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

        public override void SetFunction(FunctionNode node)
        {
            _functions[node.Name] = node;
        }

        public override FunctionNode GetFunction(string name)
        {
            FunctionNode result = null;

            if (!_functions.TryGetValue(name, out result))
                throw new Exception($"Could not find function: {name}");


            return result;
        }
    }
}