using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimationSetup : EditorWindow
{
    private const string BasePath = "Assets/Sprites";
    private const string AnimPath = "Assets/Animations";
    private const string ControllerPath = "Assets/Animations/PlayerController.controller";

    [MenuItem("Tools/Stick Wick/Setup Player Animations")]
    public static void ShowWindow()
    {
        GetWindow<AnimationSetup>("Animation Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Player Animation Setup", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("1. Generate/Verify Sprites"))
        {
            EnsureSpritesExist(force: false);
        }

        if (GUILayout.Button("1b. Force Regenerate Sprites (overwrite)"))
        {
            EnsureSpritesExist(force: true);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("2. Create Animator & Clips"))
        {
            CreateAnimatorAndClips();
        }

        if (GUILayout.Button("3. Setup Player Object"))
        {
            SetupPlayer();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // SPRITE GENERATION
    // ─────────────────────────────────────────────────────────────

    private void EnsureSpritesExist(bool force)
    {
        if (!Directory.Exists(BasePath))
            Directory.CreateDirectory(BasePath);

        if (force)
        {
            // Delete old sprites so they will be regenerated
            string[] names = { "Player_Idle.png", "Player_Run_1.png", "Player_Run_2.png", "Player_Jump.png" };
            foreach (var name in names)
            {
                string full = Path.Combine(BasePath, name);
                if (File.Exists(full)) File.Delete(full);
            }
        }

        // All frames share the same palette – no colour flash between states
        GenerateIdleSprite();
        GenerateRunSprite1();
        GenerateRunSprite2();
        GenerateJumpSprite();

        AssetDatabase.Refresh();

        // Make every new PNG readable as a sprite
        string[] allNames = { "Player_Idle.png", "Player_Run_1.png", "Player_Run_2.png", "Player_Jump.png" };
        foreach (var name in allNames)
        {
            string assetPath = BasePath + "/" + name;
            TextureImporter imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp != null)
            {
                imp.textureType          = TextureImporterType.Sprite;
                imp.spriteImportMode     = SpriteImportMode.Single;
                imp.spritePixelsPerUnit  = 64;
                imp.filterMode           = FilterMode.Point;
                imp.textureCompression   = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
            }
        }

        Debug.Log("[AnimationSetup] Stick-figure sprites generated in " + BasePath);
    }

    // ── Drawing helpers ──────────────────────────────────────────

    private const int W = 64;
    private const int H = 128;

    // Background: transparent white
    private static readonly Color BG   = new Color(1f, 1f, 1f, 0f);
    // Body colour: dark charcoal – consistent across all frames
    private static readonly Color BODY = new Color(0.15f, 0.15f, 0.15f, 1f);
    // Head fill: light skin tone
    private static readonly Color HEAD = new Color(0.95f, 0.80f, 0.65f, 1f);

    private Texture2D NewTex()
    {
        var tex = new Texture2D(W, H);
        Color[] bg = new Color[W * H];
        for (int i = 0; i < bg.Length; i++) bg[i] = BG;
        tex.SetPixels(bg);
        return tex;
    }

    // Draw a filled circle of radius r centred at (cx, cy)
    private void DrawCircle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r; y++)
        for (int x = cx - r; x <= cx + r; x++)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) continue;
            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                tex.SetPixel(x, y, col);
        }
    }

    // Draw a thick line (Bresenham + thickness)
    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col, int thickness = 2)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
            for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
            {
                int px = x0 + tx, py = y0 + ty;
                if (px >= 0 && px < W && py >= 0 && py < H)
                    tex.SetPixel(px, py, col);
            }
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    private void SaveSprite(Texture2D tex, string fileName)
    {
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(BasePath, fileName);
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(tex);
    }

    // ── Idle: upright, arms at sides ────────────────────────────
    //  Grid (y=0 is bottom of texture in Unity's coord, flip for readability):
    //  Head centre  ~ (32, 108)
    //  Neck base    ~ (32, 96)
    //  Hips         ~ (32, 72)
    //  Left/Right shoulder ~ (22/42, 96)
    //  Left/Right hand     ~ (18/46, 80)
    //  Left/Right knee     ~ (26/38, 50)
    //  Left/Right foot     ~ (26/38, 32)
    private void GenerateIdleSprite()
    {
        var tex = NewTex();
        DrawCircle(tex, 32, 108, 10, HEAD);  // head
        DrawCircle(tex, 32, 108, 10, BODY);  // head outline – ring
        DrawCircle(tex, 32, 108,  8, HEAD);

        DrawLine(tex, 32, 97, 32, 72, BODY); // spine
        DrawLine(tex, 32, 92, 22, 80, BODY); // L arm
        DrawLine(tex, 32, 92, 42, 80, BODY); // R arm
        DrawLine(tex, 32, 72, 26, 50, BODY); // L thigh
        DrawLine(tex, 32, 72, 38, 50, BODY); // R thigh
        DrawLine(tex, 26, 50, 26, 32, BODY); // L shin
        DrawLine(tex, 38, 50, 38, 32, BODY); // R shin
        DrawLine(tex, 26, 32, 18, 32, BODY); // L foot
        DrawLine(tex, 38, 32, 46, 32, BODY); // R foot

        SaveSprite(tex, "Player_Idle.png");
    }

    // ── Run frame 1: left leg forward, right arm forward ────────
    private void GenerateRunSprite1()
    {
        var tex = NewTex();
        // Slight forward lean – head shifted +2 right, spine tilted
        DrawCircle(tex, 34, 108, 10, HEAD);
        DrawCircle(tex, 34, 108, 10, BODY);
        DrawCircle(tex, 34, 108,  8, HEAD);

        // Spine (slight lean forward)
        DrawLine(tex, 34, 97, 30, 72, BODY);

        // Arms: R arm forward/up, L arm back/down
        DrawLine(tex, 30, 91, 42, 100, BODY); // R arm forward
        DrawLine(tex, 30, 91, 18,  82, BODY); // L arm back

        // Legs: L leg forward (knee up), R leg back
        DrawLine(tex, 30, 72, 20, 52, BODY); // L thigh (forward)
        DrawLine(tex, 20, 52, 24, 34, BODY); // L shin
        DrawLine(tex, 24, 34, 14, 34, BODY); // L foot

        DrawLine(tex, 30, 72, 40, 58, BODY); // R thigh (back)
        DrawLine(tex, 40, 58, 46, 40, BODY); // R shin
        DrawLine(tex, 46, 40, 54, 40, BODY); // R foot

        SaveSprite(tex, "Player_Run_1.png");
    }

    // ── Run frame 2: right leg forward, left arm forward ────────
    // Mirror of Run_1 for a two-frame stride cycle
    private void GenerateRunSprite2()
    {
        var tex = NewTex();
        DrawCircle(tex, 30, 108, 10, HEAD);
        DrawCircle(tex, 30, 108, 10, BODY);
        DrawCircle(tex, 30, 108,  8, HEAD);

        DrawLine(tex, 30, 97, 34, 72, BODY); // spine (lean other way)

        // Arms: L arm forward/up, R arm back/down
        DrawLine(tex, 34, 91, 22, 100, BODY); // L arm forward
        DrawLine(tex, 34, 91, 46,  82, BODY); // R arm back

        // Legs: R leg forward, L leg back
        DrawLine(tex, 34, 72, 44, 52, BODY); // R thigh (forward)
        DrawLine(tex, 44, 52, 40, 34, BODY); // R shin
        DrawLine(tex, 40, 34, 50, 34, BODY); // R foot

        DrawLine(tex, 34, 72, 24, 58, BODY); // L thigh (back)
        DrawLine(tex, 24, 58, 18, 40, BODY); // L shin
        DrawLine(tex, 18, 40, 10, 40, BODY); // L foot

        SaveSprite(tex, "Player_Run_2.png");
    }

    // ── Jump: arms raised, knees tucked ─────────────────────────
    private void GenerateJumpSprite()
    {
        var tex = NewTex();
        DrawCircle(tex, 32, 112, 10, HEAD);
        DrawCircle(tex, 32, 112, 10, BODY);
        DrawCircle(tex, 32, 112,  8, HEAD);

        // Spine straight up
        DrawLine(tex, 32, 101, 32, 80, BODY);

        // Arms: both raised in a V
        DrawLine(tex, 32, 96, 20, 108, BODY); // L arm up
        DrawLine(tex, 32, 96, 44, 108, BODY); // R arm up

        // Legs: knees bent upward (tucked)
        DrawLine(tex, 32, 80, 22, 62, BODY); // L thigh up
        DrawLine(tex, 22, 62, 30, 48, BODY); // L shin curled
        DrawLine(tex, 30, 48, 20, 44, BODY); // L foot

        DrawLine(tex, 32, 80, 42, 62, BODY); // R thigh up
        DrawLine(tex, 42, 62, 34, 48, BODY); // R shin curled
        DrawLine(tex, 34, 48, 44, 44, BODY); // R foot

        SaveSprite(tex, "Player_Jump.png");
    }

    // ─────────────────────────────────────────────────────────────
    // ANIMATOR / CLIPS
    // ─────────────────────────────────────────────────────────────

    private void CreateAnimatorAndClips()
    {
        if (!Directory.Exists(AnimPath)) Directory.CreateDirectory(AnimPath);

        // Remove old controller if present so we get a clean rebuild
        if (File.Exists(ControllerPath))
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AssetDatabase.Refresh();
        }

        var controller    = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var rootStateMachine = controller.layers[0].stateMachine;

        // ── Parameters ──────────────────────────────────────────
        controller.AddParameter("Speed",           AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded",      AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);

        // ── Clips ────────────────────────────────────────────────
        AnimationClip idleClip = CreateClip("Idle", loop: true,  0.3f, "Player_Idle.png");
        AnimationClip runClip  = CreateClip("Run",  loop: true,  0.2f, "Player_Run_1.png", "Player_Run_2.png");
        AnimationClip jumpClip = CreateClip("Jump", loop: false, 0.1f, "Player_Jump.png");

        // ── States ───────────────────────────────────────────────
        var idleState = rootStateMachine.AddState("Idle"); idleState.motion = idleClip;
        var runState  = rootStateMachine.AddState("Run");  runState.motion  = runClip;
        var jumpState = rootStateMachine.AddState("Jump"); jumpState.motion = jumpClip;

        // Default state = Idle
        rootStateMachine.defaultState = idleState;

        // ── Transitions ──────────────────────────────────────────
        // Idle <-> Run
        AddTransition(idleState, runState,  0.05f).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        AddTransition(runState,  idleState, 0.05f).AddCondition(AnimatorConditionMode.Less,    0.1f, "Speed");

        // Grounded -> Jump (use AnyState for reliability)
        var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
        anyToJump.duration           = 0.05f;
        anyToJump.canTransitionToSelf = false;

        // Jump -> Idle / Run on landing
        var jumpToIdle = AddTransition(jumpState, idleState, 0.05f);
        jumpToIdle.AddCondition(AnimatorConditionMode.If,   0,    "IsGrounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        var jumpToRun = AddTransition(jumpState, runState, 0.05f);
        jumpToRun.AddCondition(AnimatorConditionMode.If,      0,    "IsGrounded");
        jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[AnimationSetup] Animator Controller created at " + ControllerPath);
    }

    private AnimatorStateTransition AddTransition(AnimatorState from, AnimatorState to, float duration)
    {
        var t = from.AddTransition(to);
        t.duration             = duration;
        t.hasExitTime          = false;
        return t;
    }

    /// <summary>
    /// Creates an AnimationClip that flips through a list of sprites.
    /// frameTime = seconds each frame is shown (e.g. 0.2 = 5 fps).
    /// </summary>
    private AnimationClip CreateClip(string clipName, bool loop, float frameTime, params string[] spriteNames)
    {
        AnimationClip clip = new AnimationClip { name = clipName };

        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteNames.Length];
        for (int i = 0; i < spriteNames.Length; i++)
        {
            string spritePath = BasePath + "/" + spriteNames[i];
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) Debug.LogWarning("[AnimationSetup] Sprite not found: " + spritePath);

            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i * frameTime,
                value = sprite
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        string clipPath = AnimPath + "/" + clipName + ".anim";
        // Remove stale clip if re-creating
        if (File.Exists(clipPath)) AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────
    // PLAYER SETUP
    // ─────────────────────────────────────────────────────────────

    private void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[AnimationSetup] Player object not found in scene!");
            return;
        }

        if (!player.GetComponent<Animator>())      player.AddComponent<Animator>();
        if (!player.GetComponent<SpriteRenderer>()) player.AddComponent<SpriteRenderer>();
        if (!player.GetComponent<PlayerAnimation>()) player.AddComponent<PlayerAnimation>();

        Animator animator = player.GetComponent<Animator>();
        animator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        Debug.Log("[AnimationSetup] Player setup complete on " + player.name);
    }
}
