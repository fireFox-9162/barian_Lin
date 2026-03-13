using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WarriorAnimator  –  스프라이트 시트를 직접 재생하는 컴포넌트
/// Unity Animator와 함께 OR 단독으로 사용 가능
///
/// ■ 클립 이름 규칙 (WarriorController 와 매핑)
///   "idle"   "walk_right"  "walk_left"
///   "attack" "jump"
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WarriorAnimator : MonoBehaviour
{
    // ── Clip definition ────────────────────────────────────────────────────
    [System.Serializable]
    public class AnimClip
    {
        public string   clipName;
        public Sprite[] frames;
        [Range(1, 60)]
        public float    fps  = 12f;
        public bool     loop = true;
    }

    // ── Inspector ──────────────────────────────────────────────────────────
    [Header("Clips (assign in Inspector or via SpriteSheetLoader)")]
    public List<AnimClip> clips = new List<AnimClip>();

    [Header("Start")]
    public string startClip = "idle";

    // ── Runtime ────────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private AnimClip        current;
    private int             frame;
    private float           timer;

    public System.Action<string> OnClipFinished;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Play(startClip);
    }

    void Update()
    {
        if (current == null || current.frames == null || current.frames.Length == 0) return;

        timer += Time.deltaTime;
        float dur = 1f / current.fps;
        while (timer >= dur)
        {
            timer -= dur;
            frame++;
            if (frame >= current.frames.Length)
            {
                if (current.loop)
                    frame = 0;
                else
                {
                    frame = current.frames.Length - 1;
                    OnClipFinished?.Invoke(current.clipName);
                    return;
                }
            }
        }
        sr.sprite = current.frames[frame];
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public void Play(string clipName, bool restart = false)
    {
        if (current != null && current.clipName == clipName && !restart) return;

        AnimClip found = clips.Find(c => c.clipName == clipName);
        if (found == null) { Debug.LogWarning($"[WarriorAnimator] clip not found: {clipName}"); return; }

        current = found;
        frame   = 0;
        timer   = 0f;
        if (found.frames.Length > 0) sr.sprite = found.frames[0];
    }

    public string Current => current?.clipName ?? "";
    public bool   IsPlaying(string n) => current?.clipName == n;
}
