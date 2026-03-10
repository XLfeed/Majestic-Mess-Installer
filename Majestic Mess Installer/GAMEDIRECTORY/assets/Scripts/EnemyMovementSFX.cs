using Engine;
using System;

public class EnemyMovementSFX : Entity
{
    // Enemy footstep clips (string paths to assets)
    public string[] walkClips =
    {
        "assets/Audio/SFX/Footsteps_Enemy_01.wav",
        "assets/Audio/SFX/Footsteps_Enemy_02.wav",
        "assets/Audio/SFX/Footsteps_Enemy_03.wav",
        "assets/Audio/SFX/Footsteps_Enemy_04.wav",
        "assets/Audio/SFX/Footsteps_Enemy_05.wav",
        "assets/Audio/SFX/Footsteps_Enemy_06.wav",
        "assets/Audio/SFX/Footsteps_Enemy_07.wav",
        "assets/Audio/SFX/Footsteps_Enemy_08.wav"
    };

    // Stride (meters between steps)
    private float patrolStride = 3.75f;
    private float chaseStride = 4.0f;

    // Animation loop times (seconds)
    public float patrolLoop = 2.5f;
    public float chaseLoop = 2.5f;

    // Volumes
    public float patrolVolume = 0.8f;
    public float chaseVolume = 0.8f;

    // Step detection
    public float minSpeedToStep = 0.10f;
    public float tapStepMinDistance = 0.20f;

    public bool avoidImmediateRepeat = true;

    // set by your AI/gameplay
    public bool isPatrolling = true;
    public bool isChasing = false;

    private TransformComponent tf;
    private RigidBodyComponent rb;
    private AudioComponent ac;
    private EnemyMovement movement;
    private AIController ai;

    private Vector3 lastPos;
    private float distSinceLast = 0f;

    private enum MoveState { None, Patrol, Chase }
    private MoveState currentState = MoveState.None;

    private int lastWalkIndex = -1;
    private static readonly Random s_Random = new Random();

    public override void OnInit()
    {
        EnsureRuntimeDefaults();
        tf = Transform;
        rb = HasComponent<RigidBodyComponent>() ? new RigidBodyComponent(ID) : null;
        ac = HasComponent<AudioComponent>() ? new AudioComponent(ID) : null;
        movement = GetScript<EnemyMovement>();
        ai = GetScript<AIController>();

        if (tf != null)
            lastPos = tf.Position;

        UpdateStrides();
    }

    private void EnsureRuntimeDefaults()
    {
        // Serialized prefab values can become zero, guard tuning.
        if (patrolLoop <= 0f) patrolLoop = 2.5f;
        if (chaseLoop <= 0f) chaseLoop = 2.5f;
        if (patrolVolume <= 0f) patrolVolume = 0.8f;
        if (chaseVolume <= 0f) chaseVolume = 0.8f;
        if (minSpeedToStep <= 0f) minSpeedToStep = 0.10f;
        if (tapStepMinDistance <= 0f) tapStepMinDistance = 0.20f;
    }

    public override void OnUpdate(float dt)
    {
        if (tf == null || walkClips == null || walkClips.Length == 0)
            return;

        // Reacquire components if hot-added
        if (rb == null && HasComponent<RigidBodyComponent>()) rb = new RigidBodyComponent(ID);
        if (ac == null && HasComponent<AudioComponent>()) ac = new AudioComponent(ID);
        if (movement == null) movement = GetScript<EnemyMovement>();
        if (ai == null) ai = GetScript<AIController>();
        if (ac == null) return;

        Vector3 pos = tf.Position;
        Vector3 delta = pos - lastPos; delta.y = 0f;

        float distance = delta.Mag;
        float speed = distance / MathF.Max(dt, 0.0001f);
        bool grounded = rb == null || MathF.Abs(rb.Velocity.y) < 0.5f;
        bool movingEnough = speed > minSpeedToStep;

        // Determine state (toggle via isChasing / isPatrolling flags)
        MoveState newState = MoveState.None;
        bool chasingNow = ai != null ? ai.isChasing : isChasing;
        bool patrollingNow = ai != null ? ai.isPatrolling : isPatrolling;
        if (chasingNow) newState = MoveState.Chase;
        else if (patrollingNow) newState = MoveState.Patrol;

        if (newState != currentState)
        {
            currentState = newState;
            distSinceLast = 0f;
            UpdateStrides();
        }

        float stride = currentState == MoveState.Chase ? chaseStride : patrolStride;
        float vol = currentState == MoveState.Chase ? chaseVolume : patrolVolume;

        // cadence
        if (grounded && movingEnough)
        {
            distSinceLast += distance;
            if (distSinceLast >= stride)
            {
                PlayRandomFootstep(vol);
                distSinceLast = 0f;
            }
        }
        else
        {
            // play a single tap if stopped mid-stride
            if (grounded && distSinceLast >= tapStepMinDistance && !movingEnough)
            {
                PlayRandomFootstep(vol);
            }
            distSinceLast = 0f;
        }

        lastPos = pos;
    }

    private void UpdateStrides()
    {
        // stride = speed * (loop time / 2)
        float baseSpeed = 3.5f;
        if (ai != null) baseSpeed = ai.moveSpeed;
        else if (movement != null) baseSpeed = movement.moveSpeed;
        patrolStride = baseSpeed * (patrolLoop / 2f);
        chaseStride = baseSpeed * 1.25f * (chaseLoop / 2f);
    }

    private void PlayRandomFootstep(float volume)
    {
        if (walkClips == null || walkClips.Length == 0 || ac == null) return;

        int idx = s_Random.Next(walkClips.Length);
        if (avoidImmediateRepeat && walkClips.Length > 1 && idx == lastWalkIndex)
            idx = (idx + 1) % walkClips.Length;

        lastWalkIndex = idx;
        string clip = walkClips[idx];
        if (string.IsNullOrEmpty(clip))
            return;

        ac.SetVolume(volume);
        ac.Play(clip);
    }
}
