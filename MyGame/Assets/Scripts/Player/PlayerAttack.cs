using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制Player进行攻击。
/// </summary>
/// <remarks>
/// 配合对象池释放炸弹。从池中取出炸弹后，将准备下一次攻击，归还炸弹由炸弹对象自己完成。
/// </remarks>
public class PlayerAttack : MonoBehaviour
{
    /// <summary>
    /// 攻击冷却时间（秒）。
    /// </summary>
    public float coolingTime;
    /// <summary>
    /// 最后一次攻击时间。由Time.Time赋值。
    /// </summary>
    private float _lastTime;
    /// <summary>
    /// 炸弹对象池。
    /// </summary>
    public BombPool bombPool;

    /// <summary>
    /// 进行攻击。
    /// </summary>
    /// <remarks>
    /// 当用户按下攻击键时，由InputSystem调用，如果满足冷却时间，则Player放置炸弹。
    /// </remarks>
    public void Bomb()
    {
        if (_lastTime + coolingTime <= Time.time)
        {
            GameObject bomb;
            bombPool.Allocate(out bomb);
            bomb.SetActive(true);
            bomb.transform.position = transform.position;
            _lastTime = Time.time;
        }
    }
}
