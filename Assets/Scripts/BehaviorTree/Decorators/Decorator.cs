using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public abstract class Decorator : Node
    {
        protected Node Child { get; private set; }
        private readonly Node[] _children;

        public Decorator(Node child)
        {
            Child = child;
            Child.Parent = this;
            _children = new Node[1] { Child };
        }

        protected override void OnInit()
        {
            Tree.Scheduled.AddFirst(Child);
            base.OnInit();
        }

        public override void Abort()
        {
            base.Abort();
            Child.Abort();
        }

        public override Node[] GetChildren() => _children;
    }
}
