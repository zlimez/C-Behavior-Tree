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

namespace Game.Controller
{
    public enum BrainCommandChannelList
    {
        Move,
        Rotate,
        Strike,
        Dodge,
        LookSignal,
        StrikeDoneSignal,
        DodgeDoneSignal,
        MoveMemorySignal
    }

    public struct MoveCommand
    {
        public static readonly MoveCommand Stop = new(Vector3.zero, 0f);
        public readonly Vector3 Velocity;
        public readonly float MagnitudeScale;

        public MoveCommand(Vector3 velocity, float magnitudeScale)
        {
            Velocity = velocity;
            MagnitudeScale = magnitudeScale;
        }
    }

    public struct RotateCommand
    {
        public readonly Quaternion TargetRotation;
        public RotateCommand(Quaternion targetRotation) => TargetRotation = targetRotation;
    }

    public struct StrikeCommand
    {
        public readonly int StrikeId;
        public readonly float StrikeDuration;

        public StrikeCommand(int strikeId, float strikeDuration)
        {
            StrikeId = strikeId;
            StrikeDuration = strikeDuration;
        }
    }

    public struct DodgeCommand
    {
        public readonly float Duration;
        public DodgeCommand(float duration) => Duration = duration;
    }

    public static class SignalCommands
    {
        public struct Look
        {
            public readonly bool IsStart;
            public Look(bool isStart) => IsStart = isStart;
        }

        public struct StrikeDone
        {
            public readonly int StrikeId;
            public StrikeDone(int strikeId) => StrikeId = strikeId;
        }

        public struct DodgeDone { }

        public struct MoveMemory
        {
            public static readonly MoveMemory Resume = new (Type.Resume);
            public static readonly MoveMemory Memorize = new (Type.Memorize);
            public enum Type { Resume, Memorize }
            public readonly Type Variant;
            public MoveMemory(Type variant) => Variant = variant;
        }
    }
}
