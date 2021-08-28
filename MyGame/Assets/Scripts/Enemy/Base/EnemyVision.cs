using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    /// <summary>
    /// 拥有此视野的敌人，相当于大脑。
    /// </summary>
    private Enemy _head;
    /// <summary>
    /// 当前拥有此视野的敌人，首选的追击目标。值为首选目标的Tag。
    /// </summary>
    private string _preference;
    /// <summary>
    /// 当前锁定的目标。
    /// </summary>
    private Transform _currentTarget;
    /// <summary>
    /// 当前锁定的目标是不是首选目标。
    /// </summary>
    private bool _isPreferred;
    /// <summary>
    /// 当前锁定目标的最后出现位置。即目标离开“视野”触发器时的位置。
    /// </summary>
    private Vector2? _lastKnownLocation;
    /// <summary>
    /// 如果目标离开视野时跳跃了，那敌人追击到此位置也应该跳跃。
    /// </summary>
    private bool _shouldJump;
    
    
    /// <summary>
    /// 当前拥有此视野的敌人，首选的追击目标。值为首选目标的Tag。
    /// </summary>
    public string Preference { set => _preference = value; }
    /// <summary>
    /// 当前锁定的目标是不是首选目标。
    /// </summary>
    public bool IsPreferred { get => _isPreferred; }

    private void Start()
    {
        _head = transform.parent.GetComponent<Enemy>();
    }

    /// <summary>
    /// 如果有被关注目标在敌人视野中时，分情况更新锁定的目标，
    /// </summary>
    /// <remarks>
    /// 如果当前没有锁定目标，则将当前目标锁定；如果有锁定目标，但不是不是首选，此时进入的新目标是首选，则更换目标。
    /// </remarks>
    /// <param name="other">进入视野的目标。</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        //如果炸弹处于未点燃状态
        if (other.CompareTag("Bomb") && other.GetComponent<Bomb>().State != Bomb.BombState.BombOn)
        {
            //如果炸弹处于未点燃状态，并且是当前目标，则更改当前目标。
            if (_currentTarget == other.transform)
            {
                _currentTarget = null;
            }
            return;
        }
        //如果Player已死亡
        if (other.CompareTag("Player") && other.GetComponent<PlayerController>().IsDead)
        {
            //如果Player已死亡，并且是当前目标，则更改当前目标。
            if (_currentTarget == other.transform)
            {
                _currentTarget = null;
            }
            return;
        }
        if (_currentTarget is null)
        {
            _currentTarget = other.transform;
            if (other.CompareTag(_preference))
            {
                _isPreferred = true;
            }
            else
            {
                _isPreferred = false;
            }
            //找到新目标，则不再关注旧目标的最后出现位置。
            _lastKnownLocation = null;
            _shouldJump = false;
            //告知主控，已发现目标。
            _head.FindTarget = true;
            //告知主控，当前目标是否为首要目标。
            _head.IsPreferredSetter = _isPreferred;
            //告知主控，当前目标位置。
            ReportTargetPos();
        }
        else if (!_isPreferred && other.CompareTag(_preference))
        {
            _currentTarget = other.transform;
            _isPreferred = true;
            //找到新目标，则不再关注旧目标的最后出现位置。
            _lastKnownLocation = null;
            _shouldJump = false;
            //告知主控，当前目标是否为首要目标。
            _head.IsPreferredSetter = _isPreferred;
            //告知主控，当前目标位置。
            ReportTargetPos();
        }
    }

    /// <summary>
    /// 如果有对象离开视野，且此对象是当前锁定的目标，则记录目标位置，并试图寻找新目标。
    /// </summary>
    /// <param name="other">离开视野的目标。</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (_currentTarget == other.transform)
        {
            //记录最后出现位置。
            _lastKnownLocation = other.transform.position;
            _currentTarget = null;
            //如果是Player，记录是否应当跳跃。
            if (other.CompareTag("Player") && other.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.player_jump"))
            {
                _shouldJump = true;
            }
        }
    }

    /// <summary>
    /// 当敌人追击目标时，报告下一个当前敌人应当移向的位置，并返回是不是应该跳跃。
    /// </summary>
    /// <remarks>计划在攻击完成和到达上一个目标点时执行。</remarks>
    public void ReportTargetPos()
    {
        if (_currentTarget != null)
        {
            _head.TargetPosSetter = _currentTarget.position;
        }
        else if(!(_lastKnownLocation is null))
        {
            _head.TargetPosSetter =  _lastKnownLocation ?? transform.position;
            _head.ShouldJumpSetter = _shouldJump;
            _lastKnownLocation = null;
        }
        else
        {
            _head.FindTarget = false;
        }
        //设置获得目标点的时间。
        _head.GetTargetTimeSetter = Time.time;
    }
}
