using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

public class BigGuy : Enemy
{
    /// <summary>
    /// 追击玩家的方向。只能使用-1（向左），0（不动），1（向右）。
    /// </summary>
    private sbyte _directionOfPursuit;

    /// <summary>
    /// 此敌人用于控制手中炸弹的关节。
    /// </summary>
    private FixedJoint2D _fixedJoint;
    /// <summary>
    /// 用于获取炸弹的炸弹池。
    /// </summary>
    [SerializeField]
    private BombPool bombPool;
    /// <summary>
    /// 此敌人手中的炸弹。
    /// </summary>
    private Bomb _bomb;
    public Bomb bombGetter { get => _bomb; }
    
    public override void Awake()
    {
        base.Awake();
        _fixedJoint = transform.GetChild(3).GetComponent<FixedJoint2D>();
        Preference = "Player";
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
        tempStateTranslations.Add(new FSMTranslation("FindTarget",() => IsPreferred));
        //找到目标状态中受伤转换到受伤状态。因为基础类中，只有受伤状态能进入死亡状态。此类敌人一击必杀，而且出击后必死，不需要返回巡逻状态。
        tempStateTranslations = AllStates["FindTarget"].FsmTranslations;
        tempStateTranslations.Add(new FSMTranslation("GitHit",() => _getHit));
    }

    private void Start()
    {
        StartCoroutine(GetBomb());
    }

    IEnumerator GetBomb()
    {
        while (true)
        {
            GameObject bomb;
            bombPool.Allocate(out bomb);
            if (!(bomb is null))
            {
                bomb.SetActive(true);
                bomb.transform.position = transform.position;
                _fixedJoint.connectedBody = bomb.GetComponent<Rigidbody2D>();
                _fixedJoint.enabled = true;
                _bomb = bomb.GetComponent<Bomb>();
                _bomb.Extinguish();
                _bomb.State = Bomb.BombState.IsHeld;
                yield break;
            }

            yield return null;
        }
    }
    
    /// <summary>
    /// 进入发现目标的状态后，打开攻击触发器，准备攻击；并获取目标位置。
    /// </summary>
    private void GetReady()
    {
        _attacker.enabled = true;
        //点燃炸弹。
        _bomb.Ignite();
        //确定追击方向。
        if (TargetPos.x - transform.position.x < 0)
        {
            _directionOfPursuit = -1;
        }
        else
        {
            _directionOfPursuit = 1;
        }
    }
    
    /// <summary>
    /// 退出发现目标的状态后，关闭攻击触发器，不再攻击。
    /// </summary>
    /// <remarks>
    /// 默认情况下攻击触发器为不启用状态。这种设计可以避免在受伤死亡等状态进入时更改攻击触发器状态。
    /// </remarks>
    private void Relax()
    {
        _bomb = null;
    }

    /// <summary>
    /// 移动到目标位置。
    /// </summary>
    private void MoveToTarget()
    {
        //攻击时不能移动。
        if (CanMove)
        {
            _rb.velocity = new Vector2(_directionOfPursuit * Speed, _rb.velocity.y);
        }
    }
}
