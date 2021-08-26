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
    protected Animator EnemyAnimator;
    /// <summary>
    /// 当前敌人的Animator。
    /// </summary>
    public Animator AnimatorGetter { get => EnemyAnimator; }
    /// <summary>
    /// 当前敌人的Rigidbody2D。
    /// </summary>
    protected Rigidbody2D _rb;
    /// <summary>
    /// 当前敌人的Rigidbody2D。
    /// </summary>
    public Rigidbody2D RigidbodyGetter { get => _rb; }
    
    #endregion
    
    #region 状态机转换需要的参数

    /// <summary>
    /// 是否被攻击。
    /// </summary>
    protected bool _getHit;
    /// <summary>
    /// 攻击动画是否播放完成。
    /// </summary>
    private bool _getHitAnimationFinished;
    /// <summary>
    /// 是否已经死亡。
    /// </summary>
    protected bool _dead;
    /// <summary>
    /// 是否找到目标。
    /// </summary>
    protected bool _findTarget;
    /// <summary>
    /// 是否找到目标。
    /// </summary>
    public bool FindTarget { set { _findTarget = value; } }

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
    /// 敌人能不能移动。
    /// </summary>
    protected bool CanMove;
    public bool CanMoveSetter { set => CanMove = value; }
    /// <summary>
    /// 敌人的速度。
    /// </summary>
    [SerializeField][Header("Movement")]
    protected float Speed;
    //TODO:此值作为真实使用值。将在巡逻速度与追击速度间切换。
    /// <summary>
    /// 敌人的弹跳力。
    /// </summary>
    [SerializeField]
    protected float JumpForce;
    /// <summary>
    /// 跳跃时收到的向上的力的向量。
    /// </summary>
    protected Vector2 JumpForceVector;
    /// <summary>
    /// 用于控制人物跳跃，开始跳跃时设置为true，落地设为false。
    /// </summary>
    protected bool IsJumping;
    /// <summary>
    /// 跳跃结束。
    /// </summary>
    protected bool JumpEnd;
    /// <summary>
    /// Enemy跳跃时的方向。用于与速度相乘，只能取值-1（向左），0（没有左右移动），1（向右）.
    /// </summary>
    protected sbyte JumpDirection;
    /// <summary>
    /// 默认物理材质。
    /// </summary>
    [SerializeField]
    protected PhysicsMaterial2D defaultMaterial2D;
    /// <summary>
    /// 跳跃时使用的物理材质。
    /// </summary>
    [SerializeField]
    protected PhysicsMaterial2D jumpMaterial2D;

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
    
    #region 追击目标状态参数

    /// <summary>
    /// 控制当前敌人攻击的对象的碰撞器，用于控制敌人能不能发起攻击。
    /// </summary>
    protected Collider2D _attacker;
    /// <summary>
    /// 当前敌人的视野对象。
    /// </summary>
    protected EnemyVision Vision;
    /// <summary>
    /// 当前敌人的视野对象。
    /// </summary>
    public EnemyVision VisionGetter { get => Vision; }
    /// <summary>
    /// 此敌人首选的追击目标。值为首选目标的Tag。
    /// </summary>
    protected string Preference;
    /// <summary>
    /// 当前目标是不是首选目标。
    /// </summary>
    protected bool IsPreferred;
    /// <summary>
    /// 当前目标是不是首选目标。
    /// </summary>
    public bool IsPreferredSetter { set => IsPreferred = value; }
    /// <summary>
    /// 当前目标的位置。
    /// </summary>
    protected Vector2 TargetPos;
    /// <summary>
    /// 当前目标的位置。
    /// </summary>
    public Vector2 TargetPosSetter { set => TargetPos = value; }
    /// <summary>
    /// 获得当前目标位置的时间。用于如果目标位置无法到达时，强制更新目标位置。
    /// </summary>
    protected float GetTargetTime;
    /// <summary>
    /// 获得当前目标位置的时间。用于如果目标位置无法到达时，强制更新目标位置。
    /// </summary>
    public float GetTargetTimeSetter { set => GetTargetTime = value; }
    /// <summary>
    /// 等待到达目标点的极限时间。
    /// </summary>
    protected float PrescribedTime = 3;
    /// <summary>
    /// 追到当前目标点时是否需要跳跃。
    /// </summary>
    protected bool ShouldJump;
    /// <summary>
    /// 追到当前目标点时是否需要跳跃。
    /// </summary>
    public bool ShouldJumpSetter { set => ShouldJump = value; }

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
    /// <summary>
    /// 动画控制参数Dead的id值。
    /// </summary>
    private static readonly int DeadId = Animator.StringToHash("Dead");
    /// <summary>
    /// 动画控制参数GetHit的id值。
    /// </summary>
    private static readonly int GETHitId = Animator.StringToHash("GetHit");
    /// <summary>
    /// 动画控制参数StartJump的id值。
    /// </summary>
    protected static readonly int StartJump = Animator.StringToHash("StartJump");

    #endregion

    #region 地面检测

    /// <summary>
    /// 位于角色脚下的点的引用。以此为中心检测脚下是否有地面。
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
        _attacker = transform.GetChild(2).GetComponent<Collider2D>();
        Vision = transform.GetChild(1).GetComponent<EnemyVision>();
        EnemyAnimator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        Route = new List<Vector2>();
        CanGetHit = true;
        CanMove = true;
        JumpForceVector = new Vector2(0, JumpForce);
        _rb.sharedMaterial = defaultMaterial2D;
        //添加状态。
        AllStates.Add("Patrol",new FSMState("Patrol"));
        AllStates.Add("GitHit",new FSMState("GitHit"));
        AllStates.Add("Dead",new FSMState("Dead"));
        //添加每个状态要执行的内容。
        FSMState tempState;
        tempState = AllStates["Patrol"];
        tempState.OnStateEnter += OnPatrolStateEnter;
        tempState.OnStateStay += OnPatrolStateStay;
        tempState = AllStates["GitHit"];
        tempState.OnStateStay += StartGitHitAnimation;
        tempState.OnStateExit += GetHitStateExit;
        tempState = AllStates["Dead"];
        tempState.OnStateStay += Dead;
        //添加转换条件。
        List<FSMTranslation> tempStateTranslations;
        //无条件从默认开始状态转移到真正的默认状态。
        AllStates["Start"].FsmTranslations.Add(new FSMTranslation("Patrol",null));
        //巡逻状态转换其他状态条件。
        tempStateTranslations = AllStates["Patrol"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => _getHit));
        //受伤状态结束后将进入巡逻状态或者死亡状态。
        tempStateTranslations = AllStates["GitHit"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Dead",(() => _dead)));
        tempStateTranslations.Add(new FSMTranslation("Patrol", () => _getHitAnimationFinished));
        //当敌人死亡后，状态机将停留在死亡状态，不能向其他状态转换，所以不添加转换条件。
    }

    private void FixedUpdate()
    {
        GroundCheck();
        ChangeDirection();
        EnemyAnimator.SetBool(ONGround,IsGround);
        EnemyAnimator.SetFloat(VelocityX,Mathf.Abs(_rb.velocity.x));
        EnemyAnimator.SetFloat(VelocityY,_rb.velocity.y);
    }

    #region 巡逻状态使用

    /// <summary>
    /// 在巡逻状态进入时执行。获取当前所在平面的预设巡逻点。
    /// </summary>
    private void OnPatrolStateEnter()
    {
        ResetRoute();
    }
    
    /// <summary>
    /// 在巡逻状态时执行。在各巡逻点间移动。
    /// </summary>
    private void OnPatrolStateStay()
    {
        PatrolMove();
    }
    
    /// <summary>
    /// 在移动时，确定当前应当的朝向。
    /// </summary>
    public void ChangeDirection()
    {
        if (_rb.velocity.x > 0.1)
        {
            transform.rotation = Quaternion.Euler(0,180,0);
        }
        else if (_rb.velocity.x < -0.1)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
        }
    }
    
    /// <summary>
    /// 在巡逻时，进行移动的方法。
    /// </summary>
    protected void PatrolMove()
    {
        if (Route.Count == 0)
        {
            //暂时没有找到路径，则跳过移动。
            return;
        }
        if (ReachTime + PatrolIdleTime < Time.time)
        {
            if (Mathf.Abs(GetCurrentPatrolPoint().x - transform.position.x) <= 0.4)
            {
                _rb.velocity = new Vector2(0, _rb.velocity.y);
                ReachTime = Time.time;
                CurrentRouteId = CurrentRouteId + 1 >= Route.Count ? 0 : CurrentRouteId + 1;
                return;
            }
            if (GetCurrentPatrolPoint().x - transform.position.x > 0)
            {
                _rb.velocity = new Vector2(Speed, _rb.velocity.y);
            }
            else
            {
                _rb.velocity = new Vector2(-Speed, _rb.velocity.y);
            }
        }
    }
    
    /// <summary>
    /// 敌人初始化或者到达新地点时，获得新的巡逻点列表的方法。
    /// </summary>
    private void GetRoute()
    {
        Route.Clear();
        Collider2D[] routes = Physics2D.OverlapCircleAll(groundChecker.position, checkRadius, routeLayerMask);
        if (routes.Length != 0)
        {
            List<float> routesX= routes[Random.Range(0,routes.Length)].GetComponent<Route>().GetRoute();
            for (int i = 0; i < routesX.Count; i++)
            {
                Route.Add(new Vector2(routesX[i],transform.position.y));
            }
        }
    }

    /// <summary>
    /// 如果在刚刚到达状态状态时，无法获取巡逻路径，将在此协程中持续尝试获取新路径。
    /// </summary>
    /// <returns></returns>
    IEnumerator OnCanNotGetRoute()
    {
        while (true)
        {
            GetRoute();
            if (Route.Count > 0 || !CurrentState.stateName.Equals("Patrol"))
            {
                //如果找到路径，或者不再巡逻，将终止此协程。
                yield break;
            }
            //如果没有找到，则继续此协程。
            yield return null;
        }
    }
    
    /// <summary>
    /// 重置巡逻路径。
    /// </summary>
    private void ResetRoute()
    {
        GetRoute();
        if (Route.Count == 0)
        {
            //如果找不到路径，将持续尝试。
            StartCoroutine(OnCanNotGetRoute());
        }
        ReachTime = Time.time;
        CurrentRouteId = 0;
    }
    
    /// <summary>
    /// 返回当前作为目标的巡逻点。
    /// </summary>
    /// <returns>当前作为目标的巡逻点</returns>
    private Vector2 GetCurrentPatrolPoint()
    {
        return Route[CurrentRouteId];
    }
    
    #endregion

    #region 追击状态用

    
    /// <summary>
    /// 攻击动画播放完成，可以进行下一次攻击。动画系统调用。
    /// </summary>
    public void HitAnimationFinished()
    {
        //可以进行下一次攻击。
        _attacker.GetComponent<EnemyAttack>().CanHitSetter = true;
        //报告攻击完成后目标位置。
        Vision.ReportTargetPos();
        //攻击完成后可以移动。
        CanMove = true;
    }

    /// <summary>
    /// 跳跃动画完成，可以获取下一个目标点。
    /// </summary>
    public void JumpAnimationFinished()
    {
        if (IsJumping)
        {
            JumpEnd = true;
        }
    }
    
    #endregion
    
    #region 地面检测
    
    /// <summary>
    /// 展示地面检测圈的位置与大小。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        //绘制地面检测范围圈。
        Gizmos.DrawWireSphere(groundChecker.position,checkRadius);
    }
    
    /// <summary>
    /// 检查Enemy是否站在地面上。
    /// <para>
    /// 并根据结果改变Enemy受到的重力，提高玩家在跳跃和跳下时的手感。
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
    
    #endregion

    #region 受伤与死亡
    
    /// <inheritdoc />
    public void GetHit(int damage)
    {
        if (CanGetHit)
        {
            //如果防御力大于受到伤害，则不受伤害。
            int real = damage - Defense;
            if (real <= 0)
            {
                return;
            }
            //受到伤害。
            _getHit = true;
            CanGetHit = false;
            Health = Health - real;
            //如果此次受伤导致死亡，将标记为已死亡。
            if (Health <= 0)
            {
                //TODO:避免再次受到伤害。
                Health = 0;
                _dead = true;
            }
        }
    }

    /// <summary>
    /// 播放受伤动画。保持在受伤状态时执行。
    /// </summary>
    public void StartGitHitAnimation()
    {
        if (_getHit)
        {
            EnemyAnimator.SetTrigger(GETHitId);
            _getHit = false;
        }
    }

    /// <summary>
    /// 受伤动画播放完成。动画系统调用。
    /// </summary>
    public void GetHitAnimationFinished()
    {
        _getHitAnimationFinished = true;
    }

    /// <summary>
    /// 在Enemy受伤状态结束后时执行。
    /// </summary>
    public void GetHitStateExit()
    {
        _getHit = false;
        _getHitAnimationFinished = false;
        if (!_dead)
        {
            CanGetHit = true;
        }
    }
    
    /// <inheritdoc />
    public void Dead()
    {
        EnemyAnimator.SetTrigger(DeadId);
    }
    
    #endregion
}
