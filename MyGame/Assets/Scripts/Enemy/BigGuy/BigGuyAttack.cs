using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGuyAttack : EnemyAttack
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (CanHit)
        {
            if (other.CompareTag("Player") && !other.GetComponent<PlayerController>().IsDead)
            {
                //不给予伤害，直接由炸弹攻击。
                transform.parent.GetComponent<BigGuy>().bombGetter.Ignite(0);
            }
            else
            {
                //如果不攻击，则不进行下面的操作。
                return;
            }
            //不能每帧开始一次攻击，要等待上一次进攻完成。
            CanHit = false;
            //攻击时不能移动。
            Head.CanMoveSetter = false;
            Head.RigidbodyGetter.velocity = new Vector2(0, Head.RigidbodyGetter.velocity.y);
        }
    }
}
