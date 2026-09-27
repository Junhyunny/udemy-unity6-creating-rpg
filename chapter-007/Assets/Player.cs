using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// TODO: [todos/chapter-007/physics-material-2d-and-materials.md](../../todos/chapter-007/physics-material-2d-and-materials.md)
// TODO: [todos/chapter-007/collider-2d-role.md](../../todos/chapter-007/collider-2d-role.md)
// TODO: [todos/chapter-007/sprite-pivot-and-rotation-center.md](../../todos/chapter-007/sprite-pivot-and-rotation-center.md)
public class Player : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    // TODO: [todos/chapter-007/serializefield-and-serialization.md](../../todos/chapter-007/serializefield-and-serialization.md)
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float jumpForce = 8.0f;
    private float xInput;
    [SerializeField] private bool facingRight = true;

    [Header("Collision details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private bool isGrounded;
    // TODO: [todos/chapter-007/layer-and-layermask.md](../../todos/chapter-007/layer-and-layermask.md)
    [SerializeField] private LayerMask whatIsGround;
    
    private bool canMove = true;
    private bool canJump = true;

    // TODO: [todos/chapter-007/array-vs-list.md](../../todos/chapter-007/array-vs-list.md)
    // public Collider2D[] enemies;
    // public List<Collider2D> enemyList;

    [Header("Attack details")]
    [SerializeField] private float attackRadius;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask whatIsEnermy;

    private void Awake()
    {
        // TODO: [todos/chapter-007/getcomponent-and-null-handling.md](../../todos/chapter-007/getcomponent-and-null-handling.md)
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        groundCheckDistance = 1.5f;
        Debug.Log("Player Awake");
    }

    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        // TODO: [todos/chapter-007/inspector-reference-vs-code-assignment.md](../../todos/chapter-007/inspector-reference-vs-code-assignment.md)
        HandleMovement();
        HandleAnimations();
        HandleFlip();
        HandleCollision();
        // Debug.Log("Player Update: xInput = " + xInput);
        // Debug.Log("Player Update: linearVelocity = (" + rb.linearVelocity.x + ", " + rb.linearVelocity.y + ")");
        HandleInput();

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.Log("Player Jump");
        //     // TODO: [todos/chapter-007/jump-linearvelocity-vs-addforce.md](../../todos/chapter-007/jump-linearvelocity-vs-addforce.md)
        //     rb.AddForce(Vector2.up * 5.0f, ForceMode2D.Impulse);
        // }
    }

    public void DamageEnemies()
    {
        // TODO: [todos/chapter-007/physics2d-overlapcircleall.md](../../todos/chapter-007/physics2d-overlapcircleall.md)
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnermy);
        foreach (var enemy in enemies)
        {   
            Debug.Log("enermies loop");
            enemy.GetComponent<Damaged_Example>().TakeDamage();
        }
    }

    public void EnableMovementAndJump(bool enable)
    {
        canMove = enable;
        canJump = enable;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryToJump();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToAttack();
        }
    }

    private void TryToAttack()
    {
        if (isGrounded)
        {
            animator.SetTrigger("attack");
        }
    }

    private void HandleMovement()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        } else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void HandleAnimations()
    {
        // bool isMoving = rb.linearVelocity.x != 0;
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    private void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && facingRight == false)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight == true)
        {
            Flip();
        }
    }

    [ContextMenu("Flip")]
    private void Flip()
    {
        // TODO: [todos/chapter-007/monobehaviour-transform-property.md](../../todos/chapter-007/monobehaviour-transform-property.md)
        transform.Rotate(0.0f, 180.0f, 0.0f);
        facingRight = !facingRight;
    }

    // TODO: [todos/chapter-007/ondrawgizmos-call-timing.md](../../todos/chapter-007/ondrawgizmos-call-timing.md)
    private void OnDrawGizmos()
    {
        // TODO: [todos/chapter-007/gizmos-debug-visualization.md](../../todos/chapter-007/gizmos-debug-visualization.md)
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    private void HandleCollision()
    {
        // TODO: [todos/chapter-007/physics2d-raycast.md](../../todos/chapter-007/physics2d-raycast.md)
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }
}
