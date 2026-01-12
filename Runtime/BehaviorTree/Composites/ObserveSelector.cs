using System;

namespace AI.BehaviorTree
{
    /// <summary>
    /// Requires blackboards to be set up with the variables to be observed. Does not "restart" if the previous child is the first child, such that no node ticks more that once in running state per turn.
    /// The solution is to attach an ObserveSequence with the condition checker nesting the original first child as end of the sequence, with same observed vars. Refer to DroneBT
    /// Otherwise can be solved with process all aborts at the start of BT tick, changing all children of the restarting node n* nearest to root (in depth) in original "running" branch to abort ->
    /// no processing when visited by the scheduler -> then restart with OnInit from n*
    /// </summary>
    public class ObserveSelector : Selector, IRestartable
    {
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

        public bool ShouldRestart()
        {
            if (!_shouldReevaluate) return false;
            _shouldReevaluate = false;
            return _restartCondition(Tree.GetData(_observedVars, true));
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

        protected override void OnInit()
        {
            _shouldReevaluate = false;
            base.OnInit();
        }
    }
}
