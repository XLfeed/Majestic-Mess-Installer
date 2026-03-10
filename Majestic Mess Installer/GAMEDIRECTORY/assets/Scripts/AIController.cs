using System;
using System.Collections.Generic;
using Engine;

public abstract class AIController : Entity
{
    // Player stuff
    public Entity playerObj;
    private FootstepVisualizer fv;
    private PlayerController playerController;
    private string playerName;

    // State
    protected StateMachine stateMachine;
    public string currentStateName;

    // Enemy fov
    private EnemyFOV fov;

    // waypoints
    //public Transform[] patrolWaypoints;
    //public List<Vector3> patrolWaypoints = new List<Vector3>();
    public List<Entity> patrolWaypoints = new List<Entity>();
    public int currentPatrolIndex;
    //[Header("StopTime >= 6 will play idle anim")]
    public float[] patrolStopTime;
    public float currentPatrolStopTime = 0.0f;
    public Entity WP1;
    public Entity WP2;
    public Entity WP3;
    public Entity WP4;

    // navmesh path (how to get to the current goal)
    public List<Vector3> navPath = new List<Vector3>();
    public int currentPathIndex = 0;

    // patrol
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 5.0f;


    // alert
    public float chaseSpeed = 7.0f;
    public float maxChaseDist = 15.0f;

    // wary
    public float waryGauge = 0.0f;
    public float gaugeIncreaseRate = 10.0f;
    public float gaugeDecreaseRate = 10.0f;
    public float maxWaryGuage = 100.0f;
    public float unshealthGuage = 50.0f;

    // target positions
    public Vector3 targetPosition;
    public Vector3 lastTargetPosition;

    // navigation
    //public NavMeshAgent agent;

    // player detection
    public bool isDying = false;
    public bool isPlayerDetected = false;
    public bool isAllyDying = false;


    // animations
    public bool isSheathe;
    public bool isIdle;
    public bool isPatrolling;
    public bool isChasing;
    public bool isStartled;
    public bool isAttacking;
    public bool isEmptyGauge;
    public bool isPlayingIdle;
    public bool isWithinIdleThreshold;
    //public Animator anim;

    // Per-knight idle config
    // idleType: 0=None, 1=Idle, 2=Stretching, 3=Browsing, 4=Talking
    public bool idleAtWaypoint = true;
    public int idleType = 1;
    public float idleVariantHoldSeconds = 5.0f;
    public float idleBaseHoldSeconds = 0.5f;


    // Legacy fields kept for backward compatibility with existing scene data.
    // If any is set, OnInit maps it into idleType.
    public bool isStretching;
    public bool isBrowsing;
    public bool isTalking;
    private const string ANIM_BROWSING = "Armature|Browsing";
    private const string ANIM_IDLE = "Armature|Idle";
    private const string ANIM_MUST_BE_WIND = "Armature|MustBeTheWind";
    private const string ANIM_PATROL = "Armature|Patrol";
    private const string ANIM_SHEATHE = "Armature|Sheathe";
    private const string ANIM_STARTLED = "Armature|Startled";
    private const string ANIM_STRETCHING = "Armature|Stretching";
    private const string ANIM_TALKING = "Armature|Talking";
    private const string ANIM_ATTACK = "Armature|Unsheathe_Attack";
    private const string ANIM_UNSHEATHE_IDLE = "Armature|Unsheathe_Idle";
    private const string ANIM_UNSHEATHE_RUN = "Armature|Unsheathe_Run";
    //private const string ANIM_DEATH = "";

    private bool _pendingAnimInit = true;
    private bool _wasInIdlePhase = false;
    private bool _idleCycleOnVariant = false;
    private float _idleCycleTimer = 0.0f;
    public float sheatheTimer = 0.0f;
    private bool HasSkinnedAnimator()
    {
        return HasComponent<AnimatorComponent>() && HasComponent<SkinnedMeshRendererComponent>();
    }

    private string GetConfiguredIdleClip()
    {
        switch (idleType)
        {
            case 0: return null; // None
            case 2: return ANIM_BROWSING;
            case 3: return ANIM_STRETCHING;
            case 4: return ANIM_TALKING;
            default:
                return ANIM_IDLE;
        }
    }

    private bool IsInIdlePhase()
    {
        return isIdle || isPlayingIdle;
    }

    private bool IsIdleVariantType()
    {
        return idleType == 2 || idleType == 3 || idleType == 4;
    }

    private float GetIdleTimer(bool variant)
    {
        float t = variant ? idleVariantHoldSeconds : idleBaseHoldSeconds;
        return t > 0.05f ? t : 0.05f;
    }

    private void PlayCurrentIdleCycleClip()
    {
        string clip = (_idleCycleOnVariant && IsIdleVariantType()) ? GetConfiguredIdleClip() : ANIM_IDLE;
        if (string.IsNullOrEmpty(clip))
            clip = ANIM_IDLE;

        if (Animator.CurrentAnimation != clip || Animator.NormalizedTime >= 0.98f)
        {
            Animator.Play(clip);
            Animator.Loop = false;
        }
    }

    private void UpdateIdleCycle(float dt)
    {
        if (!HasSkinnedAnimator())
            return;
        if (!IsInIdlePhase())
            return;
        if (!IsIdleVariantType())
            return;
        if (isPatrolling || isChasing || isAttacking || isStartled || isEmptyGauge || sheatheTimer > 0.0f)
            return;

        _idleCycleTimer -= dt;
        if (_idleCycleTimer > 0.0f)
            return;

        _idleCycleOnVariant = !_idleCycleOnVariant;
        _idleCycleTimer = GetIdleTimer(_idleCycleOnVariant);
        PlayCurrentIdleCycleClip();
    }

    public void UpdateAnimationFromBools()
    {
        if (!HasSkinnedAnimator())
            return;

        Animator.AutoPlay = false;

        string clip = null;
        bool loop = false;

        if (sheatheTimer > 0.0f)
        {
            clip = ANIM_SHEATHE;
            loop = false;
        }
        //if (isDying && !string.IsNullOrEmpty(ANIM_DEATH))
        //{
        //    clip = ANIM_DEATH;
        //    loop = false;
        //}
        //else 
        else if (isAttacking)
        {
            clip = ANIM_ATTACK;
            loop = false;
        }
        else if (isChasing)
        {
            clip = ANIM_UNSHEATHE_RUN;
            loop = true;
        }
        else if (isStartled)
        {
            clip = ANIM_STARTLED;
            loop = false;
        }
        else if (isEmptyGauge)
        {
            clip = ANIM_MUST_BE_WIND;
            loop = false;
        }
        else if (isPatrolling)
        {
            clip = ANIM_PATROL;
            loop = true;
        }
        else if (isIdle || isPlayingIdle)
        {
            // Start idle phases from base Idle; variant will be sequenced by UpdateIdleCycle().
            clip = ANIM_IDLE;
            loop = false;
        }
        else if (!isSheathe)
        {
            clip = ANIM_UNSHEATHE_IDLE;
            loop = false;
        }
        else
        {
            clip = ANIM_IDLE;
            loop = false;
        }

        if (string.IsNullOrEmpty(clip))
            return;

        if (clip == Animator.CurrentAnimation && Animator.IsPlaying)
        {
            if (Animator.Loop != loop)
                Animator.Loop = loop;
            return;
        }

        Animator.Play(clip);
        Animator.Loop = loop;

        if (!string.IsNullOrEmpty(Animator.CurrentAnimation) &&
            Animator.CurrentAnimation != clip)
        {
            Debug.Log($"AIController.cs [{Name}] Failed to switch animation to '{clip}'. Current='{Animator.CurrentAnimation}'");
        }
    }


    // protected virtual void Awake()
    // {
    //     //anim = GetComponentInChildren<Animator>();

    //     stateMachine = new StateMachine();

    //     if (playerObj == null)
    //     {
    //         Entity findPlayerObj = Entity.FindEntityByName("Player");
    //         if (findPlayerObj != null)
    //         {
    //             playerObj = findPlayerObj;
    //         }
    //         else
    //         {
    //             Debug.Log($"AIController.cs : playerObj is null");
    //         }
    //     }
    //     currentPatrolIndex = 0;
    //     /*if (patrolWaypoints.Count != patrolStopTime.Count)
    //     {
    //         patrolStopTime = new float[patrolWaypoints.Count];
    //     }*/
    // }

    // protected virtual void Start()
    // {
    //     //ChangeState(new StatePatrol(this, patrolWaypoints));
    //     ChangeState(new StatePatrol(this));
    // }

    public override void OnInit()
    {
        Debug.Log("AIController.cs: OnInit");
        if (HasSkinnedAnimator())
        {
            Transform.Scale = new Vector3(0.015f, 0.015f, 0.015f); // ensure scale is correct after animation change
        }
        EnemyRegistry.Register(this);
        stateMachine = new StateMachine();
        currentPatrolIndex = 0;

        //// Migrate legacy per-knight idle booleans into single idleType selector.
        //// Priority order is deterministic if multiple were accidentally set.
        //if (isBrowsing) idleType = 2;
        //else if (isStretching) idleType = 3;
        //else if (isTalking) idleType = 4;

        //// None means "do not stop for idle at waypoint".
        //if (idleType == 0)
        //    idleAtWaypoint = false;

        patrolWaypoints.Clear();
        if (WP1 != null)
        {
            patrolWaypoints.Add(WP1);
        }

        if (WP2 != null)
        {
            patrolWaypoints.Add(WP2);
        }

        if (WP3 != null)
        {
            patrolWaypoints.Add(WP3);
        }

        if (WP4 != null)
        {
            patrolWaypoints.Add(WP4);
        }

        //Debug.Log($"AIController.cs [{Name}]: Total waypoints added: {patrolWaypoints.Count}");

        // Match patrolStopTime array size to actual waypoints count.
        // Preserve scene-configured values so idle variant timing still works.
        if (patrolWaypoints.Count > 0)
        {
            float[] existing = patrolStopTime;
            patrolStopTime = new float[patrolWaypoints.Count];
            int copyCount = existing == null ? 0 : Math.Min(existing.Length, patrolWaypoints.Count);
            for (int i = 0; i < copyCount; i++)
            {
                patrolStopTime[i] = existing[i];
            }

            for (int i = copyCount; i < patrolWaypoints.Count; i++)
            {
                patrolStopTime[i] = 2.0f;
            }
        }

        if (playerObj == null)
        {
            Entity findPlayerObj = Entity.FindEntityByName("Player");
            if (findPlayerObj != null)
            {
                playerObj = findPlayerObj;
                //Debug.Log($"AIController.cs [{Name}]: playerObj is found: " + playerObj);
                fv = playerObj.GetScript<FootstepVisualizer>();
                if (fv == null)
                    Debug.Log($"AIController.cs [{Name}]: FootstepVisualizer not found.");
                playerName = playerObj.Name;
                playerController = playerObj.GetScript<PlayerController>();
            }
            else
                Debug.Log($"AIController.cs [{Name}]: playerObj is not found.");
        }
        else
        {
            fv = playerObj.GetScript<FootstepVisualizer>();
            if (fv == null)
                Debug.Log($"AIController.cs [{Name}]: FootstepVisualizer not found.");
            playerName = playerObj.Name;
            playerController = playerObj.GetScript<PlayerController>();
        }


        fov = GetScript<EnemyFOV>();
        if (fov == null)
            Debug.Log($"AIController.cs [{Name}]: EnemyFOV script not found.");

        if (HasSkinnedAnimator())
        {
            Animator.AutoPlay = false;
            Animator.Play(ANIM_IDLE);
            Animator.Loop = false;
        }

        isIdle = true;
        isSheathe = true;
        UpdateAnimationFromBools();
        ChangeState(new StatePatrol(this));
    }

    public override void OnUpdate(float dt)
    {
        // Animator might not be ready on first frame; retry Play until a current animation exists. This is to ensure scale is correct after animation change.
        if (_pendingAnimInit && HasSkinnedAnimator()) 
        {
            if (string.IsNullOrEmpty(Animator.CurrentAnimation))
            {
                UpdateAnimationFromBools();
            }
            else
            {
                _pendingAnimInit = false;
            }
        }

        bool inIdlePhase = IsInIdlePhase();
        if (inIdlePhase && !_wasInIdlePhase)
        {
            // Entering idle phase: start from chosen variant (if configured), then alternate
            // to base idle after a buffer duration.
            // Example: chosen idle -> base idle -> chosen idle -> base idle ...
            _idleCycleOnVariant = IsIdleVariantType();
            _idleCycleTimer = GetIdleTimer(_idleCycleOnVariant);
            if (_idleCycleOnVariant)
                PlayCurrentIdleCycleClip();
            else
                UpdateAnimationFromBools();
        }
        else if (!inIdlePhase && _wasInIdlePhase)
        {
            _idleCycleOnVariant = false;
            _idleCycleTimer = 0.0f;
        }
        _wasInIdlePhase = inIdlePhase;

        bool wasSheathing = sheatheTimer > 0.0f;
        if (sheatheTimer > 0.0f)
        {
            sheatheTimer -= dt;
            if (sheatheTimer < 0.0f)
                sheatheTimer = 0.0f;
        }
        if (wasSheathing && sheatheTimer <= 0.0f)
        {
            // Sheathe is a one-shot clip. When it ends, refresh to the proper current-state clip.
            UpdateAnimationFromBools();
        }

        // MustBeTheWind is a one-shot transition after chase ends.
        // In this animator, non-loop clips may remain "playing" at end, so use normalized time.
        if (isEmptyGauge && HasSkinnedAnimator() &&
            Animator.CurrentAnimation == ANIM_MUST_BE_WIND &&
            Animator.NormalizedTime >= 0.98f)
        {
            isEmptyGauge = false;
            UpdateAnimationFromBools();
        }

        UpdateIdleCycle(dt);

        FOVCheck();
        stateMachine.OnUpdate(dt);
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
        currentStateName = newState.GetType().Name;

        // When enemy enters chase, add +10 to global alertness
        // if (newState is StateChase)
        // {
        //     if (GlobalAlertSystem.Instance != null)
        //     {
        //         GlobalAlertSystem.Instance.IncreaseAlertness(10);
        //     }
        // }
    }

    //public abstract void HandleAlert();

    public void HandleAlert()
    {
        // check entity name or smth
        // if (Entity.Name == "Knight")
        stateMachine.ChangeState(new StateChase(this));
    }

    public override void OnExit()
    {
        EnemyRegistry.Unregister(this);
    }

    public void FOVCheck()
    {
        bool found = false;
        bool dyingAlly = false;

        if (fov != null && fov.VisibleEntities != null)
        {
            foreach (Entity e in fov.VisibleEntities)
            {
                if (e == null || !e.IsValid())
                    continue;

                if (!e.HasComponent<TagComponent>())
                    continue;

                var tagComp = e.GetComponent<TagComponent>();
                if (tagComp == null)
                    continue;

                string tag = tagComp.Tag;

                if (tag == playerName && playerController != null && !playerController.isHidden)
                {
                    found = true;
                    break;
                }
                if (tag == "Knight")
                {
                    AIController ally = e.GetComponent<AIController>();
                    if (ally != null)
                    {
                        if (ally.isDying)
                        {
                            dyingAlly = true;
                            targetPosition = ally.Transform.Position;
                        }
                    }
                }
            }
        }

        isPlayerDetected = found;
        if (!isPlayerDetected)
            isAllyDying = dyingAlly;
    }
    public bool CheckPlayerFootstepAudioDist()
    {
        if (fv == null || playerObj == null)
        {
            return false;
        }

        float rangeSqrt = fv._targetRadius * fv._targetRadius;
        return (playerObj.Transform.Position - Transform.Position).SqrMag <= rangeSqrt;
    }

    public void CalculateNavPath(Vector3 start, Vector3 end)
    {
        //Debug.Log($"AIController.cs [{Name}]: Requesting path: start({start.x}, {start.y}, {start.z}) goal({end.x}, {end.y}, {end.z})");
        ResetPath();
        // Query navmesh for path
        Vector3[] pathArray = NavMesh.FindPath(start, end);

        if (pathArray == null || pathArray.Length == 0)
        {
            // Fallback: move directly to target
            navPath.Add(end);
            currentPathIndex = 0;
            return;
        }

        // Copy array into existing list (no new list allocated)
        for (int i = 0; i < pathArray.Length; i++)
        {
            if (!IsValidVector(pathArray[i]))
                continue;  // skip corrupted navmesh results

            navPath.Add(pathArray[i]);
        }

        currentPathIndex = 0;
    }

    private bool IsValidVector(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z));
    }

    public void ResetPath()
    {
        navPath.Clear();
        //navPath = new List<Vector3>();   // clear current path
        currentPathIndex = 0;            // reset index
    }
}
