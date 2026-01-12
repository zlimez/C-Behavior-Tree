/* (C)2025 Rayark Inc. - All Rights Reserved
 * Rayark Confidential
 *
 * NOTICE: The intellectual and technical concepts contained herein are
 * proprietary to or under control of Rayark Inc. and its affiliates.
 * The information herein may be covered by patents, patents in process,
 * and are protected by trade secret or copyright law.
 * You may not disseminate this information or reproduce this material
 * unless otherwise prior agreed by Rayark Inc. in writing.
 */

using System.Collections.Generic;
using UnityEngine;
using Utils;
using AI.BehaviorTree;
using AI.BehaviorTree.Actions;

namespace Game.Controller
{
    public class GuardBrain : InputOutput
    {
        [SerializeField] private Combo.ComboStep[] _claymoreCombo;
        [SerializeField][Min(0)] private float _moveSpeed = 8f, _attackCooldown = 1.5f;
        [SerializeField] private CollisionNotifier _sightRangeCollider, _attackRangeCollider;

        [Header("Patrol Settings")]
        [SerializeField] private float _durationAtPatrolPoint = 2f;
        [SerializeField] private Transform[] _patrolPoints;

        private const string EnemiesInSight = "EnemiesInSight", TargetEnemy = "TargetEnemy";
        private readonly IObservableStore _store = new SimpleObservableStore();
        private readonly List<Transform> _enemiesInSight = new(), _enemiesInAtkRange = new();
        private BehaviorTree _bt;

        private void Awake()
        {
            var moveCmdChannel = GetChannel<MoveCommand>(nameof(BrainCommandChannelList.Move));
            var strikeCmdChannel = GetChannel<StrikeCommand>(nameof(BrainCommandChannelList.Strike));
            var rotateCmdChannel = GetChannel<RotateCommand>(nameof(BrainCommandChannelList.Rotate));
            var signalStrikeDoneChannel = GetChannel<SignalCommands.StrikeDone>(nameof(BrainCommandChannelList.StrikeDoneSignal));

            _store.SetValue(EnemiesInSight, false);
            _sightRangeCollider.SubscribeTriggerEnter(OnEnemyEnterSight);
            _sightRangeCollider.SubscribeTriggerExit(OnEnemyExitSight);
            _attackRangeCollider.SubscribeTriggerEnter(OnEnemyEnterAtkRange);
            _attackRangeCollider.SubscribeTriggerExit(OnEnemyExitAtkRange);

            _bt = new BehaviorTree(
                new ObserveSelector(new Node[]
                {
                    new Sequence(new Node[]
                    {
                        new Check(() => _enemiesInSight.Count > 0),
                        new Selector(new Node[]
                        {
                            new Sequence(new Node[]
                            {
                                new Check(() => _enemiesInAtkRange.Count > 0),
                                new SetVarWithFunc<Transform>(TargetEnemy, () => _enemiesInAtkRange[0]),
                                new Combo(
                                    _claymoreCombo,
                                    transform,
                                    TargetEnemy,
                                    target => _enemiesInAtkRange.Contains(target),
                                    strikeCmdChannel,
                                    moveCmdChannel,
                                    rotateCmdChannel,
                                    signalStrikeDoneChannel),
                                new Wait(_attackCooldown)
                            }),
                            new GoToBest(
                                transform,
                                _enemiesInSight,
                                (self, targetCandidate) => (targetCandidate.position - self.position).sqrMagnitude,
                                _moveSpeed,
                                moveCmdChannel,
                                rotateCmdChannel)
                        }),
                    }),
                    new Patrol(moveCmdChannel, rotateCmdChannel, transform, _patrolPoints, _moveSpeed, _durationAtPatrolPoint)
                }, new[] { EnemiesInSight }, _ => true),
            new[] { _store });
        }

        private void Update() => _bt.Tick(Time.deltaTime);

        private void OnDestroy()
        {
            _bt.Teardown();
            _sightRangeCollider.UnsubscribeTriggerEnter(OnEnemyEnterSight);
            _sightRangeCollider.UnsubscribeTriggerExit(OnEnemyExitSight);
            _attackRangeCollider.UnsubscribeTriggerEnter(OnEnemyEnterAtkRange);
            _attackRangeCollider.UnsubscribeTriggerExit(OnEnemyExitAtkRange);
        }

        private void OnEnemyEnterSight(Collider obj)
        {
            if (_enemiesInSight.Count == 0)
                _store.SetValue(EnemiesInSight, true);
            _enemiesInSight.Add(obj.transform);
        }

        private void OnEnemyExitSight(Collider obj)
        {
            _enemiesInSight.Remove(obj.transform);
            if (_enemiesInSight.Count == 0)
                _store.SetValue(EnemiesInSight, false);
        }

        private void OnEnemyEnterAtkRange(Collider obj) => _enemiesInAtkRange.Add(obj.transform);
        private void OnEnemyExitAtkRange(Collider obj) => _enemiesInAtkRange.Remove(obj.transform);
    }
}
