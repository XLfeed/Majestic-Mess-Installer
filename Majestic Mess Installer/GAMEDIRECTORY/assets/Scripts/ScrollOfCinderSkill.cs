using Engine;
using System;
using System.Collections.Generic;

public class ScrollOfCinderSkill : Entity
{
    public static event Action<ScrollOfCinderSkill> OnCinderCastSuccess;

    // HUD
    public int Charges => currCharges;
    public int MaxCharges => maxCharges;
    public float CooldownRemaining => MathF.Max(0f, cooldownTimer);
    public float CooldownDuration => cooldown;
    public bool IsOnCooldown => cooldownTimer > 0f;

    // Skill parameters

    public KeyCode castKey = KeyCode.E;
    public float cinderRange = 7.0f;
    public float cinderAngleDegrees = 60.0f;
    public string enemyTag = "Enemy";
    public bool cinderDebugLog = true;
    public float cooldown = 2.0f;
    public int maxCharges = 2;
    public int currCharges = 2;

    // VFX (Fire) - prefab spawn
    public string burnEmitterPrefabName = "FireEmitter";
    public Vector3 burnOffset = Vector3.Zero;
    public Vector3 burnRotationEuler = Vector3.Zero; // degrees

    // Audio
    public string igniteClip = "assets/Audio/SFX/Ignite";
    public string burnLoopClip = "assets/Audio/SFX/Burn";
    public float igniteVolume = 0.1f;
    public float burnLoopVolume = 0.1f;
    public float burnLoopDuration = 1.5f; // seconds to keep loop alive per target

    // Burn/kill timing
    public float killDelay = 1.5f;

    // Flash/tint while waiting to die
    public Vector3 flashColor = new Vector3(1f, 0f, 0f); // red
    public float flashInterval = 0.18f;                  // seconds between on/off

    private TransformComponent tf;
    private AudioComponent ac;
    public float cooldownTimer = 0.0f;


    private class TargetKill
    {
        public AIController ai;
        public Entity target;
        public float timer;
        public int loopIdx;
        public float loopTimer;
        public float flashTimer;
        public bool flashOn;
        public Entity burnEntity;
        public TransformComponent burnTf;
        public ParticleSystem burnPS;
    }

    private readonly List<TargetKill> activeKills = new List<TargetKill>();
    private readonly List<int> burnLoopInstances = new List<int>();

    public ScrollOfCinderSkill() : base() { }
    public ScrollOfCinderSkill(ulong id) : base(id) { }

    public override void OnInit()
    {
        tf = Transform;
        ac = HasComponent<AudioComponent>() ? new AudioComponent(ID) : null;
        if (tf == null)
        {
            //Debug.Log("[ScrollOfCinderSkill] Missing Transform");
            return;
        }
        if (currCharges <= 0)
            currCharges = maxCharges;
        else
            currCharges = Math.Min(currCharges, maxCharges);
    }

    public override void OnUpdate(float dt)
    {
        if (tf == null)
            return;

        if (cooldownTimer > 0f)
            cooldownTimer -= dt;

        if (ac == null && HasComponent<AudioComponent>())
            ac = new AudioComponent(ID);

        // Tick per-target timers, audio, and flash
        for (int i = activeKills.Count - 1; i >= 0; --i)
        {
            var tk = activeKills[i];
            tk.timer -= dt;
            tk.loopTimer -= dt;
            tk.flashTimer -= dt;

            bool targetValid = tk.target != null && tk.target.IsValid();

            // Stop loop if expired
            if (tk.loopIdx >= 0 && tk.loopTimer <= 0f && ac != null)
            {
                ac.StopInstance(tk.loopIdx);
                ReleaseBurnLoopInstance(tk.loopIdx);
                tk.loopIdx = -1;
            }

            // Toggle flash on interval
            if (targetValid && tk.flashTimer <= 0f)
            {
                tk.flashOn = !tk.flashOn;
                tk.flashTimer = flashInterval;
                if (tk.flashOn)
                    SetFlash(tk);
                else
                    ClearFlash(tk);
            }

            UpdateBurnVfx(tk, targetValid);

            // Kill on timer or if target already invalid
            if (tk.timer <= 0f || !targetValid)
            {
                if (tk.loopIdx >= 0 && ac != null)
                {
                    ac.StopInstance(tk.loopIdx);
                    ReleaseBurnLoopInstance(tk.loopIdx);
                    tk.loopIdx = -1;
                }

                ClearFlash(tk);
                StopBurnVfx(tk);

                if (targetValid)
                    SafeDelete(tk);
                //SafeDelete(tk.target);

                activeKills.RemoveAt(i);
            }
        }

        //if (!Input.IsKeyPressed(castKey))
        //    return;

        //Cast();
    }

    public void Cast()
    {
        if (tf == null)
            return;

        if (cooldownTimer > 0f || currCharges <= 0)
            return;

        Vector3 myPos = tf.Position;
        float range2 = cinderRange * cinderRange;
        Vector3 forward = GetForward(tf.Rotation);
        //float halfAngleRad = MathF.PI * (cinderAngleDegrees * 0.5f) / 180.0f;
        //float cosHalfAngle = MathF.Cos(halfAngleRad);
        // Cone check removed allow 360 around the player.
        int killed = 0;

        var list = EnemyRegistry.Snapshot();
        foreach (var enemy in list)
        {
            if (enemy == null || !enemy.IsValid()) continue;

            bool isEnemy = false;
            if (enemy.HasComponent<TagComponent>())
            {
                string tag = enemy.GetComponent<TagComponent>().Tag;
                if (tag == enemyTag) isEnemy = true;
            }
            if (!isEnemy) continue;

            Vector3 ep = enemy.Transform.Position;
            float dx = ep.x - myPos.x, dy = ep.y - myPos.y, dz = ep.z - myPos.z;
            float dist2 = dx * dx + dy * dy + dz * dz;
            if (dist2 > range2)
                continue;

            Vector3 toEnemy = new Vector3(dx, dy, dz);
            float toEnemyMag = toEnemy.Mag;
            if (toEnemyMag <= 0.0001f)
                continue;

            float dot = Vector3.Dot(forward, toEnemy / toEnemyMag);
            //if (dot < cosHalfAngle)
            //    continue;
            // Cone check removed allow 360 around the player.

            if (dist2 <= range2)
            {
                bool alreadyPending = false;
                foreach (var k in activeKills)
                {
                    if (k.target != null && k.target.ID == enemy.ID)
                    {
                        alreadyPending = true;
                        break;
                    }
                }
                if (alreadyPending)
                    continue;

                StartKillCountdown(enemy);
                killed++;
            }
        }

        if (cinderDebugLog)
            //Debug.Log($"[Cinder] Cast. Queued {killed} enemies in cone");

        if (killed > 0)
        {
            cooldownTimer = MathF.Max(0f, cooldown);
            currCharges = Math.Max(0, currCharges - 1);
            PlayIgnite();
            OnCinderCastSuccess?.Invoke(this);
        }
    }

    private Vector3 GetForward(Vector3 rotation)
    {
        // Match engine TransformComponent::GetForward (YXZ order, forward = -Z)
        float sx = MathF.Sin(rotation.x);
        float cx = MathF.Cos(rotation.x);
        float sy = MathF.Sin(rotation.y);
        float cy = MathF.Cos(rotation.y);
        float sz = MathF.Sin(rotation.z);
        float cz = MathF.Cos(rotation.z);

        float m00 = cy * cz + sy * sx * sz;
        float m01 = cx * sz;
        float m02 = -sy * cz + cy * sx * sz;

        float m10 = -cy * sz + sy * sx * cz;
        float m11 = cx * cz;
        float m12 = sy * sz + cy * sx * cz;

        float m20 = sy * cx;
        float m21 = -sx;
        float m22 = cy * cx;

        // Forward is -Z in local space
        return new Vector3(
            -(m02),
            -(m12),
            -(m22)
        );
    }

    //private void SafeDelete(Entity e)
    //{
    //    var t = e.Transform;
    //    t.Position = new Vector3(t.Position.x, -10000f, t.Position.z);
    //    t.Scale = new Vector3(0f, 0f, 0f);
    //}
    //private void SafeDelete(Entity e)
    //{
    //    //if (e == null || !e.IsValid())
    //    //    return;

    //    //InternalCalls.Scene_DestroyEntity(e.ID);
    //    //// Prefer actual destroy if available
    //    //try { InternalCalls.Scene_DestroyEntity(e.ID); return; }
    //    //catch { /* fallback below */ }

    //    //var t = e.Transform;
    //    //t.Position = new Vector3(t.Position.x, -10000f, t.Position.z);
    //    //t.Scale = new Vector3(0f, 0f, 0f);
    //}
    private void SafeDelete(TargetKill tk)
    {

        // Use cached AI if we found one at cast time
        var enemyAI = tk.ai ?? tk.target.GetScript<AIController>();
        if (enemyAI != null)
        {
            enemyAI.ChangeState(new StateDead(enemyAI));
            return;
        }

        // Fallback destroy to avoid stranded entities
        try { Scene.DestroyEntity(tk.target.ID); }
        catch { /* ignore */ }
    }
    private void PlayIgnite()
    {
        if (ac == null || string.IsNullOrEmpty(igniteClip))
            return;

        ac.SetVolume(igniteVolume);
        ac.Play(igniteClip);
    }

    private void StartKillCountdown(Entity target)
    {
        var tk = new TargetKill
        {
            target = target,
            timer = MathF.Max(0.05f, killDelay),
            loopIdx = -1,
            loopTimer = MathF.Max(killDelay, burnLoopDuration),
            flashTimer = 0f,
            flashOn = false,
            ai = target.GetScript<AIController>() // cache for death handoff
        };

        SpawnBurnVfx(tk);

        if (ac != null && !string.IsNullOrEmpty(burnLoopClip))
        {
            tk.loopIdx = AcquireBurnLoopInstance();
            if (tk.loopIdx >= 0)
            {
                ac.SetInstanceVolume(tk.loopIdx, burnLoopVolume);
                ac.SetInstanceLoop(tk.loopIdx, false);
                ac.PlayInstance(tk.loopIdx);
            }
        }

        // start with flash on immediately
        tk.flashOn = true;
        SetFlash(tk);
        tk.flashTimer = flashInterval;

        activeKills.Add(tk);
    }

    private void SetFlash(TargetKill tk)
    {
        if (tk.target == null || !tk.target.IsValid())
            return;
        if (!tk.target.HasComponent<MeshRendererComponent>())
            return;

        var mr = tk.target.GetComponent<MeshRendererComponent>();
        mr?.SetColor(flashColor);
    }

    private void ClearFlash(TargetKill tk)
    {
        if (tk.target == null || !tk.target.IsValid())
            return;
        if (!tk.target.HasComponent<MeshRendererComponent>())
            return;

        var mr = tk.target.GetComponent<MeshRendererComponent>();
        mr?.ClearColor();
        tk.flashOn = false;
        tk.flashTimer = 0f;
    }

    private void SpawnBurnVfx(TargetKill tk)
    {
        if (string.IsNullOrEmpty(burnEmitterPrefabName) || tk.target == null || !tk.target.IsValid())
            return;

        ulong spawnedID = InternalCalls.Prefab_Instantiate(burnEmitterPrefabName, tk.target.ID);
        if (spawnedID == 0)
            return;

        tk.burnEntity = new Entity(spawnedID);
        if (tk.burnEntity != null && tk.burnEntity.IsValid())
        {
            tk.burnTf = tk.burnEntity.Transform;
            tk.burnPS = tk.burnEntity.GetComponent<ParticleSystem>();

            UpdateBurnVfx(tk, true);
            tk.burnPS?.Play();
        }
    }

    private void UpdateBurnVfx(TargetKill tk, bool targetValid)
    {
        if (tk.burnTf == null)
            return;

        if (!targetValid || tk.target == null)
            return;

        Vector3 pos = tk.target.Transform.Position + burnOffset;
        tk.burnTf.Position = pos;
        tk.burnTf.Rotation = new Vector3(
            DegreesToRadians(burnRotationEuler.x),
            DegreesToRadians(burnRotationEuler.y),
            DegreesToRadians(burnRotationEuler.z)
        );
    }

    private void StopBurnVfx(TargetKill tk)
    {
        if (tk.burnPS != null)
        {
            tk.burnPS.Stop();
            tk.burnPS.Clear();
        }

        if (tk.burnEntity != null && tk.burnEntity.IsValid())
        {
            Scene.DestroyEntity(tk.burnEntity.ID);
        }

        tk.burnEntity = null;
        tk.burnTf = null;
        tk.burnPS = null;
    }

    private float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180.0f);
    }

    public bool TryUseSkill()
    {
        if (cooldownTimer > 0f || currCharges <= 0)
            return false;
        Cast();
        return true;
    }

    private int AcquireBurnLoopInstance()
    {
        if (ac == null || string.IsNullOrEmpty(burnLoopClip))
            return -1;

        for (int i = 0; i < burnLoopInstances.Count; ++i)
        {
            int idx = burnLoopInstances[i];
            if (idx >= 0 && !ac.IsInstancePlaying(idx))
                return idx;
        }

        int created = ac.AddInstance(burnLoopClip);
        if (created >= 0)
            burnLoopInstances.Add(created);
        return created;
    }

    private void ReleaseBurnLoopInstance(int index)
    {
        if (index < 0)
            return;
        if (!burnLoopInstances.Contains(index))
            burnLoopInstances.Add(index);
    }

}
