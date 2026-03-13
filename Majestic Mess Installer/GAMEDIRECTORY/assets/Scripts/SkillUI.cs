using Engine;
using System;
using System.Collections.Generic;

public class SkillUI : Entity
{
    public string cinderPanelName = "CinderHUD";
    public string disguisePanelName = "DisguiseHUD";

    private Entity cinderPanel, disguisePanel;

    public override void OnInit()
    {
        ResolvePanels();
    }

    public override void OnUpdate(float dt)
    {
        if ((cinderPanelName != "" && (cinderPanel == null || !cinderPanel.IsValid())) ||
            (disguisePanelName != "" && (disguisePanel == null || !disguisePanel.IsValid())))
        {
            ResolvePanels();
        }

        SetHudVisible(PickUpItemManager.IsCinderUnlocked() || PickUpItemManager.IsDisguiseUnlocked());
    }

    public void SetSelection(SkillSwitcher.SkillSlot slot)
    {
        //Debug.Log($"[SkillHUD] SetSelection {slot}");
        bool anyUnlocked = PickUpItemManager.IsCinderUnlocked() || PickUpItemManager.IsDisguiseUnlocked();
        SetHudVisible(anyUnlocked);

        bool showCinder = slot == SkillSwitcher.SkillSlot.Cinder && PickUpItemManager.IsCinderUnlocked();
        bool showDisguise = slot == SkillSwitcher.SkillSlot.Disguise && PickUpItemManager.IsDisguiseUnlocked();
        SetPanelVisible(cinderPanel, showCinder);
        SetPanelVisible(disguisePanel, showDisguise);
    }

    private void ResolvePanels()
    {
        if (!string.IsNullOrEmpty(cinderPanelName))
        {
            cinderPanel = Entity.FindEntityByName(cinderPanelName);
            //Debug.Log($"[SkillHUD] Cinder panel found? {(cinderPanel != null ? "yes" : "no")}");
        }
        if (!string.IsNullOrEmpty(disguisePanelName))
        {
            disguisePanel = Entity.FindEntityByName(disguisePanelName);
            //Debug.Log($"[SkillHUD] Disguise panel found? {(disguisePanel != null ? "yes" : "no")}");
        }
    }

    private void SetPanelVisible(Entity panel, bool visible)
    {
        if (panel == null || !panel.IsValid())
        {
            //Debug.Log("[SkillHUD] SetPanelVisible: panel null/invalid");
            return;
        }
        //Debug.Log($"[SkillHUD] SetVisible {panel.Name} -> {visible}");
        InternalCalls.UIElementComponent_SetVisible(panel.ID, visible); // 
    }

    private void SetHudVisible(bool visible)
    {
        InternalCalls.UIElementComponent_SetVisible(ID, visible);
    }
    
}
