namespace Engine
{
    /// <summary>
    /// Input key codes - must match your C++ engine's key codes
    /// </summary>
    public enum KeyCode
    {
        // Alphabet keys
        A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72,
        I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80,
        Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88,
        Y = 89, Z = 90,

        // Number keys
        D0 = 48, D1 = 49, D2 = 50, D3 = 51, D4 = 52,
        D5 = 53, D6 = 54, D7 = 55, D8 = 56, D9 = 57,

        // Function keys
        F1 = 290, F2 = 291, F3 = 292, F4 = 293, F5 = 294, F6 = 295,
        F7 = 296, F8 = 297, F9 = 298, F10 = 299, F11 = 300, F12 = 301,

        // Special keys
        Space = 32,
        Enter = 257,
        Escape = 256,
        Tab = 258,
        Backspace = 259,
        Delete = 261,

        // Arrow keys
        LeftArrow = 263,
        RightArrow = 262,
        UpArrow = 265,
        DownArrow = 264,

        // Modifiers
        LeftShift = 340,
        RightShift = 344,
        LeftControl = 341,
        RightControl = 345,
        LeftAlt = 342,
        RightAlt = 346,
    }

    /// <summary>
    /// Input handling for keyboard and mouse
    /// </summary>
    public static class Input
    {
        public static bool IsKeyPressed(KeyCode key)
        {
            return InternalCalls.Input_IsKeyPressed(key);
        }

        public static bool IsKeyHeld(KeyCode key)
        {
            return InternalCalls.Input_IsKeyHeld(key);
        }

        public static bool IsKeyReleased(KeyCode key)
        {
            return InternalCalls.Input_IsKeyReleased(key);
        }

        public static bool IsMouseButtonPressed(int button)
        {
            return InternalCalls.Input_IsMouseButtonPressed(button);
        }

        public static bool IsMouseButtonHeld(int button)
        {
            return InternalCalls.Input_IsMouseButtonHeld(button);
        }

        public static bool IsMouseButtonReleased(int button)
        {
            return InternalCalls.Input_IsMouseButtonReleased(button);
        }

        public static Vector2 GetMousePosition()
        {
            InternalCalls.Input_GetMousePosition(out Vector2 position);
            return position;
        }

        public static Vector2 GetMouseDelta()
        {
            InternalCalls.Input_GetMouseDelta(out Vector2 delta);
            return delta;
        }
    }
}
