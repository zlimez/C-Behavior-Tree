namespace AI.BehaviorTree
{
    public class ActiveSelector : Selector, IRestartable
    {
        public ActiveSelector(Node[] children) : base(children) { }
        public bool ShouldRestart() => true;
    }
}
