using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于实现传送门的触发器。
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    /// <summary>
    /// 检测是否有可以使用门的对象进入触发器范围。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        GetComponentInParent<Door>().Register(other);
    }

    /// <summary>
    /// 检测是否有可以使用门的对象走出触发器范围。
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        GetComponentInParent<Door>().Remove(other);
    }
}
