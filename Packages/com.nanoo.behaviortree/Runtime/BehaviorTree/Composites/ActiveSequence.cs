using UnityEngine.Assertions;

namespace BehaviorTree
{
    public class ActiveSequence : Sequence
    {
        private bool _startedFromInactive;
        public bool Restarted { get; private set; }
        private Node _prevChild;
        public ActiveSequence(Node[] children) : base(children) { }

        public override void OnChildComplete(Node child, State childState)
        {
            child.Done();
            if (childState == State.Failure)
            {
                State = State.Failure;
                return;
            }

            if (childState == State.Success)
            {
                if (CurrChildInd + 1 < Children.Length)
                {
                    // Do not want to push a child in running state onto the scheduler to have it ticked again
                    if (!Restarted || Restarted && _prevChild != Children[CurrChildInd + 1])
                        Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                    else ++CurrChildInd;
                }
                else State = State.Success;
            }
        }

        // INVARIANT: Restarted is false end of each turn
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
                // Success means prevChild have alr been executed this turn and succeeded violating at most one tick starting at running state per turn
#if UNITY_EDITOR
                Assert.IsTrue(State == State.Failure);
#endif
                _prevChild.Abort();
                Restarted = false;
            }

            return State;
        }
    }
}
