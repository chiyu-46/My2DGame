using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于游戏其他部分向UI展示内容。
/// </summary>
public interface IUIDisplay
{
    /// <summary>
    /// 如果游戏其他部分产生的信息需要让玩家知道，则调用此方法。
    /// </summary>
    /// <param name="message">需要玩家知道的消息。</param>
    void ShowMessage(string message);
    /// <summary>
    /// 如果游戏其他部分产生的没有立即结束进程但无法继续玩下去的错误，则调用此方法，在UI界面展示错误信息。
    /// </summary>
    /// <param name="message">错误信息。</param>
    void ShowError(string message);
}
