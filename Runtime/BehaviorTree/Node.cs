using UnityEngine.Assertions;

namespace BehaviorTree
{
    public enum State
    {
        Inactive,
        Running,
        Success,
        Failure,
        Suspended
    }

    public abstract class Node
    {
#if UNITY_EDITOR
        public int Id;
#endif

        public State State { get; protected set; } = State.Inactive;
        protected BehaviorTree Tree;
        public Node Parent;

        public virtual State Tick(float deltaTime)
        {
            if (State == State.Suspended) Done();
            else if (State == State.Inactive) OnInit();
            return State;
        }

        public virtual void Setup(BehaviorTree tree) => Tree = tree;
        public virtual void Teardown() { }
        protected virtual void OnInit() => State = State.Running;
        public virtual void Done() => State = State.Inactive; // Cleanup resources here

        public virtual void Abort()
        {
#if UNITY_EDITOR
            Assert.IsTrue(State == State.Running);
            Assert.IsTrue(this is not Root);
#endif
            State = State.Suspended;
        }

        public bool Completed => State is State.Success or State.Failure;
        public virtual void OnChildComplete(Node child, State childState) => child.Done();
        public abstract Node[] GetChildren();
    }
}
