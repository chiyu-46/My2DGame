using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于实现大炮（Cannon）的触发器。
/// </summary>
/// <remarks>
/// Player可以从大炮旁走过，所以二者不能在同一层，但这样触发器也不能正常使用，故使用在不同层的空子物体进行触发。
/// </remarks>
public class CannonTrigger : MonoBehaviour
{
    /// <summary>
    /// 检测是否有可以使用大炮的对象进入触发器范围。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        GetComponentInParent<Cannon>().Register(other);
    }

    /// <summary>
    /// 检测是否有可以使用大炮的对象走出触发器范围。
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        GetComponentInParent<Cannon>().Remove(other);
    }
}
