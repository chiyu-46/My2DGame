using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Enemy : BaseFSM, IWoundable
{
    #region 基础部分
    
    /// <summary>
    /// 生命值。用于从Unity显式获取值。
    /// </summary>
    [Header("Base")][SerializeField] 
    private int health;
    /// <summary>
    /// 防御力。用于从Unity显式获取值。
    /// </summary>
    [SerializeField] 
    private int defense;
    /// <inheritdoc />
    public int Health { get => health; set => health = value; }
    /// <inheritdoc />
    public int Defense { get => defense; set => defense = value; }
    /// <inheritdoc />
    public bool CanGetHit { get; set; }

    #endregion

    #region 组件
    
    /// <summary>
    /// 当前敌人的Animator。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// 当前敌人的Rigidbody2D。
    /// </summary>
    private Rigidbody2D _rb;
    
    #endregion
    
    #region 状态机转换需要的参数

    /// <summary>
    /// 是否发现玩家。
    /// </summary>
    private bool _findPlayer = false;
    /// <summary>
    /// 是否发现炸弹。
    /// </summary>
    private bool _findBomb = false;
    /// <summary>
    /// 是否被攻击。
    /// </summary>
    private bool _gitHit = false;
    /// <summary>
    /// 是否已经死亡。
    /// </summary>
    private bool _dead = false;

    #endregion

    #region 巡逻点

    /// <summary>
    /// 路径点的触发器所在layer。
    /// </summary>
    [SerializeField][Header("Route")]
    protected LayerMask routeLayerMask;
    /// <summary>
    /// 当前的路径点列表。
    /// </summary>
    protected List<Vector2> Route;
    /// <summary>
    /// 当前目标路径点在路径点列表中的id。
    /// </summary>
    protected int CurrentRouteId;
    /// <summary>
    /// 
    /// </summary>
    protected Vector2 CurrentPoint;

    #endregion

    #region 运动参数。

    /// <summary>
    /// 敌人的速度。
    /// </summary>
    [SerializeField][Header("Movement")]
    protected float Speed;
    /// <summary>
    /// 敌人的弹跳力。
    /// </summary>
    [SerializeField]
    protected float JumpForce;
    /// <summary>
    /// 用于Player动画控制器，Player开始跳跃时设置为true，Player动画控制器检测到后设为false。
    /// </summary>
    public bool StartJump { get; set; }

    #endregion

    #region 巡逻状态参数

    /// <summary>
    /// 敌人在巡逻状态下，到达路径点后停顿的时间。
    /// </summary>
    [SerializeField]
    protected float PatrolIdleTime;
    /// <summary>
    /// 敌人丢失目标后，发呆的时间。
    /// </summary>
    [SerializeField]
    protected float LostIdleTime;
    /// <summary>
    /// 敌人到达巡逻路径点的时间。
    /// </summary>
    protected float ReachTime;

    #endregion
    
    #region 动画触发器

    /// <summary>
    /// Animator触发器Velocity_x的id。
    /// </summary>
    private static readonly int VelocityX = Animator.StringToHash("Velocity_x");
    /// <summary>
    /// Animator触发器Velocity_y的id。
    /// </summary>
    private static readonly int VelocityY = Animator.StringToHash("Velocity_y");
    /// <summary>
    /// 动画控制参数onGround的id值。
    /// </summary>
    private static readonly int ONGround = Animator.StringToHash("OnGround");

    #endregion

    #region 地面检测

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

    #endregion
    
    
    
    public override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        Route = new List<Vector2>();
        //添加状态。
        AllStates.Add("Patrol",new FSMState("Patrol"));
        AllStates.Add("FindPlayer",new FSMState("FindPlayer"));
        AllStates.Add("FindBomb",new FSMState("FindBomb"));
        AllStates.Add("GitHit",new FSMState("GitHit"));
        AllStates.Add("Dead",new FSMState("Dead"));
        //添加转换条件。
        List<FSMTranslation> tempStateTranslations;
        //无条件从默认开始状态转移到真正的默认状态。
        AllStates["Start"].FsmTranslations.Add(new FSMTranslation("Patrol",null));
        //巡逻状态转换其他状态条件。
        tempStateTranslations = AllStates["Patrol"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("FindPlayer",(() => _findPlayer)));
        tempStateTranslations.Add(new FSMTranslation("FindBomb",() => _findBomb));
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => _gitHit));
        //发现玩家状态转换其他状态条件。
        //发现炸弹的优先级比发现玩家高。
        tempStateTranslations = AllStates["FindPlayer"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Patrol",(() => _findPlayer == false)));
        tempStateTranslations.Add(new FSMTranslation("FindBomb",(() => _findBomb)));
        tempStateTranslations.Add(new FSMTranslation("GitHit",(() => _gitHit)));
        //发现炸弹状态转换其他状态条件。
        tempStateTranslations = AllStates["FindBomb"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Patrol",(() => _findBomb == false)));
        tempStateTranslations.Add(new FSMTranslation("GitHit",(() => _gitHit)));
        //受伤状态后将进入巡逻状态。
        tempStateTranslations = AllStates["FindBomb"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Patrol",null));
        tempStateTranslations.Add(new FSMTranslation("Dead",(() => _dead)));
        //当敌人死亡后，状态机将停留在死亡状态，不能向其他状态转换，所以不添加转换条件。
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

    /// <summary>
    /// 在巡逻时，根据目标点与自己的位置确定当前应当的朝向。
    /// </summary>
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
    
    /// <summary>
    /// 在巡逻时，进行移动的方法。
    /// </summary>
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
    
    /// <summary>
    /// 敌人初始化或者到达新地点时，获得新的巡逻点列表的方法。
    /// </summary>
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
    
    /// <summary>
    /// 返回当前作为目标的巡逻点。
    /// </summary>
    /// <returns>当前作为目标的巡逻点</returns>
    private Vector2 GetCurrentPatrolPoint()
    {
        return Route[CurrentRouteId];
    }

    /// <summary>
    /// 展示地面检测圈的位置与大小。
    /// </summary>
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
