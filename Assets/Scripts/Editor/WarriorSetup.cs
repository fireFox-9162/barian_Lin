using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
/// <summary>
/// WarriorSetup  –  Unity Editor 자동 셋업 툴
///
/// 메뉴:  Tools ▶ Warrior Setup ▶ Run Full Setup
///
/// 실행 시:
///   1. Assets/Sprites/Warrior/*.png  →  Sprite Sheet 임포트 & 슬라이스
///   2. Animator Controller + Animation Clips 자동 생성
///   3. Warrior Prefab 자동 생성 (WarriorController, WarriorAnimator 포함)
/// </summary>
public class WarriorSetup : EditorWindow
{
    // ── Sheet definitions ─────────────────────────────────────────────────
    static readonly (string file, string clip, int frames, float fps, bool loop)[] SHEETS =
    {
        ("warrior_idle",        "idle",        4,  8f,  true ),
        ("warrior_walk_right",  "walk_right",  8,  12f, true ),
        ("warrior_walk_left",   "walk_left",   8,  12f, true ),
        ("warrior_attack",      "attack",      6,  14f, false),
        ("warrior_jump",        "jump",        6,  10f, false),
    };

    // Frame size in pixels
    const int FW = 144;
    const int FH = 192;

    const string SPR_PATH  = "Assets/Sprites/Warrior";
    const string ANIM_PATH = "Assets/Animations/Warrior";
    const string PREF_PATH = "Assets/Prefabs";
    const string CTRL_PATH = "Assets/Animations/Warrior/Warrior.controller";

    // ── Menu ──────────────────────────────────────────────────────────────
    [MenuItem("Tools/Warrior Setup/▶ Run Full Setup")]
    static void RunFull()
    {
        EnsureFolders();
        ImportSprites();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        BuildAnimatorController();
        BuildPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("✅ Warrior Setup 완료",
            "스프라이트 슬라이싱, 애니메이션 클립, Animator Controller, Prefab 생성이 완료되었습니다!\n\n" +
            "Assets/Prefabs/Warrior.prefab 을 씬에 배치하고 플레이해 보세요.", "OK");
    }

    [MenuItem("Tools/Warrior Setup/Sprites Only")]
    static void RunSprites() { EnsureFolders(); ImportSprites(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    // ── Folder helpers ────────────────────────────────────────────────────
    static void EnsureFolders()
    {
        MkDir("Assets/Sprites/Warrior");
        MkDir("Assets/Animations/Warrior");
        MkDir("Assets/Prefabs");
    }
    static void MkDir(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path), Path.GetFileName(path));
    }

    // ── Sprite import & slice ─────────────────────────────────────────────
    static void ImportSprites()
    {
        foreach (var (file, clip, frames, fps, loop) in SHEETS)
        {
            string ap = $"{SPR_PATH}/{file}.png";
            string fp = Application.dataPath + ap.Substring("Assets".Length);
            if (!File.Exists(fp)) { Debug.LogWarning($"Missing: {ap}"); continue; }

            AssetDatabase.ImportAsset(ap, ImportAssetOptions.ForceUpdate);
            var ti = AssetImporter.GetAtPath(ap) as TextureImporter;
            if (ti == null) continue;

            ti.textureType          = TextureImporterType.Sprite;
            ti.spriteImportMode     = SpriteImportMode.Multiple;
            ti.filterMode           = FilterMode.Point;
            ti.textureCompression   = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency  = true;
            ti.mipmapEnabled        = false;
            ti.maxTextureSize       = 4096;
            ti.spritePixelsPerUnit  = 32;

            var metas = new SpriteMetaData[frames];
            for (int i = 0; i < frames; i++)
                metas[i] = new SpriteMetaData
                {
                    name      = $"{file}_{i}",
                    rect      = new Rect(i * FW, 0, FW, FH),
                    pivot     = new Vector2(0.5f, 0f),
                    alignment = (int)SpriteAlignment.BottomCenter,
                };
            ti.spritesheet = metas;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            Debug.Log($"  ✓ sliced {file}  ({frames} frames)");
        }
    }

    // ── Animator Controller ───────────────────────────────────────────────
    static void BuildAnimatorController()
    {
        var ctrl = UnityEditor.Animations.AnimatorController
            .CreateAnimatorControllerAtPath(CTRL_PATH);

        // Parameters
        ctrl.AddParameter("State",     UnityEditor.Animations.AnimatorControllerParameterType.Int);
        ctrl.AddParameter("IsGrounded",UnityEditor.Animations.AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("VelocityY", UnityEditor.Animations.AnimatorControllerParameterType.Float);
        ctrl.AddParameter("MoveSpeed", UnityEditor.Animations.AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Attack",    UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Jump",      UnityEditor.Animations.AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var states = new Dictionary<string,
            UnityEditor.Animations.AnimatorState>();

        // Create states
        foreach (var (file, clip, frames, fps, loop) in SHEETS)
        {
            var animClip  = BuildAnimClip(file, clip, frames, fps, loop);
            var st        = sm.AddState(clip);
            st.motion     = animClip;
            st.speed      = 1f;
            states[clip]  = st;
        }

        // Set default state
        if (states.TryGetValue("idle", out var idleState))
            sm.defaultState = idleState;

        // ── Transitions ──────────────────────────────────────────────────
        // Any → attack (Trigger)
        AddAnyTrigger(sm, states["attack"], "Attack");
        // Any → jump (Trigger)
        AddAnyTrigger(sm, states["jump"], "Jump");

        // idle → walk_right  (State == 1)
        AddIntTrans(states["idle"], states["walk_right"], "State", 1);
        // idle → walk_left   (State == 2)  -- we use flipX on SpriteRenderer instead
        AddIntTrans(states["idle"], states["walk_left"],  "State", 2);

        // walk_right → idle
        AddIntTrans(states["walk_right"], states["idle"], "State", 0);
        // walk_left  → idle
        AddIntTrans(states["walk_left"],  states["idle"], "State", 0);
        // walk_right ↔ walk_left
        AddIntTrans(states["walk_right"], states["walk_left"],  "State", 2);
        AddIntTrans(states["walk_left"],  states["walk_right"], "State", 1);

        // attack → idle  (exit time)
        AddExitTrans(states["attack"], states["idle"]);
        // jump → idle (exit + IsGrounded)
        var jt = states["jump"].AddTransition(states["idle"]);
        jt.hasExitTime  = true; jt.exitTime = 0.9f;
        jt.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGrounded");

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Animator Controller created: " + CTRL_PATH);
    }

    static AnimationClip BuildAnimClip(string file, string clipName, int frames, float fps, bool loop)
    {
        string ap = $"{SPR_PATH}/{file}.png";
        var allObjs = AssetDatabase.LoadAllAssetsAtPath(ap);

        var clip        = new AnimationClip();
        clip.name       = clipName;
        clip.frameRate  = fps;

        var kfs = new ObjectReferenceKeyframe[frames + 1];
        for (int i = 0; i < frames; i++)
        {
            Sprite spr = null;
            foreach (var o in allObjs)
                if (o is Sprite s && s.name == $"{file}_{i}") { spr = s; break; }
            kfs[i] = new ObjectReferenceKeyframe { time = i / fps, value = spr };
        }
        kfs[frames] = new ObjectReferenceKeyframe { time = frames / fps, value = kfs[0].value };

        var binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, kfs);

        var settings       = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime  = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (!loop) clip.wrapMode = WrapMode.Once;

        string cp = $"{ANIM_PATH}/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, cp);
        return clip;
    }

    static void AddAnyTrigger(UnityEditor.Animations.AnimatorStateMachine sm,
                               UnityEditor.Animations.AnimatorState dest,
                               string trigger)
    {
        var t = sm.AddAnyStateTransition(dest);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, trigger);
        t.canTransitionToSelf = false;
        t.hasExitTime         = false;
        t.duration            = 0f;
    }

    static void AddIntTrans(UnityEditor.Animations.AnimatorState from,
                             UnityEditor.Animations.AnimatorState to,
                             string param, int val)
    {
        var t = from.AddTransition(to);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, val, param);
        t.hasExitTime = false; t.duration = 0.05f;
    }

    static void AddExitTrans(UnityEditor.Animations.AnimatorState from,
                              UnityEditor.Animations.AnimatorState to)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true; t.exitTime = 1f; t.duration = 0.05f;
    }

    // ── Prefab builder ────────────────────────────────────────────────────
    static void BuildPrefab()
    {
        var go = new GameObject("Warrior");

        // SpriteRenderer
        var sr             = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder    = 0;

        // Load idle sprite as default
        string ap = $"{SPR_PATH}/warrior_idle.png";
        var objs  = AssetDatabase.LoadAllAssetsAtPath(ap);
        foreach (var o in objs)
            if (o is Sprite s) { sr.sprite = s; break; }

        // Rigidbody2D
        var rb                    = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;
        rb.gravityScale           = 4f;
        rb.drag                   = 0f;

        // BoxCollider2D
        var col    = go.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(0.55f, 1.1f);
        col.offset = new Vector2(0f, 0.55f);

        // Animator
        var anim = go.AddComponent<Animator>();
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CTRL_PATH);
        if (ctrl != null) anim.runtimeAnimatorController = ctrl;

        // WarriorController
        var wc = go.AddComponent<WarriorController>();

        // Ground check child
        var gc = new GameObject("GroundCheck");
        gc.transform.SetParent(go.transform);
        gc.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        wc.groundCheck = gc.transform;

        // WarriorAnimator  (optional – works alongside Animator)
        go.AddComponent<WarriorAnimator>();

        // Save prefab
        MkDir(PREF_PATH);
        string pp = $"{PREF_PATH}/Warrior.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, pp);
        Object.DestroyImmediate(go);
        Debug.Log("✅ Prefab saved: " + pp);
    }
}
#endif
