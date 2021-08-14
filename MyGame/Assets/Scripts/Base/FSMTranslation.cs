using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 用于有限状态机。表示一种状态向另一种状态转换的转换关系。
    /// </summary>
    public class FSMTranslation
    {
        /// <summary>
        /// 此转换更新的目标状态名称。（只读）
        /// </summary>
        public readonly string TargetState;
        /// <summary>
        /// 用于存储检测条件的委托。
        /// </summary>
        public Func<bool> Condition;

        /// <summary>
        /// 转换更新的构造函数。
        /// </summary>
        /// <param name="targetState">此转换更新的目标状态名称。</param>
        /// <param name="condition">用于存储检测条件的委托。</param>
        public FSMTranslation(string targetState, Func<bool> condition)
        {
            TargetState = targetState;
            Condition = condition;
        }

        /// <summary>
        /// 检查条件是否满足。
        /// </summary>
        /// <remarks>
        /// 条件为空，说明这条转换条件为无条件转换，直接返回true。
        /// </remarks>
        public bool Check()
        {
            if (!(Condition is null))
            {
                return Condition();
            }
            return true;
        }
    }
}
