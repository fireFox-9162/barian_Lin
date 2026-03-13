using System.Collections;
using UnityEngine;

/// <summary>
/// WarriorAnimatorSetup – Animator Controller가 없을 때
/// SpriteAnimator와 WarriorController를 연결해 주는 브릿지
/// </summary>
[RequireComponent(typeof(SpriteAnimator))]
[RequireComponent(typeof(WarriorController))]
public class WarriorAnimatorSetup : MonoBehaviour
{
    private SpriteAnimator  sprAnim;
    private WarriorController ctrl;
    private WarriorController.State lastState;

    // Clip name constants (SpriteAnimator 클립 이름과 일치해야 함)
    const string IDLE          = "idle";
    const string WALK_RIGHT    = "walk_right";
    const string WALK_LEFT     = "walk_left";
    const string SLASH1        = "slash1";
    const string SLASH2        = "slash2";
    const string SLASH3        = "slash3";
    const string PICKUP        = "pickup";
    const string JUMP          = "jump";

    void Awake()
    {
        sprAnim = GetComponent<SpriteAnimator>();
        ctrl    = GetComponent<WarriorController>();
    }

    void Start()
    {
        // 공격 끝나면 자동으로 idle로 돌아오기
        sprAnim.OnAnimationEnd += OnClipEnd;
        ctrl.OnSlash1  += () => sprAnim.Play(SLASH1,  true);
        ctrl.OnSlash2  += () => sprAnim.Play(SLASH2,  true);
        ctrl.OnSlash3  += () => sprAnim.Play(SLASH3,  true);
        ctrl.OnPickUp  += () => sprAnim.Play(PICKUP,  true);
    }

    void Update()
    {
        var state = ctrl.CurrentState;
        if (state == lastState) return;
        lastState = state;

        switch (state)
        {
            case WarriorController.State.Idle:      sprAnim.Play(IDLE);       break;
            case WarriorController.State.WalkRight: sprAnim.Play(WALK_RIGHT); break;
            case WarriorController.State.WalkLeft:  sprAnim.Play(WALK_LEFT);  break;
            case WarriorController.State.Jump:      sprAnim.Play(JUMP, true); break;
            // Slash / PickUp states handled via events above
        }
    }

    void OnClipEnd(string clipName)
    {
        if (clipName == SLASH1 || clipName == SLASH2 ||
            clipName == SLASH3 || clipName == PICKUP)
        {
            sprAnim.Play(IDLE);
        }
    }
}
