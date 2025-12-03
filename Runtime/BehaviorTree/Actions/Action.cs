namespace BehaviorTree
{
    public abstract class Action : Node
    {
        public override State Tick(float deltaTime)
        {
            if (State == State.Suspended) Done();
            else if (State == State.Inactive) OnInit();
            else if (State == State.Running) Update(deltaTime);
            return State;
        }

        protected abstract void Update(float deltaTime);
        public override Node[] GetChildren() { return null; }
    }
}
