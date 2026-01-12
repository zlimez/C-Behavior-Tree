namespace AI.BehaviorTree
{
    public class Selector : Composite
    {
        protected int CurrChildInd;

        public Selector(Node[] children) : base(children) { }

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
                if (CurrChildInd + 1 < Children.Length) Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                else State = State.Failure;
            }
        }

        protected override void OnInit()
        {
            CurrChildInd = 0;
            Tree.Scheduled.AddFirst(Children[CurrChildInd]);
            base.OnInit();
        }
    }
}
