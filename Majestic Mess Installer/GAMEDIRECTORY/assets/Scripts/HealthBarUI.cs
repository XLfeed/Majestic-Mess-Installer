using Engine;
using System;

public class HealthBarUI : Entity
{
    public string playerName = "Player";
    public string fillEntityName = "HealthBarFill";
    //public KeyCode debugDamageKey = KeyCode.H;
    public float debugDamageAmount = 10f;

    private Entity player;
    private Health playerHealth;
    private Entity fillEntity;
    private RectTransformComponent fillRect;
    private float fullWidth; // fallback if we can't read it

    public override void OnInit()
    {
        ResolvePlayer();
        ResolveFill();

    }

    public override void OnUpdate(float dt)
    {
        if (playerHealth == null || !playerHealth.IsValid())
            ResolvePlayer();

        if (fillRect == null || (fillEntity != null && !fillEntity.IsValid()))
            ResolveFill();

        //if (playerHealth != null && Input.IsKeyPressed(debugDamageKey))
        //    playerHealth.TakeDamage(debugDamageAmount);

        if (playerHealth == null || fillRect == null)
            return;

        float hp01 = playerHealth.HP01;
        var size = fillRect.SizeDelta;
        if (fullWidth <= 0f) fullWidth = size.x > 0f ? size.x : 300f;
        size.x = fullWidth * hp01;
        fillRect.SizeDelta = size;
    }

    private void ResolvePlayer()
    {
        player = Entity.FindEntityByName(playerName);
        playerHealth = (player != null && player.IsValid()) ? player.GetScript<Health>() : null;
    }

    private void ResolveFill()
    {
        fillEntity = Entity.FindEntityByName(fillEntityName);
        if (fillEntity != null && fillEntity.IsValid())
        {
            fillRect = fillEntity.GetComponent<RectTransformComponent>();
            if (fillRect != null)
            {
                var size = fillRect.SizeDelta;
                if (size.x > 0f) fullWidth = size.x;
            }
        }
        else
        {
            fillRect = null;
        }
    }
}

//using Engine;
//using System;

//public class HealthBarUI : Entity
//{
//    public string playerName = "Player";
//    public string fillEntityName = "HealthBarFill";
//    public KeyCode debugDamageKey = KeyCode.H;
//    public float debugDamageAmount = 10f;

//    private Entity player;
//    private Health playerHealth;
//    private Entity fillEntity;
//    private RectTransformComponent fillRect;
//    private float fullAnchorMaxX = 1f; // original anchor max x
//    private float fullAnchorMinX = 0f; // assume anchorMin.x stays fixed

//    public override void OnInit()
//    {
//        ResolvePlayer();
//        ResolveFill();
//    }

//    public override void OnUpdate(float dt)
//    {
//        if (playerHealth == null || !playerHealth.IsValid())
//            ResolvePlayer();

//        if (fillRect == null || (fillEntity != null && !fillEntity.IsValid()))
//            ResolveFill();

//        if (playerHealth != null && Input.IsKeyPressed(debugDamageKey))
//            playerHealth.TakeDamage(debugDamageAmount);

//        if (playerHealth == null || fillRect == null)
//            return;
//        var anchorMax = fillRect.AnchorMax;
//        anchorMax.x = fullAnchorMinX + (fullAnchorMaxX - fullAnchorMinX) * playerHealth.HP01;
//        fillRect.AnchorMax = anchorMax;
//    }

//    private void ResolvePlayer()
//    {
//        player = Entity.FindEntityByName(playerName);
//        playerHealth = (player != null && player.IsValid()) ? player.GetScript<Health>() : null;
//    }

//    private void ResolveFill()
//    {
//        fillEntity = Entity.FindEntityByName(fillEntityName);
//        if (fillEntity != null && fillEntity.IsValid())
//        {
//            fillRect = fillEntity.GetComponent<RectTransformComponent>();
//            if (fillRect != null)
//            {
//                var anchorMax = fillRect.AnchorMax;
//                //var anchorMin = fillRect.AnchorMin; // add AnchorMin property similarly if needed
//                fullAnchorMaxX = anchorMax.x;
//                //fullAnchorMinX = anchorMin.x;
//            }
//        }
//        else
//        {
//            fillRect = null;
//        }
//    }
//}