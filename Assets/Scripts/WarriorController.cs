using System.Collections;
using UnityEngine;

/// <summary>
/// WarriorController  –  barian_Lin 프로젝트 메인 컨트롤러
/// 
///  조작법
///  ────────────────────────────────────────
///  A  / ←   : 왼쪽 이동
///  D  / →   : 오른쪽 이동
///  Space     : 점프
///  마우스 좌클릭 : 검 공격
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WarriorController : MonoBehaviour
{
    // ── Inspector Settings ─────────────────────────────────────────────────
    [Header("Movement")]
    [Tooltip("이동 속도 (m/s)")]
    public float moveSpeed = 5f;

    [Tooltip("점프 힘")]
    public float jumpForce = 12f;

    [Tooltip("지면 레이어")]
    public LayerMask groundLayer;

    [Header("Ground Check")]
    [Tooltip("발 위치 오브젝트 (없으면 캐릭터 아래 자동 계산)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Attack")]
    [Tooltip("공격 1회당 쿨타임 (초)")]
    public float attackCooldown = 0.5f;

    [Tooltip("공격 히트박스 범위")]
    public float attackRange  = 1.2f;
    public Vector2 attackOffset = new Vector2(0.6f, 0f);

    [Header("Attack Hit Effect")]
    public GameObject hitEffectPrefab;   // (선택) 타격 이펙트 프리팹

    // ── State ──────────────────────────────────────────────────────────────
    public enum State { Idle, Walk, Jump, Fall, Attack }
    public State CurrentState { get; private set; } = State.Idle;

    // ── Components ─────────────────────────────────────────────────────────
    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;

    // ── Runtime vars ───────────────────────────────────────────────────────
    private bool  isGrounded     = false;
    private bool  isAttacking    = false;
    private float attackTimer    = 0f;
    private float horizontal     = 0f;
    private bool  facingRight    = true;

    // ── Animator Hashes ────────────────────────────────────────────────────
    // These must match the parameter names in the Animator Controller
    private static readonly int H_State     = Animator.StringToHash("State");
    private static readonly int H_IsGround  = Animator.StringToHash("IsGrounded");
    private static readonly int H_VelY      = Animator.StringToHash("VelocityY");
    private static readonly int H_Attack    = Animator.StringToHash("Attack");
    private static readonly int H_Jump      = Animator.StringToHash("Jump");
    private static readonly int H_MoveSpeed = Animator.StringToHash("MoveSpeed");

    // State integer values (used in Animator)
    const int S_IDLE   = 0;
    const int S_WALK   = 1;
    const int S_JUMP   = 2;
    const int S_FALL   = 3;
    const int S_ATTACK = 4;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
    }

    void Update()
    {
        TickAttackCooldown();
        CheckGround();
        ReadInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // Only apply horizontal movement when not attacking
        if (!isAttacking)
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
    }

    // ── Ground Check ───────────────────────────────────────────────────────
    void CheckGround()
    {
        Vector2 origin = groundCheck
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.5f;

        isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);
    }

    // ── Input ──────────────────────────────────────────────────────────────
    void ReadInput()
    {
        // Horizontal movement (A/D and arrow keys)
        horizontal = Input.GetAxisRaw("Horizontal");

        // Flip sprite based on direction
        if (horizontal > 0f  && !facingRight) Flip(true);
        if (horizontal < 0f  && facingRight)  Flip(false);

        // Jump  (Space)
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
            DoJump();

        // Attack  (Left mouse button)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
            DoAttack();

        // Determine state
        if (!isAttacking)
        {
            if (!isGrounded)
                SetState(rb.velocity.y >= 0 ? State.Jump : State.Fall);
            else if (Mathf.Abs(horizontal) > 0.01f)
                SetState(State.Walk);
            else
                SetState(State.Idle);
        }
    }

    // ── Jump ───────────────────────────────────────────────────────────────
    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        SetState(State.Jump);
        anim.SetTrigger(H_Jump);
    }

    // ── Attack ─────────────────────────────────────────────────────────────
    void DoAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        SetState(State.Attack);
        anim.SetTrigger(H_Attack);

        // 실제 히트 판정 (약간의 딜레이 후 실행 – 검이 앞으로 나오는 타이밍)
        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        // 공격 애니메이션의 "임팩트 프레임" 타이밍 (0.15초 후)
        yield return new WaitForSeconds(0.15f);
        PerformHit();

        // 공격 끝나면 상태 초기화
        yield return new WaitForSeconds(attackCooldown - 0.15f);
        isAttacking = false;
        SetState(State.Idle);
    }

    void PerformHit()
    {
        // 히트박스 중심 (캐릭터가 보는 방향으로 오프셋)
        Vector2 hitCenter = (Vector2)transform.position
            + new Vector2(attackOffset.x * (facingRight ? 1f : -1f), attackOffset.y);

        // 범위 안의 적 탐지
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackRange);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // IDamageable 인터페이스 지원 오브젝트에 데미지 전달
            IDamageable dmg = hit.GetComponent<IDamageable>();
            dmg?.TakeDamage(1, hitCenter);

            // 타격 이펙트
            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, hit.ClosestPoint(hitCenter), Quaternion.identity);
        }
    }

    // ── Cooldown tick ─────────────────────────────────────────────────────
    void TickAttackCooldown()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    void Flip(bool toRight)
    {
        facingRight = toRight;
        sr.flipX    = !toRight;   // flipX: false=right, true=left
    }

    void SetState(State s)
    {
        if (CurrentState == s) return;
        CurrentState = s;
    }

    // ── Animator sync ─────────────────────────────────────────────────────
    void UpdateAnimator()
    {
        int si = CurrentState switch
        {
            State.Idle   => S_IDLE,
            State.Walk   => S_WALK,
            State.Jump   => S_JUMP,
            State.Fall   => S_FALL,
            State.Attack => S_ATTACK,
            _            => S_IDLE,
        };

        anim.SetInteger(H_State,    si);
        anim.SetBool(H_IsGround,    isGrounded);
        anim.SetFloat(H_VelY,       rb.velocity.y);
        anim.SetFloat(H_MoveSpeed,  Mathf.Abs(horizontal));
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Ground check
        Vector3 gc = groundCheck
            ? groundCheck.position
            : transform.position + Vector3.down * 0.5f;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(gc, groundCheckRadius);

        // Attack range
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Vector3 hitC = transform.position +
            new Vector3(attackOffset.x * (facingRight ? 1f : -1f), attackOffset.y, 0f);
        Gizmos.DrawWireSphere(hitC, attackRange);
    }
}

// ── Damageable interface ──────────────────────────────────────────────────
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 hitPoint);
}
