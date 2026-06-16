using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Platform.XBL.Save.MasterFileTable.Latest;

namespace Platform.XBL.Save;

public class BlobCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct FileScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlobCache m_parent;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ulong m_blobId;

		public FileScope(BlobCache parent, ulong blobId)
		{
			m_parent = parent;
			m_blobId = blobId;
			while (!m_parent.m_inUse.TryAdd(m_blobId, 0))
			{
				Thread.Sleep(0);
			}
		}

		public void Dispose()
		{
			if (!m_parent.m_inUse.TryRemove(m_blobId, out var _))
			{
				LogError($"Expected to release '{m_blobId}'.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_root;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_blobRefsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ulong, BlobRef> m_blobRefs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ConcurrentDictionary<ulong, byte> m_inUse;

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: BlobCache] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: BlobCache] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string text)
	{
		Log.Warning("[XBL: BlobCache] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: BlobCache] " + text);
	}

	public BlobCache(string containerName, IEnumerable<BlobRef> expectedBlobRefs)
	{
		if (!LaunchPrefs.GameCoreBlobCache.Value)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		m_root = GameIO.GetNormalizedPath(Path.Combine(GameIO.GetNormalizedPath(GameIO.GetDeviceLocalUserGameDataDir()) + "BlobCache", containerName));
		Directory.CreateDirectory(m_root);
		m_blobRefs = expectedBlobRefs.ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (BlobRef blobRef) => blobRef.Id, [PublicizedFrom(EAccessModifier.Internal)] (BlobRef blobRef) => blobRef);
		m_inUse = new ConcurrentDictionary<ulong, byte>();
		HashSet<ulong> cachedIds = new HashSet<ulong>();
		foreach (string item in Directory.EnumerateDirectories(m_root))
		{
			Directory.Delete(item, recursive: true);
		}
		HashAlgorithm hashAlgorithm = BlobRef.GetHashAlgorithm();
		foreach (FileInfo item2 in new DirectoryInfo(m_root).EnumerateFiles())
		{
			string name = item2.Name;
			if (!SaveContainer.TryConvertToId(name, out var id))
			{
				LogWarning("Invalid cached blob name '" + name + "'.");
				continue;
			}
			if (!m_blobRefs.TryGetValue(id, out var value))
			{
				LogWarning("Extraneous cached blob '" + name + "'.");
				item2.Delete();
				continue;
			}
			if (item2.Length != value.Length)
			{
				LogWarning($"Length mismatch for blob '{name}'. {item2.Length} != {value.Length}");
				m_blobRefs.Remove(value.Id);
				item2.Delete();
				continue;
			}
			byte[] array = null;
			using (FileStream fileStream = item2.OpenRead())
			{
				if (fileStream.Length == value.Length)
				{
					array = hashAlgorithm.ComputeHash(fileStream);
				}
			}
			if (array == null)
			{
				LogWarning($"Length mismatch for blob '{name}'. {item2.Length} != {value.Length}");
				m_blobRefs.Remove(value.Id);
				item2.Delete();
			}
			else if (!value.Hash.Span.SequenceEqual(array))
			{
				LogWarning("Hash mismatch for '" + name + "'. " + array.ToHexString() + " != " + value.Hash.ToHexString());
				m_blobRefs.Remove(value.Id);
				item2.Delete();
			}
			else
			{
				cachedIds.Add(id);
			}
		}
		m_blobRefs.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (ulong blobId) => !cachedIds.Contains(blobId));
		LogInfo($"Cached validated in {microStopwatch.Elapsed.TotalMilliseconds:F3} ms.");
	}

	public bool Contains(BlobRef requestedRef)
	{
		if (!LaunchPrefs.GameCoreBlobCache.Value)
		{
			return false;
		}
		BlobRef value;
		lock (m_blobRefsLock)
		{
			if (!m_blobRefs.TryGetValue(requestedRef.Id, out value))
			{
				return false;
			}
		}
		if (requestedRef != value)
		{
			return false;
		}
		return true;
	}

	public bool TryGet(BlobRef requestedRef, out RefCountedBuffer buffer)
	{
		if (!LaunchPrefs.GameCoreBlobCache.Value)
		{
			buffer = null;
			return false;
		}
		BlobRef value;
		lock (m_blobRefsLock)
		{
			if (!m_blobRefs.TryGetValue(requestedRef.Id, out value))
			{
				buffer = null;
				return false;
			}
		}
		if (requestedRef != value)
		{
			buffer = null;
			return false;
		}
		ulong id = value.Id;
		string filePath;
		using (GetCachedPath(id, out filePath))
		{
			if (!File.Exists(filePath))
			{
				LogWarning($"Cache MISS Expected {value} to exist in the cache?");
				Invalidate(id);
				buffer = null;
				return false;
			}
			buffer = null;
			try
			{
				using FileStream fileStream = File.OpenRead(filePath);
				if (fileStream.Length != value.Length)
				{
					LogWarning($"Cache MISS Unexpected length of {fileStream.Length} for {value}.");
					Invalidate(id);
					buffer = null;
					return false;
				}
				buffer = RefCountedBuffer.CreatePooled((int)fileStream.Length);
				int num2;
				for (int i = 0; i < fileStream.Length; i += num2)
				{
					Span<byte> span = buffer.Span;
					int num = i;
					num2 = fileStream.Read(span.Slice(num, span.Length - num));
					if (num2 <= 0)
					{
						throw new IOException($"Expected {fileStream.Length} bytes but only read {i}.");
					}
				}
				return true;
			}
			catch (Exception arg)
			{
				LogError($"Cache MISS Failed to read {value}: {arg}");
				Invalidate(id);
				buffer?.Dispose();
				buffer = null;
				return false;
			}
		}
	}

	public BlobRef Set(ulong blobId, RefCountedBuffer buffer)
	{
		BlobRef blobRef = new BlobRef
		{
			Id = blobId,
			Length = (uint)buffer.Length,
			Hash = BlobRef.CalculateHash(buffer)
		};
		if (!LaunchPrefs.GameCoreBlobCache.Value)
		{
			return blobRef;
		}
		lock (m_blobRefsLock)
		{
			if (m_blobRefs.TryGetValue(blobId, out var value) && blobRef == value)
			{
				return value;
			}
		}
		string filePath;
		using (GetCachedPath(blobId, out filePath))
		{
			try
			{
				using FileStream fileStream = File.Create(filePath);
				fileStream.Write(buffer.Span);
			}
			catch (Exception arg)
			{
				LogError($"Cache FAIL {blobId}: {arg}");
				Invalidate(blobId);
				return blobRef;
			}
			lock (m_blobRefsLock)
			{
				m_blobRefs[blobId] = blobRef;
			}
			return blobRef;
		}
	}

	public void Invalidate(ulong blobId)
	{
		if (!LaunchPrefs.GameCoreBlobCache.Value)
		{
			return;
		}
		lock (m_blobRefsLock)
		{
			if (!m_blobRefs.Remove(blobId))
			{
				return;
			}
		}
		try
		{
			string filePath;
			using (GetCachedPath(blobId, out filePath))
			{
				File.Delete(filePath);
			}
		}
		catch (Exception arg)
		{
			LogError($"Failed to invalidate '{blobId}': {arg}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileScope GetCachedPath(ulong blobId, out string filePath)
	{
		FileScope result = new FileScope(this, blobId);
		filePath = null;
		try
		{
			filePath = GameIO.GetNormalizedPath(Path.Join(m_root, SaveContainer.IdToString(blobId)));
			return result;
		}
		finally
		{
			if (filePath == null)
			{
				result.Dispose();
			}
		}
	}
}
