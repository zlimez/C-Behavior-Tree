using UnityEngine.Assertions;

namespace AI.BehaviorTree
{
    public class ActiveSelector : Selector
    {
        private bool _startedFromInactive;
        public bool Restarted { get; private set; }
        private Node _prevChild;
        public ActiveSelector(Node[] children) : base(children) { }

        public override void OnChildComplete(Node child, State childState)
        {
            child.Done();
            if (childState == State.Success)
            {
                State = State.Success;
                return;
            }

            if (childState == State.Failure)
            {
                if (CurrChildInd + 1 < Children.Length)
                {
                    // Do not want to push a child in running state onto the scheduler to have it ticked again
                    if (!Restarted || (Restarted && _prevChild != Children[CurrChildInd + 1]))
                        Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                    else ++CurrChildInd;
                }
                else State = State.Failure;
            }
        }

        // INVARIANT: Restarted is false at the end of each turn
        public override State Tick(float deltaTime)
        {
            if (State == State.Suspended) Done();
            else if (State == State.Inactive)
            {
                _startedFromInactive = true;
                OnInit();
            }
            else if (State == State.Running) // if final status acquired no running children require abort
            {
                if (_startedFromInactive)
                {
                    _startedFromInactive = false;
                    return State;
                }

                if (!Restarted)
                {
                    Restarted = true;
                    _prevChild = Children[CurrChildInd];
                    if (CurrChildInd != 0)
                        OnInit();
                    else Restarted = false;
                }
                else
                {
                    if (_prevChild != Children[CurrChildInd]) _prevChild.Abort();
                    Restarted = false;
                }
            }
            else if (Restarted)
            {
                // Failure means prevChild have also been executed this turn and failed violating at most one tick starting at running state per turn
#if UNITY_EDITOR
                Assert.IsTrue(State == State.Success);
#endif
                _prevChild.Abort();
                Restarted = false;
            }

            return State;
        }
    }
}
