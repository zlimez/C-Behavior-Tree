using System;

namespace AI.BehaviorTree
{
    public class CheckVars : Action
    {
        private readonly string[] _vars;
        private readonly Func<object[], bool> _condition;

        public CheckVars(string[] vars, Func<object[], bool> condition)
        {
            _vars = vars;
            _condition = condition;
        }

        protected override void Update(float deltaTime)
            => State = _condition(Tree.GetData(_vars)) ? State.Success : State.Failure;
    }

    public class Check : Action
    {
        private readonly Func<bool> _condition;

        public Check(Func<bool> condition) => _condition = condition;

        protected override void Update(float deltaTime) =>
            State = _condition() ? State.Success : State.Failure;
    }
}
