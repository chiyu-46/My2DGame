using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此类用于控制Player的动画播放。
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    /// <summary>
    /// PlayerController组件的引用。
    /// </summary>
    private PlayerController _playerController;
    /// <summary>
    /// Player刚体组件的引用。
    /// </summary>
    private Rigidbody2D _rigidbody;
    /// <summary>
    /// Player动画器组件的引用。
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// 用于显示起跳特效的GameObject。
    /// </summary>
    public Transform jumpVFX;
    /// <summary>
    /// 起跳特效与Player之间的位置偏移。
    /// </summary>
    public Vector3 jumpOffset;
    /// <summary>
    /// 用于显示落地特效的GameObject。
    /// </summary>
    public Transform fallVFX;
    /// <summary>
    /// 落地特效与Player之间的位置偏移。
    /// </summary>
    public Vector3 fallOffset;
    /// <summary>
    /// 动画控制参数velocity_x的id值。
    /// </summary>
    private static readonly int VelocityX = Animator.StringToHash("velocity_x");
    /// <summary>
    /// 动画控制参数velocity_y的id值。
    /// </summary>
    private static readonly int VelocityY = Animator.StringToHash("velocity_y");
    /// <summary>
    /// 动画控制参数startJump的id值。
    /// </summary>
    private static readonly int StartJump = Animator.StringToHash("startJump");
    /// <summary>
    /// 动画控制参数onGround的id值。
    /// </summary>
    private static readonly int ONGround = Animator.StringToHash("onGround");

    void Start()
    {
        //获取PlayerController，刚体，动画器组件。
        _playerController = GetComponent<PlayerController>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        //更新各个动画控制参数的值。
        _animator.SetFloat(VelocityX,Mathf.Abs(_rigidbody.velocity.x));
        _animator.SetFloat(VelocityY,_rigidbody.velocity.y);
        if (_playerController.StartJump)
        {
            //起跳时执行，执行后恢复startJump值为false。
            _animator.SetTrigger(StartJump);
            _playerController.StartJump = false;
        }
        _animator.SetBool(ONGround,_playerController.IsGround);
    }

    /// <summary>
    /// 显示起跳特效。由动画事件调用。
    /// </summary>
    public void ShowJumpVFX()
    {
        jumpVFX.gameObject.SetActive(true);
        jumpVFX.position = transform.position + jumpOffset;
    }

    /// <summary>
    /// 显示落地特效。由动画事件调用。
    /// </summary>
    public void ShowFallVFX()
    {
        fallVFX.gameObject.SetActive(true);
        fallVFX.position = transform.position + fallOffset;
    }
}
