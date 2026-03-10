using Engine;
using System;
using System.Collections.Generic;

public class DisguiseSkill : Entity
{
    // ---------------- Disguise ----------------

    // HUD
    public int Charges => currCharges;
    public int MaxCharges => maxCharges;
    public float CooldownRemaining => MathF.Max(0f, cooldownTimer);
    public float CooldownDuration => cooldown;
    public bool IsOnCooldown => cooldownTimer > 0f;

    // Skill parameters
    public KeyCode disguiseKey = KeyCode.R;
    public bool disguiseDebugLog = true;
    public float cooldown = 2.0f;
    public float cooldownTimer = 0.0f;
    public int maxCharges = 3;
    public int currCharges = 3;
    // Audio + timing
    public string disguiseSfxPath = "assets/Audio/SFX/Smoke.wav";
    public float disguiseSfxVolume = 0.4f;
    public float disguiseDelay = 2f;

    public float duration = 7.0f;

    // Shader/tint effect tuning
    public Vector3 enterFlashColor = new Vector3(1f, 1f, 1f);
    public float enterFlashDuration = 0.08f;
    public Vector3 disguisedTintColor = new Vector3(0.8f, 0.8f, 0.8f);
    public float enterLerpDuration = 0.35f;
    public float exitLerpDuration = 0.3f;
    public float exitFlashDuration = 0.05f;

    private bool isDisguised = false;
    private bool disguisePending = false;
    private float timer = 0.0f;
    private float pendingTimer = 0f;

    private ulong disguisedSourceID = 0;
    private ulong pendingSourceID = 0;

    private TransformComponent tf;
    private AudioComponent ac;
    private DisguiseComponent disguiseComp;
    private MeshRendererComponent mr;
    private SkinnedMeshRendererComponent smr;
    private PlayerController pc;
    private bool cachedHiddenState = false;
    private bool cachedHiddenValid = false;

    // Smoke emitter (prefab)
    public string smokeEmitterPrefabName = "SmokeEmitter";
    public Vector3 smokeOffset = Vector3.Zero;
    private Entity smokeEntity;
    private TransformComponent smokeTf;
    private ParticleSystem smokePS;

    private TintState tintState = TintState.None;
    private float tintTimer = 0f;
    private float tintStateDuration = 0f;

    private bool useMeshDuringDisguise = false;

    private static readonly System.Random s_Random = new System.Random();

    private enum TintState { None, EnterFlash, EnterLerp, ExitLerp, ExitFlash }

    public DisguiseSkill() : base() { }
    public DisguiseSkill(ulong id) : base(id) { }

    public override void OnInit()
    {
        tf = Transform;
        ac = HasComponent<AudioComponent>() ? new AudioComponent(ID) : null;
        mr = HasComponent<MeshRendererComponent>() ? GetComponent<MeshRendererComponent>() : null;
        smr = HasComponent<SkinnedMeshRendererComponent>() ? GetComponent<SkinnedMeshRendererComponent>() : null;
        pc = GetScript<PlayerController>();
        if (currCharges <= 0)
            currCharges = maxCharges;
        else
            currCharges = Math.Min(currCharges, maxCharges);
        // Default to skinned visible, mesh hidden to avoid overlap at start
        SetRendererVisibility(false);
        //Debug.Log($"[Skill_Disguise] OnInit. Entity={ID}, HasTransform={(tf != null)}");
    }

    public override void OnUpdate(float dt)
    {
        if (tf == null)
            return;
        if (cooldownTimer > 0f)
            cooldownTimer = MathF.Max(0f, cooldownTimer - dt);

        if (isDisguised && Input.IsKeyHeld(KeyCode.LeftShift))
        {
            Revert();
            return;
        }

        if (smokeTf != null)
        {
            smokeTf.Position = tf.Position + smokeOffset;
        }

        if (ac == null && HasComponent<AudioComponent>()) ac = new AudioComponent(ID);
        if (mr == null && HasComponent<MeshRendererComponent>()) mr = GetComponent<MeshRendererComponent>();
        if (smr == null && HasComponent<SkinnedMeshRendererComponent>()) smr = GetComponent<SkinnedMeshRendererComponent>();

        // Handle delay between SFX and disguise application
        if (disguisePending)
        {
            pendingTimer -= dt;
            if (pendingTimer <= 0f)
                ApplyPendingDisguise();
        }

        // Tick disguise timer
        if (isDisguised)
        {
            timer -= dt;
            if (timer <= 0f)
                Revert();
        }

        UpdateTint(dt);
    }

    private void TryActivate()
    {
        if (cooldownTimer > 0f)
            return;
        var list = EnemyRegistry.Snapshot();
        if (list.Count == 0)
        {
            //Debug.Log("[Disguise] No enemies to mimic.");
            return;
        }

        Entity target = list[s_Random.Next(list.Count)];

        pendingSourceID = target.ID;
        disguisePending = true;
        pendingTimer = disguiseDelay;

        PlayDisguiseSfx();
        StartEnterTint();
        SpawnSmoke();
        //Debug.Log($"[Disguise] Pending activate in {disguiseDelay}s.");
    }

    private void ApplyPendingDisguise()
    {
        disguisePending = false;
        pendingTimer = 0f;

        if (pendingSourceID == 0)
            return;

        disguisedSourceID = pendingSourceID;
        pendingSourceID = 0;

        isDisguised = true;
        timer = duration;

        if (disguiseComp == null) disguiseComp = new DisguiseComponent(ID);
        disguiseComp.ApplyFrom(disguisedSourceID);
        if (pc == null)
            pc = GetScript<PlayerController>();
        if (pc != null)
        {
            cachedHiddenState = pc.isHidden;
            cachedHiddenValid = true;
            pc.isHidden = true; // hide from enemies while disguised
        }

        // Decide which renderer to show based on source
        var src = new Entity(disguisedSourceID);
        bool sourceHasMesh = src.HasComponent<MeshRendererComponent>();
        bool sourceHasSkinned = src.HasComponent<SkinnedMeshRendererComponent>();
        useMeshDuringDisguise = sourceHasMesh || !sourceHasSkinned; // prefer mesh if source has it
        SetRendererVisibility(useMeshDuringDisguise);

        DisguiseFlag.Set(this.ID, true);

        //Debug.Log($"[Disguise] Activated for {duration}s.");
    }

    private void Revert()
    {
        // Cancel pending activation if any
        if (disguisePending)
        {
            disguisePending = false;
            pendingTimer = 0f;
            pendingSourceID = 0;
            StopSmoke();
        }

        if (!isDisguised)
            return;

        isDisguised = false;
        timer = 0f;
        disguisedSourceID = 0;

        if (disguiseComp == null) disguiseComp = new DisguiseComponent(ID);
        disguiseComp.Revert();

        // Restore to skinned visible
        SetRendererVisibility(false);
        if (pc == null)
            pc = GetScript<PlayerController>();
        if (pc != null && cachedHiddenValid)
        {
            pc.isHidden = cachedHiddenState;
            cachedHiddenValid = false;
        }
        DisguiseFlag.Set(this.ID, false);
        cooldownTimer = MathF.Max(0f, cooldown);

        StartExitTint();
        StopSmoke();

        //Debug.Log("[Disguise] Reverted to original appearance.");
    }

    private void PlayDisguiseSfx()
    {
        if (string.IsNullOrEmpty(disguiseSfxPath))
            return;

        Audio.Play2D(disguiseSfxPath, disguiseSfxVolume);
    }

    private void SpawnSmoke()
    {
        if (string.IsNullOrEmpty(smokeEmitterPrefabName))
            return;

        if (smokeEntity != null && smokeEntity.IsValid())
            return;

        ulong spawnedID = InternalCalls.Prefab_Instantiate(smokeEmitterPrefabName, ID);
        if (spawnedID == 0)
            return;

        smokeEntity = new Entity(spawnedID);
        if (smokeEntity != null && smokeEntity.IsValid())
        {
            smokeTf = smokeEntity.Transform;
            smokePS = smokeEntity.GetComponent<ParticleSystem>();

            if (smokeTf != null)
            {
                smokeTf.Position = tf.Position + smokeOffset;
            }

            smokePS?.Play();
        }
    }

    private void StopSmoke()
    {
        if (smokePS != null)
        {
            smokePS.Stop();
            smokePS.Clear();
        }

        if (smokeEntity != null && smokeEntity.IsValid())
        {
            Scene.DestroyEntity(smokeEntity.ID);
        }

        smokeEntity = null;
        smokeTf = null;
        smokePS = null;
    }

    private void StartEnterTint()
    {
        if (!EnsureRenderer())
            return;

        tintState = TintState.EnterFlash;
        tintStateDuration = enterFlashDuration;
        tintTimer = enterFlashDuration;
        SetTint(enterFlashColor);
    }

    private void StartEnterLerp()
    {
        if (!EnsureRenderer())
            return;

        tintState = TintState.EnterLerp;
        tintStateDuration = MathF.Max(enterLerpDuration, 0.01f);
        tintTimer = tintStateDuration;
    }

    private void StartExitTint()
    {
        if (!EnsureRenderer())
            return;

        tintState = TintState.ExitLerp;
        tintStateDuration = MathF.Max(exitLerpDuration, 0.01f);
        tintTimer = tintStateDuration;
    }

    private void StartExitFlash()
    {
        if (!EnsureRenderer())
            return;

        tintState = TintState.ExitFlash;
        tintStateDuration = MathF.Max(exitFlashDuration, 0.01f);
        tintTimer = tintStateDuration;
        SetTint(enterFlashColor);
    }

    private void ClearTint()
    {
        smr?.ClearColor();
        mr?.ClearColor();
        tintState = TintState.None;
        tintTimer = 0f;
        tintStateDuration = 0f;
    }

    private void SetTint(Vector3 c)
    {
        if (smr != null && smr.SetColor(c)) return;
        mr?.SetColor(c);
    }

    private void UpdateTint(float dt)
    {
        if (tintState == TintState.None) return;
        if (!EnsureRenderer()) { tintState = TintState.None; return; }
        tintTimer -= dt;
        switch (tintState)
        {
            case TintState.EnterFlash:
                if (tintTimer <= 0f) StartEnterLerp();
                break;
            case TintState.EnterLerp:
                float tIn = Clamp01(1f - MathF.Max(0f, tintTimer) / tintStateDuration);
                SetTint(Lerp(enterFlashColor, disguisedTintColor, tIn));
                if (tintTimer <= 0f) { SetTint(disguisedTintColor); tintState = TintState.None; }
                break;
            case TintState.ExitLerp:
                float tOut = Clamp01(1f - MathF.Max(0f, tintTimer) / tintStateDuration);
                SetTint(Lerp(disguisedTintColor, Vector3.One, tOut));
                if (tintTimer <= 0f) StartExitFlash();
                break;
            case TintState.ExitFlash:
                if (tintTimer <= 0f) ClearTint();
                break;
        }
    }

    private bool EnsureRenderer()
    {
        if (mr != null || smr != null) return true;
        if (HasComponent<MeshRendererComponent>()) mr = GetComponent<MeshRendererComponent>();
        if (HasComponent<SkinnedMeshRendererComponent>()) smr = GetComponent<SkinnedMeshRendererComponent>();
        return mr != null || smr != null;
    }

    private void SetRendererVisibility(bool showMesh)
    {
        EnsureRenderer();
        mr?.SetVisible(showMesh);
        smr?.SetVisible(!showMesh);
    }

    private static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        t = Clamp01(t);
        return new Vector3(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t,
            a.z + (b.z - a.z) * t);
    }

    private static float Clamp01(float v)
    {
        if (v < 0f) return 0f;
        if (v > 1f) return 1f;
        return v;
    }

    public bool TryUseSkill()
    {
        if (cooldownTimer > 0f || isDisguised || disguisePending || currCharges <= 0)
            return false;
        TryActivate();
        currCharges = Math.Max(0, currCharges - 1);
        return true;
    }
}
