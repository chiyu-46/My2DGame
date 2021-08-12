using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Enemy : BaseFSM<Enemy>, IWoundable
{
    [Header("Base")][SerializeField] 
    private int health;
    [SerializeField] 
    private int defense;
    /// <inheritdoc />
    public int Health { get => health; set => health = value; }
    /// <inheritdoc />
    public int Defense { get => defense; set => defense = value; }

    private Animator _animator;
    private Rigidbody2D _rb;

    private bool _findPlayer = false;
    private bool _findBomb = false;
    private bool _dead = false;

    public bool FindPlayer { get => _findPlayer; }

    public bool FindBomb { get => _findBomb; }

    public bool Dead1 { get => _dead; }

    [SerializeField][Header("Route")]
    protected LayerMask routeLayerMask;
    protected List<Vector2> Route;
    protected int CurrentRouteId;
    protected Vector2 CurrentPoint;

    [SerializeField][Header("Movement")]
    protected float Speed;
    [SerializeField]
    protected float PatrolIdleTime;
    [SerializeField]
    protected float LostIdleTime;
    protected float ReachTime;
    
    [SerializeField]
    protected float JumpForce;

    private static readonly int VelocityX = Animator.StringToHash("Velocity_x");
    private static readonly int VelocityY = Animator.StringToHash("Velocity_y");
    /// <summary>
    /// 动画控制参数onGround的id值。
    /// </summary>
    private static readonly int ONGround = Animator.StringToHash("OnGround");

    /// <summary>
    /// 位于Player脚下的点的引用。以此为中心检测脚下是否有地面。
    /// </summary>
    [Header("Ground Check")]
    public Transform groundChecker;
    /// <summary>
    /// 地面检测圈的半径。
    /// </summary>
    public float checkRadius;
    /// <summary>
    /// 地面所在图层。
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 地面检测的结果。
    /// </summary>
    private bool _isGround;
    /// <summary>
    /// 只读，返回Player是否在地上。
    /// </summary>
    public bool IsGround { get => _isGround; }
    /// <summary>
    /// 用于Player动画控制器，Player开始跳跃时设置为true，Player动画控制器检测到后设为false。
    /// </summary>
    public bool StartJump { get; set; }
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        Route = new List<Vector2>();
        AllStates.Add("Patrol",new FSMState<Enemy>("Patrol"));
        AllStates.Add("FindPlayer",new FSMState<Enemy>("FindPlayer"));
        AllStates.Add("FindBomb",new FSMState<Enemy>("FindBomb"));
        AllStates.Add("Dead",new FSMState<Enemy>("Dead"));
        CurrentState = AllStates["Patrol"];
    }

    private void Start()
    {
        GetRoute();
        ReachTime = Time.time;
        CurrentRouteId = 0;
    }

    public override void Update()
    {
        base.Update();

        //TODO:状态转换时执行某些方法。
        switch (CurrentState.stateName)
        {
            case "Patrol":
                PatrolMove();
                break;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        _animator.SetBool(ONGround,IsGround);
        _animator.SetFloat(VelocityX,Mathf.Abs(_rb.velocity.x));
        _animator.SetFloat(VelocityY,_rb.velocity.y);
    }

    public void ChangeDirection()
    {
        if (GetCurrentPatrolPoint().x - transform.position.x > 0)
        {
            transform.rotation = Quaternion.Euler(0,180,0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0,0,0);
        }
    }
    
    protected void PatrolMove()
    {
        if (ReachTime + PatrolIdleTime < Time.time)
        {
            if (GetCurrentPatrolPoint().x - transform.position.x > 0)
            {
                _rb.velocity = new Vector2(Speed, _rb.velocity.y);
            }
            else
            {
                _rb.velocity = new Vector2(-Speed, _rb.velocity.y);
            }
            transform.position = Vector2.MoveTowards(transform.position, GetCurrentPatrolPoint(), Speed * Time.deltaTime);
            if (Vector2.Distance(GetCurrentPatrolPoint(),transform.position) <= 0.1)
            {
                _rb.velocity = new Vector2(0, _rb.velocity.y);
                ReachTime = Time.time;
                CurrentRouteId = CurrentRouteId + 1 >= Route.Count ? 0 : CurrentRouteId + 1;
            }
        }
    }
    
    private void GetRoute()
    {
        Route.Clear();
        Collider2D[] routes = Physics2D.OverlapCircleAll(transform.position, .5f, routeLayerMask);
        List<float> routesX= routes[Random.Range(0,routes.Length)].GetComponent<Route>().GetRoute();
        for (int i = 0; i < routesX.Count; i++)
        {
            Route.Add(new Vector2(routesX[i],transform.position.y));
        }
    }
    
    private Vector2 GetCurrentPatrolPoint()
    {
        return Route[CurrentRouteId];
    }

    private void OnDrawGizmosSelected()
    {
        //绘制地面检测范围圈。
        Gizmos.DrawWireSphere(groundChecker.position,checkRadius);
    }
    /// <summary>
    /// 检查Player是否站在地面上。
    /// <para>
    /// 并根据结果改变Player受到的重力，提高玩家在跳跃和跳下时的手感。
    /// </para>
    /// </summary>
    void GroundCheck()
    {
        _isGround = Physics2D.OverlapCircle(groundChecker.position, checkRadius, groundLayer);
        if (_isGround)
        {
            _rb.gravityScale = 1;
        }
        else
        {
            _rb.gravityScale = 4;
        }
    }
    /// <inheritdoc />
    public void GetHit(int damage)
    {
        //TODO:播放受伤动画。
        //如果防御力大于受到伤害，则不受伤害。
        int real = damage - Defense;
        if (real <= 0)
        {
            return;
        }
        //受到伤害。
        Health = Health - real;
        //如果此次受伤导致死亡，将调用Dead方法。
        if (Health <= 0)
        {
            //TODO:避免再次受到伤害。
            Health = 0;
            Dead();
        }
    }

    /// <inheritdoc />
    public void Dead()
    {
        //TODO:显示死亡动画。
        
    }
}
