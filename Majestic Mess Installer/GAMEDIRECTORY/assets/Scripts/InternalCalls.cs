using System;
using System.Runtime.CompilerServices;

namespace Engine
{
    /// <summary>
    /// Internal calls to C++ engine functions
    /// These are implemented in ScriptGlue.cpp
    /// </summary>
    public static class InternalCalls
    {
        // Entity
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Entity_HasComponent(ulong entityID, Type componentType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static ulong Entity_FindEntityByName(string name);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static string Entity_GetName(ulong entityID);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Entity_IsValid(ulong entityID);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static ulong Entity_FindEntityWithScript(Type scriptType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static ulong[] Entity_FindEntitiesWithScript(Type scriptType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static object Entity_GetScriptInstance(ulong entityID, Type scriptType);

        // Transform Component
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_GetPosition(ulong entityID, out Vector3 position);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_SetPosition(ulong entityID, ref Vector3 position);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_GetRotation(ulong entityID, out Vector3 rotation);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_SetRotation(ulong entityID, ref Vector3 rotation);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_GetScale(ulong entityID, out Vector3 scale);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_SetScale(ulong entityID, ref Vector3 scale);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void TransformComponent_LookAt(ulong entityID, ref Vector3 target, ref Vector3 up);

        // RigidBody Component
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void RigidBodyComponent_ApplyForce(ulong entityID, ref Vector3 force);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void RigidBodyComponent_ApplyForceAtPoint(ulong entityID, ref Vector3 force, ref Vector3 point);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void RigidBodyComponent_GetVelocity(ulong entityID, out Vector3 velocity);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void RigidBodyComponent_SetVelocity(ulong entityID, ref Vector3 velocity);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static float RigidBodyComponent_GetMass(ulong entityID);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void RigidBodyComponent_SetMass(ulong entityID, float mass);

        // Input
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsKeyPressed(KeyCode key);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsKeyHeld(KeyCode key);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsKeyReleased(KeyCode key);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsMouseButtonPressed(int button);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsMouseButtonHeld(int button);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool Input_IsMouseButtonReleased(int button);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Input_GetMousePosition(out Vector2 position);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Input_GetMouseDelta(out Vector2 delta);

        // Scene
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Scene_LoadScene(string sceneName);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Scene_LoadSceneByGUID(ulong guid);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static int Scene_GetActiveSceneType();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static string Scene_GetActiveSceneName();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Window_SetShouldClose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Window_GetSize(out Vector2 size);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void GameView_GetPosition(out Vector2 position);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void GameView_GetSize(out Vector2 size);

        // Prefab
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static ulong Prefab_Instantiate(string name, ulong parentID);

        // VideoPlayer
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void VideoPlayerComponent_Play(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void VideoPlayerComponent_Pause(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void VideoPlayerComponent_Stop(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool VideoPlayerComponent_IsPlaying(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool VideoPlayerComponent_IsPaused(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool VideoPlayerComponent_IsFinished(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float VideoPlayerComponent_GetCurrentTime(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float VideoPlayerComponent_GetDuration(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void VideoPlayerComponent_SetVideoGUID(ulong entityID, ulong guid);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void VideoPlayerComponent_SetVolume(ulong entityID, float volume);

        // Scene Stack
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void Scene_PushScene(string sceneName);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void Scene_PopScene();

        // Time
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static float Time_GetDeltaTime();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static float Time_GetTime();

        // Debug
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void Debug_Log(string message);

        // Tag
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static string TagComponent_GetTag(ulong entityID);

        // Scroll of cinder
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void Scene_DestroyEntity(ulong id);

        // Disguise
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void Disguise_ApplyFromTo(ulong sourceEntityID, ulong targetEntityID);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void Disguise_Revert(ulong targetEntityID);

        // NavMesh
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern Vector3[] NavMesh_FindPath(Vector3 startPos, Vector3 goalPos);

        // Audio
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_PlaySound(ulong entityID, string soundName);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_StopSound(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AudioComponent_IsSoundPlaying(ulong entityID, string soundName);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_SetVolume(ulong entityID, float volume);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int AudioComponent_AddInstance(ulong entityID, string soundName);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_RemoveInstance(ulong entityID, int index);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_PlayInstance(ulong entityID, int index);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_StopInstance(ulong entityID, int index);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_SetInstanceVolume(ulong entityID, int index, float volume);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioComponent_SetInstanceLoop(ulong entityID, int index, bool loop);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AudioComponent_IsInstancePlaying(ulong entityID, int index);

        // Global Audio (Entity-independent 2D audio)
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern ulong GlobalAudio_Play2D(string clipPath, float volume, bool loop);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_Stop(ulong audioID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_StopAll();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_SetVolume(ulong audioID, float volume);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_Pause(ulong audioID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_Resume(ulong audioID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool GlobalAudio_IsPlaying(ulong audioID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GlobalAudio_SetLoop(ulong audioID, bool loop);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AudioSystem_SetMasterVolume(float volume);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float AudioSystem_GetMasterVolume();

        // Render Settings (Gamma)
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void RenderSystem_SetGamma(float gamma);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float RenderSystem_GetGamma();

        // Animator
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_Play(ulong entityID, string animationName);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_Stop(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_Pause(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_Resume(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AnimatorComponent_GetLoop(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_SetLoop(ulong entityID, bool loop);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float AnimatorComponent_GetSpeed(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_SetSpeed(ulong entityID, float speed);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AnimatorComponent_GetAutoPlay(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void AnimatorComponent_SetAutoPlay(ulong entityID, bool autoPlay);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AnimatorComponent_IsPlaying(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AnimatorComponent_IsPaused(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float AnimatorComponent_GetCurrentTime(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float AnimatorComponent_GetNormalizedTime(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern string AnimatorComponent_GetCurrentAnimation(ulong entityID);

        // ParticleSystem
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Play(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Stop(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Clear(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Emit(ulong entityID, int count);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ParticleSystem_GetIsPlaying(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ParticleSystem_GetParticleCount(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetMaterial(ulong entityID, string guidOrAlias);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetTexture(ulong entityID, string guidOrAlias);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern string ParticleSystem_GetMaterial(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern string ParticleSystem_GetTexture(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_GetOffsetPosition(ulong entityID, out Vector3 position);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetOffsetPosition(ulong entityID, ref Vector3 position);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_GetOffsetRotation(ulong entityID, out Vector3 rotation);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetOffsetRotation(ulong entityID, ref Vector3 rotation);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_GetOffsetScale(ulong entityID, out Vector3 scale);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetOffsetScale(ulong entityID, ref Vector3 scale);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ParticleSystem_GetOffsetWorldSpace(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_SetOffsetWorldSpace(ulong entityID, bool value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetDuration(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetDuration(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ParticleSystem_Main_GetLoop(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetLoop(ulong entityID, bool value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetStartDelay(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetStartDelay(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetStartLifetime(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetStartLifetime(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetStartSpeed(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetStartSpeed(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetStartSize(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetStartSize(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_GetStartColor(ulong entityID, out Vector4 color);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetStartColor(ulong entityID, ref Vector4 color);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Main_GetGravityModifier(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetGravityModifier(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ParticleSystem_Main_GetSimulationSpace(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetSimulationSpace(ulong entityID, int value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ParticleSystem_Main_GetMaxParticles(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetMaxParticles(ulong entityID, int value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ParticleSystem_Main_GetPlayOnAwake(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Main_SetPlayOnAwake(ulong entityID, bool value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ParticleSystem_Emission_GetEnabled(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Emission_SetEnabled(ulong entityID, bool value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Emission_GetRateOverTime(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Emission_SetRateOverTime(ulong entityID, float value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ParticleSystem_Shape_GetShapeType(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Shape_SetShapeType(ulong entityID, int value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Shape_GetAngle(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Shape_SetAngle(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float ParticleSystem_Shape_GetRadius(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Shape_SetRadius(ulong entityID, float value);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Shape_GetBoxSize(ulong entityID, out Vector3 size);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParticleSystem_Shape_SetBoxSize(ulong entityID, ref Vector3 size);

        // UIElement Component
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UIElementComponent_GetVisible(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void UIElementComponent_SetVisible(ulong entityID, bool visible);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UIElementComponent_GetActive(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void UIElementComponent_SetActive(ulong entityID, bool active);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int UIElementComponent_GetLayerID(ulong entityID);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void UIElementComponent_SetLayerID(ulong entityID, int layerID);

        //EnemyFOV
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void EnemyFOV_EnsureComponent(ulong entityID);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool EnemyFOV_HasLineOfSight(ulong entityID);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string EnemyFOV_GetCurrentTargetName(ulong entityID);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern static ulong[] EnemyFOV_GetVisibleEntities(ulong entityID);

        // MeshRenderer Component
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool MeshRendererComponent_SetColor(ulong entityID, ref Vector3 color);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void MeshRendererComponent_ClearColor(ulong entityID);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void MeshRendererComponent_SetVisible(ulong entityID, bool visible);
        // SkinnedMeshRenderer Component
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool SkinnedMeshRendererComponent_SetColor(ulong entityID, ref Vector3 color);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void SkinnedMeshRendererComponent_ClearColor(ulong entityID);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void SkinnedMeshRendererComponent_SetVisible(ulong entityID, bool visible);
        // RectTransform
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_GetSizeDelta(ulong entityID, out Vector2 size);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_SetSizeDelta(ulong entityID, ref Vector2 size);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_GetAnchorMax(ulong id, out Vector2 anchorMax);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_SetAnchorMax(ulong id, ref Vector2 anchorMax);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_GetAnchorMin(ulong id, out Vector2 anchorMin);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_SetAnchorMin(ulong id, ref Vector2 anchorMin);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_GetPivot(ulong id, out Vector2 pivot);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_SetPivot(ulong id, ref Vector2 pivot);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_GetAnchoredPosition(ulong id, out Vector2 position);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool RectTransformComponent_SetAnchoredPosition(ulong id, ref Vector2 position);

        // UIImageComponent
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UIImageComponent_GetTextureGuid(ulong id, out ulong guid);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UIImageComponent_SetTextureGuid(ulong id, ulong guid);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UIImageComponent_SetTextureByName(ulong id, string name);

        // UITextComponent
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern string UITextComponent_GetText(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetText(ulong id, string text);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float UITextComponent_GetFontSize(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetFontSize(ulong id, float size);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_GetColor(ulong id, out Vector4 color);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetColor(ulong id, ref Vector4 color);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_GetFontGuid(ulong id, out ulong guid);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetFontGuid(ulong id, ulong guid);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetFontByName(ulong id, string name);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int UITextComponent_GetAlignment(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetAlignment(ulong id, int alignment);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int UITextComponent_GetVerticalAlignment(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetVerticalAlignment(ulong id, int alignment);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float UITextComponent_GetLineSpacing(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetLineSpacing(ulong id, float spacing);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern float UITextComponent_GetCharacterSpacing(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetCharacterSpacing(ulong id, float spacing);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_GetWordWrap(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetWordWrap(ulong id, bool wrap);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int UITextComponent_GetOverflow(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetOverflow(ulong id, int overflow);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_GetEnabled(ulong id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool UITextComponent_SetEnabled(ulong id, bool enabled);
    }
}
