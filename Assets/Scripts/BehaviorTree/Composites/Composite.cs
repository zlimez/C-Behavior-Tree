using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public abstract class Composite : Node
    {
        public Node[] Children { get; private set; }

        public Composite(Node[] children)
        {
            Children = children;
            foreach (var child in children) child.Parent = this;
        }

        public override Node[] GetChildren() => Children;
    }
}
