using System;

public static class PlayerInputBlocker
{
    private static bool _blocked;
    private static int _lockCount;

    public static bool IsBlocked => _blocked;

    public static void SetBlocked(bool blocked)
    {
        // Allow nested calls (e.g., multiple systems requesting a lock)
        if (blocked)
        {
            _lockCount = Math.Max(1, _lockCount + 1);
        }
        else
        {
            _lockCount = Math.Max(0, _lockCount - 1);
        }

        _blocked = _lockCount > 0;
    }
}
