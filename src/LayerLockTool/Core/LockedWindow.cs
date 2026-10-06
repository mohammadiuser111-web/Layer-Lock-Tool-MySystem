namespace LayerLockTool.Core;

internal sealed record LockedWindow(
    nint Handle,
    string Title,
    bool WasTopMost,
    DateTimeOffset LockedAt);
