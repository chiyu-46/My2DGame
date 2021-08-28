using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

public class Whale : Enemy
{
    public override void Awake()
    {
        base.Awake();
        Preference = "Bomb";
        //设置此敌人的首选目标。
        Vision.Preference = Preference;
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
        tempStateTranslations.Add(new FSMTranslation("FindTarget",() => _findTarget));
        //找到目标状态中受伤转换到受伤状态。
        tempStateTranslations = AllStates["FindTarget"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => _getHit));
        //找到目标状态转换到巡逻状态。
        tempStateTranslations.Add(new FSMTranslation("Patrol",() => !_findTarget));
    }

    /// <summary>
    /// 进入发现目标的状态后，打开攻击触发器，准备攻击；并获取目标位置。
    /// </summary>
    private void GetReady()
    {
        _attacker.enabled = true;
        _rb.sharedMaterial = jumpMaterial2D;
    }
    
    /// <summary>
    /// 退出发现目标的状态后，关闭攻击触发器，不再攻击。
    /// </summary>
    /// <remarks>
    /// 默认情况下攻击触发器为不启用状态。这种设计可以避免在受伤死亡等状态进入时更改攻击触发器状态。
    /// </remarks>
    private void Relax()
    {
        _attacker.enabled = false;
        Vision.GetComponent<Collider2D>().enabled = true;
        _rb.sharedMaterial = defaultMaterial2D;
    }

    /// <summary>
    /// 移动到目标位置。
    /// </summary>
    private void MoveToTarget()
    {
        //如果正在跳跃，将只执行此if。
        if (IsJumping)
        {
            if (JumpEnd)
            {
                JumpEnd = false;
                IsJumping = false;
                Vision.GetComponent<Collider2D>().enabled = true;
                Vision.ReportTargetPos();
            }
            _rb.velocity = new Vector2(JumpDirection * Speed, _rb.velocity.y);
            return;
        }
        //攻击时不能移动。
        if (CanMove)
        {
            //如果目标在极限时间，无法达到，强制更新目标位置。
            if (Time.time - GetTargetTime > PrescribedTime)
            {
                Vision.ReportTargetPos();
            }
            if (TargetPos.x - transform.position.x > 0.1)
            {
                //向右追
                _rb.velocity = new Vector2(Speed, _rb.velocity.y);
            }
            else if(TargetPos.x - transform.position.x < -0.1)
            {
                //向左追
                _rb.velocity = new Vector2(-Speed, _rb.velocity.y);
            }
            else
            {
                if (ShouldJump)
                {
                    if (_rb.velocity.x > 0)
                    {
                        JumpDirection = 1;
                    }
                    else
                    {
                        JumpDirection = -1;
                    }
                    ShouldJump = false;
                    _rb.AddForce(JumpForceVector,ForceMode2D.Impulse);
                    EnemyAnimator.SetTrigger(StartJump);
                    IsJumping = true;
                    Vision.GetComponent<Collider2D>().enabled = false;
                    return;
                }
                //到达目标位置，获取下一个目标位置。
                Vision.ReportTargetPos();
            }
        }
    }
}
