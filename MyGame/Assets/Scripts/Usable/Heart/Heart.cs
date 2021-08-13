using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 加血道具控制类。
/// </summary>
public class Heart : MonoBehaviour, IUsable
{
    /// <inheritdoc />
    public float LastTime { get; set; }
    /// <inheritdoc />
    public float CoolingTime { get; set; }
    /// <inheritdoc />
    public bool CanUse { get; set; }
    /// <summary>
    /// 加血道具的动画控制器。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// Animator的触发器Use的id。
    /// </summary>
    private static readonly int Use1 = Animator.StringToHash("Use");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        CanUse = true;
    }

    /// <inheritdoc />
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Use(GameObject user)
    {
        if (CanUse)
        {
            user.GetComponent<PlayerController>().Health++;
            CanUse = false;
            _animator.SetTrigger(Use1);
        }
    }

    public void SpecialUse(GameObject user)
    {
        Debug.Log("加血道具没有特殊用法！");
    }

    /// <summary>
    /// 当可用加血道具的对象进入触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">触发触发器的碰撞器。</param>
    public void Register(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Register(this);
        }
    }

    /// <summary>
    /// 当可用加血道具的对象走出触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">离开触发器的碰撞器。</param>
    public void Remove(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Remove(this);
        }
    }
    
    /// <summary>
    /// 当加血道具被使用完成，应当销毁自己。由动画系统调用。
    /// </summary>
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
