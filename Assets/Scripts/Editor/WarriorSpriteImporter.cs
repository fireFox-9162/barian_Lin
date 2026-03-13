using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
/// <summary>
/// WarriorSpriteImporter – Editor Tool
/// 
/// Unity 메뉴 → Tools → Warrior Sprite Importer → Setup All
/// 를 실행하면:
///   1. Sprites/Warrior/*.png  를 Sprite Sheet 로 임포트
///   2. 각 스프라이트 시트를 올바른 프레임 수로 Slice
///   3. Animator Controller + Animation Clips 자동 생성
///   4. 프리팹 생성 (WarriorController, SpriteAnimator 포함)
/// </summary>
public class WarriorSpriteImporter : EditorWindow
{
    // ── 스프라이트 시트 정보 ────────────────────────────────────────────────
    static readonly (string file, string clipName, int frames, float fps, bool loop)[] SHEETS =
    {
        ("warrior_walk_right",        "walk_right", 8,  12f, true ),
        ("warrior_walk_left",         "walk_left",  8,  12f, true ),
        ("warrior_slash1_horizontal", "slash1",     6,  14f, false),
        ("warrior_slash2_downslash",  "slash2",     6,  14f, false),
        ("warrior_slash3_combo",      "slash3",     8,  14f, false),
        ("warrior_pickup",            "pickup",     5,   8f, false),
        ("warrior_jump",              "jump",       6,  10f, false),
    };

    const int SPRITE_SIZE = 128;
    const string SPRITES_PATH    = "Assets/Sprites/Warrior";
    const string ANIM_PATH       = "Assets/Animations/Warrior";
    const string PREFAB_PATH     = "Assets/Prefabs";
    const string ANIM_CTRL_PATH  = "Assets/Animations/Warrior/Warrior.controller";

    // ── Menu Item ──────────────────────────────────────────────────────────
    [MenuItem("Tools/Warrior Sprite Importer/Setup All")]
    static void SetupAll()
    {
        EnsureDirectories();
        ImportAndSliceSprites();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CreateAnimatorController();
        CreatePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Warrior setup complete!");
        EditorUtility.DisplayDialog("완료", "Warrior 스프라이트 & 애니메이션 설정이 완료되었습니다!", "OK");
    }

    [MenuItem("Tools/Warrior Sprite Importer/Import Sprites Only")]
    static void ImportSpritesOnly()
    {
        EnsureDirectories();
        ImportAndSliceSprites();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Sprites imported!");
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    static void EnsureDirectories()
    {
        CreateFolderIfNeeded("Assets/Sprites/Warrior");
        CreateFolderIfNeeded("Assets/Animations/Warrior");
        CreateFolderIfNeeded("Assets/Prefabs");
    }

    static void CreateFolderIfNeeded(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    static void ImportAndSliceSprites()
    {
        foreach (var (file, clipName, frames, fps, loop) in SHEETS)
        {
            string assetPath = $"{SPRITES_PATH}/{file}.png";
            if (!File.Exists(Application.dataPath + assetPath.Substring("Assets".Length)))
            {
                Debug.LogWarning($"⚠ Sprite not found: {assetPath}");
                continue;
            }

            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti == null)
            {
                AssetDatabase.ImportAsset(assetPath);
                ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            }
            if (ti == null) continue;

            // Sprite sheet settings
            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.filterMode          = FilterMode.Point;            // pixel-art sharp
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize      = 2048;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;

            // Slice into frames
            var sprMeta = new List<SpriteMetaData>();
            for (int i = 0; i < frames; i++)
            {
                sprMeta.Add(new SpriteMetaData
                {
                    name = $"{file}_{i}",
                    rect = new Rect(i * SPRITE_SIZE, 0, SPRITE_SIZE, SPRITE_SIZE),
                    pivot = new Vector2(0.5f, 0f),
                    alignment = (int)SpriteAlignment.BottomCenter,
                });
            }
            ti.spritesheet = sprMeta.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            Debug.Log($"  ✓ Sliced: {file} ({frames} frames)");
        }
    }

    static void CreateAnimatorController()
    {
        // Create Animator Controller
        var ctrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(ANIM_CTRL_PATH);

        // Parameters
        ctrl.AddParameter("State",      UnityEditor.Animations.AnimatorControllerParameterType.Int);
        ctrl.AddParameter("IsGrounded", UnityEditor.Animations.AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("VelocityY",  UnityEditor.Animations.AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Slash1",     UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Slash2",     UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Slash3",     UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("PickUp",     UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Jump",       UnityEditor.Animations.AnimatorControllerParameterType.Trigger);

        var root = ctrl.layers[0].stateMachine;

        // Create states for each animation
        foreach (var (file, clipName, frames, fps, loop) in SHEETS)
        {
            string assetPath = $"{SPRITES_PATH}/{file}.png";
            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // Build AnimationClip
            var clip   = new AnimationClip();
            clip.name  = clipName;
            clip.frameRate = fps;

            if (!loop)
                clip.wrapMode = WrapMode.Once;

            // Keyframes
            var keyframes = new ObjectReferenceKeyframe[frames + 1];
            for (int i = 0; i < frames; i++)
            {
                Sprite spr = null;
                foreach (var obj in sprites)
                    if (obj is Sprite s && s.name == $"{file}_{i}")
                    { spr = s; break; }

                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time  = i / fps,
                    value = spr,
                };
            }
            // Last frame same as first (loop)
            keyframes[frames] = new ObjectReferenceKeyframe
            {
                time  = frames / fps,
                value = keyframes[0].value,
            };

            var binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite",
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Loop settings
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            string clipPath = $"{ANIM_PATH}/{clipName}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);

            // Add state to controller
            var state = root.AddState(clipName);
            state.motion = clip;
            Debug.Log($"  ✓ Animation clip: {clipName}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Animator Controller created: " + ANIM_CTRL_PATH);
    }

    static void CreatePrefab()
    {
        // Create GameObject
        var go = new GameObject("Warrior");

        // SpriteRenderer
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder     = 0;

        // Rigidbody2D
        var rb = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;
        rb.gravityScale           = 3f;

        // BoxCollider2D
        var col = go.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(0.6f, 1.0f);
        col.offset = new Vector2(0f, 0.5f);

        // Animator
        var anim = go.AddComponent<Animator>();
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ANIM_CTRL_PATH);
        if (ctrl != null) anim.runtimeAnimatorController = ctrl;

        // WarriorController
        var wc = go.AddComponent<WarriorController>();

        // Ground check child
        var gcheck = new GameObject("GroundCheck");
        gcheck.transform.SetParent(go.transform);
        gcheck.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        wc.groundCheck = gcheck.transform;

        // Save prefab
        string prefabPath = $"{PREFAB_PATH}/Warrior.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log("✅ Prefab saved: " + prefabPath);
    }
}
#endif
