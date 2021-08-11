using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于实现加血道具（Heart）的触发器。
/// </summary>
public class HeartTrigger : MonoBehaviour
{
    /// <summary>
    /// 检测是否有可以使用加血道具的对象进入触发器范围。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        GetComponentInParent<Heart>().Register(other);
    }

    /// <summary>
    /// 检测是否有可以使用加血道具的对象走出触发器范围。
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        GetComponentInParent<Heart>().Remove(other);
    }
}
