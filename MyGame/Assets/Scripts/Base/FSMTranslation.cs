using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 用于有限状态机。表示一种状态向另一种状态转换的转换关系。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FSMTranslation<T>
    {
        /// <summary>
        /// 此转换更新的目标状态名称。（只读）
        /// </summary>
        public readonly string TargetState;
        /// <summary>
        /// 检测时被检测的对象，一般为FSM的使用者自己。
        /// </summary>
        private T _checkObject;
        /// <summary>
        /// 用于存储检测条件的委托。
        /// </summary>
        public Predicate<T> Condition;

        /// <summary>
        /// 转换更新的构造函数。
        /// </summary>
        /// <param name="targetState">此转换更新的目标状态名称。</param>
        /// <param name="checkObject">检测时被检测的对象，一般为FSM的使用者自己。</param>
        /// <param name="condition">用于存储检测条件的委托。</param>
        public FSMTranslation(string targetState, T checkObject, Predicate<T> condition)
        {
            TargetState = targetState;
            _checkObject = checkObject;
            Condition = condition;
        }

        /// <summary>
        /// 检查条件是否满足。
        /// </summary>
        public bool Check()
        {
            if (!(Condition is null))
            {
                return Condition(_checkObject);
            }

            return false;
        }
    }
}
