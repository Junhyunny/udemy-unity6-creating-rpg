
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
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool isGroundDetected = false;
    public bool isWallDetected = false;

    protected virtual void Awake()
    {
        // TODO: [todos/chapter-045/component-reference-before-state-creation.md](../../../todos/chapter-045/component-reference-before-state-creation.md)
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
    }

    protected virtual void Start()
    { }

    protected virtual void Update()
    {
        isGroundDetected = IsOnGround();
        isWallDetected = IsOnWall();
        stateMachine.currentState.Update();
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDirection = facingDirection * -1;
    }

    public void HandleFlip(float xVelocity)
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
        return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    internal bool IsOnWall()
    {
        if (secondaryWallCheck != null)
        {
            return Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround)
            && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
        }
        return Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
        Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
    }

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

    public void CurrentStateAnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
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