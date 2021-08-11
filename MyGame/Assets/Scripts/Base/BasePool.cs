using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 对象池基础类，实现有个对象池的基本功能，所有对象池均为本类的子类实例。
/// </summary>
public class BasePool<T> : MonoBehaviour where T : IPoolAble
{
    /// <summary>
    /// 对象池的初始水位线，游戏开始后将在对象池中自动注入相应数量的预制体。
    /// </summary>
    public int waterLine;
    /// <summary>
    /// 对象池中应注入的预制体。
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// 对象池队列。特征：先进先出。
    /// </summary>
    private Queue<GameObject> _pool;
    /// <summary>
    /// 初始化对象池。
    /// </summary>
    void Awake()
    {
        _pool = new Queue<GameObject>();
        //初始化水位。
        for (int i = 0; i < waterLine; i++)
        {
            _pool.Enqueue(Instantiate(prefab,transform));
        }
        //避免队列过长。
        _pool.TrimExcess();
    }

    /// <summary>
    /// 分配可用的池中对象。
    /// </summary>
    /// <remarks>
    /// 优先返回池中对象，空池将实例化新的对象。
    /// </remarks>
    /// <param name="obj">用于接收可用的对象。</param>
    public void Allocate(out GameObject obj)
    {
        if (_pool.Count >= 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }

        obj.GetComponent<T>().IsRecycled = false;
    }
    /// <summary>
    /// 回收用完的对象。
    /// </summary>
    /// <param name="obj">将要回收的对象，回收后值为null。</param>
    public void Recycle(ref GameObject obj)
    {
        if (!obj.GetComponent<T>().IsRecycled)
        {
            _pool.Enqueue(obj);
            obj.GetComponent<T>().IsRecycled = true;
            obj = null;
        }
        
    }
}
