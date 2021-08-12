using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 通用有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机进行转换条件检测需要检测的对象。</typeparam>
    public class BaseFSM<T> : MonoBehaviour where T : BaseFSM<T>
    {
        /// <summary>
        /// 状态机当前状态。
        /// </summary>
        public FSMState<T> CurrentState;
        /// <summary>
        /// 状态机所有状态的“名称——状态”字典。
        /// </summary>
        public Dictionary<string, FSMState<T>> AllStates = new Dictionary<string, FSMState<T>>();

        public virtual void Update()
        {
            //实时判断状态转换条件是否满足。
            if (!(CurrentState is null))
            {
                foreach (var translation in CurrentState.FsmTranslations)
                {
                    if (translation.Check())
                    {
                        AllStates.TryGetValue(translation.TargetState, out CurrentState);
                    }
                }
            }
        }
    }
}

