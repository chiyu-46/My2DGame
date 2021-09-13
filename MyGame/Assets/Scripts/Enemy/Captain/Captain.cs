using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

/// <summary>
/// 用于管理敌人——船长的AI逻辑。
/// </summary>
/// <remarks>
/// 基本逻辑：
/// 发现玩家，加速追击。
/// 发现炸弹，转头逃跑。
/// </remarks>
public class Captain : Enemy
{
    /// <summary>
    /// 如果进入发现炸弹状态，此值为true则可以结束此状态。
    /// </summary>
    private bool _isFindBombStateOver;
    /// <summary>
    /// 发现炸弹状态进入的时间。
    /// </summary>
    private float _findBombStateStartTime;
    /// <summary>
    /// 如果进入发现炸弹状态，保持慌张逃窜状态的时间。
    /// </summary>
    [SerializeField][Header("Personalization")]
    private float findBombStateWaitTime;
    /// <summary>
    /// 逃离炸弹时奔跑的方向。只能使用-1（向左），0（不动），1（向右）。
    /// </summary>
    private sbyte _runDirection;
    /// <summary>
    /// 遇到炸弹时的速度。
    /// </summary>
    [SerializeField]
    private float escapeSpeed;
    /// <summary>
    /// 追逐玩家时的速度。
    /// </summary>
    [SerializeField]
    private float chasingSpeed;

    private static readonly int FindBomb = Animator.StringToHash("FindBomb");

    public override void Awake()
    {
        base.Awake();
        preference = "Bomb";
        //设置此敌人的首选目标。
        vision.Preference = preference;
        //添加状态。
        AllStates.Add("FindTarget",new FSMState("FindTarget"));
        AllStates.Add("FindBomb",new FSMState("FindBomb"));
        //添加每个状态要执行的内容。
        FSMState tempState;
        tempState = AllStates["FindTarget"];
        tempState.OnStateEnter += GetReady;
        tempState.OnStateStay += MoveToTarget;
        tempState.OnStateExit += Relax;
        tempState = AllStates["FindBomb"];
        tempState.OnStateEnter += StartFindBombState;
        tempState.OnStateStay += ScareRun;
        tempState.OnStateExit += EndFindBombState;
        tempState.OnStateExit += Relax;
        //添加转换条件。
        List<FSMTranslation> tempStateTranslations;
        //巡逻状态转换到找到目标。
        tempStateTranslations = AllStates["Patrol"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("FindTarget",() => isFindTarget));
        //找到目标状态中受伤转换到受伤状态。
        tempStateTranslations = AllStates["FindTarget"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => isGetHit));
        //找到目标状态转换到巡逻状态。
        tempStateTranslations.Add(new FSMTranslation("Patrol",() => !isFindTarget));
        //找到目标状态转换到发现炸弹状态。
        tempStateTranslations.Add(new FSMTranslation("FindBomb",() => isPreferred));
        //发现炸弹状态转换到巡逻状态。
        tempStateTranslations = AllStates["FindBomb"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("Patrol",() => _isFindBombStateOver));
    }

    #region 发现炸弹状态

    /// <summary>
    /// 开始发现炸弹状态。
    /// </summary>
    private void StartFindBombState()
    {
        realSpeed = escapeSpeed;
        _findBombStateStartTime = Time.time;
        AnimatorGetter.SetBool(FindBomb,true);
    }
    
    /// <summary>
    /// 结束发现炸弹状态。
    /// </summary>
    private void EndFindBombState()
    {
        realSpeed = patrolSpeed;
        //重置结束发现炸弹状态的条件。
        _isFindBombStateOver = false;
        AnimatorGetter.SetBool(FindBomb,false);
        vision.ReportTargetPos();
    }
    
    /// <summary>
    /// 发现炸弹后，反向逃跑。结束后设置结束状态的标志。
    /// </summary>
    private void ScareRun()
    {
        if (_findBombStateStartTime + findBombStateWaitTime > Time.time)
        {
            if (isPreferred)
            {
                if (targetPos.x - transform.position.x < 0)
                {
                    _runDirection = 1;
                }
                else
                {
                    _runDirection = -1;
                
                }
            }
            rb.velocity = new Vector2(_runDirection * realSpeed, rb.velocity.y);
        }
        else
        {
            _isFindBombStateOver = true;
        }
    }

    #endregion
    
    
    /// <summary>
    /// 进入发现目标的状态后，打开攻击触发器，准备攻击；并获取目标位置。
    /// </summary>
    private void GetReady()
    {
        realSpeed = chasingSpeed;
        attacker.enabled = true;
        rb.sharedMaterial = jumpMaterial2D;
        StartCoroutine(WaitForStartMove());
    }
    
    /// <summary>
    /// 退出发现目标的状态后，关闭攻击触发器，不再攻击。
    /// </summary>
    /// <remarks>
    /// 默认情况下攻击触发器为不启用状态。这种设计可以避免在受伤死亡等状态进入时更改攻击触发器状态。
    /// </remarks>
    private void Relax()
    {
        realSpeed = patrolSpeed;
        attacker.enabled = false;
        vision.GetComponent<Collider2D>().enabled = true;
        rb.sharedMaterial = defaultMaterial2D;
    }

    /// <summary>
    /// 移动到目标位置。
    /// </summary>
    private void MoveToTarget()
    {
        //如果正在跳跃，将只执行此if。
        if (isJumping)
        {
            if (isJumpEnd)
            {
                isJumpEnd = false;
                isJumping = false;
                vision.GetComponent<Collider2D>().enabled = true;
                vision.ReportTargetPos();
            }
            rb.velocity = new Vector2(jumpDirection * realSpeed, rb.velocity.y);
            return;
        }
        //攻击时不能移动。
        if (canMove)
        {
            //如果目标在极限时间，无法达到，强制更新目标位置。
            if (Time.time - getTargetTime > prescribedTime)
            {
                vision.ReportTargetPos();
            }
            if (targetPos.x - transform.position.x > 0.1)
            {
                //向右追
                rb.velocity = new Vector2(realSpeed, rb.velocity.y);
            }
            else if(targetPos.x - transform.position.x < -0.1)
            {
                //向左追
                rb.velocity = new Vector2(-realSpeed, rb.velocity.y);
            }
            else
            {
                if (isShouldJump)
                {
                    if (rb.velocity.x > 0)
                    {
                        jumpDirection = 1;
                    }
                    else
                    {
                        jumpDirection = -1;
                    }
                    isShouldJump = false;
                    rb.AddForce(jumpForceVector,ForceMode2D.Impulse);
                    enemyAnimator.SetTrigger(startJump);
                    isJumping = true;
                    vision.GetComponent<Collider2D>().enabled = false;
                    return;
                }
                //到达目标位置，获取下一个目标位置。
                vision.ReportTargetPos();
            }
        }
    }
}
