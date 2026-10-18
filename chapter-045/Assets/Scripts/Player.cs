using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: [todos/chapter-025/state-machine-circular-references.md](../../todos/chapter-025/state-machine-circular-references.md)
public class Player : Entity
{
    private PlayerInputSet input;
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

    public override void Awake()
    {
        base.Awake();
        // TODO, 강의에서 애니메이터를 먼저 찾지 않으면 제대로 상태가 할당되지 않는다는 이야기를 하던데, 어떤 내용이지? 컴포넌트를 먼저 메모리에 로딩하지 않으면 어떤 문제가 있는지 이해할 수 있도록 쉽게 설명해줘. 
        input = new PlayerInputSet();
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

    public virtual void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void Aired()
    {
        SetVelocity(moveInput.x * (moveSpeed * inAirMoveMultiplier), this.rb.linearVelocity.y);
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

    internal void Jump()
    {
        SetVelocity(rb.linearVelocityX, this.jumpForce);
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

    internal bool WasAttackPerformed()
    {
        return input.Player.Attack.WasPerformedThisFrame();
    }

    internal void GenerateAttackVelocity(int index)
    {
        SetVelocity(attackVelocity[index].x * facingDirection, attackVelocity[index].y);
    }

    internal void SetJumpAttackVelocity()
    {
        SetVelocity(jumpAttackVelocity.x * facingDirection, jumpAttackVelocity.y);
    }
}
