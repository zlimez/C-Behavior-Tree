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

using UnityEngine;
using UnityEngine.InputSystem;
using Game.Utils;

namespace Game.Controller
{
    public interface IOutput
    {
        public IChannel<T> GetChannel<T>(string channelName) where T : struct;
        public ReadonlyQueue<T> GetReader<T>(string channelName) where T : struct;
    }

    public abstract class InputOutput : MonoBehaviour, IOutput
    {
        private Channels OutputChannels { get; } = new();
        public IChannel<T> GetChannel<T>(string channelName) where T : struct
            => OutputChannels.Get<T>(channelName);
        public ReadonlyQueue<T> GetReader<T>(string channelName) where T : struct
            => GetChannel<T>(channelName).Subscribe();
    }

    public enum InputChannelList
    {
        MoveDirection,
        LookDirection,
        StrikeSignal,
        DodgeSignal
    }

    public class InputSource : InputOutput
    {
        private IChannel<Vector2> _moveChannel;
        private IChannel<Vector2> _lookChannel;
        private IChannel<bool> _strikeChannel;
        private IChannel<bool> _dodgeChannel;

        private void Awake()
        {
            _moveChannel = GetChannel<Vector2>(nameof(InputChannelList.MoveDirection));
            _lookChannel = GetChannel<Vector2>(nameof(InputChannelList.LookDirection));
            _strikeChannel = GetChannel<bool>(nameof(InputChannelList.StrikeSignal));
            _dodgeChannel = GetChannel<bool>(nameof(InputChannelList.DodgeSignal));
        }

        public void OnMove(InputValue value) => _moveChannel.Write(value.Get<Vector2>());
        public void OnLook(InputValue value) => _lookChannel.Write(value.Get<Vector2>());
        public void OnAttack(InputValue value) => _strikeChannel.Write(value.isPressed);
        public void OnJump(InputValue value) => _dodgeChannel.Write(value.isPressed);
    }
}
