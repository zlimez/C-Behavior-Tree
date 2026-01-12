using System;

namespace AI.BehaviorTree
{
    public class ObserveSequence : Sequence, IRestartable
    {
        private readonly string[] _observedVars;
        private bool _shouldReevaluate;
        private readonly Func<object[], bool> _restartCondition;

        private void PromptReevaluate() { if (State == State.Running) _shouldReevaluate = true; }

        public ObserveSequence(Node[] children, string[] observedVars, Func<object, bool> restartCondition)
            : base(children)
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
