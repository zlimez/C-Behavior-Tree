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

using System;
using UnityEngine;

namespace Game
{
    public class CollisionNotifier : MonoBehaviour
    {
        private Action<Collision> _collisionEnterEvent, _collisionExitEvent;
        private Action<Collider> _triggerEnterEvent, _triggerExitEvent;
        private Action<ControllerColliderHit> _controllerColliderHitEvent;

        private void OnCollisionEnter(Collision collision) => _collisionEnterEvent?.Invoke(collision);
        private void OnCollisionExit(Collision collision) => _collisionExitEvent?.Invoke(collision);
        private void OnTriggerEnter(Collider other) => _triggerEnterEvent?.Invoke(other);
        private void OnTriggerExit(Collider other) => _triggerExitEvent?.Invoke(other);
        private void OnControllerColliderHit(ControllerColliderHit hit) => _controllerColliderHitEvent?.Invoke(hit);

        public void SubscribeCollisionEnter(Action<Collision> callback) => _collisionEnterEvent += callback;
        public void UnsubscribeCollisionEnter(Action<Collision> callback) => _collisionEnterEvent -= callback;
        public void SubscribeCollisionExit(Action<Collision> callback) => _collisionExitEvent += callback;
        public void UnsubscribeCollisionExit(Action<Collision> callback) => _collisionExitEvent -= callback;
        public void SubscribeTriggerEnter(Action<Collider> callback) => _triggerEnterEvent += callback;
        public void UnsubscribeTriggerEnter(Action<Collider> callback) => _triggerEnterEvent -= callback;
        public void SubscribeTriggerExit(Action<Collider> callback) => _triggerExitEvent += callback;
        public void UnsubscribeTriggerExit(Action<Collider> callback) => _triggerExitEvent -= callback;
        public void SubscribeControllerColliderHit(Action<ControllerColliderHit> callback) => _controllerColliderHitEvent += callback;
        public void UnsubscribeControllerColliderHit(Action<ControllerColliderHit> callback) => _controllerColliderHitEvent -= callback;
    }
}
