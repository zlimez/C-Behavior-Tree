namespace BehaviorTree
{
    public abstract class Composite : Node
    {
        public Node[] Children { get; }

        public Composite(Node[] children)
        {
            Children = children;
            foreach (var child in children) child.Parent = this;
        }

        public override Node[] GetChildren() => Children;
    }
}
