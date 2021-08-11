using UnityEngine;

/// <summary>
/// 所有能够被使用的对象应实现此接口。
/// </summary>
public interface IUsable
{
    /// <summary>
    /// 最后一次被使用的时间。
    /// </summary>
    /// <remarks>
    /// 初次使用为场景加载时间。
    /// </remarks>
    float LastTime { get; set; }
    /// <summary>
    /// 使用冷却时间。
    /// </summary>
    float CoolingTime { get; set; }

    /// <summary>
    /// 获取挂载实现本接口的脚本的游戏对象。
    /// </summary>
    /// <returns>当前脚本对应的游戏对象。</returns>
    GameObject GetGameObject();

    /// <summary>
    /// 对象被使用时执行。
    /// </summary>
    /// <param name="user">使用此物品的用户。</param>
    void Use(GameObject user);

    /// <summary>
    /// 物品的特殊用法，例如大炮可以转向。
    /// </summary>
    /// <param name="user">使用此物品的用户。</param>
    void SpecialUse(GameObject user);
}
