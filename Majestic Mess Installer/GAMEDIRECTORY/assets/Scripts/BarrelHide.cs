using Engine;
using System;

public class BarrelHide : Entity
{
    public KeyCode interactKey = KeyCode.F;
    public string playerName = "Player";
    public float interactRadius = 3.0f;
    public bool ignoreY = true;
    public Vector3 hideOffset = Vector3.Zero;
    public float ejectLaunchSpeed = 10f;
    public bool zeroVelocityOnHide = true;
    public bool logMessages = false;

    private Entity _player;
    private PlayerController playerController;
    private TransformComponent _playerTransform;
    private RigidBodyComponent _playerRigidBody;
    private TransformComponent _barrelTransform;
    private Vector3 _cachedPlayerPosition;
    private Vector3 _cachedPlayerScale;
    private bool _cachedScaleValid = false;
    private bool _playerHidden = false;
    private bool _hasLock = false;

    private AudioComponent ac;
    public string clips =  "assets/Audio/SFX/Barell_temp_sfx.wav";

    public override void OnInit()
    {
         ac = GetComponent<AudioComponent>();
        _barrelTransform = Transform;
        ResolvePlayer();
        if (logMessages)
            Debug.Log("[BarrelHide] Ready on " + Name);
    }

    public override void OnUpdate(float dt)
    {
        if (_barrelTransform == null)
            return;

        if (_playerHidden && _playerTransform != null)
        {
            // Keep the player pinned to the barrel's origin each frame while hidden
            _playerTransform.Position = _barrelTransform.Position + hideOffset;
            MaintainHiddenScale();
        }

        if (!ResolvePlayer())
            return;

        Vector3 playerPos = _playerTransform.Position;
        Vector3 barrelPos = _barrelTransform.Position;
        if (ignoreY)
            playerPos.y = barrelPos.y;

        float dx = playerPos.x - barrelPos.x;
        float dy = playerPos.y - barrelPos.y;
        float dz = playerPos.z - barrelPos.z;
        float distSq = dx * dx + dy * dy + dz * dz;

        float radius = Math.Max(0.01f, interactRadius);
        if (distSq > radius * radius)
            return;

        if (!Input.IsKeyPressed(interactKey))
            return;

        if (!_playerHidden)
        {
            //Play sfx
            PlaySound();
            HidePlayer();
        }       
        else
        {
            //Play sfx
            PlaySound();
            EjectPlayer();
        }
           
    }

    public override void OnTriggerExit(ColliderComponent collider)
    {
        if (_playerHidden)
            return;

        if (_player != null && collider.Entity.ID == _player.ID)
            ClearPlayerCache();
    }

    public override void OnExit()
    {
        if (_playerHidden)
            ReleaseInputLock();
    }

    private bool ResolvePlayer()
    {
        if (_player != null && _player.IsValid())
        {
            if (_playerTransform == null)
                _playerTransform = _player.Transform;
            return _playerTransform != null;
        }

        if (string.IsNullOrEmpty(playerName))
            return false;

        _player = Entity.FindEntityByName(playerName);
        if (_player == null || !_player.IsValid())
        {
            ClearPlayerCache();
            return false;
        }

        _playerTransform = _player.Transform;
        _playerRigidBody = _player.HasComponent<RigidBodyComponent>() ? _player.RigidBody : null;
        playerController = _player.GetScript<PlayerController>();

        return _playerTransform != null;
    }

    private void ClearPlayerCache()
    {
        _player = null;
        _playerTransform = null;
        _playerRigidBody = null;
    }

    private void HidePlayer()
    {
        if (_playerTransform == null || _barrelTransform == null)
            return;

        _cachedPlayerPosition = _playerTransform.Position;
        CacheScaleIfNeeded();

        // Snap to the barrel's origin (local 0,0,0 in world space) with optional offset tweak
        Vector3 hidePosition = _barrelTransform.Position + hideOffset;
        _playerTransform.Position = hidePosition;
        ApplyHiddenScale();

        if (zeroVelocityOnHide)
        {
            _playerRigidBody ??= _player.HasComponent<RigidBodyComponent>() ? _player.RigidBody : null;
            if (_playerRigidBody != null)
                _playerRigidBody.Velocity = Vector3.Zero;
        }

        _playerHidden = true;
        playerController.isHidden = _playerHidden;
        AcquireInputLock();

        if (logMessages)
            Debug.Log($"[BarrelHide] {_player.Name} hidden at {hidePosition} (saved {_cachedPlayerPosition}).");
    }

    private void EjectPlayer()
    {
        if (_playerTransform == null)
            return;

        _playerTransform.Position = _cachedPlayerPosition;

        if (_playerRigidBody == null && _player.HasComponent<RigidBodyComponent>())
            _playerRigidBody = _player.RigidBody;

        if (_playerRigidBody != null && ejectLaunchSpeed > 0f && _barrelTransform != null)
        {
            Vector3 launchDir = _cachedPlayerPosition - _barrelTransform.Position;
            float lenSq = launchDir.SqrMag;
            if (lenSq > 0.0001f)
            {
                float invLen = 1f / (float)Math.Sqrt(lenSq);
                launchDir = new Vector3(launchDir.x * invLen, launchDir.y * invLen, launchDir.z * invLen);
            }
            else
            {
                launchDir = Vector3.Up;
            }

            _playerRigidBody.Velocity = launchDir * ejectLaunchSpeed;
        }

        _playerHidden = false;
        playerController.isHidden = _playerHidden;
        ReleaseInputLock();

        // Restore original scale
        if (_cachedScaleValid && _playerTransform != null)
        {
            _playerTransform.Scale = _cachedPlayerScale;
            _cachedScaleValid = false;
        }

        if (logMessages)
            Debug.Log($"[BarrelHide] {_player.Name} ejected to {_cachedPlayerPosition}.");
    }

    private void AcquireInputLock()
    {
        if (_hasLock)
            return;

        PlayerInputBlocker.SetBlocked(true);
        _hasLock = true;
    }

    private void ReleaseInputLock()
    {
        if (!_hasLock)
            return;

        PlayerInputBlocker.SetBlocked(false);
        _hasLock = false;
    }

    private void CacheScaleIfNeeded()
    {
        if (_cachedScaleValid || _playerTransform == null)
            return;

        _cachedPlayerScale = _playerTransform.Scale;
        _cachedScaleValid = true;
    }

    private void ApplyHiddenScale()
    {
        if (_playerTransform == null)
            return;

        // Shrink collider footprint to avoid physics pushing while inside
        const float hiddenScale = 0.001f;
        _playerTransform.Scale = new Vector3(hiddenScale, hiddenScale, hiddenScale);
    }

    private void MaintainHiddenScale()
    {
        if (_playerTransform == null)
            return;

        const float hiddenScale = 0.001f;
        Vector3 s = _playerTransform.Scale;
        if (Math.Abs(s.x - hiddenScale) > 0.0001f ||
            Math.Abs(s.y - hiddenScale) > 0.0001f ||
            Math.Abs(s.z - hiddenScale) > 0.0001f)
        {
            _playerTransform.Scale = new Vector3(hiddenScale, hiddenScale, hiddenScale);
        }
    }

     void PlaySound()
    {
        if (ac == null)
            return;


        ac.Play(clips);
    }
}
