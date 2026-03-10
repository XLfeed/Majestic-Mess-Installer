using System;

namespace Engine
{
    /// <summary>
    /// Static class for playing 2D audio from scripts without requiring AudioComponent.
    /// Audio played through this class is entity-independent and perfect for UI sounds,
    /// music, and global sound effects.
    /// </summary>
    public static class Audio
    {
        /// <summary>
        /// Plays a 2D sound effect. Returns a handle that can be used to control playback.
        /// </summary>
        /// <param name="clipPath">Path to the audio file (relative to assets folder)</param>
        /// <param name="volume">Volume level (0.0 to 1.0, default 1.0)</param>
        /// <param name="loop">Whether the sound should loop (default false)</param>
        /// <returns>Audio handle ID for controlling playback, or 0 if failed</returns>
        public static ulong Play2D(string clipPath, float volume = 1.0f, bool loop = false)
        {
            return InternalCalls.GlobalAudio_Play2D(clipPath, volume, loop);
        }

        /// <summary>
        /// Stops a playing sound by its handle.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        public static void Stop(ulong audioID)
        {
            InternalCalls.GlobalAudio_Stop(audioID);
        }

        /// <summary>
        /// Stops all currently playing global 2D audio.
        /// </summary>
        public static void StopAll()
        {
            InternalCalls.GlobalAudio_StopAll();
        }

        /// <summary>
        /// Sets the volume of a playing sound.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        public static void SetVolume(ulong audioID, float volume)
        {
            InternalCalls.GlobalAudio_SetVolume(audioID, volume);
        }

        /// <summary>
        /// Pauses a playing sound.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        public static void Pause(ulong audioID)
        {
            InternalCalls.GlobalAudio_Pause(audioID);
        }

        /// <summary>
        /// Resumes a paused sound.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        public static void Resume(ulong audioID)
        {
            InternalCalls.GlobalAudio_Resume(audioID);
        }

        /// <summary>
        /// Checks if a sound is currently playing.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        /// <returns>True if the sound is playing and not paused</returns>
        public static bool IsPlaying(ulong audioID)
        {
            return InternalCalls.GlobalAudio_IsPlaying(audioID);
        }

        /// <summary>
        /// Sets whether a sound should loop.
        /// </summary>
        /// <param name="audioID">The audio handle returned from Play2D</param>
        /// <param name="loop">True to enable looping, false to disable</param>
        public static void SetLoop(ulong audioID, bool loop)
        {
            InternalCalls.GlobalAudio_SetLoop(audioID, loop);
        }
    }
}
