using Engine;
using System;
using System.Collections.Generic;

public class SkillSwitcher : Entity
{
    public ScrollOfCinderSkill cinder;
    public DisguiseSkill disguise;

    public KeyCode selectCinderKey = KeyCode.D1;
    public KeyCode selectDisguiseKey = KeyCode.D2;
    public KeyCode castKey = KeyCode.E;

    public SkillUI hud;

    public enum SkillSlot { Cinder, Disguise }
    public SkillSlot current = SkillSlot.Cinder;

    //cooldown
    public Entity UISkillCDAnim;
    public string UISkillCDAnimName = "UISkillCDAnim";
    private RectTransformComponent UISkillCDReact;
    private float UISkillCDAnimMaxScale = 150.0f;

    // skill charges text
    public string chargeText;
    public string UIChargeTextName = "UISkillChargeText";
    public Entity UIText;
    public UITextComponent UITextComp;

    public override void OnInit()
    {
        if (cinder == null) cinder = GetScript<ScrollOfCinderSkill>();
        if (disguise == null) disguise = GetScript<DisguiseSkill>();

        if (hud == null)
        {
            var hudEnt = Entity.FindEntityByName("SkillHUD"); 
            if (hudEnt != null && hudEnt.IsValid())
                hud = hudEnt.GetScript<SkillUI>();
        }

        //Debug.Log($"[SkillSwitcher] Init. HUD set? {(hud != null)}");
        hud?.SetSelection(current);

        UISkillCDAnim = Entity.FindEntityByName(UISkillCDAnimName);
        if (UISkillCDAnim != null)
        {
            UISkillCDReact = UISkillCDAnim.GetComponent<RectTransformComponent>();
            //cinderCDReact.SizeDelta.y = 0.0f;
        }

        UIText = Entity.FindEntityByName(UIChargeTextName);
        if (UIText != null)
        {
            //UITextComp = UIText.GetComponent<UITextComponent>();
            UITextComp = new UITextComponent(UIText.ID);
            if (UITextComp != null)
            {
                UITextComp.FontSize = 30.0f;
                UITextComp.Color = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
                UITextComp.SetFontByName("MedievalSharp-Book");
                // Layout
                UITextComp.WordWrap = true;
                UITextComp.Overflow = UITextOverflow.Ellipsis;
            }
        }
    }

    public override void OnUpdate(float dt)
    {
        if (Input.IsKeyPressed(selectCinderKey))
        {
            //Debug.Log("[SkillSwitcher] Select Cinder");
            current = SkillSlot.Cinder;
            hud?.SetSelection(current);
        }
        else if (Input.IsKeyPressed(selectDisguiseKey))
        {
            //Debug.Log("[SkillSwitcher] Select Disguise");
            current = SkillSlot.Disguise;
            hud?.SetSelection(current);
        }

        if (Input.IsKeyPressed(castKey))
        {
            switch (current)
            {
                case SkillSlot.Cinder:
                    cinder?.TryUseSkill();
                    break;
                case SkillSlot.Disguise:
                    disguise?.TryUseSkill();
                    break;
            }
        }

        SkillCooldownAnimation();
        UpdateUIText();
    }

    public void SkillCooldownAnimation()
    {
        if (UISkillCDReact == null) return;
        if (current == SkillSlot.Cinder && cinder == null) return;
        if (current == SkillSlot.Disguise && disguise == null) return;

        float t = 0.0f;
        //float t = cooldownTimer / cooldown;
        if (current == SkillSlot.Cinder)
        {
            // cinder cooldown
            t = cinder.cooldown > 0.0f ? (cinder.cooldownTimer / cinder.cooldown) : 0.0f;
        }
        else if (current == SkillSlot.Disguise)
        {
            // disguise cooldown
            t = disguise.cooldown > 0.0f ? (disguise.cooldownTimer / disguise.cooldown) : 0.0f;
        }
        
        t = Math.Clamp(t, 0f, 1f);

        float eased = 1f - (1f - t) * (1f - t);

        // HUD cooldown panel
        var size = UISkillCDReact.SizeDelta;
        size.y = UISkillCDAnimMaxScale * eased;
        if (size.y < 0.0f) size.y = 0.0f;
        UISkillCDReact.SizeDelta = size;
    }

    public void UpdateUIText()
    {
        if (UITextComp == null)
            return;

        if (current == SkillSlot.Cinder)
        {
            // cinder
            if (cinder != null)
                UITextComp.Text = $"Charge: {cinder.currCharges}";
        }
        else if (current == SkillSlot.Disguise)
        {
            // disguise
            if (disguise != null)
                UITextComp.Text = $"Charge: {disguise.currCharges}";
        }
        
    }

}
