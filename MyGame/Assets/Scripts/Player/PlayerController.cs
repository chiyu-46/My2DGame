using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 此类是Player游戏对象的控制器。
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Player的刚体组件。
    /// </summary>
    private Rigidbody2D _rb;
    /// <summary>
    /// Player的PlayerInput组件。
    /// </summary>
    private PlayerInput _playerInput;
    /// <summary>
    /// Player在输入系统中用于控制移动的InputAction。
    /// </summary>
    private InputAction _movementAction;
    /// <summary>
    /// Player跳跃时收到的向上的力的值。
    /// </summary>
    public float jumpForce;
    /// <summary>
    /// Player跳跃时收到的向上的力的向量。
    /// </summary>
    private Vector2 _jumpForceVector;
    /// <summary>
    /// Player的速度，缓慢加减速由Unity输入轴实现。
    /// </summary>
    public float speed;
    
    /// <summary>
    /// 位于Player脚下的点的引用。以此为中心检测脚下是否有地面。
    /// </summary>
    [Header("Ground Check")]
    public Transform groundChecker;
    /// <summary>
    /// 地面检测圈的半径。
    /// </summary>
    public float checkRadius;
    /// <summary>
    /// 地面所在图层。
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 地面检测的结果。
    /// </summary>
    private bool _isGround;
    /// <summary>
    /// 只读，返回Player是否在地上。
    /// </summary>
    public bool IsGround { get => _isGround; }
    /// <summary>
    /// 用于Player动画控制器，Player开始跳跃时设置为true，Player动画控制器检测到后设为false。
    /// </summary>
    public bool StartJump { get; set; }
    
    void Start()
    {
        //获取刚体组件。
        _rb = GetComponent<Rigidbody2D>();
        //获取PlayerInput组件。
        _playerInput = GetComponent<PlayerInput>();
        //获取控制移动的Action。
        _movementAction = _playerInput.actions["Movement"];
        //将跳跃力的值变成跳跃力向量。
        _jumpForceVector = Vector2.up * jumpForce;
    }
    
    void Update()
    {
        
    }

    /// <summary>
    /// 调用检查是否在地面上的方法，以及进行移动的方法。
    /// </summary>
    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
    }

    /// <summary>
    /// 控制Player左右移动并改变Player朝向。
    /// </summary>
    /// <remarks>
    /// 本方法依靠InputSystem运行，但不由PlayerInput调用，而是直接在FixedUpdate中查询_movementAction的值。
    /// </remarks>
    private void Movement()
    {
        //获取水平轴输入。
        float horizontalInput = _movementAction.ReadValue<float>();
        //确定Player速度（向量）。
        _rb.velocity = new Vector2(horizontalInput * speed, _rb.velocity.y);
        if (horizontalInput != 0)
        {
            //根据运动方向确定Player朝向。
            if (horizontalInput > 0 )
            { 
                transform.rotation = Quaternion.Euler(0,0,0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0,180,0);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        //绘制地面检测范围圈。
        Gizmos.DrawWireSphere(groundChecker.position,checkRadius);
    }
    
    /// <summary>
    /// 控制Player跳跃。
    /// </summary>
    /// <remarks>
    /// 通过Input System调用，当isGround为true时施加向上力实现跳跃。
    /// </remarks>
    public void Jump()
    {
        //当player的垂直速度为零，即站在地上时，对Player施加向上力（向量）实现跳跃。
        if (_isGround)
        {
            _rb.AddForce(_jumpForceVector,ForceMode2D.Impulse);
            StartJump = true;
        }
    }

    /// <summary>
    /// 检查Player是否站在地面上。
    /// <para>
    /// 并根据结果改变Player受到的重力，提高玩家在跳跃和跳下时的手感。
    /// </para>
    /// </summary>
    void GroundCheck()
    {
        _isGround = Physics2D.OverlapCircle(groundChecker.position, checkRadius, groundLayer);
        if (_isGround)
        {
            _rb.gravityScale = 1;
        }
        else
        {
            _rb.gravityScale = 4;
        }
    }
}
