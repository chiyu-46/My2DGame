/// <summary>
/// 可以作为对象池中的对象的类应实现此接口。
/// </summary>
public interface IPoolAble
{
    /// <summary>
    /// 该对象是否已经被回收，此值用于避免重复回收。
    /// </summary>
    bool IsRecycled { get; set; }
    
    /// <summary>
    /// 执行回收，将自己归还对象池。
    /// </summary>
    void OnRecycled();
}
