using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 当前场景的UI管理器。
    /// </summary>
    private UIManager uiManager;
    /// <summary>
    /// 所有需要由GameManager主动获取的对象的数量。
    /// </summary>
    private int allNeedGetNum = 1;
    /// <summary>
    /// 当前场上敌人数量。由场上敌人主动注册获得，敌人死后应主动取消注册。
    /// <para>如果为Null，说明暂时没有得到注册。一些需要此参数的函数应暂时跳过执行。</para>
    /// </summary>
    private int? EnemyNum;
    /// <summary>
    /// 当前场上Player数量。由场上Player主动注册获得，Player死后应主动取消注册。
    /// <para>如果为Null，说明暂时没有得到注册。一些需要此参数的函数应暂时跳过执行。</para>
    /// </summary>
    private int? PlayerNum;
    /// <summary>
    /// 当前是否处于已经有协程等待生命值UI的状态。
    /// </summary>
    private bool isWaitForHealthBar;
    
    private void Start()
    {
        StartCoroutine(GetRequirementItems());
    }

    /// <summary>
    /// 避免GameManager获取对象为空的协程，如果存在空对象则持续尝试获取此对象。
    /// </summary>
    private IEnumerator GetRequirementItems()
    {
        bool isAllDone = false;
        int doneNum = 0;
        while (!isAllDone)
        {
            //依次尝试获取每一种组件，成功则已获取数量加一。
            if (uiManager is null && !(GameObject.Find("Default Canvas") is null))
            {
                uiManager = GameObject.Find("Default Canvas").GetComponent<UIManager>();
                if (!(uiManager is null))
                {
                    doneNum++;
                }
            }
            //如果已获取的组件数目达到最大数量，即已经获取所有应获取组件，则退出协程。
            if (doneNum >= allNeedGetNum)
            {
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 如果生命值UI组件注册时间晚于需要调用它的时间，将使用此协程记录请求信息。
    /// </summary>
    /// <param name="value">当前生命值。</param>
    private IEnumerator WaitForHealthBar(int value)
    {
        while (true)
        {
            yield return new WaitForSeconds(.25f);
            if (UpdateHealthPoint(value) == 0)
            {
                yield break;
            }
        }
    }
    
    /// <summary>
    /// 作为Player与生命值UI中介的函数。
    /// </summary>
    /// <param name="value">当前生命值。</param>
    public int UpdateHealthPoint(int value)
    {
        if (!(uiManager is null))
        {
            uiManager.HealthBar.UpdateHealthPoint(value);
            return 0;
        }
        else
        {
            if (isWaitForHealthBar)
            {
                StopCoroutine($"WaitForHealthBar");
                isWaitForHealthBar = false;
            }
            StartCoroutine(WaitForHealthBar(value));
            isWaitForHealthBar = true;
        }
        return -1;
    }

    /// <summary>
    /// 需要主动注册的对象的注册入口。
    /// </summary>
    /// <param name="registrant">注册者自己。</param>
    public void Register(GameObject registrant)
    {
        if (registrant.GetComponent<PlayerController>())
        {
            PlayerNum ??= 0;
            PlayerNum++;
        }
    }
    
    /// <summary>
    /// 需要主动注销的对象的注销入口。
    /// </summary>
    /// <param name="cancelled">注销者自己。</param>
    public void Cancel(GameObject cancelled)
    {
        
    }

    public void Save()
    {
        
    }
    
    
    public void Load()
    {
        
    }
    
}
