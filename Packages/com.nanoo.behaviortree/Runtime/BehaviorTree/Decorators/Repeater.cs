using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public class Repeater : Decorator
    {
        private readonly uint _times;
        private uint _timesExecuted;
        private readonly List<State> _repeatedStates = new();

        public Repeater(Node child, uint times) : base(child) => _times = times;

        public override void OnChildComplete(Node child, State childState)
        {
            child.Done();
            _repeatedStates.Add(childState);
            if (++_timesExecuted == _times)
                State = _repeatedStates.Contains(State.Failure) ? State.Failure : State.Success;
            else Tree.Scheduled.AddFirst(Child);
        }

        protected override void OnInit()
        {
            _timesExecuted = 0;
            _repeatedStates.Clear();
            base.OnInit();
        }
    }
}
