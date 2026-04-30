using System.Text;

namespace Platform;

public delegate void PlatformMemoryRenderValue<in T>(StringBuilder builder, T value);
