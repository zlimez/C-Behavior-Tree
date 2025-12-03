using System;

namespace BehaviorTree.Actions
{
    public class ClearVar : Action
    {
        private readonly string _varName;

        public ClearVar(string varName) => _varName = varName;

        protected override void Update(float deltaTime)
        {
            Tree.ClearDatum(_varName);
            State = State.Success;
        }
    }

    public class SetVar<T> : Action
    {
        private readonly string _varName;
        private readonly T _value;

        public SetVar(string varName, T value)
        {
            _varName = varName;
            _value = value;
        }

        protected override void Update(float deltaTime)
        {
            Tree.SetDatum(_varName, _value);
            State = State.Success;
        }
    }

    public class SetVarWithFunc<T> : Action
    {
        private readonly string _varName;
        private readonly Func<T> _valueFunc;

        public SetVarWithFunc(string varName, Func<T> valueFunc)
        {
            _varName = varName;
            _valueFunc = valueFunc;
        }

        protected override void Update(float deltaTime)
        {
            Tree.SetDatum(_varName, _valueFunc());
            State = State.Success;
        }
    }
}
