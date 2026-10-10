using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: [todos/chapter-025/state-machine-circular-references.md](../../todos/chapter-025/state-machine-circular-references.md)
public class Player : MonoBehaviour
{
    private PlayerInputSet input;
    private StateMachine stateMachine;
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerBasicAttackState basicAttackState { get; private set; }
    public PlayerJumpAttackState jumpAttackState { get; private set; }

    [Header("Attack details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = .1f;
    public float comboResetTime = 0.5f;
    private Coroutine queuedAttackCo;

    [Header("Movement Detail")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public Vector2 wallJumpForce;
    [Range(0, 1)]
    public float inAirMoveMultiplier = .7f;
    [Range(0, 1)]
    public float wallSlideSlowMultiplier = .7f;
    [Space]
    public float dashDuration = 0.25f;
    public float dashSpeed = 20;
    public float originalGravityScale = 0;
    public bool facingRight = true;
    public int facingDirection { get; private set; } = 1;
    public Vector2 moveInput { get; private set; }

    [Header("Collision Detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool isGroundDetected = false;
    public bool isWallDetected = false;

    private void Awake()
    {
        // TODO, 강의에서 애니메이터를 먼저 찾지 않으면 제대로 상태가 할당되지 않는다는 이야기를 하던데, 어떤 내용이지? 컴포넌트를 먼저 메모리에 로딩하지 않으면 어떤 문제가 있는지 이해할 수 있도록 쉽게 설명해줘. 
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        input = new PlayerInputSet();
        stateMachine = new StateMachine();
        idleState = new PlayerIdleState(this, stateMachine);
        moveState = new PlayerMoveState(this, stateMachine);
        jumpState = new PlayerJumpState(this, stateMachine);
        fallState = new PlayerFallState(this, stateMachine);
        wallSlideState = new PlayerWallSlideState(this, stateMachine);
        wallJumpState = new PlayerWallJumpState(this, stateMachine);
        dashState = new PlayerDashState(this, stateMachine);
        basicAttackState = new PlayerBasicAttackState(this, stateMachine);
        jumpAttackState = new PlayerJumpAttackState(this, stateMachine);
    }

    void OnEnable()
    {
        // input.Player.Movement.started - input just begun
        // input.Player.Movement.performed - input is performed
        // input.Player.Movement.canceld - input stops, when you release the key
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCanceled;
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCanceled;
        moveInput = Vector2.zero;
    }

    void OnDestroy()
    {
        input.Dispose();
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log(moveInput);
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    void Start()
    {
        stateMachine.Initialize(idleState);
    }

    void Update()
    {
        isGroundDetected = IsOnGround();
        isWallDetected = IsOnWall();
        stateMachine.currentState.Update();
    }

    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
        }
        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    private void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDirection = facingDirection * -1;
    }

    private void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && facingRight == false)
        {
            Flip();
        }
        else if (xVelocity < 0 && facingRight == true)
        {
            Flip();
        }
    }

    public void Move()
    {
        float xVelocity = this.moveInput.x * this.moveSpeed;
        float yVelocity = this.rb.linearVelocity.y;
        this.SetVelocity(xVelocity, yVelocity);
        this.HandleFlip(xVelocity);
    }

    public bool WasJumpPressed()
    {
        return input.Player.Jump.WasPressedThisFrame();
    }

    public bool WasAttackPressed()
    {
        return input.Player.Attack.WasPressedThisFrame();
    }

    public float GetYVelocity()
    {
        return rb.linearVelocityY;
    }

    public bool IsFalling()
    {
        return rb.linearVelocityY < 0;
    }

    internal void Jump()
    {
        SetVelocity(rb.linearVelocityX, this.jumpForce);
    }

    internal bool IsOnGround()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    internal bool IsOnWall()
    {
        return Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround)
        && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
        Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
    }

    public void Aired()
    {
        SetVelocity(moveInput.x * (moveSpeed * inAirMoveMultiplier), this.rb.linearVelocity.y);
    }

    public void StopMovement()
    {
        SetVelocity(0, this.rb.linearVelocity.y);
    }

    public void WallSlide(float multiplier)
    {
        SetVelocity(0, rb.linearVelocity.y * multiplier);
    }

    internal void WallJump()
    {
        SetVelocity(wallJumpForce.x * -facingDirection, wallJumpForce.y);
    }

    internal bool IsDashPressed()
    {
        return input.Player.Dash.WasPressedThisFrame();
    }

    internal void Dash()
    {
        SetVelocity(dashSpeed * facingDirection, 0);
    }

    internal void Stop()
    {
        SetVelocity(0, 0);
    }

    internal bool WasAttackPerformed()
    {
        return input.Player.Attack.WasPerformedThisFrame();
    }

    public void CallAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }

    internal void StopForAttack()
    {
        SetVelocity(0, rb.linearVelocityY);
    }

    internal void GenerateAttackVelocity(int index)
    {
        SetVelocity(attackVelocity[index].x * facingDirection, attackVelocity[index].y);
    }

    internal void JumpAttack()
    {
        SetVelocity(0, rb.linearVelocityY);
    }

    internal void SetJumpAttackVelocity()
    {
        SetVelocity(jumpAttackVelocity.x * facingDirection, jumpAttackVelocity.y);
    }
}
