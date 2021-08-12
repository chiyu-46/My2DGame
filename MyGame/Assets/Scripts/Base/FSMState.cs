using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 用于有限状态机，表示一种状态。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FSMState<T>
    {
        /// <summary>
        /// 状态的名称。（只读）
        /// </summary>
        public readonly string stateName;
        /// <summary>
        /// 此状态向其他状态转换的所有转换关系列表。
        /// </summary>
        public List<FSMTranslation<T>> FsmTranslations;

        /// <summary>
        /// 状态构造方法。
        /// </summary>
        /// <param name="name">此状态的名称。</param>
        public FSMState(string name)
        {
            stateName = name;
            FsmTranslations = new List<FSMTranslation<T>>();
        }
    }
}

