using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Base class for all entities in the game
    /// All C# scripts must inherit from this class
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Unique ID for this entity instance
        /// Set by the ScriptEngine when the entity is created
        /// </summary>
        public ulong ID { get; internal set; }

        private TransformComponent _transform;
        private RigidBodyComponent _rigidBody;
        private AnimatorComponent _animator;

        /// <summary>
        /// Parameterless constructor for derived classes
        /// </summary>
        public Entity()
        {
        }

        /// <summary>
        /// Constructor - called by ScriptEngine with entity ID
        /// </summary>
        public Entity(ulong entityID)
        {
            ID = entityID;
        }

        /// <summary>
        /// Entity name
        /// </summary>
        public string Name
        {
            get
            {
                return InternalCalls.Entity_GetName(ID);
            }
        }

        /// <summary>
        /// Transform component accessor
        /// </summary>
        public TransformComponent Transform
        {
            get
            {
                if (_transform == null)
                    _transform = new TransformComponent(ID);
                return _transform;
            }
        }

        /// <summary>
        /// RigidBody component accessor (if it exists)
        /// </summary>
        public RigidBodyComponent RigidBody
        {
            get
            {
                if (_rigidBody == null)
                    _rigidBody = new RigidBodyComponent(ID);
                return _rigidBody;
            }
        }

        /// <summary>
        /// Animator component accessor (if it exists)
        /// </summary>
        public AnimatorComponent Animator
        {
            get
            {
                if (_animator == null)
                    _animator = new AnimatorComponent(ID);
                return _animator;
            }
        }

        /// <summary>
        /// Check if this entity has a specific component
        /// </summary>
        public bool HasComponent<T>() where T : class
        {
            return InternalCalls.Entity_HasComponent(ID, typeof(T));
        }

        /// <summary>
        /// Get a component from this entity
        /// </summary>
        public T GetComponent<T>() where T : class
        {
            if (!HasComponent<T>())
                return null;

            // For known component types, return wrappers
            if (typeof(T) == typeof(TransformComponent))
                return Transform as T;

            if (typeof(T) == typeof(RigidBodyComponent))
                return RigidBody as T;

            if (typeof(T) == typeof(TagComponent))
                return new TagComponent(ID) as T;

            if (typeof(T) == typeof(UIElementComponent))
                return new UIElementComponent(ID) as T;

            if (typeof(T) == typeof(AudioComponent))
                return new AudioComponent(ID) as T;

            if (typeof(T) == typeof(ColliderComponent))
                return new ColliderComponent(ID) as T;
                
            if (typeof(T) == typeof(AnimatorComponent))
                return Animator as T;

            if (typeof(T) == typeof(MeshRendererComponent))
                return new MeshRendererComponent(ID) as T;

            if (typeof(T) == typeof(RectTransformComponent))
                return new RectTransformComponent(ID) as T;

            if (typeof(T) == typeof(SkinnedMeshRendererComponent))
                return new SkinnedMeshRendererComponent(ID) as T;

            if (typeof(T) == typeof(ParticleSystem))
                return new ParticleSystem(ID) as T;

            if (typeof(T) == typeof(VideoPlayerComponent))
                return new VideoPlayerComponent(ID) as T;

            // For other components, return a minimal instance
            // (Full component reflection would require more complex marshalling)
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Get a managed script instance attached to this entity.
        /// </summary>
        public T GetScript<T>() where T : Entity
        {
            object scriptInstance = InternalCalls.Entity_GetScriptInstance(ID, typeof(T));
            return scriptInstance as T;
        }

        /// <summary>
        /// Helper to find an entity by name and fetch one of its scripts.
        /// </summary>
        public static T FindScriptByName<T>(string name) where T : Entity
        {
	            Entity entity = FindEntityByName(name);
	            if (entity == null)
	                return null;
	            return entity.GetScript<T>();
	        }

        /// <summary>
        /// Find the first script instance of type T across all entities.
        /// </summary>
        public static T FindScript<T>() where T : Entity
        {
            ulong entityID = InternalCalls.Entity_FindEntityWithScript(typeof(T));
            if (entityID == 0)
                return null;

            return InternalCalls.Entity_GetScriptInstance(entityID, typeof(T)) as T;
        }

        /// <summary>
        /// Find all script instances of type T across the scene.
        /// </summary>
        public static List<T> FindScripts<T>() where T : Entity
        {
            ulong[] entityIDs = InternalCalls.Entity_FindEntitiesWithScript(typeof(T));
            List<T> scripts = new List<T>();
            if (entityIDs == null)
                return scripts;

            foreach (ulong id in entityIDs)
            {
                var script = InternalCalls.Entity_GetScriptInstance(id, typeof(T)) as T;
                if (script != null)
                    scripts.Add(script);
            }

            return scripts;
        }

        /// <summary>
        /// Find an entity by name
        /// </summary>
        public static Entity FindEntityByName(string name)
        {
            ulong entityID = InternalCalls.Entity_FindEntityByName(name);
            if (entityID == 0)
                return null;

            Entity entity = new Entity();
            entity.ID = entityID;
            return entity;
        }

        /// <summary>
        /// Check if this entity is still valid
        /// </summary>
        public bool IsValid()
        {
            return InternalCalls.Entity_IsValid(ID);
        }

        // ===== LIFECYCLE METHODS =====
        // Override these in your derived script classes

        /// <summary>
        /// Compare this entity's tag with the given tag string
        /// </summary>
        /// <param name="tag">The tag to compare against</param>
        /// <returns>True if the entity's tag matches</returns>
        public bool CompareTag(string tag)
        {
            if (!HasComponent<TagComponent>())
                return false;

            var tagComponent = GetComponent<TagComponent>();
            return tagComponent.Tag == tag;
        }

        /// <summary>
        /// Called when the entity is created (scene start)
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Called every frame
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds</param>
        public virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// Called when the entity is destroyed (scene end)
        /// </summary>
        public virtual void OnExit() { }

        // ===== PHYSICS CALLBACKS =====

        /// <summary>
        /// Called when collision starts with another entity
        /// </summary>
        public virtual void OnCollisionEnter(CollisionInfo collision) { }

        /// <summary>
        /// Called every frame while colliding with another entity
        /// </summary>
        public virtual void OnCollisionStay(CollisionInfo collision) { }

        /// <summary>
        /// Called when collision ends with another entity
        /// </summary>
        public virtual void OnCollisionExit(CollisionInfo collision) { }

        /// <summary>
        /// Called when entering a trigger volume
        /// </summary>
        public virtual void OnTriggerEnter(ColliderComponent collider) { }

        /// <summary>
        /// Called every frame while inside a trigger volume
        /// </summary>
        public virtual void OnTriggerStay(ColliderComponent collider) { }

        /// <summary>
        /// Called when exiting a trigger volume
        /// </summary>
        public virtual void OnTriggerExit(ColliderComponent collider) { }

        // ===== UI EVENT CALLBACKS =====

        /// <summary>
        /// Called when pointer enters this UI element
        /// </summary>
        public virtual void OnUIHoverEnter(UIPointerEventInfo eventInfo) { }

        /// <summary>
        /// Called when pointer exits this UI element
        /// </summary>
        public virtual void OnUIHoverExit(UIPointerEventInfo eventInfo) { }

        /// <summary>
        /// Called when pointer is pressed on this UI element
        /// </summary>
        public virtual void OnUIPointerDown(UIPointerEventInfo eventInfo) { }

        /// <summary>
        /// Called when pointer is released on this UI element
        /// </summary>
        public virtual void OnUIPointerUp(UIPointerEventInfo eventInfo) { }

        /// <summary>
        /// Called when a click completes on this UI element
        /// </summary>
        public virtual void OnUIClick(UIPointerEventInfo eventInfo) { }

        // ===== INTERNAL CPP CALLBACK WRAPPERS =====
        // Called from C++ engine - forwards to virtual methods

        internal void OnUIHoverEnterCPP(UIPointerEventInfo eventInfo) { OnUIHoverEnter(eventInfo); }
        internal void OnUIHoverExitCPP(UIPointerEventInfo eventInfo) { OnUIHoverExit(eventInfo); }
        internal void OnUIPointerDownCPP(UIPointerEventInfo eventInfo) { OnUIPointerDown(eventInfo); }
        internal void OnUIPointerUpCPP(UIPointerEventInfo eventInfo) { OnUIPointerUp(eventInfo); }
        internal void OnUIClickCPP(UIPointerEventInfo eventInfo) { OnUIClick(eventInfo); }
    }
}
