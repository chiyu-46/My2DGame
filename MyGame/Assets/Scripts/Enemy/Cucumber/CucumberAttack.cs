using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CucumberAttack : EnemyAttack
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (CanHit)
        {
            if (other.CompareTag("Player") && !other.GetComponent<PlayerController>().IsDead)
            {
                EnemyAnimator.SetTrigger(AttackToPlayer);
                //TODO:在攻击时刻进行攻击，而不是直接判定进入攻击范围就受到攻击。
                other.GetComponent<PlayerController>().GetHit(_attack);
            }
            else if(other.CompareTag("Bomb") && other.GetComponent<Bomb>().State == Bomb.BombState.BombOn)
            {
                EnemyAnimator.SetTrigger(AttackToBomb);
                other.GetComponent<Bomb>().Extinguish();
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
