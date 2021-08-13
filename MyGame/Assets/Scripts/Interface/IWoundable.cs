/// <summary>
/// 可以被攻击受到伤害的对象应当实现此接口。
/// </summary>
public interface IWoundable
{
    /// <summary>
    /// 生命值。
    /// </summary>
    int Health { get; set; }
    /// <summary>
    /// 防御力。
    /// </summary>
    int Defense { get; set; }
    
    /// <summary>
    /// 此对象能否被攻击。用于实现对象无敌状态。
    /// </summary>
    bool CanGetHit { get; set; }
    
    /// <summary>
    /// 受到伤害。
    /// </summary>
    /// <param name="damage">受到的伤害值。</param>
    void GetHit(int damage);
    /// <summary>
    /// 死亡。
    /// </summary>
    void Dead();
}
