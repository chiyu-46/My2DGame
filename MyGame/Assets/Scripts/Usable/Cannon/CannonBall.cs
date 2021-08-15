using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 大炮的炮弹。
/// </summary>
public class CannonBall : MonoBehaviour, IPoolAble
{
    /// <summary>
    /// 炮弹的爆炸范围。
    /// </summary>
    public float explosionRadius;
    /// <summary>
    /// 炮弹爆炸可以影响的刚体所在的Layer。
    /// </summary>
    public LayerMask targetLayer;
    /// <summary>
    /// 炮弹爆炸对周围物体产生的冲击力。
    /// </summary>
    public float force;
    /// <summary>
    /// 攻击力。
    /// </summary>
    public int attack;
    /// <inheritdoc />
    public bool IsRecycled { get; set; }

    private Animator _animator;
    private static readonly int Explosion = Animator.StringToHash("explosion");

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _animator.SetTrigger(Explosion);
    }

    /// <inheritdoc />
    public void OnRecycled()
    {
        GameObject me = gameObject;
        gameObject.SetActive(false);
        transform.parent.GetComponent<CannonBallPool>().Recycle(ref me);
    }
    
    /// <summary>
    /// 在炸弹被选中时，展示爆炸范围。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,explosionRadius);
    }

    /// <summary>
    /// 检测炸弹周围可被弹开的刚体并弹开。由动画事件调用。
    /// </summary>
    public void Explode()
    {
        //重置游戏对象的旋转状态，使爆炸效果始终朝向正确。
        transform.rotation = Quaternion.Euler(0,0,0);
        Collider2D[] aroundObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius,targetLayer);
        foreach (var item in aroundObjects)
        {
            Vector3 forceDirection = item.transform.position - transform.position;
            item.GetComponent<Rigidbody2D>().AddForce(forceDirection * force,ForceMode2D.Impulse);
            if (item.GetComponent<PlayerController>())
            {
                item.GetComponent<PlayerController>().GetHit(attack);
            }
            else if (item.GetComponent<Enemy>())
            {
                item.GetComponent<Enemy>().GetHit(attack);
            }
        }
    }
}
