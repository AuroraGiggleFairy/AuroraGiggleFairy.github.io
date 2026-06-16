using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public sealed class SaveBufferPool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<byte[]>[] m_pools;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] m_bufferLengths;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] m_poolCapacities;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] m_poolSizes;

	public static readonly SaveBufferPool Instance = new SaveBufferPool(new int[9] { 64, 128, 1024, 16384, 65536, 393216, 2097152, 8388608, 16777216 }, new int[9] { 4, 1, 2, 30, 6, 15, 2, 2, 0 }, new int[9] { 4, 1, 2, 30, 6, 15, 2, 2, 0 });

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_debugLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_debugSampleCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_debugLogPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder m_debugLogBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] m_debugConcurrentUsages;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] m_debugAverageBufferOccupancies;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveBufferPool(int[] bufferLengths, int[] poolCapacities, int[] bufferPrefillCounts)
	{
		m_poolCapacities = poolCapacities;
		m_bufferLengths = bufferLengths;
		m_poolSizes = new int[m_bufferLengths.Length];
		m_pools = new List<byte[]>[m_bufferLengths.Length];
		for (int i = 0; i < m_bufferLengths.Length; i++)
		{
			m_pools[i] = new List<byte[]>(m_poolCapacities[i]);
		}
		if (bufferPrefillCounts == null)
		{
			return;
		}
		for (int j = 0; j < m_bufferLengths.Length; j++)
		{
			for (int k = 0; k < bufferPrefillCounts[j]; k++)
			{
				m_pools[j].Add(new byte[m_bufferLengths[j]]);
				m_poolSizes[j]++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int calcPoolIndex(int bufferSize)
	{
		for (int i = 0; i < m_bufferLengths.Length; i++)
		{
			if (bufferSize <= m_bufferLengths[i])
			{
				return i;
			}
		}
		return -1;
	}

	public byte[] Alloc(int bufferSize)
	{
		int num = calcPoolIndex(bufferSize);
		if (num == -1)
		{
			Log.Warning($"SaveBufferPool was requested a buffer of length {bufferSize}, which is larger than any pool size.");
			return new byte[bufferSize];
		}
		lock (m_lock)
		{
			byte[] result;
			if (m_poolSizes[num] == 0)
			{
				result = new byte[m_bufferLengths[num]];
			}
			else
			{
				int index = --m_poolSizes[num];
				result = m_pools[num][index];
				m_pools[num][index] = null;
			}
			return result;
		}
	}

	public void Free(byte[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		int num = calcPoolIndex(buffer.Length);
		if (num < 0 || m_bufferLengths[num] != buffer.Length)
		{
			Log.Error($"SaveBufferPool had a buffer returned that does not fit into any pool. buffer.Length == {buffer.Length}");
			return;
		}
		lock (m_lock)
		{
			if (m_poolSizes[num] < m_poolCapacities[num])
			{
				if (m_poolSizes[num] >= m_pools[num].Count)
				{
					m_pools[num].Add(buffer);
					m_poolSizes[num]++;
				}
				else
				{
					m_pools[num][m_poolSizes[num]++] = buffer;
				}
			}
		}
	}

	[Conditional("DEBUG_SAVE_BUFFER_POOL")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugInit()
	{
		lock (m_debugLock)
		{
			m_debugSampleCount = 0;
			m_debugLogPath = Path.Join(GameIO.GetApplicationTempPath(), "SaveBufferPool.csv");
			m_debugLogBuilder = new StringBuilder();
			m_debugConcurrentUsages = new int[m_bufferLengths.Length];
			m_debugAverageBufferOccupancies = new float[m_bufferLengths.Length];
			if (File.Exists(m_debugLogPath))
			{
				File.Delete(m_debugLogPath);
			}
			Log.Out("SaveBufferPool logging to: " + m_debugLogPath);
			m_debugLogBuilder.Append("Sample");
			int[] bufferLengths = m_bufferLengths;
			foreach (int value in bufferLengths)
			{
				m_debugLogBuilder.Append(",").Append(value).Append(" capacity,")
					.Append(value)
					.Append(" concurrent usage,")
					.Append(value)
					.Append(" average occupancy");
			}
			m_debugLogBuilder.AppendLine();
		}
	}

	[Conditional("DEBUG_SAVE_BUFFER_POOL")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugLogStats(bool output)
	{
		lock (m_debugLock)
		{
			m_debugSampleCount++;
			m_debugLogBuilder.Append($"{m_debugSampleCount}");
			for (int i = 0; i < m_bufferLengths.Length; i++)
			{
				m_debugLogBuilder.Append(",").Append(m_poolCapacities[i]).Append(",")
					.Append(m_debugConcurrentUsages[i])
					.Append(",")
					.Append(m_debugAverageBufferOccupancies[i]);
			}
			m_debugLogBuilder.AppendLine();
			if (output)
			{
				File.AppendAllText(m_debugLogPath, m_debugLogBuilder.ToString());
				m_debugLogBuilder.Clear();
			}
		}
	}

	[Conditional("DEBUG_SAVE_BUFFER_POOL")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugAlloc(int bufferSize, int poolIndex)
	{
		m_debugConcurrentUsages[poolIndex]++;
		float num = (float)bufferSize / (float)m_bufferLengths[poolIndex];
		m_debugAverageBufferOccupancies[poolIndex] = (m_debugAverageBufferOccupancies[poolIndex] * (float)(m_debugConcurrentUsages[poolIndex] - 1) + num) / (float)m_debugConcurrentUsages[poolIndex];
	}

	[Conditional("DEBUG_SAVE_BUFFER_POOL")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugFree(int poolIndex)
	{
		m_debugConcurrentUsages[poolIndex]--;
	}
}
