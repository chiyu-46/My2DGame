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
    /// 炸弹的不同状态。用于Enemy视野判断是不是目标。
    /// </summary>
    public enum BombState
    {
        /// <summary>
        /// 炸弹处于点燃状态。
        /// </summary>
        BombOn,
        /// <summary>
        /// 炸弹处于熄灭状态。
        /// </summary>
        BombOff,
        /// <summary>
        /// 炸弹已经爆炸。
        /// </summary>
        BombExploded,
        /// <summary>
        /// 炸弹被拿着。
        /// </summary>
        IsHeld
    }

    /// <summary>
    /// 炸弹当前的状态。
    /// </summary>
    public BombState State;
    /// <summary>
    /// 炸弹被扔下的时间。
    /// </summary>
    private float _startTime;
    /// <summary>
    /// 炸弹从扔下到爆炸所需时间。
    /// </summary>
    public float waitTime;
    /// <summary>
    /// 炸弹的爆炸剩余时间。
    /// <para>waitTime用于初始化炸弹引信时间，此时间是实际使用的时间。因为炸弹熄灭等情况可能需要改变引信时间，使用另外的值不影响炸弹重置。</para>
    /// </summary>
    private float _lastTime;
    /// <summary>
    /// 当前炸弹是否处于熄灭状态。不与状态矛盾，比如处理被拿着时是否处于熄灭状态。
    /// </summary>
    private bool _isBombOff;
    /// <summary>
    /// 使用定时器。
    /// </summary>
    private bool _useTimer;
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
    /// 炸弹Animator的Trigger“on”的id。
    /// </summary>
    private static readonly int ON = Animator.StringToHash("on");
    /// <summary>
    /// 炸弹Animator的Trigger“off”的id。
    /// </summary>
    private static readonly int Off = Animator.StringToHash("off");

    /// <summary>
    /// 自己是否已经被回收到对象池。
    /// </summary>
    public bool IsRecycled { get; set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    private void OnEnable()
    {
        //初始化炸弹为点燃状态。
        State = BombState.BombOn;
        _startTime = Time.time;
        _lastTime = waitTime;
        _isBombOff = false;
    }

    void Update()
    {
        if (_isBombOff == false && _startTime + _lastTime <= Time.time)
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
        State = BombState.BombExploded;
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
            else if (item.GetComponent<Bomb>())
            {
                item.GetComponent<Bomb>().Ignite();
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

    /// <summary>
    /// 点燃炸弹，设置引信时间。
    /// </summary>
    /// <param name="lastTime">当前炸弹引信的时间（秒），即多少秒后爆炸。</param>
    public void Ignite(float? lastTime = null)
    {
        if (!(State == BombState.IsHeld))
        {
            State = BombState.BombOn;
        }
        _startTime = Time.time;
        _animator.SetTrigger(ON);
        _isBombOff = false;
        _lastTime = lastTime ?? _lastTime;
    }

    /// <summary>
    /// 熄灭炸弹。
    /// </summary>
    public void Extinguish()
    {
        _lastTime = _lastTime - (Time.time - _startTime);
        _animator.SetTrigger(Off);
        _isBombOff = true;
        State = BombState.BombOff;
    }
}
