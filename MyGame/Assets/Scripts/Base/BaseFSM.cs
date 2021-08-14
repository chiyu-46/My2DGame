using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 通用有限状态机。
    /// </summary>
    public class BaseFSM : MonoBehaviour
    {
        /// <summary>
        /// 状态机当前状态。
        /// </summary>
        public FSMState CurrentState;
        /// <summary>
        /// 状态机的上一个状态。
        /// </summary>
        /// <remarks>
        /// 用于调用上一个状态的退出方法。
        /// </remarks>
        public FSMState PreviousState;
        /// <summary>
        /// 状态机所有状态的“名称——状态”字典。
        /// </summary>
        public Dictionary<string, FSMState> AllStates = new Dictionary<string, FSMState>();

        /// <summary>
        /// 状态机都由同一个初始状态"Start"开始。用以不漏过真正的第一个状态的进入时应执行的方法。
        /// </summary>
        /// <remarks>
        /// 注意：使用FSM前，请一定将无条件转移添加到"Start"，使状态机可以顺利到达到真正的第一个状态。
        /// </remarks>
        public virtual void Awake()
        {
            PreviousState = null;
            AllStates.Add("Start",new FSMState("Start"));
            CurrentState = AllStates["Start"];
        }
        
        /// <summary>
        /// FSM主要逻辑。
        /// </summary>
        /// <remarks>
        /// 注意：状态转移优先于状态执行。即：当可以从当前状态进入下一个状态并马上可以进入下下个状态时，第二个状态的执行将被跳过，进入与退出是正常的。
        /// </remarks>
        public virtual void Update()
        {
            //如果有上一个状态，则应当执行上一个状态的退出方法，并执行当前状态的进入方法。
            if (!(PreviousState is null))
            {
                if (!(PreviousState.OnStateExit is null))
                {
                    PreviousState.OnStateExit();
                }
                if (!(CurrentState.OnStateEnter is null))
                {
                    CurrentState.OnStateEnter();
                }
                //重置上一个状态为空。
                PreviousState = null;
            }

            //如果转换条件列表是空的，则说明此状态不会向其他状态转换，永远处于此状态，此时应跳过检测。
            if (!(CurrentState.FsmTranslations is null))
            {
                //实时判断状态转换条件是否满足，如果满足则转变当前状态并存储上一个状态。
                foreach (var translation in CurrentState.FsmTranslations)
                {
                    if (translation.Check())
                    {
                        PreviousState = CurrentState;
                        AllStates.TryGetValue(translation.TargetState, out CurrentState);
                        break;
                    }
                }
            }
            
            
            //执行当前状态应该执行的方法。
            if (!(CurrentState.OnStateStay is null))
            {
                CurrentState.OnStateStay();
            }
        }
        
        
    }
}

