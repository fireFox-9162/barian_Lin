using UnityEngine;

/// <summary>
/// WarriorAnimatorBridge
/// WarriorController 상태를 읽어 WarriorAnimator 클립을 자동으로 전환
/// </summary>
[RequireComponent(typeof(WarriorController))]
[RequireComponent(typeof(WarriorAnimator))]
public class WarriorAnimatorBridge : MonoBehaviour
{
    private WarriorController ctrl;
    private WarriorAnimator   wa;
    private SpriteRenderer    sr;

    private WarriorController.State prev;
    private bool prevFacing = true;

    // Clip name constants
    const string IDLE   = "idle";
    const string WALK_R = "walk_right";
    const string WALK_L = "walk_left";
    const string ATK    = "attack";
    const string JUMP   = "jump";

    void Awake()
    {
        ctrl = GetComponent<WarriorController>();
        wa   = GetComponent<WarriorAnimator>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        wa.OnClipFinished += OnClipEnd;
        wa.Play(IDLE);
    }

    void Update()
    {
        var state = ctrl.CurrentState;

        // Attack and jump triggered via direct play from controller events;
        // track state change for walk/idle
        bool facingRight = !sr.flipX;

        if (state == WarriorController.State.Attack)
        {
            wa.Play(ATK, restart: false);
            prev = state;
            return;
        }

        if (state == WarriorController.State.Jump || state == WarriorController.State.Fall)
        {
            wa.Play(JUMP, restart: false);
            prev = state;
            return;
        }

        if (state == prev && facingRight == prevFacing) return;
        prev        = state;
        prevFacing  = facingRight;

        switch (state)
        {
            case WarriorController.State.Walk:
                wa.Play(facingRight ? WALK_R : WALK_L);
                break;
            default:
                wa.Play(IDLE);
                break;
        }
    }

    void OnClipEnd(string clipName)
    {
        if (clipName == ATK || clipName == JUMP)
            wa.Play(IDLE);
    }
}
