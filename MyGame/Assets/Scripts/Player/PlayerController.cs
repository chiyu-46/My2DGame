using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 此类是Player游戏对象的控制器。
/// </summary>
public class PlayerController : MonoBehaviour, IWoundable
{
    /// <summary>
    /// 玩家使用门时的不同状态。
    /// </summary>
    public enum UseDoorState
    {
        /// <summary>
        /// 玩家开始进门。
        /// </summary>
        UseDoor,
        /// <summary>
        /// 玩家在门中。
        /// </summary>
        InDoor,
        /// <summary>
        /// 玩家走出门。
        /// </summary>
        OutDoor,
        /// <summary>
        /// 玩家没有使用门。
        /// </summary>
        UnUseDoor
    }
    
    [Header("Base")][SerializeField] 
    private int health;
    [SerializeField] 
    private int defense;
    [SerializeField] 
    private int maxHealth;
    /// <inheritdoc />
    public int Health
    {
        get => health;
        set { health = value > maxHealth ? maxHealth : value; }
    }

    /// <inheritdoc />
    public int Defense { get => defense; set => defense = value; }
    
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
    [Header("Movement")]
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

    /// <summary>
    /// 存储当前位置所有可使用物体。
    /// </summary>
    private List<IUsable> _usableList = new List<IUsable>();

    /// <inheritdoc />
    public bool CanGetHit { get; set; }

    /// <summary>
    /// 玩家当前能不能移动。
    /// </summary>
    /// <remarks>
    /// 用于处理禁锢技能或进出门时不能移动的功能。
    /// </remarks>
    public bool canMove;
    /// <summary>
    /// 用于确定Player当前处于使用门的哪个阶段。
    /// </summary>
    public int currentUseDoorState;
    
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
        canMove = true;
        currentUseDoorState = (int)UseDoorState.UnUseDoor;
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
        if (canMove)
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
        if (canMove)
        {
            //当player的垂直速度为零，即站在地上时，对Player施加向上力（向量）实现跳跃。
            if (_isGround)
            {
                _rb.AddForce(_jumpForceVector,ForceMode2D.Impulse);
                StartJump = true;
            }
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

    /// <inheritdoc />
    public void GetHit(int damage)
    {
        //TODO:播放受伤动画。
        //如果防御力大于受到伤害，则不受伤害。
        int real = damage - Defense;
        if (real <= 0)
        {
            return;
        }
        //受到伤害。
        Health = Health - real;
        //如果此次受伤导致死亡，将调用Dead方法。
        if (Health <= 0)
        {
            //TODO:避免再次受到伤害。
            Health = 0;
            Dead();
        }
    }

    /// <inheritdoc />
    public void Dead()
    {
        //TODO:显示死亡动画。
        canMove = false;
    }

    /// <summary>
    /// 当Player进入可被使用物品的触发器后，由物品调用，将自己置于player当前可使用列表中。
    /// </summary>
    /// <param name="usable">Player碰到的可被使用物品</param>
    public void Register(IUsable usable)
    {
        if (!_usableList.Contains(usable))
        {
            _usableList.Add(usable);
        }
    }
    
    /// <summary>
    /// 当Player走出可被使用物品的触发器后，由物品调用，将自己移出player当前可使用列表中。
    /// </summary>
    /// <param name="usable">Player碰到的可被使用物品</param>
    public void Remove(IUsable usable)
    {
        if (_usableList.Contains(usable))
        {
            _usableList.Remove(usable);
        }
    }
    /// <summary>
    /// 由InputSystem调用，控制使用可使用物品。
    /// </summary>
    public void Use()
    {
        if (_usableList.Count > 0)
        {
            //TODO:选择要使用的物品。
            _usableList[0].Use(gameObject);
        }
    }

    /// <summary>
    /// 由InputSystem调用，控制使用可使用物品的特殊用法。
    /// </summary>
    public void SpecialUse()
    {
        //TODO:控制UI按钮是否显示。
        //TODO:可能存在其他有特殊用法的物品。
        foreach (var item in _usableList)
        {
            if (item.GetGameObject().CompareTag("Cannon"))
            {
                item.SpecialUse(gameObject);
            }
        }
    }

    /// <summary>
    /// Player进门动画播放完成，即完成进门动作，执行转移时由动画系统调用。
    /// </summary>
    public void InDoor()
    {
        transform.position = _usableList[0].GetGameObject().GetComponent<Door>().target.transform.position;
        currentUseDoorState = (int)UseDoorState.InDoor;
        CanGetHit = false;
    }
    
    /// <summary>
    /// Player出门动画播放时，即从门中走出，执行使玩家可被攻击，由动画系统调用。
    /// </summary>
    public void OutDoor()
    {
        CanGetHit = true;
        currentUseDoorState = (int)UseDoorState.OutDoor;
    }
    
    /// <summary>
    /// Player完全出门时，执行使玩家可移动，由动画系统调用。
    /// </summary>
    public void FinishOutDoor()
    {
        canMove = true;
        currentUseDoorState = (int)UseDoorState.UnUseDoor;
    }
}

