 namespace AI.BehaviorTree.Actions
{
    public class Wait : Action
    {
        private readonly float _duration;
        private float _timer;

        public Wait(float duration) => _duration = duration;

        protected override void Update(float deltaTime)
        {
            if (_timer >= _duration)
                State = State.Success;
            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _timer = 0;
        }
    }

    public class WaitWithVar : Action
    {
        private float _duration;
        private float _timer;
        private readonly string _durationVar;

        public WaitWithVar(string durationVar) => _durationVar = durationVar;

        protected override void Update(float deltaTime)
        {
            if (_timer >= _duration)
                State = State.Success;
            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _duration = Tree.GetDatum<float>(_durationVar);
            _timer = 0;
        }
    }
}
