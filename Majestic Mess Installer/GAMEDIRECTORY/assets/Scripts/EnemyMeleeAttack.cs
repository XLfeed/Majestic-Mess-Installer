using System;
using System.Collections.Generic;
using Engine;

public class EnemyMeleeAttack : Entity
{
    public string playerName = "Player";

    // Combat tuning
    public float damage = 25f;
    public float attackCooldown = 0.9f;

    // Timing
    public float attackAnimLength = 1.458f; // full animation length in seconds
    public float hitboxStart = 0.5f;        // time when hit should apply
    public float hitboxEnd = 0.8f;          // latest time to allow the hit
    public float attackRange = 5.8f;

    // Hit flash on player
    public Vector3 hitFlashColor = new Vector3(1f, 0f, 0f);
    public float hitFlashDuration = 0.18f;

    private TransformComponent tf;
    private Entity player;
    private TransformComponent playerTf;
    private MeshRendererComponent playerMr;
    private SkinnedMeshRendererComponent playerSmr;

    private float attackTimer = 0f;
    private float cooldownTimer = 0f;
    private bool hasAppliedHit = false;
    private float hitFlashTimer = 0f;

    private enum FlashTarget { None, Mesh, Skinned }
    private FlashTarget lastFlashTarget = FlashTarget.None;
    public override void OnInit()
    {
        tf = Transform;
        ResolvePlayer();
        // Combat tuning
        damage = 25f;
        attackCooldown = 0.9f;

        // Timing
        attackAnimLength = 1.458f; // full animation length in seconds
        hitboxStart = 0.5f;        // time when hit should apply
        hitboxEnd = 0.8f;          // latest time to allow the hit
        attackRange = 5.8f;

        // Hit flash on player
        hitFlashColor = new Vector3(1f, 0f, 0f);
        hitFlashDuration = 0.18f;
        attackTimer = 0f;
        cooldownTimer = 0f;
        hasAppliedHit = false;
        hitFlashTimer = 0f;

    }

    public override void OnUpdate(float dt)
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= dt;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= dt;
            if (hitFlashTimer <= 0f || player == null || !player.IsValid())
                ClearHitFlash();
        }

        if (attackTimer > 0f)
        {
            attackTimer -= dt;
            float elapsed = attackAnimLength - attackTimer;

            if (!hasAppliedHit && elapsed >= hitboxStart && elapsed <= hitboxEnd)
            {
                ApplyHit();
                hasAppliedHit = true;
            }

            if (attackTimer <= 0f)
            {
                attackTimer = 0f;
                hasAppliedHit = false;
                cooldownTimer = MathF.Max(attackCooldown, 0.01f);
            }
        }
    }

    public bool InRange(float range = -1f)
    {
        if (range <= 0f) range = attackRange;
        if (tf == null || playerTf == null)
            return false;
        Vector3 a = tf.Position; a.y = 0f;
        Vector3 b = playerTf.Position; b.y = 0f;
        float dx = a.x - b.x, dz = a.z - b.z, dy = a.y - b.y;
        float r2 = range * range;
        return dx * dx + dy * dy + dz * dz <= r2;
    }

    public bool TryAttack()
    {
        if (attackTimer > 0f || cooldownTimer > 0f)
            return false;

        if (player == null || !player.IsValid() || playerTf == null)
            ResolvePlayer();

        if (playerTf == null || !InRange())
            return false;

        // face player (yaw only)
        Vector3 dir = new Vector3(playerTf.Position.x - tf.Position.x, 0f, playerTf.Position.z - tf.Position.z);
        if (dir.SqrMag > 0.0001f)
        {
            float angle = MathF.Atan2(dir.x, dir.z);
            tf.Rotation = new Vector3(tf.Rotation.x, angle * (180f / MathF.PI), tf.Rotation.z);
        }

        attackTimer = MathF.Max(0.01f, attackAnimLength);
        hasAppliedHit = false;
        return true;
    }

    private void ApplyHit()
    {
        if (player == null || !player.IsValid() || playerTf == null)
            return;
        if (!InRange(attackRange + 0.2f))
            return;

        var h = player.GetScript<Health>();
        if (h != null)
            h.TakeDamage(damage, false, 0f);

        StartHitFlash();
    }

    private void ResolvePlayer()
    {
        player = Entity.FindEntityByName(playerName);
        playerTf = player != null && player.IsValid() ? player.Transform : null;
        playerSmr = (player != null && player.IsValid() && player.HasComponent<SkinnedMeshRendererComponent>())
            ? player.GetComponent<SkinnedMeshRendererComponent>()
            : null;
        playerMr = (player != null && player.IsValid() && player.HasComponent<MeshRendererComponent>())
            ? player.GetComponent<MeshRendererComponent>()
            : null;
    }

    public void SetTarget(Entity target)
    {
        player = target;
        playerTf = (player != null && player.IsValid()) ? player.Transform : null;
        playerSmr = (player != null && player.IsValid() && player.HasComponent<SkinnedMeshRendererComponent>())
            ? player.GetComponent<SkinnedMeshRendererComponent>()
            : null;
        playerMr = (player != null && player.IsValid() && player.HasComponent<MeshRendererComponent>())
            ? player.GetComponent<MeshRendererComponent>()
            : null;
    }

    private void StartHitFlash()
    {
        if (player == null || !player.IsValid())
            return;

        lastFlashTarget = FlashTarget.None;

        if (playerSmr != null && playerSmr.SetColor(hitFlashColor))
        {
            lastFlashTarget = FlashTarget.Skinned;
            hitFlashTimer = MathF.Max(hitFlashDuration, 0.01f);
            return;
        }

        if (playerMr == null && player.HasComponent<MeshRendererComponent>())
            playerMr = player.GetComponent<MeshRendererComponent>();

        if (playerMr != null)
        {
            playerMr.SetColor(hitFlashColor);
            lastFlashTarget = FlashTarget.Mesh;
            hitFlashTimer = MathF.Max(hitFlashDuration, 0.01f);
        }
    }

    private void ClearHitFlash()
    {
        if (player != null && player.IsValid())
        {
            if (lastFlashTarget == FlashTarget.Skinned)
                playerSmr?.ClearColor();
            else if (lastFlashTarget == FlashTarget.Mesh)
                playerMr?.ClearColor();
            else
            {
                playerSmr?.ClearColor();
                playerMr?.ClearColor();
            }
        }
        hitFlashTimer = 0f;
        lastFlashTarget = FlashTarget.None;
    }

}
