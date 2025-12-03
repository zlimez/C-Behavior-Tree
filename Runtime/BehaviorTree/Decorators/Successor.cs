namespace BehaviorTree
{
    public class Successor : Decorator
    {
        public Successor(Node child) : base(child) { }

        public override void OnChildComplete(Node child, State childState)
        {
            child.Done();
            State = State.Success;
        }
    }
}
