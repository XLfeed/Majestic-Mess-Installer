namespace Engine
{
    /// <summary>
    /// Scene management
    /// </summary>
    public static class Scene
    {
        /// <summary>
        /// Load a scene by name
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            InternalCalls.Scene_LoadScene(sceneName);
        }

        /// <summary>
        /// Load a scene by its asset GUID (rename-safe)
        /// </summary>
        public static void LoadSceneByGUID(ulong guid)
        {
            InternalCalls.Scene_LoadSceneByGUID(guid);
        }

        /// <summary>
        /// Destroy an entity from the scene
        /// </summary>
        /// <param name="entityID">The ID of the entity to destroy</param>
        public static void DestroyEntity(ulong entityID)
        {
            InternalCalls.Scene_DestroyEntity(entityID);
        }

        /// <summary>
        /// Push a new scene on top of the current one.
        /// The current scene's entity state is serialised to disk so it can be
        /// restored when PopScene() is called.  Use this to overlay a cutscene
        /// on top of an in-progress gameplay level.
        /// NOTE: avoid calling this in the middle of physics/update — prefer
        /// triggering it from an input-event or end-of-frame callback.
        /// </summary>
        public static void PushScene(string sceneName)
        {
            InternalCalls.Scene_PushScene(sceneName);
        }

        /// <summary>
        /// Pop the top scene and restore the previously pushed scene.
        /// Entity state is read back from the snapshot saved by PushScene().
        /// </summary>
        public static void PopScene()
        {
            InternalCalls.Scene_PopScene();
        }
    }
}
