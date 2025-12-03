using System;
using UnityEngine.Assertions;

namespace AI.BehaviorTree
{
    /// <summary>
    /// Requires blackboards to be set up with the variables to be observed. Does not "restart" if the previous child is the first child, such that no node ticks more that once in running state per turn.
    /// The solution is to attach an ObserveSequence with the condition checker nesting the original first child as end of the sequence, with same observed vars. Refer to DroneBT
    /// Otherwise can be solved with process all aborts at the start of BT tick, changing all children of the restarting node n* nearest to root (in depth) in original "running" branch to abort ->
    /// no processing when visited by the scheduler -> then restart with OnInit from n*
    /// </summary>
    public class ObserveSelector : Selector
    {
        public bool WillRestart { get; private set; }
        private Node _prevChild;
        private readonly string[] _observedVars;
        private bool _shouldReevaluate;
        private readonly Func<object[], bool> _restartCondition;

        private void PromptReevaluate() { if (State == State.Running) _shouldReevaluate = true; }

        /// <summary>
        /// The object params in restart conditions contains the values of the observed vars retrieved from the headboard
        /// </summary>
        /// <param name="children"></param>
        /// <param name="observedVars"></param>
        /// <param name="restartCondition"></param>
        public ObserveSelector(Node[] children, string[] observedVars, Func<object[], bool> restartCondition) : base(children)
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
                case State.Success:
                    State = State.Success;
                    return;
                case State.Failure when CurrChildInd + 1 < Children.Length:
                {
                    // Do not want to push a child in running state onto the scheduler to have it ticked again
                    if (!WillRestart || WillRestart && _prevChild != Children[CurrChildInd + 1])
                        Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                    else ++CurrChildInd;
                    break;
                }
                case State.Failure:
                    State = State.Failure;
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
                WillRestart = _restartCondition(Tree.GetData(_observedVars, true)); // nullable set to true as one or more env variable might not have changed the first time to be registed in headboard
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
                // Must be success due to after restart the new child succeeded, if failed must mean all inc prevChild executed again when it should be in running
#if UNITY_EDITOR
                Assert.IsTrue(State == State.Success);
#endif
                _prevChild.Abort();
                WillRestart = false;
            }

            return State;
        }
    }
}
