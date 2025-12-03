// Root must have at least one child
using UnityEngine.Assertions;

namespace BehaviorTree
{
    public class Root : Node
    {
        public Node Child { get; }
        private readonly Node[] _children;

        public Root(Node child)
        {
#if UNITY_EDITOR
            Assert.IsNull(Parent);
            Assert.IsNotNull(child);
#endif
            Child = child;
            Child.Parent = this;
            _children = new Node[1] { Child };
        }

        public override void OnChildComplete(Node child, State childState)
        {
            State = childState;
            base.OnChildComplete(child, childState);
        }

        protected override void OnInit()
        {
            Tree.Scheduled.AddFirst(Child);
            base.OnInit();
        }

        public override Node[] GetChildren() => _children;
    }
}
