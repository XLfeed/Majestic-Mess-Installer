namespace Engine
{
    /// <summary>
    /// Debug logging utility
    /// </summary>
    public static class Debug
    {
        public static void Log(string message)
        {
            InternalCalls.Debug_Log(message);
        }
    }
}
