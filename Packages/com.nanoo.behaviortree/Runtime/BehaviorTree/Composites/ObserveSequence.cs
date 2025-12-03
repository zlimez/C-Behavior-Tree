using System;
using UnityEngine.Assertions;

namespace AI.BehaviorTree
{
    public class ObserveSequence : Sequence
    {
        public bool WillRestart { get; private set; }
        private Node _prevChild;
        private readonly string[] _observedVars;
        private bool _shouldReevaluate;
        private readonly Func<object[], bool> _restartCondition;

        private void PromptReevaluate()
        {
            if (State == State.Running) _shouldReevaluate = true;
        }

        public ObserveSequence(Node[] children, string[] observedVars, Func<object, bool> restartCondition)
            : base(children)
        {
            _observedVars = observedVars;
            _restartCondition = restartCondition;
        }

        public override void Setup(BehaviorTree tree)
        {
            base.Setup(tree);
            foreach (var observedVar in _observedVars)
                Tree.AddTracker(observedVar, PromptReevaluate);
        }

        public override void Teardown()
        {
            foreach (var observedVar in _observedVars)
                Tree.RemoveTracker(observedVar, PromptReevaluate);
        }

        public override void OnChildComplete(Node child, State childState)
        {
            child.Done();
            switch (childState)
            {
                case State.Failure:
                    State = State.Failure;
                    return;
                case State.Success when CurrChildInd + 1 < Children.Length:
                {
                    // Do not want to push a child in running state onto the scheduler to have it ticked again
                    if (!WillRestart || (WillRestart && _prevChild != Children[CurrChildInd + 1]))
                        Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                    else ++CurrChildInd;
                    break;
                }
                case State.Success:
                    State = State.Success;
                    break;
            }
        }

        public override State Tick(float deltaTime)
        {
            if (State == State.Suspended) Done();
            else if (State == State.Inactive) OnInit();
            else if (State == State.Running && _shouldReevaluate)
            {
                _shouldReevaluate = false;
                WillRestart = _restartCondition(Tree.GetData(_observedVars, true));
                if (!WillRestart) return State;

                _prevChild = Children[CurrChildInd];
                if (CurrChildInd != 0) OnInit();
                else WillRestart = false;
            }
            else if (State == State.Running)
            {
                if (WillRestart && _prevChild != Children[CurrChildInd]) _prevChild.Abort();
                WillRestart = false;
            }
            else if (WillRestart)
            {
#if UNITY_EDITOR
                Assert.IsTrue(State == State.Failure);
#endif
                _prevChild.Abort();
                WillRestart = false;
            }

            return State;
        }
    }
}
