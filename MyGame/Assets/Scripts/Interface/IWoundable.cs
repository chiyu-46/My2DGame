using UnityEngine;

/// <summary>
/// 可以被攻击受到伤害的对象应当实现此接口。
/// </summary>
public interface IWoundable
{
    /// <summary>
    /// 受到伤害。
    /// </summary>
    /// <param name="damage">受到的伤害值。</param>
    /// <param name="force">受到攻击时受到的力向量。</param>
    void GetHit(int damage,Vector2? force = null);
    /// <summary>
    /// 死亡。
    /// </summary>
    void Dead();
}
