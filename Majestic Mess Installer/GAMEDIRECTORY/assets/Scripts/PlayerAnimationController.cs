using Engine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Drives the AnimatorComponent for the player based on movement keys.
/// 
/// Rules:
/// - No WASD   -> Idle
/// - WASD      -> Movement
/// - WASD+Shift -> Sprint
/// - WASD+Ctrl  -> SneakWalk
/// (Shift / Ctrl alone do nothing)
/// </summary>
public class PlayerAnimationController : Entity
{
    // Match these to your Animator's clip names
    private const string ANIM_IDLE = "Armature|Idle.001";
    private const string ANIM_MOVE = "Armature|Movement";
    private const string ANIM_SPRINT = "Armature|Sprint";
    private const string ANIM_SNEAK = "Armature|SneakWalk";

    private enum AnimState
    {
        Idle,
        Move,
        Sprint,
        Sneak
    }

    private AnimState _currentState = AnimState.Idle;

    public override void OnInit()
    {
        if (!HasComponent<AnimatorComponent>())
        {
            Debug.Log("[PlayerAnimationController] ERROR: AnimatorComponent is required.");
            return;
        }

        // Make sure we start in Idle
        Animator.Play(ANIM_IDLE);
        Animator.Loop = true;
        Animator.AutoPlay = false;    // we control it manually now

        Debug.Log("[PlayerAnimationController] Initialized, starting in Idle.");
    }

    public override void OnUpdate(float dt)
    {
        if (!HasComponent<AnimatorComponent>())
            return;

        // ---- 1. Read movement keys ----
        bool w = Input.IsKeyHeld(KeyCode.W);
        bool a = Input.IsKeyHeld(KeyCode.A);
        bool s = Input.IsKeyHeld(KeyCode.S);
        bool d = Input.IsKeyHeld(KeyCode.D);

        bool anyMoveKey = w || a || s || d;

        bool shift = Input.IsKeyHeld(KeyCode.LeftShift);
        bool c = Input.IsKeyHeld(KeyCode.C);

        // ---- 2. Decide desired state ----
        AnimState desiredState;

        if (!anyMoveKey)
        {
            desiredState = AnimState.Idle;
        }
        else if (c)
        {
            desiredState = AnimState.Sneak;
        }
        else if (shift)
        {
            desiredState = AnimState.Sprint;
        }
        else
        {
            desiredState = AnimState.Move;
        }

        // ---- 3. Only switch when state actually changes ----
        if (desiredState == _currentState)
            return;

        _currentState = desiredState;

        switch (_currentState)
        {
            case AnimState.Idle:
                Animator.Play(ANIM_IDLE);
                break;

            case AnimState.Move:
                Animator.Play(ANIM_MOVE);
                break;

            case AnimState.Sprint:
                Animator.Play(ANIM_SPRINT);
                break;

            case AnimState.Sneak:
                Animator.Play(ANIM_SNEAK);
                break;
        }

        Animator.Loop = true;   // all these are looping locomotion clips
        // (Speed stays whatever you set in the inspector)
        // Debug.Log($"[PlayerAnimationController] Now playing: {Animator.CurrentAnimation}");
    }
}
