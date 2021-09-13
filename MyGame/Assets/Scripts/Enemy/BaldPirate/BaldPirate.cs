using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

public class BaldPirate : Enemy
{
    /// <summary>
    /// 追逐玩家或炸弹时的速度。
    /// </summary>
    [SerializeField][Header("Personalization")]
    private float chasingSpeed;
    
    public override void Awake()
    {
        base.Awake();
        preference = "Player";
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
