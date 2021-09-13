using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Enemy : BaseFSM, IWoundable
{
    #region 可视化编辑变量

    /// <summary>
    /// 生命值。用于从Unity显式获取值。
    /// </summary>
    [SerializeField][Header("Base")]
    private int health;
    /// <summary>
    /// 防御力。用于从Unity显式获取值。
    /// </summary>
    [SerializeField] 
    private int defense;
    /// <summary>
    /// 此变量用于确定此角色是否具有抗打断能力。如果此值为true，则此角色不受协程BouncedOffByAttack影响。
    /// </summary>
    [SerializeField]
    protected bool antiInterruption;
    /// <summary>
    /// 路径点的触发器所在layer。
    /// </summary>
    [SerializeField][Header("Route")]
    protected LayerMask routeLayerMask;
    /// <summary>
    /// 敌人巡逻时的速度。
    /// </summary>
    [SerializeField][Header("Movement")]
    protected float patrolSpeed;
    /// <summary>
    /// 敌人的弹跳力。
    /// </summary>
    [SerializeField]
    private float JumpForce;
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
    /// 从发现目标到可以移动的反应时间。
    /// </summary>
    [SerializeField]
    protected float findToMoveTime;
    /// <summary>
    /// 默认物理材质。
    /// </summary>
    [SerializeField][Header("Material")]
    protected PhysicsMaterial2D defaultMaterial2D;
    /// <summary>
    /// 跳跃时使用的物理材质。
    /// </summary>
    [SerializeField]
    protected PhysicsMaterial2D jumpMaterial2D;
    /// <summary>
    /// 位于角色脚下的点的引用。以此为中心检测脚下是否有地面。
    /// </summary>
    [Header("Ground Check")]
    public Transform groundChecker;

    #endregion
    
    /// <summary>
    /// 是否可以被攻击。
    /// </summary>
    private bool canGetHit;
    /// <summary>
    /// 攻击动画是否播放完成。
    /// </summary>
    private bool getHitAnimationFinished;
    /// <summary>
    /// 是否已经死亡。
    /// </summary>
    private bool dead;
    /// <summary>
    /// 地面检测的结果。
    /// </summary>
    private bool isGround;
    
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
    
    #endregion
    
    /// <summary>
    /// 此变量用于确定打断效果是否被覆盖。当此敌人处于影响是否移动的buff下时，此值为true，BouncedOffByAttack将不再恢复敌人行动能力。
    /// </summary>
    protected bool isOverrideInterruption;
    /// <summary>
    /// 当前是否被攻击。
    /// </summary>
    protected bool isGetHit;
    /// <summary>
    /// 是否正在跳跃。用于控制人物跳跃，开始跳跃时设置为true，落地设为false。
    /// </summary>
    protected bool isJumping;
    /// <summary>
    /// 跳跃是否结束。
    /// </summary>
    protected bool isJumpEnd;
    /// <summary>
    /// 是否找到目标。
    /// </summary>
    protected bool isFindTarget;
    /// <summary>
    /// 敌人能不能移动。
    /// </summary>
    protected bool canMove;
    /// <summary>
    /// 当前目标是不是首选目标。
    /// </summary>
    protected bool isPreferred;
    /// <summary>
    /// 追到当前目标点时是否需要跳跃。
    /// </summary>
    protected bool isShouldJump;
    /// <summary>
    /// Enemy跳跃时的方向。用于与速度相乘，只能取值-1（向左），0（没有左右移动），1（向右）.
    /// </summary>
    protected sbyte jumpDirection;
    /// <summary>
    /// 当前目标路径点在路径点列表中的id。
    /// </summary>
    protected int currentRouteId;
    /// <summary>
    /// 敌人当前的真实速度。
    /// </summary>
    protected float realSpeed;
    /// <summary>
    /// 敌人到达巡逻路径点的时间。
    /// </summary>
    protected float reachTime;
    /// <summary>
    /// 获得当前目标位置的时间。用于如果目标位置无法到达时，强制更新目标位置。
    /// </summary>
    protected float getTargetTime;
    /// <summary>
    /// 等待到达目标点的极限时间。
    /// </summary>
    protected float prescribedTime = 3;
    /// <summary>
    /// 此敌人首选的追击目标。值为首选目标的Tag。
    /// </summary>
    protected string preference;
    /// <summary>
    /// 当前目标的位置。
    /// </summary>
    protected Vector2 targetPos;
    /// <summary>
    /// 跳跃时收到的向上的力的向量。
    /// </summary>
    protected Vector2 jumpForceVector;
    /// <summary>
    /// 当前的路径点列表。
    /// </summary>
    protected List<Vector2> route;
    /// <summary>
    /// 当前敌人的Animator。
    /// </summary>
    protected Animator enemyAnimator;
    /// <summary>
    /// 当前敌人的Rigidbody2D。
    /// </summary>
    protected Rigidbody2D rb;
    /// <summary>
    /// 控制当前敌人攻击的对象的碰撞器，用于控制敌人能不能发起攻击。
    /// </summary>
    protected Collider2D attacker;
    /// <summary>
    /// 当前敌人的视野对象。
    /// </summary>
    protected EnemyVision vision;
    /// <summary>
    /// 动画控制参数StartJump的id值。
    /// </summary>
    protected static readonly int startJump = Animator.StringToHash("StartJump");
    
    /// <summary>
    /// 地面检测圈的半径。
    /// </summary>
    public float checkRadius;
    /// <summary>
    /// 地面所在图层。
    /// </summary>
    public LayerMask groundLayer;
    
    /// <summary>
    /// 当前敌人的Animator。
    /// </summary>
    public Animator AnimatorGetter { get => enemyAnimator; }
    /// <summary>
    /// 当前敌人的Rigidbody2D。
    /// </summary>
    public Rigidbody2D RigidbodyGetter { get => rb; }
    /// <summary>
    /// 是否找到目标。
    /// </summary>
    public bool FindTarget { set { isFindTarget = value; } } 
    /// <summary>
    /// 设置当前敌人能不能被攻击。
    /// </summary>
    public bool CanMoveSetter { set => canMove = value; }
    /// <summary>
    /// 当前敌人的视野对象。
    /// </summary>
    public EnemyVision VisionGetter { get => vision; }
    /// <summary>
    /// 当前目标是不是首选目标。
    /// </summary>
    public bool IsPreferredSetter { set => isPreferred = value; }
    /// <summary>
    /// 当前目标的位置。
    /// </summary>
    public Vector2 TargetPosSetter { set => targetPos = value; }
    /// <summary>
    /// 获得当前目标位置的时间。用于如果目标位置无法到达时，强制更新目标位置。
    /// </summary>
    public float GetTargetTimeSetter { set => getTargetTime = value; }
    /// <summary>
    /// 追到当前目标点时是否需要跳跃。
    /// </summary>
    public bool ShouldJumpSetter { set => isShouldJump = value; }
    /// <summary>
    /// 只读，返回Player是否在地上。
    /// </summary>
    public bool IsGround { get => isGround; }

    
    public override void Awake()
    {
        base.Awake();
        realSpeed = patrolSpeed;
        attacker = transform.GetChild(2).GetComponent<Collider2D>();
        vision = transform.GetChild(1).GetComponent<EnemyVision>();
        enemyAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        route = new List<Vector2>();
        canGetHit = true;
        canMove = true;
        jumpForceVector = new Vector2(0, JumpForce);
        rb.sharedMaterial = defaultMaterial2D;
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
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => isGetHit));
        //受伤状态结束后将进入巡逻状态或者死亡状态。
        tempStateTranslations = AllStates["GitHit"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Dead",(() => dead)));
        tempStateTranslations.Add(new FSMTranslation("Patrol", () => getHitAnimationFinished));
        //当敌人死亡后，状态机将停留在死亡状态，不能向其他状态转换，所以不添加转换条件。
    }

    private void FixedUpdate()
    {
        GroundCheck();
        ChangeDirection();
        enemyAnimator.SetBool(ONGround,IsGround);
        enemyAnimator.SetFloat(VelocityX,Mathf.Abs(rb.velocity.x));
        enemyAnimator.SetFloat(VelocityY,rb.velocity.y);
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
        if (rb.velocity.x > 0.1)
        {
            transform.rotation = Quaternion.Euler(0,180,0);
        }
        else if (rb.velocity.x < -0.1)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
        }
    }
    
    /// <summary>
    /// 在巡逻时，进行移动的方法。
    /// </summary>
    protected void PatrolMove()
    {
        if (route.Count == 0)
        {
            //暂时没有找到路径，则跳过移动。
            return;
        }
        if (reachTime + PatrolIdleTime < Time.time)
        {
            if (Mathf.Abs(GetCurrentPatrolPoint().x - transform.position.x) <= 0.4)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                reachTime = Time.time;
                currentRouteId = currentRouteId + 1 >= route.Count ? 0 : currentRouteId + 1;
                return;
            }
            if (GetCurrentPatrolPoint().x - transform.position.x > 0)
            {
                rb.velocity = new Vector2(realSpeed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(-realSpeed, rb.velocity.y);
            }
        }
    }
    
    /// <summary>
    /// 敌人初始化或者到达新地点时，获得新的巡逻点列表的方法。
    /// </summary>
    private void GetRoute()
    {
        route.Clear();
        Collider2D[] routes = Physics2D.OverlapCircleAll(groundChecker.position, checkRadius, routeLayerMask);
        if (routes.Length != 0)
        {
            List<float> routesX= routes[Random.Range(0,routes.Length)].GetComponent<Route>().GetRoute();
            for (int i = 0; i < routesX.Count; i++)
            {
                route.Add(new Vector2(routesX[i],transform.position.y));
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
            if (route.Count > 0 || !CurrentState.stateName.Equals("Patrol"))
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
        if (route.Count == 0)
        {
            //如果找不到路径，将持续尝试。
            StartCoroutine(OnCanNotGetRoute());
        }
        reachTime = Time.time;
        currentRouteId = 0;
    }
    
    /// <summary>
    /// 返回当前作为目标的巡逻点。
    /// </summary>
    /// <returns>当前作为目标的巡逻点</returns>
    private Vector2 GetCurrentPatrolPoint()
    {
        return route[currentRouteId];
    }
    
    #endregion

    #region 追击状态用

    /// <summary>
    /// 给予敌人反射弧时间。
    /// </summary>
    protected virtual IEnumerator WaitForStartMove()
    {
        rb.velocity = Vector2.zero;
        canMove = false;
        yield return new WaitForSeconds(findToMoveTime);
        canMove = true;
    }
    
    /// <summary>
    /// 攻击动画播放完成，可以进行下一次攻击。动画系统调用。
    /// </summary>
    public void HitAnimationFinished()
    {
        //可以进行下一次攻击。
        attacker.GetComponent<EnemyAttack>().CanHitSetter = true;
        //报告攻击完成后目标位置。
        vision.ReportTargetPos();
        //攻击完成后可以移动。
        canMove = true;
    }

    /// <summary>
    /// 跳跃动画完成，可以获取下一个目标点。
    /// </summary>
    public void JumpAnimationFinished()
    {
        if (isJumping)
        {
            isJumpEnd = true;
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
        isGround = Physics2D.OverlapCircle(groundChecker.position, checkRadius, groundLayer);
        if (isGround)
        {
            rb.gravityScale = 1;
        }
        else
        {
            rb.gravityScale = 4;
        }
    }
    
    #endregion

    #region 受伤与死亡
    
    /// <inheritdoc />
    public void GetHit(int damage,Vector2? force = null)
    {
        if (canGetHit)
        {
            StartCoroutine(BouncedOffByAttack(force ?? Vector2.zero));
            //如果防御力大于受到伤害，则不受伤害。
            int real = damage - defense;
            if (real <= 0)
            {
                return;
            }
            //受到伤害。
            isGetHit = true;
            canGetHit = false;
            health = health - real;
            //如果此次受伤导致死亡，将标记为已死亡。
            if (health <= 0)
            {
                //TODO:避免再次受到伤害。
                health = 0;
                dead = true;
            }
        }
    }

    /// <summary>
    /// 用于实现游戏对象被攻击时受力弹开的效果的协程。如果此敌人拥有抗打断能力，则不受影响。
    /// </summary>
    /// <param name="force">游戏对象受到的作用力。</param>
    IEnumerator BouncedOffByAttack(Vector2 force)
    {
        if (antiInterruption)
        {
            yield break;
        }
        canMove = false;
        rb.velocity = Vector2.zero;
        rb.AddForce(force,ForceMode2D.Impulse);
        yield return new WaitForSeconds(.5f);
        if (!isOverrideInterruption)
        {
            canMove = true;
        }
    }
    
    /// <summary>
    /// 播放受伤动画。保持在受伤状态时执行。
    /// </summary>
    public void StartGitHitAnimation()
    {
        if (isGetHit)
        {
            enemyAnimator.SetTrigger(GETHitId);
            isGetHit = false;
        }
    }

    /// <summary>
    /// 受伤动画播放完成。动画系统调用。
    /// </summary>
    public void GetHitAnimationFinished()
    {
        getHitAnimationFinished = true;
    }

    /// <summary>
    /// 在Enemy受伤状态结束后时执行。
    /// </summary>
    public void GetHitStateExit()
    {
        isGetHit = false;
        getHitAnimationFinished = false;
        if (!dead)
        {
            canGetHit = true;
        }
    }
    
    /// <inheritdoc />
    public void Dead()
    {
        enemyAnimator.SetTrigger(DeadId);
        isOverrideInterruption = true;
    }
    
    #endregion
}
