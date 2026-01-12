using UnityEngine.Assertions;

namespace AI.BehaviorTree
{
    public class Root : Node
    {
        // Root must have one child
        private readonly Node _child;
        private readonly Node[] _children;

        public Root(Node child)
        {
#if UNITY_EDITOR
            Assert.IsNull(Parent);
            Assert.IsNotNull(child);
#endif
            _child = child;
            _child.Parent = this;
            _children = new[] { _child };
        }

        public override void OnChildComplete(Node child, State childState)
        {
            State = childState;
            base.OnChildComplete(child, childState);
        }

        protected override void OnInit()
        {
            Tree.Scheduled.AddFirst(_child);
            base.OnInit();
        }

        public override Node[] GetChildren() => _children;
    }
}
