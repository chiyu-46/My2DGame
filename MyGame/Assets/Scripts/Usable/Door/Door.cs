using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 门的控制类。
/// </summary>
public class Door : MonoBehaviour, IUsable
{
    /// <summary>
    /// 当前门的目标门，即此门的出口。
    /// </summary>
    public Door target;
    /// <inheritdoc />
    public float LastTime { get; set; }
    /// <inheritdoc />
    public float CoolingTime { get; set; }
    /// <inheritdoc />
    public bool CanUse { get; set; }
    /// <summary>
    /// 门当前是否是打开的状态。
    /// </summary>
    [HideInInspector]
    public bool isOpen;
    /// <summary>
    /// 当前门是不是入口。
    /// </summary>
    [HideInInspector]
    public bool isEntrance;
    /// <summary>
    /// 此门的使用者。
    /// </summary>
    private GameObject _user;
    /// <summary>
    /// 当前门的Animator。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// 获取当前门的Animator。
    /// </summary>
    public Animator Animator { get => _animator; }
    /// <summary>
    /// 动画器的触发器Open的id。
    /// </summary>
    private static readonly int Open = Animator.StringToHash("Open");
    /// <summary>
    /// 动画器的触发器InDoor的id。
    /// </summary>
    private static readonly int InDoor = Animator.StringToHash("InDoor");
    /// <summary>
    /// 动画器的触发器Close的id。
    /// </summary>
    private static readonly int Close = Animator.StringToHash("Close");
    /// <summary>
    /// 动画器的触发器OutDoor的id。
    /// </summary>
    private static readonly int OutDoor = Animator.StringToHash("OutDoor");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        isOpen = false;
        isEntrance = false;
        CanUse = true;
    }

    public void Reset()
    {
        isOpen = false;
        isEntrance = false;
        CanUse = true;
        _user = null;
    }
    
    /// <inheritdoc />
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /// <summary>
    /// 门的使用方法。
    /// </summary>
    /// <param name="user">门的使用者。</param>
    public void Use(GameObject user)
    {
        if (CanUse)
        {
            //在此次使用期间，不能再次被使用。
            CanUse = false;
            //本门的目标门，在使用结束前，不能被使用。
            target.CanUse = false;
            //注册本次使用者。
            _user = user;
            //使用者在本次使用结束前，不能移动。
            _user.GetComponent<PlayerController>().canMove = false;
            _user.GetComponent<Rigidbody2D>().isKinematic = true;
            //此门被使用，所以是入口。
            isEntrance = true;
            //Player进入使用门状态。
            _user.GetComponent<PlayerController>().currentUseDoorState = (int)PlayerController.UseDoorState.UseDoor;
            //当前门打开。
            _animator.SetTrigger(Open);
            //等待门打开。
            StartCoroutine(SecondStep());
        }
    }

    /// <summary>
    /// 等待门打开后，进行使用门的第二步。
    /// </summary>
    /// <returns>无</returns>
    IEnumerator SecondStep()
    {
        bool shouldWait = true;
        while (shouldWait)
        {
            if (isOpen)
            {
                shouldWait = false;
                //Player播放进门动画。
                _user.GetComponent<Animator>().SetTrigger(InDoor);
                //等待人完全进门。
                yield return StartCoroutine(ThirdStep(_user.GetComponent<PlayerController>()));
                yield break;
            }
            yield return null;
        }
    }
    

    /// <summary>
    /// 等待人完全进门，进行使用门的第三步。
    /// </summary>
    /// <param name="player">门的使用者。</param>
    /// <returns>无</returns>
    IEnumerator ThirdStep(PlayerController player)
    {
        bool shouldWait = true;
        while (shouldWait)
        {
            if (player.currentUseDoorState == (int)PlayerController.UseDoorState.InDoor)
            {
                shouldWait = false;
                //人已经进入，播放关门动画。
                _animator.SetTrigger(Close);
                //等待目标门打开。
                yield return StartCoroutine(FourthStep());
                yield break;
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// 等待目标门打开，进行使用门的第四步。
    /// </summary>
    /// <returns>无</returns>
    IEnumerator FourthStep()
    {
        bool shouldWait = true;
        while (shouldWait)
        {
            if (target.isOpen)
            {
                shouldWait = false;
                //人播放出目标门动画。
                _user.GetComponent<Animator>().SetTrigger(OutDoor);
                //等待人出门。
                yield return StartCoroutine(FifthStep(_user.GetComponent<PlayerController>()));
                yield break;
            } 
            yield return null;
        }
        
    }
    
    /// <summary>
    /// 等待人完全进门，进行使用门的第五步。
    /// </summary>
    /// <param name="player">门的使用者。</param>
    /// <returns>无</returns>
    IEnumerator FifthStep(PlayerController player)
    {
        bool shouldWait = true;
        while (shouldWait)
        {
            if (player.currentUseDoorState == (int)PlayerController.UseDoorState.OutDoor)
            {
                shouldWait = false;
                //关闭目标门。
                target.Animator.SetTrigger(Close);
                //门已经用完，没有入口和出口的区别了。
                isEntrance = false;
                yield break;
            }
            yield return null;
        }
        
    }
    
    /// <inheritdoc />
    public void SpecialUse(GameObject user)
    {
        Debug.Log("门没有特殊用法！");
    }

    /// <summary>
    /// 当可用门的对象进入触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">触发触发器的碰撞器。</param>
    public void Register(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Register(this);
        }
    }

    /// <summary>
    /// 当可用门的对象走出触发器对象时，由触发器对象调用。
    /// </summary>
    /// <param name="other">离开触发器的碰撞器。</param>
    public void Remove(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Remove(this);
        }
    }

    /// <summary>
    /// 打开目标门。由动画系统调用。
    /// </summary>
    public void OpenTargetDoor()
    {
        if (isEntrance)
        { 
           target.Animator.SetTrigger(Open); 
        }
        
    }
    
    /// <summary>
    /// 门已经被打开时设置状态。由动画系统调用。
    /// </summary>
    public void Opened()
    {
        isOpen = true;
    }
    
    /// <summary>
    /// 门被用完时设置状态。由动画系统调用。
    /// </summary>
    public void Used()
    {
        if (!isEntrance)
        {
            target.Reset();
            Reset();
        }
    }
}
