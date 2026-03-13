using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// Text alignment options for horizontal positioning
    /// </summary>
    public enum UITextAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    /// <summary>
    /// Text alignment options for vertical positioning
    /// </summary>
    public enum UITextVerticalAlignment
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }

    /// <summary>
    /// Text overflow modes for handling text that exceeds bounds
    /// </summary>
    public enum UITextOverflow
    {
        Overflow = 0,
        Truncate = 1,
        Ellipsis = 2
    }

    /// <summary>
    /// Transform component wrapper
    /// </summary>
    public class TransformComponent
    {
        private ulong entityID;

        public TransformComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        public Vector3 Position
        {
            get
            {
                InternalCalls.TransformComponent_GetPosition(entityID, out Vector3 position);
                return position;
            }
            set
            {
                InternalCalls.TransformComponent_SetPosition(entityID, ref value);
            }
        }

        public Vector3 Rotation
        {
            get
            {
                InternalCalls.TransformComponent_GetRotation(entityID, out Vector3 rotation);
                return rotation;
            }
            set
            {
                InternalCalls.TransformComponent_SetRotation(entityID, ref value);
            }
        }

        public Vector3 Scale
        {
            get
            {
                InternalCalls.TransformComponent_GetScale(entityID, out Vector3 scale);
                return scale;
            }
            set
            {
                InternalCalls.TransformComponent_SetScale(entityID, ref value);
            }
        }

        public void LookAt(Vector3 target)
        {
            Vector3 up = Vector3.Up;
            InternalCalls.TransformComponent_LookAt(entityID, ref target, ref up);
        }

        public void LookAt(Vector3 target, Vector3 up)
        {
            InternalCalls.TransformComponent_LookAt(entityID, ref target, ref up);
        }
    }

    /// <summary>
    /// RigidBody component wrapper
    /// </summary>
    public class RigidBodyComponent
    {
        private ulong entityID;

        public RigidBodyComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        public void ApplyForce(Vector3 force)
        {
            InternalCalls.RigidBodyComponent_ApplyForce(entityID, ref force);
        }

        public void ApplyForceAtPoint(Vector3 force, Vector3 point)
        {
            InternalCalls.RigidBodyComponent_ApplyForceAtPoint(entityID, ref force, ref point);
        }

        public Vector3 Velocity
        {
            get
            {
                InternalCalls.RigidBodyComponent_GetVelocity(entityID, out Vector3 velocity);
                return velocity;
            }
            set
            {
                InternalCalls.RigidBodyComponent_SetVelocity(entityID, ref value);
            }
        }

        public float Mass
        {
            get
            {
                return InternalCalls.RigidBodyComponent_GetMass(entityID);
            }
            set
            {
                InternalCalls.RigidBodyComponent_SetMass(entityID, value);
            }
        }
    }

    /// <summary>
    /// Tag component (minimal wrapper)
    /// </summary>
    public class TagComponent
    {
        private readonly ulong id;

        //  Parameterless constructor for the scripting runtime (reflection).
        //    This is only used when the engine scans component types.
        //    It will never be used by your normal GetComponent<TagComponent>() calls.
        public TagComponent()
        {
            id = 0; // dummy value – Tag will never be used on these instances
        }

        //  Real constructor used by Entity.GetComponent<TagComponent>()
        public TagComponent(ulong id)
        {
            this.id = id;
        }

        public string Tag
        {
            get { return InternalCalls.TagComponent_GetTag(id); }
        }
    }

    /// <summary>
    /// Collider component wrapper
    /// </summary>
    public class ColliderComponent
    {
        private readonly ulong entityID;

        public ColliderComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        /// <summary>
        /// Get the entity this collider belongs to
        /// </summary>
        public Entity Entity
        {
            get
            {
                Entity entity = new Entity();
                entity.ID = entityID;
                return entity;
            }
        }
    }

    /// <summary>
    /// Camera component (minimal wrapper)
    /// </summary>
    public class CameraComponent
    {
    }

    /// <summary>
    /// MeshFilter component (minimal wrapper)
    /// </summary>
    public class MeshFilterComponent
    {
    }

    /// <summary>
    /// MeshRenderer component (minimal wrapper)
    /// </summary>
    public class MeshRendererComponent
    {
        private readonly ulong entityID;

        // Needed for reflection; does nothing useful without a real entity ID.
        public MeshRendererComponent()
        {
            entityID = 0;
        }

        public MeshRendererComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        // Override per-entity color (tint). Returns false if component missing.
        public bool SetColor(Vector3 rgb)
        {
            return InternalCalls.MeshRendererComponent_SetColor(entityID, ref rgb);
        }

        // Clear color override and revert to material default.
        public void ClearColor()
        {
            InternalCalls.MeshRendererComponent_ClearColor(entityID);
        }
        public void SetVisible(bool visible)
        {
            InternalCalls.MeshRendererComponent_SetVisible(entityID, visible);
        }

    }

    /// <summary>
    /// SkinnedMeshRenderer component (minimal wrapper)
    /// </summary>
    public class SkinnedMeshRendererComponent
    {
        private readonly ulong entityID;
        public SkinnedMeshRendererComponent() { entityID = 0; } // for reflection
        public SkinnedMeshRendererComponent(ulong entityID) { this.entityID = entityID; }

        public bool SetColor(Vector3 rgb)
        {
            return InternalCalls.SkinnedMeshRendererComponent_SetColor(entityID, ref rgb);

        }
        public void ClearColor()
        {
            InternalCalls.SkinnedMeshRendererComponent_ClearColor(entityID);
        }

        public void SetVisible(bool visible)
        {
            InternalCalls.SkinnedMeshRendererComponent_SetVisible(entityID,visible);
        }
    }

    /// <summary>
    /// RectTransform component (minimal wrapper)
    /// </summary>
    public class RectTransformComponent
    {
        private readonly ulong entityID;
        public RectTransformComponent() { entityID = 0; } // for reflection
        public RectTransformComponent(ulong id) { entityID = id; }

        public Vector2 SizeDelta
        {
            get { InternalCalls.RectTransformComponent_GetSizeDelta(entityID, out var s); return s; }
            set { InternalCalls.RectTransformComponent_SetSizeDelta(entityID, ref value); }
        }

        public Vector2 AnchorMax
        {
            get { InternalCalls.RectTransformComponent_GetAnchorMax(entityID, out var v); return v; }
            set { InternalCalls.RectTransformComponent_SetAnchorMax(entityID, ref value); }
        }
        public Vector2 AnchorMin 
        { 
            get { InternalCalls.RectTransformComponent_GetAnchorMin(entityID, out var v); return v; } 
            set { InternalCalls.RectTransformComponent_SetAnchorMin(entityID, ref value); } 
        }
        public Vector2 Pivot
        {
            get { InternalCalls.RectTransformComponent_GetPivot(entityID, out var v); return v; }
            set { InternalCalls.RectTransformComponent_SetPivot(entityID, ref value); }
        }

        public Vector2 AnchoredPosition
        {
            get { InternalCalls.RectTransformComponent_GetAnchoredPosition(entityID, out var v); return v; }
            set { InternalCalls.RectTransformComponent_SetAnchoredPosition(entityID, ref value); }
        }
    }

    /// <summary>
    /// Script component (minimal wrapper)
    /// </summary>
    public class ScriptComponent
    {
    }

    /// <summary>
    /// Collision information passed to OnCollision callbacks
    /// </summary>
    public struct CollisionInfo
    {
        public Entity OtherEntity;
        // Add more collision data as needed (contact points, normals, etc.)
    }

    /// <summary>
    /// Information about UI pointer events (click, hover, etc.)
    /// Matches C++ Engine::UIPointerEventInfo struct layout exactly
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UIPointerEventInfo
    {
        /// <summary>
        /// The entity ID that received the event (matches entt::entity = uint32)
        /// </summary>
        public uint TargetEntityID;

        /// <summary>
        /// Screen X position of the pointer
        /// </summary>
        public float PointerX;

        /// <summary>
        /// Screen Y position of the pointer
        /// </summary>
        public float PointerY;

        /// <summary>
        /// X position relative to the UI element
        /// </summary>
        public float LocalX;

        /// <summary>
        /// Y position relative to the UI element
        /// </summary>
        public float LocalY;

        /// <summary>
        /// Mouse button (0=left, 1=right, 2=middle)
        /// </summary>
        public int Button;

        /// <summary>
        /// True if button is currently pressed (C++ bool = 1 byte)
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsPressed;

        /// <summary>
        /// Get pointer position as Vector2
        /// </summary>
        public Vector2 PointerPosition => new Vector2(PointerX, PointerY);

        /// <summary>
        /// Get local position as Vector2
        /// </summary>
        public Vector2 LocalPosition => new Vector2(LocalX, LocalY);
    }

    public class AudioComponent
    {
        private readonly ulong id;
        public AudioComponent(ulong id)
        {
            this.id = id;
        }

        public void Play(string clipPath)
        {
            InternalCalls.AudioComponent_PlaySound(id, clipPath);
        }

        public void Stop()
        {
            InternalCalls.AudioComponent_StopSound(id);
        }

        public void SetVolume(float volume)
        {
            InternalCalls.AudioComponent_SetVolume(id, volume);
        }

        public bool IsPlaying(string clipPath)
        {
            return InternalCalls.AudioComponent_IsSoundPlaying(id, clipPath);
        }

        // Multi-instance helpers (slot-based)
        public int AddInstance(string clipPath)
        {
            return InternalCalls.AudioComponent_AddInstance(id, clipPath);
        }

        public void RemoveInstance(int index)
        {
            InternalCalls.AudioComponent_RemoveInstance(id, index);
        }

        public void PlayInstance(int index)
        {
            InternalCalls.AudioComponent_PlayInstance(id, index);
        }

        public void StopInstance(int index)
        {
            InternalCalls.AudioComponent_StopInstance(id, index);
        }

        public void SetInstanceVolume(int index, float volume)
        {
            InternalCalls.AudioComponent_SetInstanceVolume(id, index, volume);
        }

        public void SetInstanceLoop(int index, bool loop)
        {
            InternalCalls.AudioComponent_SetInstanceLoop(id, index, loop);
        }

        public bool IsInstancePlaying(int index)
        {
            return InternalCalls.AudioComponent_IsInstancePlaying(id, index);
        }

        public static void SetMasterVolume(float volume)
        {
            InternalCalls.AudioSystem_SetMasterVolume(volume);
        }

        public static float GetMasterVolume()
        {
            return InternalCalls.AudioSystem_GetMasterVolume();
        }
    }

    public static class RenderSettings
    {
        public static void SetGamma(float gamma)
        {
            InternalCalls.RenderSystem_SetGamma(gamma);
        }

        public static float GetGamma()
        {
            return InternalCalls.RenderSystem_GetGamma();
        }
    }

    /// <summary>
    /// Animator component wrapper - controls skeletal animation playback
    /// </summary>
    public class AnimatorComponent
    {
        private readonly ulong entityID;

        public AnimatorComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        /// <summary>
        /// Plays an animation by name
        /// </summary>
        /// <param name="animationName">Name of the animation to play</param>
        public void Play(string animationName)
        {
            InternalCalls.AnimatorComponent_Play(entityID, animationName);
        }

        /// <summary>
        /// Stops the current animation and resets to time 0
        /// </summary>
        public void Stop()
        {
            InternalCalls.AnimatorComponent_Stop(entityID);
        }

        /// <summary>
        /// Pauses the current animation (preserves current time)
        /// </summary>
        public void Pause()
        {
            InternalCalls.AnimatorComponent_Pause(entityID);
        }

        /// <summary>
        /// Resumes the paused animation
        /// </summary>
        public void Resume()
        {
            InternalCalls.AnimatorComponent_Resume(entityID);
        }

        /// <summary>
        /// Whether the animation should loop when it reaches the end
        /// </summary>
        public bool Loop
        {
            get { return InternalCalls.AnimatorComponent_GetLoop(entityID); }
            set { InternalCalls.AnimatorComponent_SetLoop(entityID, value); }
        }

        /// <summary>
        /// Playback speed multiplier (1.0 = normal speed, 2.0 = double speed, etc.)
        /// </summary>
        public float Speed
        {
            get { return InternalCalls.AnimatorComponent_GetSpeed(entityID); }
            set { InternalCalls.AnimatorComponent_SetSpeed(entityID, value); }
        }

        /// <summary>
        /// Whether the animation should automatically start playing when the scene begins
        /// </summary>
        public bool AutoPlay
        {
            get { return InternalCalls.AnimatorComponent_GetAutoPlay(entityID); }
            set { InternalCalls.AnimatorComponent_SetAutoPlay(entityID, value); }
        }

        /// <summary>
        /// Checks if an animation is currently playing (not stopped or paused)
        /// </summary>
        public bool IsPlaying
        {
            get { return InternalCalls.AnimatorComponent_IsPlaying(entityID); }
        }

        /// <summary>
        /// Checks if the animation is currently paused
        /// </summary>
        public bool IsPaused
        {
            get { return InternalCalls.AnimatorComponent_IsPaused(entityID); }
        }

        /// <summary>
        /// Gets the current animation time in seconds
        /// </summary>
        public float CurrentTime
        {
            get { return InternalCalls.AnimatorComponent_GetCurrentTime(entityID); }
        }

        /// <summary>
        /// Gets the normalized animation time (0.0 to 1.0)
        /// </summary>
        public float NormalizedTime
        {
            get { return InternalCalls.AnimatorComponent_GetNormalizedTime(entityID); }
        }

        /// <summary>
        /// Gets the name of the currently playing animation
        /// </summary>
        public string CurrentAnimation
        {
            get { return InternalCalls.AnimatorComponent_GetCurrentAnimation(entityID); }
        }
    }

    public enum ParticleSystemSimulationSpace
    {
        Local = 0,
        World = 1
    }

    public enum ParticleSystemShapeType
    {
        Cone = 0,
        Sphere = 1,
        Box = 2
    }

    /// <summary>
    /// Unity-style particle system wrapper (main/emission/shape).
    /// </summary>
    public class ParticleSystem
    {
        private readonly ulong entityID;

        public ParticleSystem(ulong entityID)
        {
            this.entityID = entityID;
            main = new MainModule(entityID);
            emission = new EmissionModule(entityID);
            shape = new ShapeModule(entityID);
        }

        public MainModule main { get; }
        public EmissionModule emission { get; }
        public ShapeModule shape { get; }

        public void Play() => InternalCalls.ParticleSystem_Play(entityID);
        public void Stop() => InternalCalls.ParticleSystem_Stop(entityID);
        public void Clear() => InternalCalls.ParticleSystem_Clear(entityID);
        public void Emit(int count) => InternalCalls.ParticleSystem_Emit(entityID, count);

        public bool IsPlaying => InternalCalls.ParticleSystem_GetIsPlaying(entityID);
        public int ParticleCount => InternalCalls.ParticleSystem_GetParticleCount(entityID);

        public void SetMaterial(string guidOrAlias) => InternalCalls.ParticleSystem_SetMaterial(entityID, guidOrAlias);
        public void SetTexture(string guidOrAlias) => InternalCalls.ParticleSystem_SetTexture(entityID, guidOrAlias);
        public string MaterialGuid => InternalCalls.ParticleSystem_GetMaterial(entityID);
        public string TextureGuid => InternalCalls.ParticleSystem_GetTexture(entityID);

        public Vector3 OffsetPosition
        {
            get { InternalCalls.ParticleSystem_GetOffsetPosition(entityID, out Vector3 p); return p; }
            set { InternalCalls.ParticleSystem_SetOffsetPosition(entityID, ref value); }
        }

        public Vector3 OffsetRotation
        {
            get { InternalCalls.ParticleSystem_GetOffsetRotation(entityID, out Vector3 r); return r; }
            set { InternalCalls.ParticleSystem_SetOffsetRotation(entityID, ref value); }
        }

        public Vector3 OffsetScale
        {
            get { InternalCalls.ParticleSystem_GetOffsetScale(entityID, out Vector3 s); return s; }
            set { InternalCalls.ParticleSystem_SetOffsetScale(entityID, ref value); }
        }

        public bool OffsetWorldSpace
        {
            get { return InternalCalls.ParticleSystem_GetOffsetWorldSpace(entityID); }
            set { InternalCalls.ParticleSystem_SetOffsetWorldSpace(entityID, value); }
        }

        public class MainModule
        {
            private readonly ulong id;
            public MainModule(ulong id) { this.id = id; }

            public float duration
            {
                get => InternalCalls.ParticleSystem_Main_GetDuration(id);
                set => InternalCalls.ParticleSystem_Main_SetDuration(id, value);
            }

            public bool loop
            {
                get => InternalCalls.ParticleSystem_Main_GetLoop(id);
                set => InternalCalls.ParticleSystem_Main_SetLoop(id, value);
            }

            public float startDelay
            {
                get => InternalCalls.ParticleSystem_Main_GetStartDelay(id);
                set => InternalCalls.ParticleSystem_Main_SetStartDelay(id, value);
            }

            public float startLifetime
            {
                get => InternalCalls.ParticleSystem_Main_GetStartLifetime(id);
                set => InternalCalls.ParticleSystem_Main_SetStartLifetime(id, value);
            }

            public float startSpeed
            {
                get => InternalCalls.ParticleSystem_Main_GetStartSpeed(id);
                set => InternalCalls.ParticleSystem_Main_SetStartSpeed(id, value);
            }

            public float startSize
            {
                get => InternalCalls.ParticleSystem_Main_GetStartSize(id);
                set => InternalCalls.ParticleSystem_Main_SetStartSize(id, value);
            }

            public Vector4 startColor
            {
                get
                {
                    InternalCalls.ParticleSystem_Main_GetStartColor(id, out Vector4 color);
                    return color;
                }
                set => InternalCalls.ParticleSystem_Main_SetStartColor(id, ref value);
            }

            public float gravityModifier
            {
                get => InternalCalls.ParticleSystem_Main_GetGravityModifier(id);
                set => InternalCalls.ParticleSystem_Main_SetGravityModifier(id, value);
            }

            public ParticleSystemSimulationSpace simulationSpace
            {
                get => (ParticleSystemSimulationSpace)InternalCalls.ParticleSystem_Main_GetSimulationSpace(id);
                set => InternalCalls.ParticleSystem_Main_SetSimulationSpace(id, (int)value);
            }

            public int maxParticles
            {
                get => InternalCalls.ParticleSystem_Main_GetMaxParticles(id);
                set => InternalCalls.ParticleSystem_Main_SetMaxParticles(id, value);
            }

            public bool playOnAwake
            {
                get => InternalCalls.ParticleSystem_Main_GetPlayOnAwake(id);
                set => InternalCalls.ParticleSystem_Main_SetPlayOnAwake(id, value);
            }
        }

        public class EmissionModule
        {
            private readonly ulong id;
            public EmissionModule(ulong id) { this.id = id; }

            public bool enabled
            {
                get => InternalCalls.ParticleSystem_Emission_GetEnabled(id);
                set => InternalCalls.ParticleSystem_Emission_SetEnabled(id, value);
            }

            public float rateOverTime
            {
                get => InternalCalls.ParticleSystem_Emission_GetRateOverTime(id);
                set => InternalCalls.ParticleSystem_Emission_SetRateOverTime(id, value);
            }
        }

        public class ShapeModule
        {
            private readonly ulong id;
            public ShapeModule(ulong id) { this.id = id; }

            public ParticleSystemShapeType shapeType
            {
                get => (ParticleSystemShapeType)InternalCalls.ParticleSystem_Shape_GetShapeType(id);
                set => InternalCalls.ParticleSystem_Shape_SetShapeType(id, (int)value);
            }

            public float angle
            {
                get => InternalCalls.ParticleSystem_Shape_GetAngle(id);
                set => InternalCalls.ParticleSystem_Shape_SetAngle(id, value);
            }

            public float radius
            {
                get => InternalCalls.ParticleSystem_Shape_GetRadius(id);
                set => InternalCalls.ParticleSystem_Shape_SetRadius(id, value);
            }

            public Vector3 boxSize
            {
                get
                {
                    InternalCalls.ParticleSystem_Shape_GetBoxSize(id, out Vector3 size);
                    return size;
                }
                set => InternalCalls.ParticleSystem_Shape_SetBoxSize(id, ref value);
            }
        }
    }

    /// <summary>
    /// UIElement component wrapper for controlling UI visibility and state
    /// </summary>
    public class UIElementComponent
    {
        private readonly ulong id;

        public UIElementComponent(ulong id)
        {
            this.id = id;
        }

        public bool Visible
        {
            get { return InternalCalls.UIElementComponent_GetVisible(id); }
            set { InternalCalls.UIElementComponent_SetVisible(id, value); }
        }

        public bool Active
        {
            get { return InternalCalls.UIElementComponent_GetActive(id); }
            set { InternalCalls.UIElementComponent_SetActive(id, value); }
        }

        public int LayerID
        {
            get { return InternalCalls.UIElementComponent_GetLayerID(id); }
            set { InternalCalls.UIElementComponent_SetLayerID(id, value); }
        }
    }

    /// <summary>
    /// UIImage component wrapper for rendering textures/sprites
    /// </summary>
    public class UIImageComponent
    {
        private readonly ulong entityID;

        public UIImageComponent() { entityID = 0; } // for reflection
        public UIImageComponent(ulong id) { entityID = id; }

        /// <summary>
        /// Gets or sets the texture GUID for this UI image
        /// </summary>
        public ulong TextureGuid
        {
            get
            {
                InternalCalls.UIImageComponent_GetTextureGuid(entityID, out var guid);
                return guid;
            }
            set
            {
                InternalCalls.UIImageComponent_SetTextureGuid(entityID, value);
            }
        }

        /// <summary>
        /// Sets the texture by asset name (e.g., "PhoneScreen_1")
        /// Searches through all textures to find the one with matching name
        /// </summary>
        /// <param name="textureName">The name of the texture asset</param>
        /// <returns>True if texture was found and set, false otherwise</returns>
        public bool SetTextureByName(string textureName)
        {
            return InternalCalls.UIImageComponent_SetTextureByName(entityID, textureName);
        }
    }

    /// <summary>
    /// UIText component wrapper for rendering text with fonts
    /// </summary>
    public class UITextComponent
    {
        private readonly ulong entityID;

        public UITextComponent() { entityID = 0; } // for reflection
        public UITextComponent(ulong id) { entityID = id; }

        /// <summary>
        /// Gets or sets the text content
        /// </summary>
        public string Text
        {
            get { return InternalCalls.UITextComponent_GetText(entityID); }
            set { InternalCalls.UITextComponent_SetText(entityID, value); }
        }

        /// <summary>
        /// Gets or sets the font size
        /// </summary>
        public float FontSize
        {
            get { return InternalCalls.UITextComponent_GetFontSize(entityID); }
            set { InternalCalls.UITextComponent_SetFontSize(entityID, value); }
        }

        /// <summary>
        /// Gets or sets the text color (RGBA)
        /// </summary>
        public Vector4 Color
        {
            get
            {
                InternalCalls.UITextComponent_GetColor(entityID, out var color);
                return color;
            }
            set
            {
                InternalCalls.UITextComponent_SetColor(entityID, ref value);
            }
        }

        /// <summary>
        /// Gets or sets the font GUID
        /// </summary>
        public ulong FontGuid
        {
            get
            {
                InternalCalls.UITextComponent_GetFontGuid(entityID, out var guid);
                return guid;
            }
            set
            {
                InternalCalls.UITextComponent_SetFontGuid(entityID, value);
            }
        }

        /// <summary>
        /// Sets the font by asset name
        /// Searches through all fonts to find the one with matching name
        /// </summary>
        /// <param name="fontName">The name of the font asset</param>
        /// <returns>True if font was found and set, false otherwise</returns>
        public bool SetFontByName(string fontName)
        {
            return InternalCalls.UITextComponent_SetFontByName(entityID, fontName);
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment
        /// </summary>
        public UITextAlignment Alignment
        {
            get { return (UITextAlignment)InternalCalls.UITextComponent_GetAlignment(entityID); }
            set { InternalCalls.UITextComponent_SetAlignment(entityID, (int)value); }
        }

        /// <summary>
        /// Gets or sets the vertical text alignment
        /// </summary>
        public UITextVerticalAlignment VerticalAlignment
        {
            get { return (UITextVerticalAlignment)InternalCalls.UITextComponent_GetVerticalAlignment(entityID); }
            set { InternalCalls.UITextComponent_SetVerticalAlignment(entityID, (int)value); }
        }

        /// <summary>
        /// Gets or sets the line spacing multiplier (1.0 = normal)
        /// </summary>
        public float LineSpacing
        {
            get { return InternalCalls.UITextComponent_GetLineSpacing(entityID); }
            set { InternalCalls.UITextComponent_SetLineSpacing(entityID, value); }
        }

        /// <summary>
        /// Gets or sets the character spacing multiplier (1.0 = normal)
        /// </summary>
        public float CharacterSpacing
        {
            get { return InternalCalls.UITextComponent_GetCharacterSpacing(entityID); }
            set { InternalCalls.UITextComponent_SetCharacterSpacing(entityID, value); }
        }

        /// <summary>
        /// Gets or sets whether word wrapping is enabled
        /// </summary>
        public bool WordWrap
        {
            get { return InternalCalls.UITextComponent_GetWordWrap(entityID); }
            set { InternalCalls.UITextComponent_SetWordWrap(entityID, value); }
        }

        /// <summary>
        /// Gets or sets the text overflow mode
        /// </summary>
        public UITextOverflow Overflow
        {
            get { return (UITextOverflow)InternalCalls.UITextComponent_GetOverflow(entityID); }
            set { InternalCalls.UITextComponent_SetOverflow(entityID, (int)value); }
        }

        /// <summary>
        /// Gets or sets whether the text component is enabled
        /// </summary>
        public bool Enabled
        {
            get { return InternalCalls.UITextComponent_GetEnabled(entityID); }
            set { InternalCalls.UITextComponent_SetEnabled(entityID, value); }
        }
    }

    /// <summary>
    /// VideoPlayer component wrapper — controls MPEG video playback.
    /// Mirrors the C++ VideoPlayerComponent / VideoSystem API.
    /// </summary>
    public class VideoPlayerComponent
    {
        private readonly ulong entityID;

        public VideoPlayerComponent() { entityID = 0; } // for reflection
        public VideoPlayerComponent(ulong entityID) { this.entityID = entityID; }

        /// <summary>Start or resume playback.</summary>
        public void Play()
        {
            InternalCalls.VideoPlayerComponent_Play(entityID);
        }

        /// <summary>Pause playback (preserves current frame and audio position).</summary>
        public void Pause()
        {
            InternalCalls.VideoPlayerComponent_Pause(entityID);
        }

        /// <summary>Stop and fully unload the video.</summary>
        public void Stop()
        {
            InternalCalls.VideoPlayerComponent_Stop(entityID);
        }

        /// <summary>True while the video is actively decoding/rendering.</summary>
        public bool IsPlaying => InternalCalls.VideoPlayerComponent_IsPlaying(entityID);

        /// <summary>True while paused mid-playback.</summary>
        public bool IsPaused => InternalCalls.VideoPlayerComponent_IsPaused(entityID);

        /// <summary>
        /// True once the video reaches its natural end (non-looping).
        /// Resets to false when Play() is called again.
        /// Use this in Update() to detect completion and trigger the next action.
        /// </summary>
        public bool IsFinished => InternalCalls.VideoPlayerComponent_IsFinished(entityID);

        /// <summary>Current playback position in seconds.</summary>
        public float CurrentTime => InternalCalls.VideoPlayerComponent_GetCurrentTime(entityID);

        /// <summary>Total video duration in seconds.</summary>
        public float Duration => InternalCalls.VideoPlayerComponent_GetDuration(entityID);

        /// <summary>Swap the video file (stops current playback, assigns new GUID). Call Play() after.</summary>
        public void SetVideoGUID(ulong guid) =>
            InternalCalls.VideoPlayerComponent_SetVideoGUID(entityID, guid);

        /// <summary>Set this video's base volume (0–1). Master volume is applied on top.</summary>
        public void SetVolume(float volume) =>
            InternalCalls.VideoPlayerComponent_SetVolume(entityID, volume);
    }

    /// <summary>
    /// TextMeshPro component wrapper for world-space 3D text
    /// </summary>
    public class TextMeshProComponent
    {
        private readonly ulong entityID;

        public TextMeshProComponent() { entityID = 0; }
        public TextMeshProComponent(ulong id) { entityID = id; }

        public bool Enabled
        {
            get { return InternalCalls.TextMeshProComponent_GetEnabled(entityID); }
            set { InternalCalls.TextMeshProComponent_SetEnabled(entityID, value); }
        }

        public string Text
        {
            get { return InternalCalls.TextMeshProComponent_GetText(entityID); }
            set { InternalCalls.TextMeshProComponent_SetText(entityID, value); }
        }
    }

    /// <summary>
    /// Enemy Field-of-View component wrapper
    /// </summary>
    public class EnemyFOVComponent
    {
        private readonly ulong entityID;

        public EnemyFOVComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        /// <summary>
        /// Ensures the C++ EnemyFOVComponent exists on this entity
        /// </summary>
        public void EnsureComponent()
        {
            InternalCalls.EnemyFOV_EnsureComponent(entityID);
        }

        /// <summary>
        /// Checks if the enemy has line-of-sight to a target
        /// </summary>
        public bool HasLineOfSight
        {
            get { return InternalCalls.EnemyFOV_HasLineOfSight(entityID); }
        }

        /// <summary>
        /// Gets the name/tag of the current target entity
        /// </summary>
        public string CurrentTargetName
        {
            get { return InternalCalls.EnemyFOV_GetCurrentTargetName(entityID); }
        }

        /// <summary>
        /// Gets all visible entity IDs
        /// </summary>
        public ulong[] GetVisibleEntities()
        {
            return InternalCalls.EnemyFOV_GetVisibleEntities(entityID);
        }
    }

    /// <summary>
    /// Disguise component wrapper for enemy disguise mechanics
    /// </summary>
    public class DisguiseComponent
    {
        private readonly ulong entityID;

        public DisguiseComponent(ulong entityID)
        {
            this.entityID = entityID;
        }

        /// <summary>
        /// Apply disguise from a source entity to this entity
        /// </summary>
        /// <param name="sourceEntityID">The entity to copy appearance from</param>
        public void ApplyFrom(ulong sourceEntityID)
        {
            InternalCalls.Disguise_ApplyFromTo(sourceEntityID, entityID);
        }

        /// <summary>
        /// Revert this entity back to its original appearance
        /// </summary>
        public void Revert()
        {
            InternalCalls.Disguise_Revert(entityID);
        }
    }
}
