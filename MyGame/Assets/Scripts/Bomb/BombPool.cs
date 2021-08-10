using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用对象池模式管理游戏对象“炸弹”。
/// </summary>
public class BombPool : MonoBehaviour
{
    /// <summary>
    /// 对象池的初始水位线，游戏开始后将在对象池中自动注入相应数量的“炸弹”游戏对象。
    /// </summary>
    public int waterLine;
    /// <summary>
    /// “炸弹”的预制体。
    /// </summary>
    public GameObject bombPrefab;
    /// <summary>
    /// 对象池队列。特征：先进先出。
    /// </summary>
    private Queue<GameObject> _bombPool;
    /// <summary>
    /// 初始化对象池。
    /// </summary>
    void Start()
    {
        _bombPool = new Queue<GameObject>();
        //初始化水位。
        for (int i = 0; i < waterLine; i++)
        {
            _bombPool.Enqueue(Instantiate(bombPrefab,transform));
        }
        //避免队列过长。
        _bombPool.TrimExcess();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// 获取可用的“炸弹”对象。
    /// </summary>
    /// <remarks>
    /// 优先返回池中对象，空池将实例化新的“炸弹”。
    /// </remarks>
    /// <param name="bomb">用于接收可用的“炸弹”对象。</param>
    public void GetBomb(out GameObject bomb)
    {
        if (_bombPool.Count >= 0)
        {
            bomb = _bombPool.Dequeue();
        }
        else
        {
            bomb = Instantiate(bombPrefab, transform);
        }
    }
    /// <summary>
    /// 归还用完的“炸弹”对象。
    /// </summary>
    /// <param name="bomb">将要归还的“炸弹”对象，归还后值为null。</param>
    public void ReturnBomb(ref GameObject bomb)
    {
        _bombPool.Enqueue(bomb);
        bomb = null;
    }
}
