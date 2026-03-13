using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WarriorController – barian_Lin 프로젝트용 전사 캐릭터 컨트롤러
/// 
/// 조작법:
///   A / ← : 왼쪽 이동
///   D / → : 오른쪽 이동
///   Space  : 점프
///   Z      : 공격 1 (가로 참격)
///   X      : 공격 2 (내려치기)
///   C      : 공격 3 (콤보 참격)
///   F / E  : 아이템 줍기
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WarriorController : MonoBehaviour
{
    // ── Inspector Settings ─────────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed       = 5f;
    public float jumpForce       = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Attack")]
    public float attackCooldown  = 0.3f;
    public float pickupRange     = 1.5f;

    [Header("Animation FPS")]
    public float walkFPS         = 12f;
    public float slashFPS        = 14f;
    public float jumpFPS         = 10f;
    public float pickupFPS       = 8f;

    // ── State Machine ──────────────────────────────────────────────────────
    public enum State
    {
        Idle, WalkRight, WalkLeft,
        Slash1, Slash2, Slash3,
        PickUp, Jump, Fall, Land
    }

    // ── Private Fields ─────────────────────────────────────────────────────
    private Rigidbody2D     rb;
    private Animator        anim;
    private SpriteRenderer  sr;

    private State     currentState = State.Idle;
    private bool      isGrounded   = false;
    private bool      isAttacking  = false;
    private float     attackTimer  = 0f;
    private float     horizontal   = 0f;
    private bool      facingRight  = true;

    // ── Animator Parameter Hashes ──────────────────────────────────────────
    private static readonly int P_State     = Animator.StringToHash("State");
    private static readonly int P_IsGround  = Animator.StringToHash("IsGrounded");
    private static readonly int P_VelY      = Animator.StringToHash("VelocityY");
    private static readonly int P_Attack1   = Animator.StringToHash("Slash1");
    private static readonly int P_Attack2   = Animator.StringToHash("Slash2");
    private static readonly int P_Attack3   = Animator.StringToHash("Slash3");
    private static readonly int P_PickUp    = Animator.StringToHash("PickUp");
    private static readonly int P_Jump      = Animator.StringToHash("Jump");

    // ── Events (콤보/사운드 연결용) ────────────────────────────────────────
    public System.Action OnSlash1;
    public System.Action OnSlash2;
    public System.Action OnSlash3;
    public System.Action OnPickUp;
    public System.Action OnLand;

    // ── Public Properties ──────────────────────────────────────────────────
    public State CurrentState => currentState;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckGround();
        HandleInput();
        UpdateAnimatorParams();
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }
    }

    // ── Ground Check ───────────────────────────────────────────────────────
    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.5f,
            groundCheckRadius,
            groundLayer
        );

        if (!wasGrounded && isGrounded)
        {
            OnLand?.Invoke();
            SetState(State.Land);
            StartCoroutine(LandRecovery());
        }
    }

    IEnumerator LandRecovery()
    {
        yield return new WaitForSeconds(0.1f);
        if (currentState == State.Land)
            SetState(State.Idle);
    }

    // ── Input Handler ──────────────────────────────────────────────────────
    void HandleInput()
    {
        // Attack cooldown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) isAttacking = false;
        }

        if (isAttacking) return;

        // Movement
        horizontal = Input.GetAxisRaw("Horizontal");

        // ── Jump ──────────────────────────────────────────────────────────
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            SetState(State.Jump);
            anim.SetTrigger(P_Jump);
        }

        // ── Attacks ───────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Z))        TriggerSlash(1);
        else if (Input.GetKeyDown(KeyCode.X))   TriggerSlash(2);
        else if (Input.GetKeyDown(KeyCode.C))   TriggerSlash(3);

        // ── Pickup ────────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
            TriggerPickUp();

        // ── Walk state update ─────────────────────────────────────────────
        if (!isAttacking && isGrounded)
        {
            if (horizontal > 0f)
            {
                facingRight = true;
                SetState(State.WalkRight);
                sr.flipX = false;
            }
            else if (horizontal < 0f)
            {
                facingRight = false;
                SetState(State.WalkLeft);
                sr.flipX = true;
            }
            else
            {
                SetState(State.Idle);
            }
        }
    }

    // ── Attack Triggers ────────────────────────────────────────────────────
    void TriggerSlash(int type)
    {
        isAttacking  = true;
        attackTimer  = attackCooldown;

        switch (type)
        {
            case 1:
                SetState(State.Slash1);
                anim.SetTrigger(P_Attack1);
                OnSlash1?.Invoke();
                break;
            case 2:
                SetState(State.Slash2);
                anim.SetTrigger(P_Attack2);
                OnSlash2?.Invoke();
                break;
            case 3:
                SetState(State.Slash3);
                anim.SetTrigger(P_Attack3);
                OnSlash3?.Invoke();
                StartCoroutine(ResetAttackAfter(0.6f));
                return;
        }
        StartCoroutine(ResetAttackAfter(attackCooldown));
    }

    void TriggerPickUp()
    {
        isAttacking = true;
        SetState(State.PickUp);
        anim.SetTrigger(P_PickUp);
        OnPickUp?.Invoke();
        StartCoroutine(PickUpAction());
        StartCoroutine(ResetAttackAfter(0.6f));
    }

    IEnumerator PickUpAction()
    {
        yield return new WaitForSeconds(0.3f);
        // Detect nearby items
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        foreach (var col in cols)
        {
            IPickable item = col.GetComponent<IPickable>();
            item?.OnPickedUp(gameObject);
        }
    }

    IEnumerator ResetAttackAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        attackTimer = 0f;
        SetState(State.Idle);
    }

    // ── Animator ───────────────────────────────────────────────────────────
    void UpdateAnimatorParams()
    {
        anim.SetInteger(P_State,    (int)currentState);
        anim.SetBool(P_IsGround,    isGrounded);
        anim.SetFloat(P_VelY,       rb.velocity.y);
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 pos = groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.5f;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

// ── Item Interface ─────────────────────────────────────────────────────────
public interface IPickable
{
    void OnPickedUp(GameObject picker);
}
