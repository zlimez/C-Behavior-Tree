using System;
using UnityEngine;

namespace AI.BehaviorTree.Actions
{
    public class SetTarget : Action
    {
        private readonly string _targetVar;
        private readonly Transform _transform;
        private readonly Transform[] _possTargets;
        private readonly Func<Transform, Transform, float> _targetingFunc; // Min value is chosen as the target

        public SetTarget(Transform transform, string targetVar, Transform[] possTargets,
            Func<Transform, Transform, float> targetingFunc)
        {
            _transform = transform;
            _targetVar = targetVar;
            _possTargets = possTargets;
            _targetingFunc = targetingFunc;
        }

        protected override void Update(float deltaTime)
        {
            var target = _possTargets[0];
            var minVal = _targetingFunc(target, _transform);
            for (var i = 1; i < _possTargets.Length; i++)
            {
                var val = _targetingFunc(_possTargets[i], _transform);
                if (val < minVal)
                {
                    minVal = val;
                    target = _possTargets[i];
                }
            }

            Tree.SetDatum(_targetVar, target);
            State = State.Success;
        }
    }

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
