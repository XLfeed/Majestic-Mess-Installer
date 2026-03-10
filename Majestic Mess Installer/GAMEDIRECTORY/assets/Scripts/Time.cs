namespace Engine
{
    /// <summary>
    /// Time management utilities
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Get time elapsed since last frame (delta time)
        /// </summary>
        public static float GetDeltaTime()
        {
            return InternalCalls.Time_GetDeltaTime();
        }

        /// <summary>
        /// Get total time since application start
        /// </summary>
        public static float GetTime()
        {
            return InternalCalls.Time_GetTime();
        }
    }
}
