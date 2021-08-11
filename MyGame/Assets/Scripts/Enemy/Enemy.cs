using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IWoundable
{
    [Header("Base")][SerializeField] 
    private int health;
    [SerializeField] 
    private int defense;
    /// <inheritdoc />
    public int Health { get => health; set => health = value; }
    /// <inheritdoc />
    public int Defense { get => defense; set => defense = value; }

    /// <inheritdoc />
    public void GetHit(int damage)
    {
        //TODO:播放受伤动画。
        //如果防御力大于受到伤害，则不受伤害。
        int real = damage - Defense;
        if (real <= 0)
        {
            return;
        }
        //受到伤害。
        Health = Health - real;
        //如果此次受伤导致死亡，将调用Dead方法。
        if (Health <= 0)
        {
            //TODO:避免再次受到伤害。
            Health = 0;
            Dead();
        }
    }

    /// <inheritdoc />
    public void Dead()
    {
        //TODO:显示死亡动画。
        
    }
}
