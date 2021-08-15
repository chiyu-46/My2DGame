using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 用于有限状态机，表示一种状态。
    /// </summary>
    public class FSMState
    {
        /// <summary>
        /// 状态的名称。（只读）
        /// </summary>
        public readonly string stateName;
        /// <summary>
        /// 此状态向其他状态转换的所有转换关系列表。
        /// </summary>
        /// <remarks>
        /// 序号越靠前，权重越大。
        /// </remarks>
        public List<FSMTranslation> FsmTranslations;
        /// <summary>
        /// 在进入此状态时应当执行的所有方法。
        /// </summary>
        public Action OnStateEnter;
        /// <summary>
        /// 在此状态时应当执行的所有方法。
        /// </summary>
        public Action OnStateStay;
        /// <summary>
        /// 在退出此状态时应当执行的所有方法。
        /// </summary>
        public Action OnStateExit;

        /// <summary>
        /// 状态构造方法。
        /// </summary>
        /// <param name="name">此状态的名称。</param>
        public FSMState(string name)
        {
            stateName = name;
            FsmTranslations = new List<FSMTranslation>();
        }
    }
}

