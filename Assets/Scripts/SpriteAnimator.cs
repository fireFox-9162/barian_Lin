using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpriteAnimator – 직접 스프라이트 시트를 프레임 단위로 재생하는 경량 애니메이터
/// Unity Animator 대신 단순 프레임 재생이 필요할 때 사용
/// </summary>
public class SpriteAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationClip
    {
        public string       name;
        public Sprite[]     frames;
        public float        fps     = 12f;
        public bool         loop    = true;
        public bool         pingPong = false;
    }

    [Header("Clips")]
    public List<AnimationClip> clips = new List<AnimationClip>();

    [Header("Default")]
    public string defaultClip = "walk_right";

    private SpriteRenderer  sr;
    private AnimationClip   current;
    private int             frameIndex = 0;
    private float           timer      = 0f;
    private bool            playing    = true;
    private int             direction  = 1;   // for pingpong

    public System.Action<string> OnAnimationEnd;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Play(defaultClip);
    }

    void Update()
    {
        if (!playing || current == null || current.frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameTime = 1f / current.fps;

        while (timer >= frameTime)
        {
            timer -= frameTime;
            AdvanceFrame();
        }

        if (current.frames.Length > frameIndex && frameIndex >= 0)
            sr.sprite = current.frames[frameIndex];
    }

    void AdvanceFrame()
    {
        if (current.pingPong)
        {
            frameIndex += direction;
            if (frameIndex >= current.frames.Length - 1) direction = -1;
            if (frameIndex <= 0)                         direction =  1;
        }
        else
        {
            frameIndex++;
            if (frameIndex >= current.frames.Length)
            {
                if (current.loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    frameIndex = current.frames.Length - 1;
                    playing    = false;
                    OnAnimationEnd?.Invoke(current.name);
                }
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public void Play(string clipName, bool forceRestart = false)
    {
        if (current != null && current.name == clipName && !forceRestart) return;

        AnimationClip found = clips.Find(c => c.name == clipName);
        if (found == null)
        {
            Debug.LogWarning($"[SpriteAnimator] Clip not found: {clipName}");
            return;
        }

        current    = found;
        frameIndex = 0;
        timer      = 0f;
        direction  = 1;
        playing    = true;

        if (found.frames.Length > 0)
            sr.sprite = found.frames[0];
    }

    public void Stop()  { playing = false; }
    public void Resume(){ playing = true;  }

    public string CurrentClipName => current?.name ?? "";
    public bool   IsPlaying       => playing;
}
