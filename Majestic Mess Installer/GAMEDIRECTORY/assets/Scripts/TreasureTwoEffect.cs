using Engine;
using System;

public class TreasureTwoEffect : Entity
{
    //public bool enableTreasure2Logic = true;

    //public ScrollOfCinderSkill cinderSkill;
    //public Health playerHealth;

    //public float burnDuration = 1.0f;
    //public float burnDPS = 15f;
    //public bool nonLethal = true;
    //public bool bypassIFrames = true;

    // Self-burn VFX (prefab)
    //public string selfBurnEmitterPrefabName = "FireEmitter";
    //public Vector3 vfxOffset = Vector3.Zero;
    //public Vector3 burnRotationEuler = Vector3.Zero;

    //private Entity selfBurnEmitter;
    //private TransformComponent selfBurnTf;
    //private ParticleSystem selfBurnPS;

    //private bool selfBurnActive = false;
    //private float selfBurnTimer = 0f;

    //public TreasureTwoEffect() : base() { }
    //public TreasureTwoEffect(ulong id) : base(id) { }

    public override void OnInit()
    {
        //if (playerHealth == null) playerHealth = GetScript<Health>();
        //if (cinderSkill == null) cinderSkill = GetScript<ScrollOfCinderSkill>();

        //ScrollOfCinderSkill.OnCinderCastSuccess += HandleCinderCast;
    }

    public override void OnExit()
    {
        //ScrollOfCinderSkill.OnCinderCastSuccess -= HandleCinderCast;
    }

    public override void OnUpdate(float dt)
    {
        //if (!selfBurnActive)
        //    return;

        //if (selfBurnEmitter == null || !selfBurnEmitter.IsValid())
        //    return;

        //if (selfBurnTf != null)
        //{
        //    selfBurnTf.Position = Transform.Position + vfxOffset;
        //    selfBurnTf.Rotation = new Vector3(
        //        DegreesToRadians(burnRotationEuler.x),
        //        DegreesToRadians(burnRotationEuler.y),
        //        DegreesToRadians(burnRotationEuler.z)
        //    );
        //}

        //if (selfBurnTimer > 0f)
        //{
        //    float applyDt = dt > 0f ? dt : Time.GetDeltaTime();
        //    selfBurnTimer -= applyDt;

        //    if (playerHealth != null && !playerHealth.IsDead && burnDPS > 0f)
        //    {
        //        float dmg = burnDPS * applyDt;
        //        if (nonLethal)
        //        {
        //            float hp = playerHealth.HP;
        //            if (hp > 1f)
        //            {
        //                float allowed = MathF.Max(0f, hp - 1f);
        //                float apply = MathF.Min(dmg, allowed);
        //                if (apply > 0f)
        //                    playerHealth.TakeDamage(apply, bypassIFrames, 0f);
        //            }
        //        }
        //        else
        //        {
        //            playerHealth.TakeDamage(dmg, bypassIFrames, 0f);
        //        }
        //    }

        //    if (selfBurnTimer <= 0f)
        //        EndSelfBurn();
        //}
    }

    //private void HandleCinderCast(ScrollOfCinderSkill casterSkill)
    //{
    //    if (!enableTreasure2Logic)
    //        return;
    //    if (!PickUpItemManager.pickedup_Treasure_2)
    //        return;
    //    if (casterSkill == null || casterSkill != cinderSkill)
    //        return;

    //    StartSelfBurn();
    //}

    //private void StartSelfBurn()
    //{
    //    if (burnDuration <= 0f)
    //        return;

    //    selfBurnActive = true;
    //    selfBurnTimer = burnDuration;
    //    SpawnSelfBurnEmitter();
    //    PlaySelfBurnVfx();
    //}

    //private void EndSelfBurn()
    //{
    //    selfBurnActive = false;
    //    selfBurnTimer = 0f;
    //    StopSelfBurnVfx();
    //}

    //private void SpawnSelfBurnEmitter()
    //{
    //    if (string.IsNullOrEmpty(selfBurnEmitterPrefabName))
    //        return;

    //    if (selfBurnEmitter != null && selfBurnEmitter.IsValid())
    //        return;

    //    ulong spawnedID = InternalCalls.Prefab_Instantiate(selfBurnEmitterPrefabName, ID);
    //    if (spawnedID == 0)
    //        return;

    //    selfBurnEmitter = new Entity(spawnedID);
    //    if (selfBurnEmitter != null && selfBurnEmitter.IsValid())
    //    {
    //        selfBurnTf = selfBurnEmitter.Transform;
    //        selfBurnPS = selfBurnEmitter.GetComponent<ParticleSystem>();
    //    }
    //}

    //private void PlaySelfBurnVfx()
    //{
    //    if (selfBurnEmitter == null || !selfBurnEmitter.IsValid() || selfBurnPS == null)
    //        return;
    //    selfBurnPS.Play();
    //}

    //private void StopSelfBurnVfx()
    //{
    //    if (selfBurnEmitter == null || !selfBurnEmitter.IsValid() || selfBurnPS == null)
    //        return;
    //    selfBurnPS.Stop();
    //    selfBurnPS.Clear();

    //    Scene.DestroyEntity(selfBurnEmitter.ID);
    //    selfBurnEmitter = null;
    //    selfBurnTf = null;
    //    selfBurnPS = null;
    //}

    //private float DegreesToRadians(float degrees)
    //{
    //    return degrees * (MathF.PI / 180.0f);
    //}
}
