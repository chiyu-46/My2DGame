using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool isGround;
    
    void Start()
    {
        //获取_rb刚体组件。
        _rb = GetComponent<Rigidbody2D>();
        //将跳跃力的值变成跳跃力向量。
        _jumpForceVector = Vector2.up * jumpForce;
    }
    
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
        Jump();
    }

    private void OnDrawGizmosSelected()
    {
        //绘制地面检测范围圈。
        Gizmos.DrawWireSphere(groundChecker.position,checkRadius);
    }

    /// <summary>
    /// 控制Player水平移动。
    /// </summary>
    /// <remarks>
    ///通过Unity轴"Horizontal"获取用户输入，从而控制Player速度向量。并通过旋转方式确定Player朝向。
    /// </remarks>
    void Movement()
    {
        //获取水平轴输入。
        float horizontalInput = Input.GetAxis("Horizontal");
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

    /// <summary>
    /// 控制Player跳跃。
    /// </summary>
    /// <remarks>
    ///获取轴"Jump"，当isGround为true时施加向上力实现跳跃。
    /// </remarks>
    void Jump()
    {
        //获取跳跃轴输入。
        float jump = Input.GetAxis("Jump");
        //当player的垂直速度为零，即站在地上时，对Player施加向上力（向量）实现跳跃。
        if (jump != 0 && isGround)
        {
            _rb.AddForce(_jumpForceVector,ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// 检查Player是否站在地面上。
    /// <para>
    ///并根据结果改变Player受到的重力，提高玩家在跳跃和跳下时的手感。
    /// </para>
    /// </summary>
    void GroundCheck()
    {
        isGround = Physics2D.OverlapCircle(groundChecker.position, checkRadius, groundLayer);
        if (isGround)
        {
            _rb.gravityScale = 1;
        }
        else
        {
            _rb.gravityScale = 4;
        }
    }
}
