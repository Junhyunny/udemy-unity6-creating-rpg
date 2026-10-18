
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected StateMachine stateMachine;
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public float originalGravityScale = 0;
    public bool facingRight = true;
    public int facingDirection { get; private set; } = 1;
    public Vector2 moveInput { get; protected set; }

    [Header("Collision Detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool isGroundDetected = false;
    public bool isWallDetected = false;

    public virtual void Awake()
    {
        // TODO, 강의에서 애니메이터를 먼저 찾지 않으면 제대로 상태가 할당되지 않는다는 이야기를 하던데, 어떤 내용이지? 컴포넌트를 먼저 메모리에 로딩하지 않으면 어떤 문제가 있는지 이해할 수 있도록 쉽게 설명해줘. 
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
    }

    public virtual void Start()
    { }

    void Update()
    {
        isGroundDetected = IsOnGround();
        isWallDetected = IsOnWall();
        stateMachine.currentState.Update();
    }

    protected void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDirection = facingDirection * -1;
    }

    protected void HandleFlip(float xVelocity)
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

    public float GetYVelocity()
    {
        return rb.linearVelocityY;
    }

    public bool IsFalling()
    {
        return rb.linearVelocityY < 0;
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

    // public void Aired()
    // {
    //     SetVelocity(moveInput.x * (moveSpeed * inAirMoveMultiplier), this.rb.linearVelocity.y);
    // }

    public void StopMovement()
    {
        SetVelocity(0, this.rb.linearVelocity.y);
    }

    public void WallSlide(float multiplier)
    {
        SetVelocity(0, rb.linearVelocity.y * multiplier);
    }

    internal void Stop()
    {
        SetVelocity(0, 0);
    }

    public void CallAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }

    internal void StopForAttack()
    {
        SetVelocity(0, rb.linearVelocityY);
    }

    internal void JumpAttack()
    {
        SetVelocity(0, rb.linearVelocityY);
    }
}