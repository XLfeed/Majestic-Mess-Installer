using Engine;
using System;
using System.Text;
using System.Collections.Generic;

public class PlayerInteraction : Entity
{
    // ===== Interactions =====
    public KeyCode interactKey = KeyCode.F;
    public bool interactionDebugLog = true;
    public bool interactionIgnoreY = false;
    public string level2SceneName = "";

    private const float DEFAULT_INTERACT_RADIUS = 3.0f;

    // Global treasure counts (shared across all players/interactors)
    private static int s_TreasureMain = 0;
    private static int s_TreasureMisc = 0;
    private string TreasureOneName = "TreasureOne";
    private string TreasureTwoName = "TreasureTwo";
    private string TreasureThreeName = "TreasureThree";

    public string[] miscPickup =
    {
        "assets/Audio/SFX/MiscPickup_1.wav",
        "assets/Audio/SFX/MiscPickup_2.wav"
    };
    private int[] clipInstances = new int[] { -1, -1 };
    private float volume = 0.5f;
    private Random random = new Random();
    private ulong _activeBeaconId = 0;

    // Pickup glow VFX
    public string glowEmitterPrefabName = "GlowEmitter";
    public Vector3 glowOffset = Vector3.Zero;
    public float glowLifetime = 0.8f;

    private struct GlowInstance
    {
        public Entity entity;
        public float timer;
        public ParticleSystem ps;
    }
    private readonly List<GlowInstance> activeGlows = new List<GlowInstance>();

    // Cache
    private TransformComponent tf;
    private AudioComponent ac;
    private ScrollOfCinderSkill cinderSkill;
    public PlayerInteraction() : base() { }
    public PlayerInteraction(ulong id) : base(id) { }

    public override void OnInit()
    {
        tf = Transform;
        if (tf == null)
        {
          //  Debug.Log("[PlayerInteraction] Missing TransformComponent!");
        }
        else
        {
           // Debug.Log("[PlayerInteraction] Init (loot + extract).");
        }

        ac = GetComponent<AudioComponent>();
        cinderSkill = GetScript<ScrollOfCinderSkill>();
        if (ac != null)
        {
            // Create one audio instance for each clip (one-time setup)
            for (int i = 0; i < miscPickup.Length; i++)
            {
                if (clipInstances[i] < 0)
                    clipInstances[i] = ac.AddInstance(miscPickup[i]);

                if (clipInstances[i] >= 0)
                {
                    // Set volume for this instance
                    ac.SetInstanceVolume(clipInstances[i], volume);
                    ac.SetInstanceLoop(clipInstances[i], false);
                }
              
            }
        }

    }

    public override void OnUpdate(float dt)
    {
        if (tf == null)
            return;

        UpdateGlowTimers(dt);
        HandleInteractions(dt);
    }

    // ---------------- Interactions ----------------
    private void HandleInteractions(float dt)
    {
        InteractableBeacon nearest = null;
        float nearestDistSq = float.MaxValue;

        Vector3 myPos = tf.Position;
        List<InteractableBeacon> beacons = InteractableRegistry.Snapshot();

        foreach (var b in beacons)
        {
            if (b == null || !b.IsValid())
                continue;

            // === RADIUS ===
            // Start with the beacon's Radius.
            float r = b.Radius;

            // If designer left Radius at 0 (or negative), use a fixed default.
            if (r <= 0.0f)
                r = DEFAULT_INTERACT_RADIUS;

            // Simple squared radius.
            float r2 = r * r;

            // === DISTANCE ===
            Vector3 bp = b.Transform.Position;
            float dx = bp.x - myPos.x;
            float dy = interactionIgnoreY ? 0f : (bp.y - myPos.y);
            float dz = bp.z - myPos.z;

            float d2 = dx * dx + dy * dy + dz * dz;

            // Outside radius → skip
            if (d2 > r2)
                continue;

            // Keep the nearest beacon only
            if (d2 < nearestDistSq)
            {
                nearest = b;
                nearestDistSq = d2;
            }
        }

        // No beacons in range
        if (nearest == null)
        {
            _activeBeaconId = 0;
            return;
        }

        // Log prompt ONCE when we enter a new beacon
        if (interactionDebugLog && nearest.ID != _activeBeaconId)
        {
            switch (nearest.Mode)
            {
                case InteractableMode.TreasureMain:
                    Debug.Log("Press [F] to PICK UP Main Treasure");
                    break;
                case InteractableMode.TreasureMisc:
                    Debug.Log("Press [F] to PICK UP Misc Treasure");
                    break;
                case InteractableMode.Extract:
                    Debug.Log("Press [F] to EXTRACT");
                    break;
                case InteractableMode.Level1:
                    Debug.Log("Press [F] to LOAD M4_Level");
                    break;
                case InteractableMode.Level2:
                    Debug.Log("Press [F] to LOAD custom level");
                    break;
            }

            _activeBeaconId = nearest.ID;
        }

        // Wait for interact key
        if (!Input.IsKeyPressed(interactKey))
            return;

        // === ACTUAL INTERACTION ===
        switch (nearest.Mode)
        {
            case InteractableMode.TreasureMain:
            {
                s_TreasureMain++;
                int v = nearest.Value;
                if (v <= 0) v = 1;
                PickUpItemManager.AddTreasureValue(v, true);
                if (nearest.EntityName == TreasureOneName) {PickUpItemManager.Pickedup_Treasure_1();}
                if (nearest.EntityName == TreasureTwoName)
                {
                    PickUpItemManager.Pickedup_Treasure_2();
                    if (cinderSkill == null) cinderSkill = GetScript<ScrollOfCinderSkill>();
                    if (cinderSkill != null)
                    {
                        cinderSkill.maxCharges = 99;
                        cinderSkill.currCharges = 99;
                    }
                }
                if (nearest.EntityName == TreasureThreeName) {PickUpItemManager.Pickedup_Treasure_3();}
                SpawnPickupGlow(nearest);
                SafeHide(nearest);
                Debug.Log($"[Interact] Main Treasure +{v}. Main={s_TreasureMain}, Misc={s_TreasureMisc}, Money={PickUpItemManager.totalMonies}");

                // Trigger in-level cutscene if one is set up in this scene
                //Entity.FindScript<LevelCutsceneController>()?.Trigger(nearest.EntityName);

                break;
            }

            case InteractableMode.TreasureMisc:
                {
                    s_TreasureMisc++;
                    int v = nearest.Value;
                    if (v <= 0) v = 1;
                    PickUpItemManager.AddTreasureValue(v, false);
                    //Play pickup sound
                    PlaySound_PickUp_Misc();
                    SpawnPickupGlow(nearest);
                    SafeHide(nearest);
                    Debug.Log($"[Interact] Misc Treasure +{v}. Main={s_TreasureMain}, Misc={s_TreasureMisc}, Money={PickUpItemManager.totalMonies}");

                    break;
                }

            case InteractableMode.Extract:
                {
                    Debug.Log($"[Interact] At Extract. Main={s_TreasureMain}, Misc={s_TreasureMisc}");
                    if (s_TreasureMain > 0)
                    {
                        Debug.Log($"[Interact] Extracted! (Main={s_TreasureMain}, Misc={s_TreasureMisc})");
                        s_TreasureMain = 0;
                        // Optional: s_TreasureMisc = 0;
                        InternalCalls.Scene_LoadScene("WinScene");
                    }
                    else
                    {
                        Debug.Log("[Interact] Need MAIN treasure to extract.");
                    }
                    break;
                }
            case InteractableMode.Level1:
                {
                    InternalCalls.Scene_LoadScene("M4_Level");
                    break;
                }
            case InteractableMode.Level2:
                {
                    InternalCalls.Scene_LoadScene("M4_Level_2");
                    break;
                }
        }
    }

    private void SafeHide(Entity e)
    {
        var t = e.Transform;
        t.Position = new Vector3(t.Position.x, -10000f, t.Position.z);
    //    t.Scale = new Vector3(0f, 0f, 0f);
    }

    private void SpawnPickupGlow(Entity target)
    {
        if (string.IsNullOrEmpty(glowEmitterPrefabName) || target == null || !target.IsValid())
            return;

        ulong spawnedID = InternalCalls.Prefab_Instantiate(glowEmitterPrefabName, 0);
        if (spawnedID == 0)
            return;

        Entity glowEntity = new Entity(spawnedID);
        if (glowEntity == null || !glowEntity.IsValid())
            return;

        var glowTf = glowEntity.Transform;
        if (glowTf != null)
        {
            glowTf.Position = target.Transform.Position + glowOffset;
        }

        ParticleSystem ps = glowEntity.GetComponent<ParticleSystem>();
        ps?.Play();

        activeGlows.Add(new GlowInstance
        {
            entity = glowEntity,
            timer = glowLifetime,
            ps = ps
        });
    }

    private void UpdateGlowTimers(float dt)
    {
        for (int i = activeGlows.Count - 1; i >= 0; --i)
        {
            var g = activeGlows[i];
            g.timer -= dt;
            if (g.timer <= 0f || g.entity == null || !g.entity.IsValid())
            {
                if (g.ps != null)
                {
                    g.ps.Stop();
                    g.ps.Clear();
                }
                if (g.entity != null && g.entity.IsValid())
                {
                    Scene.DestroyEntity(g.entity.ID);
                }
                activeGlows.RemoveAt(i);
                continue;
            }
            activeGlows[i] = g;
        }
    }

    public void PlaySound_PickUp_Misc()
    {
       if (ac == null)
            return;

        // Pick random clip (avoid repeating same one)
        int randomIndex = random.Next(miscPickup.Length);
    
        // Play the specific instance for this clip
        // This won't interfere with footsteps (which use instance 0)
        int instanceIndex = clipInstances[randomIndex];
        if (instanceIndex >= 0)
        {
            ac.PlayInstance(instanceIndex);
        }
    }
}
