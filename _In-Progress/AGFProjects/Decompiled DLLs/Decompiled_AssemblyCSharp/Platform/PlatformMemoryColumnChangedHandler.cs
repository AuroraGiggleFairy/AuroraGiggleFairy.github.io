namespace Platform;

public delegate void PlatformMemoryColumnChangedHandler<in T>(MemoryStatColumn column, T value);
