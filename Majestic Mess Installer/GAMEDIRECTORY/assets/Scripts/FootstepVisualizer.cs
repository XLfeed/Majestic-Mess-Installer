using System;
using System.Text;
using System.Collections.Generic;
using Engine;

/// <summary>
/// How this visualizer figures out whether the source (player) is moving.
/// AutoRigidbody  – derive from a RigidBody velocity.
/// External       – only uses SetExternalState() (e.g. driven by PlayerController).
/// Hybrid         – prefer external state briefly, then fall back to AutoRigidbody.
/// </summary>
public enum FootstepDetectMode
{
    AutoRigidbody = 0
}

/// <summary>
/// Registry so other scripts (e.g. enemies) can query current footstep radii
/// without needing direct references to the player script.
/// Keyed by the emitter entity ID (typically the Player's ID).
/// </summary>
public static class FootstepRegistry
{
    private static readonly Dictionary<ulong, FootstepVisualizer> s_Emitters
        = new Dictionary<ulong, FootstepVisualizer>();

    public static void Register(FootstepVisualizer v)
    {
        if (v == null)
            return;

        s_Emitters[v.ID] = v;
    }

    public static void Unregister(FootstepVisualizer v)
    {
        if (v == null)
            return;

        if (s_Emitters.ContainsKey(v.ID))
            s_Emitters.Remove(v.ID);
    }

    /// <summary>
    /// Current detection radius for the given emitter entity (0 if none).
    /// </summary>
    public static float GetRadius(ulong emitterID)
    {
        if (s_Emitters.TryGetValue(emitterID, out var v))
            return v.CurrentDetectionRadius;
        return 0.0f;
    }

    /// <summary>
    /// Snapshot of all active emitters – for debug or AI that wants to scan all noise sources.
    /// </summary>
    public static List<FootstepVisualizer> Snapshot()
    {
        return new List<FootstepVisualizer>(s_Emitters.Values);
    }
}

/// <summary>
/// Footstep visualizer for the custom engine.
///
/// Attach this script to the *Player* entity (the one that actually moves).
/// In the level editor, create a separate flattened sphere entity (e.g. "FootstepRadius")
/// and set RadiusEntityName to that entity's name.
/// This script will:
///   - Compute a "noise radius" based on movement / crouch / disguise.
///   - Scale & position the radius entity to visualize that bubble.
///   - Expose CurrentDetectionRadius for AI / other scripts.
/// </summary>
public class FootstepVisualizer : Entity
{
    /// <summary>
    /// Name of the visual radius entity (e.g. a flattened sphere mesh).
    /// </summary>
    public string RadiusEntityName = "FootstepRadius";

    /// <summary>
    /// Vertical offset to place the radius visual above/below the player's position.
    /// </summary>
    public float RadiusYOffset = 0.05f;

    /// <summary>
    /// How we decide if the player is moving.
    /// </summary>
    public FootstepDetectMode DetectMode = FootstepDetectMode.AutoRigidbody;

    /// <summary>
    /// Horizontal speed (XZ) above which we consider the player to be "moving".
    /// </summary>
    public float MoveSpeedThreshold = 0.1f;

    /// <summary>
    /// Horizontal speed (XZ) above which we consider the player to be "sprinting".
    /// </summary>
    public float SprintSpeedThreshold = 6.0f;

    /// <summary>
    /// Noise radius when walking.
    /// </summary>
    public float WalkRadius = 3.0f;

    /// <summary>
    /// Noise radius when sprinting.
    /// </summary>
    public float SprintRadius = 6.0f;

    /// <summary>
    /// Noise radius when crouching (set to 0 if you want crouch to be silent).
    /// </summary>
    public float CrouchRadius = 0.5f;

    /// <summary>
    /// Base loudness multiplier (tuning knob, default 1).
    /// </summary>
    public float LoudnessMultiplier = 1.0f;

    /// <summary>
    /// Extra loudness boost when Treasure_1 has been picked up.
    /// Example: 2.0 = twice as loud. Change this to 3.0f, etc, if you want.
    /// </summary>
    public float TreasureLoudnessBoost = 2.0f;

    /// <summary>
    /// How quickly the radius interpolates towards its target (units per second).
    /// Higher = snappier, lower = smoother.
    /// </summary>
    public float RadiusLerpSpeed = 10.0f;

    /// <summary>
    /// Height below which we treat the player as crouching (when not disguised).
    /// PlayerController uses ~1.0 for normal and ~0.5 for crouch, so 0.75 is a safe middle.
    /// </summary>
    public float CrouchHeightThreshold = 0.75f;

    /// <summary>
    /// If true, always visualize some radius for debugging even when idle.
    /// </summary>
    public bool DebugAlwaysOn = false;

    private Entity _radiusEntity;
    private TransformComponent _radiusTransform;
    private TransformComponent _playerTransform;

    private float _baseScaleForUnitRadius = 1.0f;
    private float _baseRadiusYScale = 1.0f;

    private float _currentRadius = 0.0f;
    public float _targetRadius = 0.0f;

    /// <summary>
    /// Public read-only access for other systems.
    /// </summary>
    public float CurrentDetectionRadius => _currentRadius;

    public FootstepVisualizer() : base() { }
    public FootstepVisualizer(ulong id) : base(id) { }

    public override void OnInit()
    {
        _playerTransform = Transform;

        if (_playerTransform == null)
        {
            Debug.Log("[FootstepVisualizer] Player has no TransformComponent – cannot visualize radius.");
            return;
        }

        // Find the visual radius entity if specified.
        if (!string.IsNullOrEmpty(RadiusEntityName))
        {
            _radiusEntity = Entity.FindEntityByName(RadiusEntityName);
            if (_radiusEntity == null)
            {
                Debug.Log($"[FootstepVisualizer] Could not find radius entity '{RadiusEntityName}'. Visuals will be disabled.");
            }
            else
            {
                _radiusTransform = _radiusEntity.Transform;
            }
        }

        // Calibrate base scale so that current radius entity scale corresponds to WalkRadius.
        if (_radiusTransform != null)
        {
            Vector3 s = _radiusTransform.Scale;
            _baseRadiusYScale = s.y;

            if (WalkRadius > 0.0001f)
                _baseScaleForUnitRadius = s.x / WalkRadius;
            else if (SprintRadius > 0.0001f)
                _baseScaleForUnitRadius = s.x / SprintRadius;
            else
                _baseScaleForUnitRadius = s.x != 0.0f ? s.x : 1.0f;
        }
        else
        {
            _baseScaleForUnitRadius = 1.0f;
            _baseRadiusYScale = 1.0f;
        }

        _currentRadius = 0.0f;
        _targetRadius = 0.0f;

        FootstepRegistry.Register(this);
        Debug.Log("[FootstepVisualizer] Init complete.");
    }

    public override void OnExit()
    {
        FootstepRegistry.Unregister(this);
    }

    public override void OnUpdate(float dt)
    {
        if (_playerTransform == null)
            return;

        // ----- Determine movement state -----

        bool autoMoving = false;
        bool autoSprinting = false;
        bool autoCrouching = false;

        // Auto-detect movement from rigidbody velocity if available
        if (HasComponent<RigidBodyComponent>())
        {
            RigidBodyComponent rb = RigidBody;
            Vector3 v = rb.Velocity;

            // We only care about horizontal movement (XZ plane)
            float speedXZ = (float)Math.Sqrt(v.x * v.x + v.z * v.z);

            autoMoving = speedXZ > MoveSpeedThreshold;
            autoSprinting = speedXZ > SprintSpeedThreshold;

            float height = _playerTransform.Scale.y;
            bool disguisedFlag = DisguiseFlag.IsOn(this.ID);
            if (!disguisedFlag && height < CrouchHeightThreshold)
                autoCrouching = true;
        }

        bool isMoving = autoMoving;
        bool isSprinting = autoSprinting;
        bool isCrouching = autoCrouching;

        if (DebugAlwaysOn)
        {
            // Always show something; keep crouch as-is so you can still see crouch radius.
            if (!isMoving)
                isMoving = true;
        }

        // ----- Disguise check – disguised = completely silent -----

        bool disguisedNow = DisguiseFlag.IsOn(this.ID);

        // ----- Compute desired radius (before loudness multipliers) -----

        float desiredRadius = 0.0f;

        if (!disguisedNow)
        {
            if (isMoving)
            {
                if (isCrouching)
                    desiredRadius = CrouchRadius;
                else if (isSprinting)
                    desiredRadius = SprintRadius;
                else
                    desiredRadius = WalkRadius;
            }
        }

        // ----- Loudness / Treasure_1 boost -----

        // Base loudness multiplier (tuning knob)
        float baseLoudness = Math.Max(0.0f, LoudnessMultiplier);

        // Extra boost when Treasure_1 has been picked up
        float treasureBoost = 1.0f;
        if (PickUpItemManager.pickedup_Treasure_1)
        {
            // e.g. 2.0 => twice as loud, 3.0 => three times as loud
            treasureBoost = TreasureLoudnessBoost;
        }

        float totalMultiplier = baseLoudness * treasureBoost;
        if (totalMultiplier < 0.0f)
            totalMultiplier = 0.0f;

        desiredRadius *= totalMultiplier;

        // For debug-always-on, make sure we still see something
        if (DebugAlwaysOn && desiredRadius <= 0.0f)
            desiredRadius = WalkRadius * totalMultiplier;

        _targetRadius = desiredRadius;

        // ----- Smooth radius towards target -----

        _currentRadius = MoveTowards(_currentRadius, _targetRadius, RadiusLerpSpeed * dt);
        float r = Math.Max(0.0f, _currentRadius);

        // ----- Drive the visual radius entity (if any) -----

        if (_radiusTransform != null)
        {
            // Position: follow player
            Vector3 playerPos = _playerTransform.Position;
            Vector3 ringPos = new Vector3(playerPos.x, playerPos.y + RadiusYOffset, playerPos.z);
            _radiusTransform.Position = ringPos;

            // Scale on X/Z; keep Y at authoring value (squashed sphere)
            float targetScaleXZ = (r <= 0.0001f)
                ? 0.001f
                : _baseScaleForUnitRadius * r;

            Vector3 s = _radiusTransform.Scale;
            s.x = targetScaleXZ;
            s.z = targetScaleXZ;
            s.y = _baseRadiusYScale;
            _radiusTransform.Scale = s;
        }
    }

    // ===== HELPERS =====

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        float diff = target - current;
        if (Math.Abs(diff) <= maxDelta)
            return target;
        return current + Math.Sign(diff) * maxDelta;
    }
}