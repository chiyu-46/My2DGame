using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此类控制跳跃时的视觉特效。
/// </summary>
public class JumpVFX : MonoBehaviour
{
    /// <summary>
    /// 动画播放完成后将用于播放动画的GameObject关闭。
    /// </summary>
    public void Finish()
    {
        gameObject.SetActive(false);
    }
}
