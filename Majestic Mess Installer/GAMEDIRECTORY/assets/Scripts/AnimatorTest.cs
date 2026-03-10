using System;
using Engine;

/// <summary>
/// Test script to verify bridge between C# and AnimatorComponent works
///
/// Controls:
/// - 1, 2, 3 keys: Switch between animations
/// - Space: Pause/Resume
/// - Up/Down arrows: Change speed
/// - L: Toggle loop
/// - I: Print animator info
/// </summary>
public class AnimatorTest : Entity
{
    // Update these to match your animation names
    private const string ANIM_1 = "Idle.001";
    private const string ANIM_2 = "Movement";
    private const string ANIM_3 = "Sprint";
    private const string ANIM_4 = "SneakWalk";

    public override void OnInit()
    {
        // Check if we have an animator component
        if (!HasComponent<AnimatorComponent>())
        {
            Debug.Log("ERROR: SimpleAnimatorTest requires an AnimatorComponent!");
            return;
        }

        Debug.Log("=== SimpleAnimatorTest Started ===");
        Debug.Log($"Current animation: {Animator.CurrentAnimation}");
        Debug.Log($"Is playing: {Animator.IsPlaying}");
        Debug.Log("Press I to see controls and info");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Skip if we don't have animator
        if (!HasComponent<AnimatorComponent>())
            return;

        // Switch animations with 1, 2, 3 keys
        if (Input.IsKeyPressed(KeyCode.D1))
        {
            Animator.Play(ANIM_1);
            Debug.Log($"Playing: {ANIM_1}");
        }

        if (Input.IsKeyPressed(KeyCode.D2))
        {
            Animator.Play(ANIM_2);
            Debug.Log($"Playing: {ANIM_2}");
        }

        if (Input.IsKeyPressed(KeyCode.D3))
        {
            Animator.Play(ANIM_3);
            Debug.Log($"Playing: {ANIM_3}");
        }

        if (Input.IsKeyPressed(KeyCode.D4))
        {
            Animator.Play(ANIM_4);
            Debug.Log($"Playing: {ANIM_4}");
        }

        // Pause/Resume with Space
        if (Input.IsKeyPressed(KeyCode.Space))
        {
            if (Animator.IsPlaying)
            {
                Animator.Pause();
                Debug.Log("Animation PAUSED");
            }
            else
            {
                Animator.Resume();
                Debug.Log("Animation RESUMED");
            }
        }

        // Change speed with Up/Down arrows
        if (Input.IsKeyPressed(KeyCode.UpArrow))
        {
            Animator.Speed += 0.25f;
            Debug.Log($"Speed increased to: {Animator.Speed:F2}x");
        }

        if (Input.IsKeyPressed(KeyCode.DownArrow))
        {
            Animator.Speed = Math.Max(0.1f, Animator.Speed - 0.25f);
            Debug.Log($"Speed decreased to: {Animator.Speed:F2}x");
        }

        // Toggle loop with L
        if (Input.IsKeyPressed(KeyCode.L))
        {
            Animator.Loop = !Animator.Loop;
            Debug.Log($"Loop: {Animator.Loop}");
        }

        // Print info with I
        if (Input.IsKeyPressed(KeyCode.I))
        {
            Debug.Log("=== Animator Info ===");
            Debug.Log($"Current Animation: {Animator.CurrentAnimation}");
            Debug.Log($"Is Playing: {Animator.IsPlaying}");
            Debug.Log($"Is Paused: {Animator.IsPaused}");
            Debug.Log($"Current Time: {Animator.CurrentTime:F2}s");
            Debug.Log($"Normalized Time: {Animator.NormalizedTime:F2} ({Animator.NormalizedTime * 100:F0}%)");
            Debug.Log($"Speed: {Animator.Speed:F2}x");
            Debug.Log($"Loop: {Animator.Loop}");
            Debug.Log($"AutoPlay: {Animator.AutoPlay}");
            Debug.Log("===========================================");
            Debug.Log("CONTROLS:");
            Debug.Log("  1, 2, 3, 4 - Switch animations");
            Debug.Log("  Space - Pause/Resume");
            Debug.Log("  Up/Down - Change speed");
            Debug.Log("  L - Toggle loop");
            Debug.Log("  I - Show this info");
            Debug.Log("===========================================");
        }
    }
}
