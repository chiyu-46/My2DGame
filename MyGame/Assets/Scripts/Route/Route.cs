using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巡逻路径。
/// </summary>
public class Route : MonoBehaviour
{
    /// <summary>
    /// 路径点的列表。
    /// </summary>
    public List<float> routePoints = new List<float>();

    /// <summary>
    /// 初始化路径点列表。
    /// </summary>
    private void Awake()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            routePoints.Add(transform.GetChild(i).position.x);
        }
    }

    public List<float> GetRoute()
    {
        return routePoints;
    }
}
