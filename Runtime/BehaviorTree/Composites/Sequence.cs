namespace BehaviorTree
{
    public class Sequence : Composite
    {
        protected int CurrChildInd;

        public Sequence(Node[] children) : base(children) { }

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
                    Tree.Scheduled.AddFirst(Children[++CurrChildInd]);
                    return;
                }
                else State = State.Success;
            }
        }

        protected override void OnInit()
        {
            CurrChildInd = 0;
            Tree.Scheduled.AddFirst(Children[CurrChildInd]);
            base.OnInit();
        }

        public override void Abort()
        {
            base.Abort();
            Children[CurrChildInd].Abort();
        }
    }
}
