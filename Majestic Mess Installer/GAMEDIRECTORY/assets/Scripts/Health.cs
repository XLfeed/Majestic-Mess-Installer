using Engine;
using System;

public class Health : Entity
{
    // HUD getters
    public float HP => currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => isDead;
    public bool IsInvulnerable => invulTimer > 0f;
    public float HP01 => maxHP <= 0f ? 0f : Clamp01(currentHP / maxHP);
    public float Invul01 => invulDuration <= 0f ? 0f : Clamp01(invulTimer / invulDuration);

    // Tuning
    public float maxHP = 100f;
    public float startHP = 100f;
    public float invulDuration = 0.6f; // seconds of i-frames after a hit

    // Runtime
    public float currentHP = 0f;
    private float invulTimer = 0f;
    private float damageBlockTimer = 0f;
    private bool isDead = false;

    public string loseSceneName = "LoseScene";
    private bool loseTriggered = false;

    public override void OnInit()
    {
        currentHP = Clamp(startHP, 0f, maxHP);
        isDead = currentHP <= 0f;
        invulTimer = 0f;
        damageBlockTimer = 0f;
    }

    public override void OnUpdate(float dt)
    {
        // prefer engine time if dt not passed
        if (dt <= 0f)
            dt = Time.GetDeltaTime();
        if (isDead && !loseTriggered && !string.IsNullOrEmpty(loseSceneName))
        {
            loseTriggered = true;
            Scene.LoadScene(loseSceneName);
            return;
        }
        //if (isDead)
        //    return;

        if (invulTimer > 0f)
        {
            invulTimer -= dt;
            if (invulTimer < 0f) invulTimer = 0f;
        }

        if (damageBlockTimer > 0f)
        {
            damageBlockTimer -= dt;
            if (damageBlockTimer < 0f) damageBlockTimer = 0f;
        }
    }

    // Returns true if damage was applied
    public bool TakeDamage(float amount, bool bypassInvulnerability = false, float extraIFrames = 0f)
    {
        if (isDead) return false;
        if (amount <= 0f) return false;
        if (damageBlockTimer > 0f) return false;
        if (!bypassInvulnerability && IsInvulnerable) return false;

        currentHP -= amount;
        if (currentHP < 0f) currentHP = 0f;

        if (!bypassInvulnerability)
        {
            float total = MathF.Max(invulDuration, extraIFrames);
            if (total > 0f) invulTimer = MathF.Max(invulTimer, total);
        }

        if (currentHP <= 0f && !isDead)
        {
            isDead = true;
        }

        return true;
    }

    //public void SetInvulnerable(float seconds)
    //{
    //    if (seconds <= 0f) return;
    //    invulTimer = MathF.Max(invulTimer, seconds);
    //}

    //public void KillInstant()
    //{
    //    if (isDead) return;
    //    currentHP = 0f;
    //    isDead = true;
    //    invulTimer = 0f;
    //}

    //public void ReviveTo(float hp)
    //{
    //    currentHP = Clamp(hp, 1f, maxHP);
    //    isDead = false;
    //    invulTimer = 0f;
    //    damageBlockTimer = 0f;
    //}

    //public void ReviveFull()
    //{
    //    currentHP = maxHP;
    //    isDead = false;
    //    invulTimer = 0f;
    //    damageBlockTimer = 0f;
    //}

    //public void BlockDamageFor(float seconds)
    //{
    //    if (seconds <= 0f) return;
    //    damageBlockTimer = MathF.Max(damageBlockTimer, seconds);
    //}

    // Helpers
    private static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    private static float Clamp01(float v) => Clamp(v, 0f, 1f);
}