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
    /// <summary>
    /// Player是否已经死了。
    /// </summary>
    private bool _isDead;
    /// <inheritdoc />
    public int Health { get => health; set { health = value > maxHealth ? maxHealth : value; } }
    /// <inheritdoc />
    public int Defense { get => defense; set => defense = value; }
    /// <summary>
    /// Player是否已经死了。
    /// </summary>
    public bool IsDead { get => _isDead; }
    /// <summary>
    /// 此变量用于确定此角色是否具有抗打断能力。如果此值为true，则此角色不受协程BouncedOffByAttack影响。
    /// </summary>
    [SerializeField]
    private bool antiInterruption;
    /// <summary>
    /// 此变量用于确定打断效果是否被覆盖。当此角色处于影响是否移动的buff下时，此值为true，BouncedOffByAttack将不再恢复敌人行动能力。
    /// </summary>
    private bool isOverrideInterruption;
    
    /// <summary>
    /// Player的刚体组件。
    /// </summary>
    private Rigidbody2D _rb;
    /// <summary>
    /// Player动画器组件的引用。
    /// </summary>
    private Animator _animator;
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
    /// 默认物理材质。
    /// </summary>
    [SerializeField]
    protected PhysicsMaterial2D defaultMaterial2D;
    /// <summary>
    /// 跳跃时使用的物理材质。
    /// </summary>
    [SerializeField]
    protected PhysicsMaterial2D jumpMaterial2D;
    /// <summary>
    /// 动画控制参数Dead的id值。
    /// </summary>
    private static readonly int DeadId = Animator.StringToHash("Dead");
    /// <summary>
    /// 动画控制参数GetHit的id值。
    /// </summary>
    private static readonly int GETHitId = Animator.StringToHash("GetHit");
    
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
    [HideInInspector]
    public bool canMove;
    /// <summary>
    /// 用于确定Player当前处于使用门的哪个阶段。
    /// </summary>
    [HideInInspector]
    public int currentUseDoorState;
    
    void Start()
    {
        //获取刚体组件。
        _rb = GetComponent<Rigidbody2D>();
        _rb.sharedMaterial = defaultMaterial2D;
        //获取动画器组件。
        _animator = GetComponent<Animator>();
        //获取PlayerInput组件。
        _playerInput = GetComponent<PlayerInput>();
        //获取控制移动的Action。
        _movementAction = _playerInput.actions["Movement"];
        //将跳跃力的值变成跳跃力向量。
        _jumpForceVector = Vector2.up * jumpForce;
        canMove = true;
        CanGetHit = true;
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
            _rb.sharedMaterial = defaultMaterial2D;
        }
        else
        {
            _rb.gravityScale = 4;
            _rb.sharedMaterial = jumpMaterial2D;
        }
    }

    /// <inheritdoc />
    public void GetHit(int damage, Vector2? force = null)
    {
        if (CanGetHit)
        {
            StartCoroutine(BouncedOffByAttack(force ?? Vector2.zero));
            //如果防御力大于受到伤害，则不受伤害。
            int real = damage - Defense;
            if (real <= 0)
            {
                return;
            }
            //受到伤害。
            CanGetHit = false;
            Health = Health - real;
            //如果此次受伤导致死亡，将不播放受伤动画，直接死亡。
            if (Health <= 0)
            {
                Dead();
            }
            else
            {
                _animator.SetTrigger(GETHitId);
            }
        }
    }

    /// <summary>
    /// 用于实现游戏对象被攻击时受力弹开的效果的协程。如果此敌人拥有抗打断能力，则不受影响。
    /// </summary>
    /// <param name="force">游戏对象受到的作用力。</param>
    IEnumerator BouncedOffByAttack(Vector2 force)
    {
        if (antiInterruption)
        {
            yield break;
        }
        canMove = false;
        _rb.velocity = Vector2.zero;
        _rb.AddForce(force,ForceMode2D.Impulse);
        yield return new WaitForSeconds(.5f);
        if (!isOverrideInterruption)
        {
            canMove = true;
        }
        
    }
    
    /// <summary>
    /// 受伤动画播放完成，可以接受下一次攻击。动画系统调用。
    /// </summary>
    public void GetHitAnimationFinished()
    {
        CanGetHit = true;
    }
    
    /// <inheritdoc />
    public void Dead()
    {
        _animator.SetTrigger(DeadId);
        canMove = false;
        isOverrideInterruption = true;
        CanGetHit = false;
        _isDead = true;
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
        //恢复使用者移动能力。
        canMove = true;
        _rb.isKinematic = false;
        currentUseDoorState = (int)UseDoorState.UnUseDoor;
    }
}

