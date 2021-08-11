using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour, IUsable
{
    public float LastTime { get; set; }
    public float CoolingTime { get; set; }
    private Animator _animator;
    private static readonly int Use1 = Animator.StringToHash("Use");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Use(GameObject user)
    {
        user.GetComponent<PlayerController>().Health++;
        _animator.SetTrigger(Use1);
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
    
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
