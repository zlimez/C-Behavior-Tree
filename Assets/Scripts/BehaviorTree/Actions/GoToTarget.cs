using UnityEngine;

namespace AI.BehaviorTree.Actions
{
    public class GotoTargetBySpd : Action
    {
        private readonly Transform _transform;
        private readonly float _speed;
        private readonly string _targetName;

        public GotoTargetBySpd(Transform transform, float speed, string targetName)
        {
            _transform = transform;
            _speed = speed;
            _targetName = targetName;
        }

        // Indefinite pursuit
        protected override void Update(float deltaTime)
        {
            var target = Tree.GetDatum<Transform>(_targetName, true);

            if (Vector2.Distance(_transform.position, target.position) > 0.01f)
                _transform.position = Vector3.MoveTowards(
                    _transform.position, target.position, _speed * deltaTime);
            else State = State.Success;
        }
    }

    public class GotoTargetByCurve : Action
    {
        private readonly Transform _transform;
        private readonly string _targetName;
        private readonly AnimationCurve _moveCurve;
        private readonly float _durOrSpeed;
        private float _duration;
        private readonly TargetType _type;
        private readonly MoveBy _moveBy;

        private float _timer;
        private Vector3 _startPos, _destPos;

        public enum TargetType { Transform, Vector3 }
        public enum MoveBy { Speed, Duration }

        public GotoTargetByCurve(Transform transform, string targetName, AnimationCurve moveCurve,
            TargetType type, MoveBy moveBy, float durOrSpeed)
        {
            _transform = transform;
            _targetName = targetName;
            _moveCurve = moveCurve;
            _type = type;
            _moveBy = moveBy;
            _durOrSpeed = durOrSpeed;
        }

        protected override void Update(float deltaTime)
        {
            if (_timer < _duration)
                _transform.position = Vector3.Lerp(_startPos, _destPos, _moveCurve.Evaluate(_timer / _duration));
            else State = State.Success;
            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _startPos = _transform.position;
            _destPos = _type == TargetType.Transform ? Tree.GetDatum<Transform>(_targetName).position : Tree.GetDatum<Vector3>(_targetName);
            _timer = 0;
            if (_moveBy == MoveBy.Speed)
                _duration = Vector2.Distance(_startPos, _destPos) / _durOrSpeed;
        }
    }
}
