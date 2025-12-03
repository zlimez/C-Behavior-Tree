/* (C)2022 Rayark Inc. - All Rights Reserved
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

namespace AI.BehaviorTree.Actions
{
    public class CheckVars : Action
    {
        private readonly string[] _vars;
        private readonly Func<object[], bool> _condition;

        public CheckVars(string[] vars, Func<object[], bool> condition)
        {
            _vars = vars;
            _condition = condition;
        }

        protected override void Update(float deltaTime)
            => State = _condition(Tree.GetData(_vars)) ? State.Success : State.Failure;
    }

    public class Check : Action
    {
        private readonly Func<bool> _condition;

        public Check(Func<bool> condition) => _condition = condition;

        protected override void Update(float deltaTime) =>
            State = _condition() ? State.Success : State.Failure;
    }
}
