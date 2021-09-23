using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记录玩家存档内容的ScriptableObject。
/// </summary>
public class PlayerArchive : ScriptableObject
{
    /// <summary>
    /// 玩家正在挑战的关卡。
    /// </summary>
    [SerializeField]
    private int currentLevelNumber = 1;
    
    /// <summary>
    /// 玩家正在挑战的关卡。
    /// </summary>
    public int CurrentLevelNumber
    {
        get 
        {
            if (currentLevelNumber < 1)
            {
                return 1; 
            }
            return currentLevelNumber;
        }
        set
        {
            if (value < 1)
            {
                return;
            }
            currentLevelNumber = value;
        }
    }
}
