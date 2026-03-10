using Engine;
using System;

public class PlayerMovementSFX : Entity
{
    // Optional: point to the entity hosting PlayerController if different
    public string controllerEntityName = "";

    // Clip banks (string paths to audio assets)
    public string[] walkClips =
    {
          "assets/Audio/SFX/Footstep_Player_Walk_01.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_02.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_03.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_04.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_05.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_06.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_07.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_08.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_09.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_10.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_11.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_12.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_13.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_14.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_15.wav",
          "assets/Audio/SFX/Footstep_Player_Walk_16.wav"
      };

    public string[] runClips =
    {
          "assets/Audio/SFX/Footstep_Player_Run_01.wav",
          "assets/Audio/SFX/Footstep_Player_Run_02.wav",
          "assets/Audio/SFX/Footstep_Player_Run_03.wav",
          "assets/Audio/SFX/Footstep_Player_Run_04.wav",
          "assets/Audio/SFX/Footstep_Player_Run_05.wav",
          "assets/Audio/SFX/Footstep_Player_Run_06.wav",
          "assets/Audio/SFX/Footstep_Player_Run_07.wav",
          "assets/Audio/SFX/Footstep_Player_Run_08.wav",
          "assets/Audio/SFX/Footstep_Player_Run_09.wav",
          "assets/Audio/SFX/Footstep_Player_Run_10.wav",
          "assets/Audio/SFX/Footstep_Player_Run_11.wav",
          "assets/Audio/SFX/Footstep_Player_Run_12.wav",
          "assets/Audio/SFX/Footstep_Player_Run_13.wav",
          "assets/Audio/SFX/Footstep_Player_Run_14.wav",
          "assets/Audio/SFX/Footstep_Player_Run_15.wav"
      };

    // Base movement values (will sync from PlayerController if found)
    //public float baseMoveSpeed = 5.0f;
    //public float sprintMultiplier = 2f;
    //public float crouchMultiplier = 0.5f;

    // Stride (meters between steps)
    private float walkStride = 4.167f;
    private float runStride = 5.20625f;
    private float crouchStride = 3.125f;

    // Animation loop times (seconds)
    public float walkLoop = 1.667f;
    public float runLoop = 0.833f;
    public float crouchLoop = 1.667f;

    // State volume scale
    public float walkVolume = 0.40f;
    public float runVolume = 0.50f;
    public float crouchVolume = 0.10f; // quieter crouch (uses walk bank)

    // Gating
    public float minSpeedToStep = 0.10f;
    public float tapStepMinDistance = 0.20f;

    // Animation event toggle (kept for parity; no animator hook here)
    public bool useAnimationEvents = false;
    public float animWalkVolume = 1.0f;
    public float animRunVolume = 1.0f;
    public float animCrouchVolume = 0.60f;

    public bool avoidImmediateRepeat = true;

    private TransformComponent tf;
    private RigidBodyComponent rb;
    private AudioComponent ac;
    private PlayerController pc;

    private enum MoveState { None, Walk, Run, Crouch }
    private MoveState currentState = MoveState.None;

    private float distSinceLast = 0f;
    private Vector3 lastPos;

    private int lastWalkIdx = -1;
    private int lastRunIdx = -1;
    private int lastCrouchIdx = -1;

    private static readonly Random s_Random = new Random();

    public override void OnInit()
    {
        tf = Transform;
        rb = HasComponent<RigidBodyComponent>() ? new RigidBodyComponent(ID) : null;
        ac = HasComponent<AudioComponent>() ? new AudioComponent(ID) : null;
        pc = ResolveController();

        //SyncMovementConfig();
        if (tf != null)
            lastPos = tf.Position;
    }

    public override void OnUpdate(float dt)
    {
        if (tf == null) return;

        // Reacquire if hot-added
        if (rb == null && HasComponent<RigidBodyComponent>()) rb = new RigidBodyComponent(ID);
        if (ac == null && HasComponent<AudioComponent>()) ac = new AudioComponent(ID);
        if (pc == null) pc = ResolveController();

        if (PlayerInputBlocker.IsBlocked)
        {
            currentState = MoveState.None;
            distSinceLast = 0f;
            lastPos = tf.Position;
            return;
        }

        //SyncMovementConfig();

        Vector3 pos = tf.Position;
        Vector3 delta = pos - lastPos; delta.y = 0f;

        float speed = delta.Mag / MathF.Max(dt, 0.0001f);
        //bool grounded = rb == null || MathF.Abs(rb.Velocity.y) < 0.5f;
        bool movingEnough = speed > minSpeedToStep;

        // Sample input similar to Unity script; uses player input rather than private pc booleans
        float x, z;
        bool hasInput = SampleMoveInput(out x, out z);

        bool crouching = Input.IsKeyHeld(KeyCode.LeftControl);
        bool sprinting = Input.IsKeyHeld(KeyCode.LeftShift) && !crouching;
        bool walking = movingEnough && hasInput && !sprinting && !crouching;

        // State priority: crouch > run > walk
        MoveState newState = MoveState.None;
        if (crouching) newState = MoveState.Crouch;
        else if (sprinting) newState = MoveState.Run;
        else if (walking) newState = MoveState.Walk;

        if (newState != currentState)
        {
            currentState = newState;
            distSinceLast = 0f;
            UpdateStrides();
        }

        string[] bank = null;
        float stride = 0f;
        float vol = 1f;
        switch (currentState)
        {
            case MoveState.Walk: bank = walkClips; stride = walkStride; vol = walkVolume; break;
            case MoveState.Run:
                bank = runClips != null && runClips.Length > 0 ? runClips : walkClips; stride =
runStride; vol = runVolume; break;
            case MoveState.Crouch: bank = walkClips; stride = crouchStride; vol = crouchVolume; break;
            default: bank = null; stride = 0f; break;
        }

        if (!useAnimationEvents && movingEnough && bank != null && bank.Length > 0)
        {
            distSinceLast += delta.Mag;
            if (distSinceLast >= stride)
            {
                PlayFootstep(bank, vol);
                distSinceLast = 0f;
            }
        }
        else
        {
            if (!useAnimationEvents && bank != null && bank.Length > 0 && distSinceLast >=
tapStepMinDistance && !movingEnough)
            {
                PlayFootstep(bank, vol);
            }
            distSinceLast = 0f;
        }

        lastPos = pos;
    }

    private PlayerController ResolveController()
    {
        // Prefer same-entity script
        var local = GetScript<PlayerController>();
        if (local != null)
            return local;

        // Fallback: find by name if provided
        if (!string.IsNullOrEmpty(controllerEntityName))
            return Entity.FindScriptByName<PlayerController>(controllerEntityName);

        return null;
    }

    //private void SyncMovementConfig()
    //{
    //    if (pc == null) return;
    //    baseMoveSpeed = pc.moveSpeed;
    //    sprintMultiplier = pc.sprintMultiplier;
    //    crouchMultiplier = pc.crouchMultiplier;
    //    UpdateStrides();
    //}

    private void UpdateStrides()
    {
        walkStride = pc.moveSpeed * (walkLoop / 2f);//baseMoveSpeed * (walkLoop / 2f);
        runStride = (pc.moveSpeed * pc.sprintMultiplier) * (runLoop / 2f);//(baseMoveSpeed * sprintMultiplier) * (runLoop / 2f);
        crouchStride = (pc.moveSpeed * pc.crouchMultiplier) * (crouchLoop / 2f);//(baseMoveSpeed * crouchMultiplier) * (crouchLoop / 2f);
    }

    private bool SampleMoveInput(out float x, out float z)
    {
        x = 0f; z = 0f;
        if (Input.IsKeyHeld(KeyCode.W)) { z -= 1f; }
        if (Input.IsKeyHeld(KeyCode.S)) { z += 1f; }
        if (Input.IsKeyHeld(KeyCode.A)) { x -= 1f; }
        if (Input.IsKeyHeld(KeyCode.D)) { x += 1f; }
        float mag = MathF.Sqrt(x * x + z * z);
        if (mag > 1f) { x /= mag; z /= mag; }
        return MathF.Abs(x) > 0.01f || MathF.Abs(z) > 0.01f;
    }

    private void PlayFootstep(string[] bank, float volume)
    {
        if (bank == null || bank.Length == 0 || ac == null)
            return;

        ref int lastIdxRef = ref lastWalkIdx;
        if (bank == runClips) lastIdxRef = ref lastRunIdx;
        else if (bank == walkClips) lastIdxRef = ref lastWalkIdx;
        else lastIdxRef = ref lastCrouchIdx; // placeholder if you add separate crouch bank later

        int idx = s_Random.Next(bank.Length);
        if (avoidImmediateRepeat && bank.Length > 1 && idx == lastIdxRef)
            idx = (idx + 1) % bank.Length;

        lastIdxRef = idx;
        string clip = bank[idx];
        if (string.IsNullOrEmpty(clip))
            return;

        ac.SetVolume(volume);
        ac.Play(clip);
    }
}
