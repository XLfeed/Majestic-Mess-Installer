using Engine;
using System;

public class ThirdPersonCamera : Entity
{
    public string targetName = "Player";
    public float followDistance = 10.0f;
    public Vector3 targetOffset = new Vector3(0.0f, 4.3f, 0.0f);
    public float horizontalSensitivity = 3.0f;
    public float verticalSensitivity = 2.5f;

    // Static sensitivity set by MouseSensitivitySlider (persists across scenes)
    public static float GlobalSensitivity = -1f;
    public float minVerticalAngle = 0f;
    public float maxVerticalAngle = 65f;
    public bool invertY = false;

    private Entity target;
    private float yaw;   // degrees
    private float pitch; // degrees

    private const float Deg2Rad = (float)(Math.PI / 180.0);
    private const float Rad2Deg = (float)(180.0 / Math.PI);

    public override void OnInit()
    {
        if (!ResolveTarget(true))
            return;

        AlignToCurrentPlacement();
        pitch = Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        yaw = NormalizeAngle(yaw);
    }

    public override void OnUpdate(float dt)
    {
        if (!ResolveTarget(false))
            return;

        if (GlobalSensitivity >= 0f)
        {
            horizontalSensitivity = GlobalSensitivity;
            verticalSensitivity = GlobalSensitivity * 0.83f;
        }

        Vector2 mouseDelta = Input.GetMouseDelta();
        yaw = NormalizeAngle(yaw + mouseDelta.x * horizontalSensitivity);

        float pitchDelta = mouseDelta.y * verticalSensitivity;
        pitch += invertY ? -pitchDelta : pitchDelta;
        pitch = Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        Vector3 focusPoint = target.Transform.Position + targetOffset;
        Vector3 orbitDirection = CalculateOrbitDirection();

        Transform.Position = focusPoint + orbitDirection * followDistance;

        Transform.LookAt(focusPoint);
    }

    private bool ResolveTarget(bool logFailure)
    {
        if (target != null && target.IsValid())
            return true;

        if (string.IsNullOrEmpty(targetName))
        {
            if (logFailure)
                Debug.Log("[ThirdPersonCamera] Target name is empty.");
            return false;
        }

        target = Entity.FindEntityByName(targetName);
        if ((target == null || !target.IsValid()) && logFailure)
            Debug.Log($"[ThirdPersonCamera] Unable to find entity '{targetName}'.");
        return target != null && target.IsValid();
    }

    private void AlignToCurrentPlacement()
    {
        Vector3 focusPoint = target.Transform.Position + targetOffset;
        Vector3 toCamera = Transform.Position - focusPoint;

        float sqrMag = toCamera.SqrMag;
        if (sqrMag <= 0.0001f)
        {
            yaw = 180f;
            pitch = 0f;
            return;
        }

        float distance = (float)Math.Sqrt(sqrMag);
        if (followDistance <= 0.001f)
            followDistance = distance;

        Vector3 dir = toCamera / distance;
        pitch = (float)Math.Asin(Clamp(dir.y, -1f, 1f)) * Rad2Deg;
        yaw = NormalizeAngle((float)Math.Atan2(dir.x, -dir.z) * Rad2Deg);
    }

    private Vector3 CalculateOrbitDirection()
    {
        float yawRad = yaw * Deg2Rad;
        float pitchRad = pitch * Deg2Rad;
        float cosPitch = (float)Math.Cos(pitchRad);

        Vector3 dir = new Vector3(
            (float)(Math.Sin(yawRad) * cosPitch),
            (float)Math.Sin(pitchRad),
            (float)(-Math.Cos(yawRad) * cosPitch)
        );

        return Normalize(dir);
    }

    private static Vector3 Normalize(Vector3 v)
    {
        float length = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        if (length <= 0.0001f)
            return Vector3.Forward;

        float inv = 1f / length;
        return new Vector3(v.x * inv, v.y * inv, v.z * inv);
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }
}
