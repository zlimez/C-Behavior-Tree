namespace AI.BehaviorTree
{
    public class ActiveSequence : Sequence, IRestartable
    {
        public ActiveSequence(Node[] children) : base(children) { }
        public bool ShouldRestart() => true;
    }
}
