using System.Text;

namespace Platform;

public delegate void PlatformMemoryRenderDelta<in T>(StringBuilder builder, T current, T last);
