using Engine;
using System;

public class AmbienceSoundManager : Entity
{
    // Clip paths
    public string baseLoop = "assets/Audio/SFX/BGM_Level_Normal_01.wav";
    public string chaseLoop = "assets/Audio/SFX/BGM_Level_Chase_01.wav";

    // Volumes
    public float baseVolume = 0.10f;
    public float baseVolumeWhileChasing = 0.0f; // target base volume when chase is active
    public float chaseMaxVolume = 0.4f;         // target chase volume when active
    public float fadeDuration = 2.0f;           // seconds for 0 to 1 or 1 to 0

    public string audioEntityName = "";

    public float interval = 0.25f;
    //public bool enableDebugGlobalAudioHotkeys = true;
    //public KeyCode stopAllGlobalAudioKey = KeyCode.F9;
    //public KeyCode restartManagedBgmKey = KeyCode.F10;

    private static ulong sBaseAudioID = 0;
    private static ulong sChaseAudioID = 0;
    private static ulong sOwnerID = 0;

    private bool chaseActive = false;
    private float baseVolCurrent = 0f;
    private float chaseVolCurrent = 0f;
    private float intervalTimer = 0f;

    public override void OnInit()
    {
        // Enforce one global owner for BGM loops to prevent duplicate tracks across entities/scenes.
        if (sOwnerID != 0 && sOwnerID != ID)
            StopManagedLoops();
        sOwnerID = ID;

        StartManagedLoops();

        intervalTimer = interval;
    }

    public override void OnUpdate(float dt)
    {
        //if (enableDebugGlobalAudioHotkeys)
        //{
        //    if (Input.IsKeyPressed(stopAllGlobalAudioKey))
        //    {
        //        Audio.StopAll();
        //        sBaseAudioID = 0;
        //        sChaseAudioID = 0;
        //        sOwnerID = 0;
        //    }
        //    if (Input.IsKeyPressed(restartManagedBgmKey))
        //    {
        //        if (sOwnerID == 0 || sOwnerID == ID)
        //        {
        //            sOwnerID = ID;
        //            StartManagedLoops();
        //        }
        //    }
        //}

        if (sOwnerID != ID)
            return;

        intervalTimer -= dt;
        if (intervalTimer <= 0f)
        {
            intervalTimer = MathF.Max(0.05f, interval);
            chaseActive = IsAnyEnemyChasing();
        }

        float step = dt / MathF.Max(fadeDuration, 0.0001f);
        float targetBase = chaseActive ? baseVolumeWhileChasing : baseVolume;
        float targetChase = chaseActive ? chaseMaxVolume : 0f;

        // Keep loops alive
        if (!string.IsNullOrEmpty(baseLoop) && (sBaseAudioID == 0 || !Audio.IsPlaying(sBaseAudioID)))
            sBaseAudioID = Audio.Play2D(baseLoop, baseVolCurrent, true);
        if (!string.IsNullOrEmpty(chaseLoop) && (sChaseAudioID == 0 || !Audio.IsPlaying(sChaseAudioID)))
            sChaseAudioID = Audio.Play2D(chaseLoop, chaseVolCurrent, true);

        if (sBaseAudioID != 0)
        {
            baseVolCurrent = MoveTowards(baseVolCurrent, targetBase, step);
            Audio.SetVolume(sBaseAudioID, baseVolCurrent);
        }

        if (sChaseAudioID != 0)
        {
            chaseVolCurrent = MoveTowards(chaseVolCurrent, targetChase, step);
            Audio.SetVolume(sChaseAudioID, chaseVolCurrent);
        }
    }

    public override void OnExit()
    {
        if (sOwnerID != ID)
            return;

        StopManagedLoops();
        sOwnerID = 0;
    }

    //public void BeginChase() { }
    //public void EndChase() { }
    //public void SetChaseState(bool active) { }

    private float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }

    private static void StopManagedLoops()
    {
        if (sBaseAudioID != 0)
        {
            Audio.Stop(sBaseAudioID);
            sBaseAudioID = 0;
        }
        if (sChaseAudioID != 0)
        {
            Audio.Stop(sChaseAudioID);
            sChaseAudioID = 0;
        }
    }

    private void StartManagedLoops()
    {
        if (!string.IsNullOrEmpty(baseLoop))
        {
            baseVolCurrent = baseVolume;
            sBaseAudioID = Audio.Play2D(baseLoop, baseVolCurrent, true);
        }

        if (!string.IsNullOrEmpty(chaseLoop))
        {
            chaseVolCurrent = 0f; // start silent
            sChaseAudioID = Audio.Play2D(chaseLoop, chaseVolCurrent, true);
        }
    }

    private bool IsAnyEnemyChasing()
    {
        var enemies = EnemyRegistry.Snapshot();
        for (int i = 0; i < enemies.Count; ++i)
        {
            Entity e = enemies[i];
            if (e == null || !e.IsValid())
                continue;

            AIController ai = e.GetScript<AIController>();
            if (ai != null && ai.IsValid() && ai.isChasing)
                return true;
        }

        return false;
    }
}
