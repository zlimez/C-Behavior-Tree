using System;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class ProbSelector : Selector
    {
        public struct ProbabilityFunction
        {
            public readonly Func<object[], float> Function;
            public readonly string[] Arguments;

            public ProbabilityFunction(Func<object[], float> function, string[] arguments)
            {
                Function = function;
                Arguments = arguments;
            }
        }

        // NOTE: User's responsibility to ensure the order of probFactor key match args in a probFunc and order of probFuncs match children
        private readonly ProbabilityFunction[] _probFuncs;
        private readonly Node[] _childrenPool;
        private int[] _order;

        public ProbSelector(Node[] children, ProbabilityFunction[] probFuncs) : base(children)
        {
            _probFuncs = probFuncs;
            _order = new int[children.Length];
            _childrenPool = new Node[children.Length];
            for (var i = 0; i < children.Length; i++)
                _childrenPool[i] = children[i];
        }

        private void ShuffleChildren()
        {
            List<float> probDist = new();
            for (var i = 0; i < Children.Length; i++) probDist.Add(_probFuncs[i].Function(Tree.GetData(_probFuncs[i].Arguments)));

            Utils.Random.WeightedShuffle(probDist, ref _order);
            for (var i = 0; i < _order.Length; i++) Children[i] = _childrenPool[_order[i]];
        }

        protected override void OnInit()
        {
            ShuffleChildren();
            base.OnInit();
        }
    }
}
