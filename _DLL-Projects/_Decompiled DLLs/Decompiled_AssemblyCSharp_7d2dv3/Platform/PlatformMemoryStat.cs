using System;
using System.Collections.Generic;
using System.Text;

namespace Platform;

public sealed class PlatformMemoryStat<T> : IPlatformMemoryStat<T>, IPlatformMemoryStat
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EnumDictionary<MemoryStatColumn, T> m_columnValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EnumDictionary<MemoryStatColumn, T> m_columnLastValues;

	public string Name { get; }

	public PlatformMemoryRenderValue<T> RenderValue { get; set; }

	public PlatformMemoryRenderDelta<T> RenderDelta { get; set; }

	public event PlatformMemoryColumnChangedHandler<T> ColumnSetAfter;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformMemoryStat(string name)
	{
		m_columnValues = new EnumDictionary<MemoryStatColumn, T>();
		m_columnLastValues = new EnumDictionary<MemoryStatColumn, T>();
		Name = name;
	}

	public void UpdateLast()
	{
		foreach (var (key, value) in m_columnValues)
		{
			m_columnLastValues[key] = value;
		}
	}

	public void RenderColumn(StringBuilder builder, MemoryStatColumn column, bool delta)
	{
		if (m_columnValues.TryGetValue(column, out var value))
		{
			if (!delta)
			{
				RenderValue?.Invoke(builder, value);
				return;
			}
			T last = m_columnLastValues[column];
			RenderDelta?.Invoke(builder, value, last);
		}
	}

	public void Set(MemoryStatColumn column, T value)
	{
		m_columnValues[column] = value;
		if (!m_columnLastValues.ContainsKey(column))
		{
			m_columnLastValues[column] = value;
		}
		this.ColumnSetAfter?.Invoke(column, value);
	}

	public bool TryGet(MemoryStatColumn column, out T value)
	{
		return m_columnValues.TryGetValue(column, out value);
	}

	public bool TryGetLast(MemoryStatColumn column, out T value)
	{
		return m_columnLastValues.TryGetValue(column, out value);
	}

	public static IPlatformMemoryStat<T> Create(string name)
	{
		return new PlatformMemoryStat<T>(name);
	}
}
public static class PlatformMemoryStat
{
	public static IPlatformMemoryStat<T> Create<T>(string name)
	{
		IPlatformMemoryStat<T> platformMemoryStat = PlatformMemoryStat<T>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, T value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<long> CreateBytes(string name)
	{
		IPlatformMemoryStat<long> platformMemoryStat = CreateInt64(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, long current, long last)
		{
			if (current != last)
			{
				RenderSize(builder, current - last);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderSize(StringBuilder builder, long sizeBytes)
		{
			if (Math.Abs(sizeBytes) < 1024)
			{
				builder.Append(sizeBytes).Append("  ").Append('B');
				return;
			}
			double num = (double)sizeBytes / 1024.0;
			string text = "kMGTPE";
			foreach (char value in text)
			{
				if (Math.Abs(num) < 1024.0)
				{
					builder.AppendFormat("{0:F3} ", num).Append(value).Append('B');
					return;
				}
				num /= 1024.0;
			}
			throw new InvalidOperationException("Should not be reachable... Are there enough prefixes?");
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, long value)
		{
			RenderSize(builder, value);
		}
	}

	public static IPlatformMemoryStat<int> CreateInt32(string name)
	{
		IPlatformMemoryStat<int> platformMemoryStat = PlatformMemoryStat<int>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, int current, int last)
		{
			if (current != last)
			{
				if (current >= last)
				{
					builder.AppendFormat("{0}", current - last);
				}
				else
				{
					builder.AppendFormat("-{0}", last - current);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, int value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<uint> CreateUInt32(string name)
	{
		IPlatformMemoryStat<uint> platformMemoryStat = PlatformMemoryStat<uint>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, uint current, uint last)
		{
			if (current != last)
			{
				if (current >= last)
				{
					builder.AppendFormat("{0}", current - last);
				}
				else
				{
					builder.AppendFormat("-{0}", last - current);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, uint value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<long> CreateInt64(string name)
	{
		IPlatformMemoryStat<long> platformMemoryStat = PlatformMemoryStat<long>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, long current, long last)
		{
			if (current != last)
			{
				if (current >= last)
				{
					builder.AppendFormat("{0}", current - last);
				}
				else
				{
					builder.AppendFormat("-{0}", last - current);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, long value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<ulong> CreateUInt64(string name)
	{
		IPlatformMemoryStat<ulong> platformMemoryStat = PlatformMemoryStat<ulong>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, ulong current, ulong last)
		{
			if (current != last)
			{
				if (current >= last)
				{
					builder.AppendFormat("{0}", current - last);
				}
				else
				{
					builder.AppendFormat("-{0}", last - current);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, ulong value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<float> CreateFloat(string name)
	{
		IPlatformMemoryStat<float> platformMemoryStat = PlatformMemoryStat<float>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, float current, float last)
		{
			if (current != last)
			{
				builder.AppendFormat("{0}", current - last);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, float value)
		{
			builder.AppendFormat("{0}", value);
		}
	}

	public static IPlatformMemoryStat<double> CreateDouble(string name)
	{
		IPlatformMemoryStat<double> platformMemoryStat = PlatformMemoryStat<double>.Create(name);
		platformMemoryStat.RenderValue = RenderValue;
		platformMemoryStat.RenderDelta = RenderDelta;
		return platformMemoryStat;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderDelta(StringBuilder builder, double current, double last)
		{
			if (current != last)
			{
				builder.AppendFormat("{0}", current - last);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void RenderValue(StringBuilder builder, double value)
		{
			builder.AppendFormat("{0}", value);
		}
	}
}
