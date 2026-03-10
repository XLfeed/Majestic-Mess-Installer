using Engine;
using System;
using System.Collections.Generic;

public class EnemyStatesSFX : Entity
{
    public string[] attackVariations =
      {
          "assets/Audio/SFX/Enemy_SwordAttack_01 1.wav",
          "assets/Audio/SFX/Enemy_SwordAttack_02 1.wav",
          "assets/Audio/SFX/Enemy_SwordAttack_03 1.wav",
          "assets/Audio/SFX/Enemy_SwordAttack_04 1.wav",
      };
    public float attackVolume = 0.7f;
    public bool avoidImmediateRepeat = true;

    // Sheath / Unsheath
    public string unsheathClip = "assets/Audio/SFX/Enemy_Sword_Unsheath.wav";
    public string sheathClip = "assets/Audio/SFX/Enemy_Sword_Sheath.wav";
    public float sheathVolume = 0.7f;

    // ---- Enemy VO: Alert / Chase / Chase End ----
    public string[] alertVoiceClips =
    {
           "assets/Audio/SFX/EnemyVO/VO_Enemy_01.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_02.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_03.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_04.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_05.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_06.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_07.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_08.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_09.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_10.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_11.wav",
      };
    public string[] chaseVoiceClips =
    {
           "assets/Audio/SFX/EnemyVO/VO_Enemy_12.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_13.wav",
      };
    public string[] chaseEndVoiceClips =
    {
           "assets/Audio/SFX/EnemyVO/VO_Enemy_14.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_15.wav",
           "assets/Audio/SFX/EnemyVO/VO_Enemy_16.wav",
      };
    public float alertVoiceVolume = 1.0f;
    public float chaseVoiceVolume = 1.0f;
    public float chaseEndVoiceVolume = 1.0f;

    // VO cooldowns
    public float alertVoCooldown = 0.5f;
    public float chaseVoCooldown = 10.0f;
    public float chaseEndVoCooldown = 0.5f;
    private float alertVoTimer = 0.0f;
    private float chaseVoTimer = 0.0f;
    private float chaseEndVoTimer = 0.0f;

    private AudioComponent ac;
    private readonly Dictionary<string, int> clipInstanceByPath = new Dictionary<string, int>();
    private int lastAttack = -1;
    private int lastAlertIdx = -1;
    private int lastChaseIdx = -1;
    private int lastChaseEndIdx = -1;

    private static readonly Random s_Random = new Random();

    public override void OnInit()
    {
        EnsureRuntimeDefaults();
        ac = HasComponent<AudioComponent>() ? new AudioComponent(ID) : null;
        InitializeClipInstances();
        // Allow immediate VO on first trigger after spawn.
        alertVoTimer = alertVoCooldown;
        chaseVoTimer = chaseVoCooldown;
        chaseEndVoTimer = chaseEndVoCooldown;
    }

    private void EnsureRuntimeDefaults()
    {
        // Serialized prefab values can be zero, guard against silent output.
        if (attackVolume <= 0f) attackVolume = 0.7f;
        if (sheathVolume <= 0f) sheathVolume = 0.7f;
        if (alertVoiceVolume <= 0f) alertVoiceVolume = 1.0f;
        if (chaseVoiceVolume <= 0f) chaseVoiceVolume = 1.0f;
        if (chaseEndVoiceVolume <= 0f) chaseEndVoiceVolume = 1.0f;
        if (alertVoCooldown < 0f) alertVoCooldown = 0f;
        if (chaseVoCooldown < 0f) chaseVoCooldown = 0f;
        if (chaseEndVoCooldown < 0f) chaseEndVoCooldown = 0f;
    }

    public override void OnUpdate(float dt)
    {
        if (ac == null && HasComponent<AudioComponent>())
        {
            ac = new AudioComponent(ID);
            InitializeClipInstances();
        }

        alertVoTimer += dt;
        chaseVoTimer += dt;
        chaseEndVoTimer += dt;
    }

    // ---- external triggers ----
    public void PlayAttackSfx() { PlayAttack(); }
    public void PlayUnsheath() { PlayOne(unsheathClip, sheathVolume); }
    public void PlaySheath() { PlayOne(sheathClip, sheathVolume); }

    // Enemy VO events:
    // - When first spotting player / entering Wary/Alert
    public void PlayAlertVO()
    {
        if (alertVoTimer < alertVoCooldown)
            return;

        PlayRandomFrom(alertVoiceClips, ref lastAlertIdx, alertVoiceVolume);
        alertVoTimer = 0.0f;
    }
    // - While in Chase state (e.g. on enter or on some cooldown)
    public void PlayChaseVO(bool force = false)
    {
        if (!force && chaseVoTimer < chaseVoCooldown)
            return;

        PlayRandomFrom(chaseVoiceClips, ref lastChaseIdx, chaseVoiceVolume);
        chaseVoTimer = 0.0f;
    }
    // - When losing sight of player / chase ends
    public void PlayChaseEndVO()
    {
        if (chaseEndVoTimer < chaseEndVoCooldown)
            return;

        PlayRandomFrom(chaseEndVoiceClips, ref lastChaseEndIdx, chaseEndVoiceVolume);
        chaseEndVoTimer = 0.0f;
    }

    // ---- Helpers ----
    private void PlayAttack()
    {
        if (ac == null || attackVariations == null || attackVariations.Length == 0)
            return;

        int idx = s_Random.Next(attackVariations.Length);
        if (avoidImmediateRepeat && attackVariations.Length > 1 && idx == lastAttack)
            idx = (idx + 1) % attackVariations.Length;

        lastAttack = idx;
        string clip = attackVariations[idx];
        if (string.IsNullOrEmpty(clip))
            return;

        PlayClip(clip, attackVolume);
    }

    private void PlayOne(string clip, float vol)
    {
        if (ac == null || string.IsNullOrEmpty(clip))
            return;

        PlayClip(clip, vol);
    }

    private void PlayRandomFrom(string[] clips, ref int lastIdx, float vol)
    {
        if (ac == null && HasComponent<AudioComponent>())
            ac = new AudioComponent(ID);
        if (ac == null || clips == null || clips.Length == 0)
            return;

        int idx = s_Random.Next(clips.Length);
        if (avoidImmediateRepeat && clips.Length > 1 && idx == lastIdx)
            idx = (idx + 1) % clips.Length;

        lastIdx = idx;
        string clip = clips[idx];
        if (string.IsNullOrEmpty(clip)) return;

        PlayClip(clip, vol);
    }

    private void InitializeClipInstances()
    {
        if (ac == null)
            return;

        clipInstanceByPath.Clear();

        RegisterClipArray(attackVariations);
        RegisterClip(unsheathClip);
        RegisterClip(sheathClip);
        RegisterClipArray(alertVoiceClips);
        RegisterClipArray(chaseVoiceClips);
        RegisterClipArray(chaseEndVoiceClips);
    }

    private void RegisterClipArray(string[] clips)
    {
        if (clips == null)
            return;
        for (int i = 0; i < clips.Length; i++)
            RegisterClip(clips[i]);
    }

    private void RegisterClip(string clip)
    {
        if (ac == null || string.IsNullOrEmpty(clip) || clipInstanceByPath.ContainsKey(clip))
            return;

        int instance = ac.AddInstance(clip);
        if (instance < 0)
            return;

        ac.SetInstanceLoop(instance, false);
        ac.SetInstanceVolume(instance, 1.0f);
        clipInstanceByPath[clip] = instance;
    }

    private void PlayClip(string clip, float volume)
    {
        if (ac == null || string.IsNullOrEmpty(clip))
            return;

        if (!clipInstanceByPath.TryGetValue(clip, out int instance))
        {
            RegisterClip(clip);
            if (!clipInstanceByPath.TryGetValue(clip, out instance))
            {
                // Fallback path if instance registration fails.
                ac.SetVolume(volume);
                ac.Play(clip);
                return;
            }
        }

        ac.SetInstanceVolume(instance, volume);
        ac.PlayInstance(instance);
    }

}
