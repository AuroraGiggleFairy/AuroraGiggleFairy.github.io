using System;

namespace Platform;

public static class IPlatformMemoryStatExtensions
{
	public static IPlatformMemoryStat<T> AddColumnSetHandler<T>(this IPlatformMemoryStat<T> stat, PlatformMemoryColumnChangedHandler<T> handler)
	{
		stat.ColumnSetAfter += handler;
		return stat;
	}

	public static IPlatformMemoryStat<T> WithUpdatePeak<T>(this IPlatformMemoryStat<T> stat) where T : IComparable<T>
	{
		return stat.AddColumnSetHandler([PublicizedFrom(EAccessModifier.Internal)] (MemoryStatColumn column, T value) =>
		{
			if (column == MemoryStatColumn.Current && (!stat.TryGet(MemoryStatColumn.Peak, out var value2) || value.CompareTo(value2) > 0))
			{
				stat.Set(MemoryStatColumn.Peak, value);
			}
		});
	}

	public static IPlatformMemoryStat<T> WithUpdateMin<T>(this IPlatformMemoryStat<T> stat) where T : IComparable<T>
	{
		return stat.AddColumnSetHandler([PublicizedFrom(EAccessModifier.Internal)] (MemoryStatColumn column, T value) =>
		{
			if (column == MemoryStatColumn.Current && (!stat.TryGet(MemoryStatColumn.Min, out var value2) || value.CompareTo(value2) < 0))
			{
				stat.Set(MemoryStatColumn.Min, value);
			}
		});
	}

	public static bool TryGetCurrentAndLast<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column, out T current, out T last)
	{
		if (!stat.TryGet(column, out current))
		{
			last = default(T);
			return false;
		}
		return stat.TryGetLast(column, out last);
	}

	public static bool HasColumnChanged<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column, PlatformMemoryStatHasChangedSignificantly<T> checkCurrentVsLast)
	{
		if (stat.TryGetCurrentAndLast(column, out var current, out var last))
		{
			return checkCurrentVsLast(current, last);
		}
		return false;
	}

	public static bool HasColumnIncreased<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column) where T : IComparable<T>
	{
		return stat.HasColumnChanged(column, [PublicizedFrom(EAccessModifier.Internal)] (T current, T last) => current.CompareTo(last) > 0);
	}

	public static bool HasColumnDecreased<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column) where T : IComparable<T>
	{
		return stat.HasColumnChanged(column, [PublicizedFrom(EAccessModifier.Internal)] (T current, T last) => current.CompareTo(last) < 0);
	}

	public static bool HasBytesChangedSignificantly(this IPlatformMemoryStat<long> stat, MemoryStatColumn column)
	{
		return stat.HasColumnChanged(column, [PublicizedFrom(EAccessModifier.Internal)] (long current, long last) =>
		{
			long num = Math.Abs(current - last);
			long value;
			long num2 = ((!stat.TryGet(MemoryStatColumn.Limit, out value) || last <= value / 2) ? Math.Abs(last) : Math.Abs(value - last));
			return num >= num2 / 128;
		});
	}
}
