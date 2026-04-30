using System.Text;

namespace Platform;

public interface IPlatformMemoryStat
{
	string Name { get; }

	void RenderColumn(StringBuilder builder, MemoryStatColumn column, bool delta);

	void UpdateLast();
}
public interface IPlatformMemoryStat<T> : IPlatformMemoryStat
{
	PlatformMemoryRenderValue<T> RenderValue { get; set; }

	PlatformMemoryRenderDelta<T> RenderDelta { get; set; }

	event PlatformMemoryColumnChangedHandler<T> ColumnSetAfter;

	void Set(MemoryStatColumn column, T value);

	bool TryGet(MemoryStatColumn column, out T value);

	bool TryGetLast(MemoryStatColumn column, out T value);
}
