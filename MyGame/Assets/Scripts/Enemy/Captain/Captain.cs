using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

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
    /// 如果进入发现炸弹状态，将要等待的时间。
    /// </summary>
    [SerializeField]
    private float findBombStateWaitTime;
    /// <summary>
    /// 逃离炸弹时奔跑的方向。只能使用-1（向左），0（不动），1（向右）。
    /// </summary>
    private sbyte _runDirection;

    private static readonly int FindBomb = Animator.StringToHash("FindBomb");

    public override void Awake()
    {
        base.Awake();
        Preference = "Bomb";
        //设置此敌人的首选目标。
        Vision.Preference = Preference;
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
        tempStateTranslations.Add(new FSMTranslation("FindTarget",() => _findTarget));
        //找到目标状态中受伤转换到受伤状态。
        tempStateTranslations = AllStates["FindTarget"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => _getHit));
        //找到目标状态转换到巡逻状态。
        tempStateTranslations.Add(new FSMTranslation("Patrol",() => !_findTarget));
        //找到目标状态转换到发现炸弹状态。
        tempStateTranslations.Add(new FSMTranslation("FindBomb",() => IsPreferred));
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
        _findBombStateStartTime = Time.time;
        AnimatorGetter.SetBool(FindBomb,true);
    }
    
    /// <summary>
    /// 结束发现炸弹状态。
    /// </summary>
    private void EndFindBombState()
    {
        //重置结束发现炸弹状态的条件。
        _isFindBombStateOver = false;
        AnimatorGetter.SetBool(FindBomb,false);
        Vision.ReportTargetPos();
    }
    
    /// <summary>
    /// 发现炸弹后，反向逃跑。结束后设置结束状态的标志。
    /// </summary>
    private void ScareRun()
    {
        if (_findBombStateStartTime + findBombStateWaitTime > Time.time)
        {
            if (IsPreferred)
            {
                if (TargetPos.x - transform.position.x < 0)
                {
                    _runDirection = 1;
                }
                else
                {
                    _runDirection = -1;
                
                }
            }
            _rb.velocity = new Vector2(_runDirection * Speed, _rb.velocity.y);
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
