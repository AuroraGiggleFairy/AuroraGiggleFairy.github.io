using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using MemoryPack;
using Platform;
using UnityEngine;

public class DynamicPropertiesCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream m_fileStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayBufferWriter<byte> m_buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_STREAM_BUFFER_SIZE = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public (long, int)[] offsetsAndLengths;

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<int> m_queue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, DynamicProperties> m_cache;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_cacheHits;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_cacheMisses;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cacheSize = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _cacheLock = new object();

	public DynamicPropertiesCache()
	{
		Debug.Log($"[BLOCKPROPERTIES] Creating DynamicProperties Cache, max cache size {1000}");
		m_filePath = PlatformManager.NativePlatform.Utils.GetTempFileName("dpc", ".dpc");
		m_fileStream = new FileStream(m_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
		m_buffer = new ArrayBufferWriter<byte>(65536);
		m_cache = new Dictionary<int, DynamicProperties>(1000);
		m_queue = new LinkedList<int>();
		offsetsAndLengths = new(long, int)[Block.MAX_BLOCKS];
	}

	public void Cleanup()
	{
		m_cache.Clear();
		m_cache = null;
		m_queue.Clear();
		m_queue = null;
		m_buffer.Clear();
		m_buffer = null;
		m_fileStream.Close();
	}

	public bool Store(int blockID, DynamicProperties props)
	{
		long position = m_fileStream.Position;
		m_buffer.Clear();
		IBufferWriter<byte> bufferWriter = m_buffer;
		MemoryPackSerializer.Serialize(in bufferWriter, in props);
		int writtenCount = m_buffer.WrittenCount;
		m_fileStream.Write(m_buffer.WrittenSpan);
		offsetsAndLengths[blockID] = (position, writtenCount);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicProperties Retrieve(long offset, int length)
	{
		m_buffer.Clear();
		Span<byte> span = m_buffer.GetSpan(length).Slice(0, length);
		m_fileStream.Seek(offset, SeekOrigin.Begin);
		int num;
		for (int i = 0; i < length; i += num)
		{
			num = m_fileStream.Read(span.Slice(i, length - i));
			if (num <= 0)
			{
				throw new IOException($"Expected to read {length} bytes total but only read {i} bytes.");
			}
		}
		return MemoryPackSerializer.Deserialize<DynamicProperties>(span);
	}

	public DynamicProperties Cache(int blockID)
	{
		LinkedListNode<int> linkedListNode = null;
		DynamicProperties value;
		lock (_cacheLock)
		{
			if (!m_cache.TryGetValue(blockID, out value))
			{
				m_cacheMisses++;
				value = Retrieve(offsetsAndLengths[blockID].Item1, offsetsAndLengths[blockID].Item2);
				m_cache.Add(blockID, value);
				while (m_queue.Count >= 1000)
				{
					linkedListNode = m_queue.Last;
					m_queue.Remove(linkedListNode);
					m_cache.Remove(linkedListNode.Value);
				}
				if (linkedListNode == null)
				{
					linkedListNode = new LinkedListNode<int>(blockID);
				}
				else
				{
					linkedListNode.Value = blockID;
				}
				m_queue.AddFirst(linkedListNode);
			}
			else
			{
				m_cacheHits++;
				linkedListNode = m_queue.Find(blockID);
				m_queue.Remove(linkedListNode);
				m_queue.AddFirst(linkedListNode);
			}
		}
		return value;
	}

	public void Stats()
	{
		Debug.Log("[BLOCKPROPERTIES] Block DynamicProperties Cache Stats:");
		Debug.Log($"[BLOCKPROPERTIES] Cache Size: {m_cache.Count}");
		Debug.Log($"[BLOCKPROPERTIES] Hits: {m_cacheHits}, Misses: {m_cacheMisses}, Rate: {(float)m_cacheHits / (float)(m_cacheHits + m_cacheMisses) * 100f}%");
	}
}
