namespace Platform;

public delegate bool PlatformMemoryStatHasChangedSignificantly<in T>(T current, T last);
