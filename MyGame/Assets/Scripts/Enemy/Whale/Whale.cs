using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

/// <summary>
/// 用于管理敌人——鲸鱼的AI逻辑。
/// </summary>
/// <remarks>
/// 基本逻辑：
/// 发现玩家，加速追击。
/// 发现炸弹，加速吃掉，吃掉后巡逻和追击速度下降，一定值后死亡。
/// 长时间未吃到炸弹将恢复状态。
/// </remarks>
public class Whale : Enemy
{
    /// <summary>
    /// 追逐玩家或炸弹时的速度。
    /// </summary>
    [SerializeField][Header("Personalization")]
    private float chasingSpeed;
    /// <summary>
    /// 鲸鱼吃下炸弹到速度恢复一级所需时间。
    /// </summary>
    [SerializeField]
    private float RecoverTime;
    
    /// <summary>
    /// 此敌人被减速的等级。10代表未减速，0代表完全减速。
    /// </summary>
    private int decelerateLevel = 10;
    /// <summary>
    /// 上一次被减速的时间。
    /// </summary>
    private float LastDecelerateTime;
    /// <summary>
    /// 当前巡逻速度。
    /// </summary>
    private float currentPatrolSpeed;
    /// <summary>
    /// 当前追击速度。
    /// </summary>
    private float currentChasingSpeed;
    
    public override void Awake()
    {
        base.Awake();
        preference = "Bomb";
        currentPatrolSpeed = patrolSpeed;
        currentChasingSpeed = chasingSpeed;
        //设置此敌人的首选目标。
        vision.Preference = preference;
        //添加状态。
        AllStates.Add("FindTarget",new FSMState("FindTarget"));
        //添加每个状态要执行的内容。
        FSMState tempState;
        tempState = AllStates["FindTarget"];
        tempState.OnStateEnter += GetReady;
        tempState.OnStateStay += MoveToTarget;
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
    }

    public override void Update()
    {
        base.Update();
        if (decelerateLevel < 10 && decelerateLevel != 1 && LastDecelerateTime + RecoverTime < Time.time)
        {
            Recover();
        }
    }

    /// <summary>
    /// 进入发现目标的状态后，打开攻击触发器，准备攻击；并获取目标位置。
    /// </summary>
    private void GetReady()
    {
        attacker.enabled = true;
        rb.sharedMaterial = jumpMaterial2D;
        realSpeed = currentChasingSpeed;
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
        attacker.enabled = false;
        realSpeed = currentPatrolSpeed;
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

    /// <summary>
    /// 由鲸鱼攻击方法调用，用于实现鲸鱼减速功能。
    /// </summary>
    public void Decelerate()
    {
        LastDecelerateTime = Time.time;
        decelerateLevel -= 3;
        if (decelerateLevel == 1)
        {
            StartCoroutine(DelayedDeath());
        }
        currentChasingSpeed = 0.5f * currentChasingSpeed;;
        currentPatrolSpeed = 0.5f * currentPatrolSpeed;
        if (CurrentState.stateName == "Patrol")
        {
            realSpeed = currentPatrolSpeed;
        }
        else
        {
            realSpeed = currentChasingSpeed;
        }
    }

    /// <summary>
    /// 实现鲸鱼吞下足够炸弹后，延迟死亡效果。
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(2f);
        this.GetHit(Int32.MaxValue);
    }
    
    /// <summary>
    /// 在update中调用，用于恢复鲸鱼速度功能。
    /// </summary>
    public void Recover()
    {
        LastDecelerateTime = Time.time;
        decelerateLevel += 3;
        currentChasingSpeed = 1.5f * currentChasingSpeed;
        currentPatrolSpeed = 1.5f * currentPatrolSpeed;
        if (CurrentState.stateName == "Patrol")
        {
            realSpeed = currentPatrolSpeed;
        }
        else
        {
            realSpeed = currentChasingSpeed;
        }
    }
}
