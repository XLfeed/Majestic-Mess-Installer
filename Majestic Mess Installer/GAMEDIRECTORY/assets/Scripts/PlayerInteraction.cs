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

    // Per-treasure collected flags for portal requirement checks
    private static bool s_HasTreasure1 = false;
    private static bool s_HasTreasure2 = false;
    private static bool s_HasTreasure3 = false;
    private static bool s_HasTreasure4 = false;

    // Legacy name-based mapping (kept for backward compatibility)
    private string TreasureOneName = "TreasureOne";
    private string TreasureTwoName = "TreasureTwo";
    private string TreasureThreeName = "TreasureThree";
    private string TreasureFourName = "TreasureFour";

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

        ac = GetComponent<AudioComponent>();
        cinderSkill = GetScript<ScrollOfCinderSkill>();

        if (ac != null)
        {
            for (int i = 0; i < miscPickup.Length; i++)
            {
                if (clipInstances[i] < 0)
                    clipInstances[i] = ac.AddInstance(miscPickup[i]);

                if (clipInstances[i] >= 0)
                {
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

            float r = b.Radius;
            if (r <= 0.0f)
                r = DEFAULT_INTERACT_RADIUS;

            float r2 = r * r;

            Vector3 bp = b.Transform.Position;
            float dx = bp.x - myPos.x;
            float dy = interactionIgnoreY ? 0f : (bp.y - myPos.y);
            float dz = bp.z - myPos.z;

            float d2 = dx * dx + dy * dy + dz * dz;

            if (d2 > r2)
                continue;

            if (d2 < nearestDistSq)
            {
                nearest = b;
                nearestDistSq = d2;
            }
        }

        if (nearest == null)
        {
            _activeBeaconId = 0;
            return;
        }

        if (interactionDebugLog && nearest.ID != _activeBeaconId)
        {
            InteractableMode promptMode = nearest.Mode;

            switch (promptMode)
            {
                case InteractableMode.TreasureMain:
                    Debug.Log("Press [F] to PICK UP Main Treasure");
                    break;

                case InteractableMode.TreasureMisc:
                    Debug.Log("Press [F] to PICK UP Misc Treasure");
                    break;

                case InteractableMode.Extract:
                    if (nearest.IsPortal)
                        Debug.Log("Press [F] to EXTRACT / USE PORTAL");
                    else
                        Debug.Log("Press [F] to EXTRACT");
                    break;

                case InteractableMode.Level1:
                case InteractableMode.Level2:
                    if (nearest.IsPortal)
                    {
                        if (nearest.UseSceneChange)
                            Debug.Log("Press [F] to USE PORTAL (Scene Change)");
                        else
                            Debug.Log("Press [F] to USE PORTAL (Teleport)");
                    }
                    else
                    {
                        Debug.Log("Press [F] to INTERACT");
                    }
                    break;
            }

            _activeBeaconId = nearest.ID;
        }

        if (!Input.IsKeyPressed(interactKey))
            return;

        InteractableMode mode = nearest.Mode;

        switch (mode)
        {
            case InteractableMode.TreasureMain:
                {
                    s_TreasureMain++;
                    int v = nearest.Value;
                    if (v <= 0) v = 1;

                    PickUpItemManager.AddTreasureValue(v, true);

                    MarkTreasureCollected(nearest);

                    if (nearest.EntityName == TreasureOneName)
                    {
                        PickUpItemManager.Pickedup_Treasure_1();
                    }

                    if (nearest.EntityName == TreasureTwoName)
                    {
                        PickUpItemManager.Pickedup_Treasure_2();

                        //if (cinderSkill == null)
                        //    cinderSkill = GetScript<ScrollOfCinderSkill>();

                        //if (cinderSkill != null)
                        //{
                        //    cinderSkill.maxCharges = 99;
                        //    cinderSkill.currCharges = 99;
                        //}
                    }

                    if (nearest.EntityName == TreasureThreeName)
                    {
                        PickUpItemManager.Pickedup_Treasure_3();
                    }

                    if (nearest.EntityName == TreasureFourName)
                    {
                        PickUpItemManager.Pickedup_Treasure_4();
                    }

                    SpawnPickupGlow(nearest);
                    SafeHide(nearest);

                    Debug.Log($"[Interact] Main Treasure +{v}. Main={s_TreasureMain}, Misc={s_TreasureMisc}, Money={PickUpItemManager.totalMonies}");
                    
                    // Trigger in-level cutscene if one is set up in this scene
                    Entity.FindScript<LevelCutsceneController>()?.Trigger(nearest.EntityName);
                    
                    break;
                }

            case InteractableMode.TreasureMisc:
                {
                    s_TreasureMisc++;
                    int v = nearest.Value;
                    if (v <= 0) v = 1;

                    PickUpItemManager.AddTreasureValue(v, false);
                    PlaySound_PickUp_Misc();
                    SpawnPickupGlow(nearest);
                    SafeHide(nearest);

                    Debug.Log($"[Interact] Misc Treasure +{v}. Main={s_TreasureMain}, Misc={s_TreasureMisc}, Money={PickUpItemManager.totalMonies}");
                    break;
                }

            case InteractableMode.Extract:
                {
                    if (nearest.IsPortal)
                    {
                        if (CanUseTransition(nearest))
                        {
                            Debug.Log("[Interact] Extraction/progression allowed.");
                            s_TreasureMain = 0;
                            HandlePortalInteraction(nearest);
                        }
                        else
                        {
                            Debug.Log("[Interact] Portal requirements not met.");
                        }
                    }
                    else
                    {
                        Debug.Log($"[Interact] At Extract. Main={s_TreasureMain}, Misc={s_TreasureMisc}");

                        if (s_TreasureMain > 0)
                        {
                            Debug.Log($"[Interact] Extracted! (Main={s_TreasureMain}, Misc={s_TreasureMisc})");
                            s_TreasureMain = 0;
                            InternalCalls.Scene_LoadScene("WinScene");
                        }
                        else
                        {
                            Debug.Log("[Interact] Need MAIN treasure to extract.");
                        }
                    }
                    break;
                }

            case InteractableMode.Level1:
            case InteractableMode.Level2:
                {
                    if (nearest.IsPortal)
                    {
                        if (CanUseTransition(nearest))
                        {
                            HandlePortalInteraction(nearest);
                        }
                        else
                        {
                            Debug.Log("[Interact] Portal requirements not met.");
                        }
                    }
                    else
                    {
                        Debug.Log("[Interact] This beacon is not marked as a portal.");
                    }
                    break;
                }
        }
    }

    private bool CanUseTransition(InteractableBeacon beacon)
    {
        if (beacon == null)
            return false;

        if (!beacon.IsPortal)
            return false;

        if (beacon.HasSpecificTreasureRequirements())
        {
            if (beacon.RequireTreasure1 && !s_HasTreasure1) return false;
            if (beacon.RequireTreasure2 && !s_HasTreasure2) return false;
            if (beacon.RequireTreasure3 && !s_HasTreasure3) return false;
            if (beacon.RequireTreasure4 && !s_HasTreasure4) return false;
            return true;
        }

        if (beacon.Mode == InteractableMode.Extract)
        {
            return true;
        }

        return true;
    }

    private string ResolveSceneTarget(InteractableBeacon beacon)
    {
        if (beacon == null)
            return "";

        switch (beacon.SceneTarget)
        {
            case 1: return "Wizardroom_level_1";
            case 2: return "M5_Level1";
            case 3: return "Wizardroom_level_2";
            case 4: return "M5_Level2";
            case 5: return "Wizardroom_level_conclusion";
            case 6: return "GameEndCutscene";
            default: return "";
        }
    }

    private void HandlePortalInteraction(InteractableBeacon beacon)
    {
        if (beacon == null || !beacon.IsPortal)
            return;

        if (beacon.UseSceneChange)
        {
            beacon.OnPortalUsed();
            string sceneToLoad = ResolveSceneTarget(beacon);

            // Keep string field as optional fallback in case it starts working later
            if (string.IsNullOrEmpty(sceneToLoad) && !string.IsNullOrEmpty(beacon.TargetSceneName))
            {
                sceneToLoad = beacon.TargetSceneName;
            }

            Debug.Log($"[PortalDebug] BeaconName='{beacon.EntityName}', ID={beacon.ID}, SceneTarget={beacon.SceneTarget}, TargetSceneName='{beacon.TargetSceneName}', ResolvedScene='{sceneToLoad}'");

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.Log($"[Interact] Loading scene: {sceneToLoad}");
                InternalCalls.Scene_LoadScene(sceneToLoad);
            }
            else
            {
                Debug.Log($"[Interact] No valid scene target set. SceneTarget={beacon.SceneTarget}, TargetSceneName='{beacon.TargetSceneName}'");
            }
        }
        else
        {
            Vector3 target = beacon.TeleportTarget;

            if (beacon.PreservePlayerY)
                target.y = tf.Position.y;

            tf.Position = target;
            Debug.Log($"[Interact] Teleported player to {target}");
        }
    }

    private void MarkTreasureCollected(InteractableBeacon beacon)
    {
        if (beacon == null)
            return;

        if (beacon.TreasureSlot == 1) s_HasTreasure1 = true;
        else if (beacon.TreasureSlot == 2) s_HasTreasure2 = true;
        else if (beacon.TreasureSlot == 3) s_HasTreasure3 = true;
        else if (beacon.TreasureSlot == 4) s_HasTreasure4 = true;
        else
        {
            if (beacon.EntityName == TreasureOneName) s_HasTreasure1 = true;
            else if (beacon.EntityName == TreasureTwoName) s_HasTreasure2 = true;
            else if (beacon.EntityName == TreasureThreeName) s_HasTreasure3 = true;
            else if (beacon.EntityName == TreasureFourName) s_HasTreasure4 = true;
        }
    }

    private void SafeHide(Entity e)
    {
        var t = e.Transform;
        t.Position = new Vector3(t.Position.x, -10000f, t.Position.z);
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
        if (ac == null || miscPickup == null || miscPickup.Length == 0)
            return;

        int randomIndex = random.Next(miscPickup.Length);
        int instanceIndex = clipInstances[randomIndex];

        if (instanceIndex >= 0)
        {
            ac.PlayInstance(instanceIndex);
        }
    }
}