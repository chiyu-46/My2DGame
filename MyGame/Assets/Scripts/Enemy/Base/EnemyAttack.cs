using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此类控制敌人攻击，可攻击对象进入攻击范围即发起攻击，当前能不能发起攻击，由Enemy类的FSM，控制触发器是否启用控制。
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    /// <summary>
    /// 拥有此视野的敌人，相当于大脑。
    /// </summary>
    protected Enemy Head;
    /// <summary>
    /// 当前敌人的视野对象。
    /// </summary>
    protected EnemyVision Vision;
    /// <summary>
    /// 当前敌人的Animator。
    /// </summary>
    protected Animator EnemyAnimator;
    /// <summary>
    /// 当前是否应当攻击。用于避免在攻击动画完成前发起下一次攻击。
    /// </summary>
    protected bool CanHit;
    /// <summary>
    /// 当前是否应当攻击。用于避免在攻击动画完成前发起下一次攻击。
    /// </summary>
    public bool CanHitSetter { set => CanHit = value; }
    /// <summary>
    /// 此敌人的攻击力。
    /// </summary>
    [SerializeField]
    protected int _attack;
    /// <summary>
    /// 敌人攻击目标时给予目标的作用力向量。
    /// </summary>
    [SerializeField]
    protected float attackForce;
    /// <summary>
    /// 此敌人正前方的点。
    /// </summary>
    [SerializeField]
    protected Transform frontPoint;
    
    protected static readonly int AttackToBomb = Animator.StringToHash("AttackToBomb");
    protected static readonly int AttackToPlayer = Animator.StringToHash("AttackToPlayer");

    private void Start()
    {
        Head = transform.parent.GetComponent<Enemy>();
        EnemyAnimator = Head.AnimatorGetter;
        Vision = Head.VisionGetter;
        CanHit = true;
    }
}
