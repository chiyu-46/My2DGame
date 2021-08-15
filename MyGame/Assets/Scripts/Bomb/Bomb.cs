using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此类用于控制炸弹。
/// </summary>
public class Bomb : MonoBehaviour, IPoolAble
{
    /// <summary>
    /// 炸弹被扔下的时间。
    /// </summary>
    private float _startTime;
    /// <summary>
    /// 炸弹从扔下到爆炸所需时间。
    /// </summary>
    public float waitTime;
    /// <summary>
    /// 炸弹的动画控制器。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// 炸弹的刚体。
    /// </summary>
    private Rigidbody2D _rigidbody;
    /// <summary>
    /// 炸弹的爆炸范围。
    /// </summary>
    public float explosionRadius;
    /// <summary>
    /// 炸弹爆炸可以影响的刚体所在的Layer。
    /// </summary>
    public LayerMask targetLayer;
    /// <summary>
    /// 炸弹爆炸对周围物体产生的冲击力。
    /// </summary>
    public float bombForce;
    /// <summary>
    /// 攻击力。
    /// </summary>
    public int attack;
    /// <summary>
    /// 炸弹Animator的Trigger“explosion”的id。
    /// </summary>
    private static readonly int Explosion = Animator.StringToHash("explosion");
    /// <summary>
    /// 自己是否已经被回收到对象池。
    /// </summary>
    public bool IsRecycled { get; set; }
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _startTime = Time.time;
    }

    void Update()
    {
        if (_startTime + waitTime <= Time.time)
        {
            _animator.SetTrigger(Explosion);
        }
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
        //重置游戏对象的旋转状态并锁定，使爆炸效果始终朝向正确。
        transform.rotation = Quaternion.Euler(0,0,0);
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        Collider2D[] aroundObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius,targetLayer);
        foreach (var item in aroundObjects)
        {
            Vector3 forceDirection = item.transform.position - transform.position;
            item.GetComponent<Rigidbody2D>().AddForce(forceDirection * bombForce,ForceMode2D.Impulse);
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

    /// <summary>
    /// 炸弹爆炸后将自己归还炸弹对象池。由动画事件调用。
    /// </summary>
    public void OnRecycled()
    {
        _rigidbody.constraints = RigidbodyConstraints2D.None;
        GameObject me = gameObject;
        gameObject.SetActive(false);
        transform.parent.GetComponent<BombPool>().Recycle(ref me);
    }
}
