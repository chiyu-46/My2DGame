using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 大炮控制脚本。
/// </summary>
public class Cannon : MonoBehaviour, IUsable
{
    /// <summary>
    /// 属性CoolingTime的对应字段。
    /// </summary>
    [SerializeField]
    private float _coolTime;
    /// <inheritdoc />
    public float LastTime { get; set; }
    /// <inheritdoc />
    public float CoolingTime { get => _coolTime; set => _coolTime = value; }
    /// <inheritdoc />
    public bool CanUse { get; set; }

    /// <summary>
    /// 大炮的动画器。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// Animator的开火控制量的id。
    /// </summary>
    private static readonly int Fire = Animator.StringToHash("Fire");
    /// <summary>
    /// 炸弹对象池。
    /// </summary>
    public CannonBallPool _Pool;
    /// <summary>
    /// 炮弹被推出时瞬间的冲击力。
    /// </summary>
    public float Force;

    private void Awake()
    {
        //初始最后一次使用时间为大炮启用时间。
        LastTime = Time.time;
        //获取Animator初始值。
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 当可用大炮的对象进入触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">触发触发器的碰撞器。</param>
    public void Register(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Register(this);
        }
        //TODO:敌人可以使用大炮。
    }

    /// <summary>
    /// 当可用大炮的对象走出触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">离开触发器的碰撞器。</param>
    public void Remove(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Remove(this);
        }
        //TODO:敌人可以使用大炮。
    }

    /// <inheritdoc />
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /// <summary>
    /// 使用大炮开炮。
    /// </summary>
    public void Use(GameObject user)
    {
        if (LastTime + CoolingTime < Time.time)
        {
            _animator.SetTrigger(Fire);
            LastTime = Time.time;
        }
    }

    /// <summary>
    /// 大炮转向。
    /// </summary>
    public void SpecialUse(GameObject user)
    {
        transform.Rotate(0,180,0);
    }

    /// <summary>
    /// 由动画系统调用，将炮弹射出。
    /// </summary>
    public void Shoot()
    {
        GameObject cannonBall;
        _Pool.Allocate(out cannonBall);
        cannonBall.SetActive(true);
        cannonBall.transform.position = transform.GetChild(0).position;
        //判断炮弹射出点与大炮的相对位置，确定炮弹应当向什么方向发射。
        if ((transform.GetChild(0).position - transform.position).x > 0)
        {
            cannonBall.GetComponent<Rigidbody2D>().AddForce(Vector2.right * Force,ForceMode2D.Impulse);
        }
        else
        {
            cannonBall.GetComponent<Rigidbody2D>().AddForce(Vector2.left * Force,ForceMode2D.Impulse);
        }
    }
}
