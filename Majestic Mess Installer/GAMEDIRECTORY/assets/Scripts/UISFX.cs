using Engine;
using System;

public class UISFX : Entity
{
    public string moveClipPath = "assets/Audio/SFX/UI/UI_Move_04.wav";
    public string selectClipPath = "assets/Audio/SFX/UI/UI_Click_2_F.wav";

    public KeyCode settingIncreaseKey = KeyCode.UpArrow;
    public KeyCode settingDecreaseKey = KeyCode.DownArrow;

    public float sfxVolume = 0.5f;

    public override void OnInit()
    {
        if (string.IsNullOrEmpty(moveClipPath))
            moveClipPath = "assets/Audio/SFX/UI/UI_Move_04.wav";
        if (string.IsNullOrEmpty(selectClipPath))
            selectClipPath = "assets/Audio/SFX/UI/UI_Click_2_F.wav";
    }

    public override void OnUpdate(float dt)
    {
        HandleSettingSfx();
    }

    private void HandleSettingSfx()
    {
        // Volume adjust keys
        if (Input.IsKeyPressed(settingIncreaseKey) ||
            Input.IsKeyPressed(settingDecreaseKey))
        {
            PlayMove();
        }
    }

    public void PlayMove()
    {
        Play(moveClipPath);
    }

    public void PlaySelect()
    {
        Play(selectClipPath);
    }

    private void Play(string clipPath)
    {
        if (string.IsNullOrEmpty(clipPath))
            return;

        Audio.Play2D(clipPath, sfxVolume);
    }
}
