using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Platform.Shared;
using Platform.XBL.Save.MasterFileTable;
using Platform.XBL.Save.MasterFileTable.Latest;
using Platform.XBL.Save.Storage;
using Unity.XGamingRuntime;

namespace Platform.XBL.Save;

public sealed class SaveContainer : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxBlobSize = 16777216;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxNodeBlobSize = 16777216;

	public const ulong RootNodeId = 18364758544493064720uL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxBlobCacheReadSize = 1048576;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan BlobCacheInactivityThreshold = TimeSpan.FromSeconds(1.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public SizeTracker m_sizeTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveStorageContainer m_saveStorageContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ContainerData m_containerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_mftLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream m_mftMemoryStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter m_mftBinaryWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ulong, XGameSaveBlobInfo> m_blobIdToInfo = new Dictionary<ulong, XGameSaveBlobInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_blobCacheLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public BlobCache m_blobCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedDictionary<ulong, uint> m_blobCacheQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_blobCacheQueueCancellationTokenSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public Task m_blobCacheQueueTask;

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: SaveContainer] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: SaveContainer] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string text)
	{
		Log.Warning("[XBL: SaveContainer] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveContainer] " + text);
	}

	public SaveContainer(ISaveStorageContainer saveStorageContainer, SizeTracker sizeTracker)
	{
		bool flag = false;
		try
		{
			m_mftMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			m_mftBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
			m_mftBinaryWriter.SetBaseStream(m_mftMemoryStream);
			m_saveStorageContainer = saveStorageContainer;
			m_sizeTracker = sizeTracker;
			m_containerData = new ContainerData();
			if (!m_saveStorageContainer.TryEnumerateBlobInfos(out var blobInfos))
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			XGameSaveBlobInfo[] array = blobInfos;
			foreach (XGameSaveBlobInfo xGameSaveBlobInfo in array)
			{
				hashSet.Add(xGameSaveBlobInfo.Name);
				if (!TryConvertToId(xGameSaveBlobInfo.Name, out var id))
				{
					LogWarning("Container '" + saveStorageContainer.Name + "' has a blob with non-id name '" + xGameSaveBlobInfo.Name + "'. Will be deleted.");
				}
				else
				{
					m_blobIdToInfo[id] = xGameSaveBlobInfo;
				}
			}
			if (m_blobIdToInfo.TryGetValue(18364758544493064720uL, out var value))
			{
				using RefCountedBuffer buffer = m_saveStorageContainer.GetBlob(value.Name, "$MFT");
				ReadMFT(buffer);
			}
			else
			{
				WriteMFT();
			}
			HashSet<string> hashSet2 = new HashSet<string> { IdToString(18364758544493064720uL) };
			foreach (Node item in m_containerData.RootNode.Enumerate(includeSelf: false, recursive: true))
			{
				foreach (BlobRef blobRef3 in item.BlobRefs)
				{
					hashSet2.Add(IdToString(blobRef3.Id));
				}
			}
			HashSet<string> hashSet3 = new HashSet<string>(hashSet);
			hashSet3.ExceptWith(hashSet2);
			if (hashSet3.Count > 0)
			{
				foreach (string item2 in hashSet3)
				{
					LogWarning("Deleting unreachable blob named '" + item2 + "' from container named '" + saveStorageContainer.Name + "'.");
					DeleteBlob(item2);
				}
			}
			foreach (Node item3 in m_containerData.RootNode.Enumerate(includeSelf: false, recursive: true))
			{
				IReadOnlyList<BlobRef> blobRefs = item3.BlobRefs;
				List<BlobRef> list = null;
				for (int num = blobRefs.Count - 1; num >= 0; num--)
				{
					BlobRef blobRef = blobRefs[num];
					if (!m_blobIdToInfo.ContainsKey(blobRef.Id))
					{
						LogWarning($"Remove non-existent {blobRef} in the metadata for node '{item3.Name}' from container named '{saveStorageContainer.Name}'.");
						if (list == null)
						{
							list = new List<BlobRef>(blobRefs);
						}
						list.RemoveAt(num);
					}
				}
				if (list != null)
				{
					item3.SetBlobRefs(list.ToArray());
				}
			}
			foreach (Node item4 in m_containerData.RootNode.Enumerate(includeSelf: false, recursive: true))
			{
				IReadOnlyList<BlobRef> blobRefs2 = item4.BlobRefs;
				BlobRef[] array2 = null;
				for (int j = 0; j < blobRefs2.Count; j++)
				{
					BlobRef blobRef2 = blobRefs2[j];
					if (!m_blobIdToInfo.TryGetValue(blobRef2.Id, out var value2))
					{
						LogWarning($"Expected blob info to exist for {blobRef2}.");
					}
					else if (blobRef2.Length != value2.Size)
					{
						LogWarning($"Length of {blobRef2} (in node '{item4.Name}') is out of sync. Updating to {value2.Size.FormatSize()}.");
						if (array2 == null)
						{
							array2 = blobRefs2.ToArray();
						}
						array2[j] = new BlobRef
						{
							Id = blobRef2.Id,
							Length = value2.Size,
							Hash = blobRef2.Hash
						};
					}
				}
				if (array2 != null)
				{
					item4.SetBlobRefs(array2);
				}
			}
			m_blobCache = new BlobCache(saveStorageContainer.Name, m_containerData.RootNode.Enumerate(includeSelf: false, recursive: true).SelectMany([PublicizedFrom(EAccessModifier.Internal)] (Node node) => node.BlobRefs));
			if (LaunchPrefs.GameCoreBlobCache.Value)
			{
				if (LaunchPrefs.GameCoreBlobCacheProactive.Value)
				{
					m_blobCacheQueue = (from blobRef3 in m_containerData.RootNode.Enumerate(includeSelf: false, recursive: true).SelectMany([PublicizedFrom(EAccessModifier.Internal)] (Node node) => node.BlobRefs)
						where !m_blobCache.Contains(blobRef3)
						orderby blobRef3.Length
						select blobRef3).Aggregate(new LinkedDictionary<ulong, uint>(), [PublicizedFrom(EAccessModifier.Internal)] (LinkedDictionary<ulong, uint> queue, BlobRef blobRef3) =>
					{
						queue.Add(blobRef3.Id, blobRef3.Length);
						return queue;
					});
					if (m_blobCacheQueue.Count > 0)
					{
						m_blobCacheQueueCancellationTokenSource = new CancellationTokenSource();
						m_blobCacheQueueTask = Task.Run([PublicizedFrom(EAccessModifier.Private)] () => BlobCacheQueueTask(m_blobCacheQueueCancellationTokenSource.Token), m_blobCacheQueueCancellationTokenSource.Token);
					}
					else
					{
						LogInfo("BlobCacheQueueTask does not need to run as there are no uncached blobs.");
					}
				}
				else
				{
					LogInfo("BlobCacheQueueTask does not need to be run because proactive caching is disabled.");
					m_blobCacheQueue = new LinkedDictionary<ulong, uint>();
				}
			}
			else
			{
				LogInfo("BlobCacheQueueTask does not need to run as the BlobCache is disabled.");
				m_blobCacheQueue = new LinkedDictionary<ulong, uint>();
			}
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				Dispose();
			}
		}
	}

	public void Dispose()
	{
		m_blobCacheQueueCancellationTokenSource?.Cancel();
		m_blobCacheQueueTask?.Wait();
		m_blobCacheQueueTask = null;
		m_blobCacheQueueCancellationTokenSource?.Dispose();
		m_blobCacheQueueCancellationTokenSource = null;
		lock (m_blobCacheLock)
		{
			m_blobCacheQueue?.Clear();
			m_blobCacheQueue = null;
			m_blobCache = null;
		}
		lock (m_blobIdToInfo)
		{
			m_blobIdToInfo?.Clear();
			m_blobIdToInfo = null;
		}
		m_containerData?.Dispose();
		m_containerData = null;
		if (m_mftBinaryWriter != null)
		{
			MemoryPools.poolBinaryWriter.FreeSync(m_mftBinaryWriter);
			m_mftBinaryWriter = null;
		}
		if (m_mftMemoryStream != null)
		{
			MemoryPools.poolMemoryStream.FreeSync(m_mftMemoryStream);
			m_mftMemoryStream = null;
		}
		m_sizeTracker = null;
		m_saveStorageContainer = null;
	}

	public bool FileExists(StringSpan relativePath)
	{
		return GetNode(relativePath)?.IsFile() ?? false;
	}

	public void FileRead(StringSpan relativePath, Stream outputStream)
	{
		Node node = GetNode(relativePath);
		if (node == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' does not exist."));
		}
		if (!node.IsFile())
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' is not a file."));
		}
		if (node.Children.Count > 0)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has children so can not be read from."));
		}
		lock (node.m_blobLock)
		{
			IReadOnlyList<BlobRef> blobRefs = node.BlobRefs;
			BlobRef[] array = null;
			for (int i = 0; i < blobRefs.Count; i++)
			{
				BlobRef blobRef = blobRefs[i];
				bool flag;
				RefCountedBuffer buffer;
				lock (m_blobCacheLock)
				{
					flag = m_blobCache.TryGet(blobRef, out buffer);
				}
				if (flag)
				{
					using (buffer)
					{
						outputStream.Write(buffer.Span);
					}
					continue;
				}
				XGameSaveBlobInfo blobInfoCached = GetBlobInfoCached(blobRef.Id);
				using RefCountedBuffer refCountedBuffer = m_saveStorageContainer.GetBlob(blobInfoCached.Name, relativePath);
				if (refCountedBuffer.Length == blobInfoCached.Size)
				{
					outputStream.Write(refCountedBuffer.Span);
					BlobRef blobRef2;
					lock (m_blobCacheLock)
					{
						blobRef2 = m_blobCache.Set(blobRef.Id, refCountedBuffer);
						m_blobCacheQueue.Remove(blobRef.Id);
					}
					if (blobRef2 != blobRef)
					{
						if (array == null)
						{
							array = blobRefs.ToArray();
						}
						array[i] = blobRef2;
					}
					continue;
				}
				throw new IOException($"Expected blob data for blob with name '{blobInfoCached.Name}' and size '{blobInfoCached.Size}' bytes, but was {refCountedBuffer.Length} bytes.");
			}
			if (array != null)
			{
				node.SetBlobRefs(array);
			}
		}
	}

	public void FileWrite(StringSpan relativePath, Stream inputStream)
	{
		bool wasCreated;
		Node orCreateNode = GetOrCreateNode(relativePath, createDirectory: false, createParents: false, out wasCreated);
		if (!orCreateNode.IsFile())
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' is not a file."));
		}
		if (orCreateNode.Children.Count > 0)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has children so can not be written to."));
		}
		long num = inputStream.Length - inputStream.Position;
		ulong[] array = null;
		int num2 = 0;
		bool flag = false;
		ulong[] array2;
		try
		{
			lock (orCreateNode.m_blobLock)
			{
				array2 = orCreateNode.BlobRefs.Select([PublicizedFrom(EAccessModifier.Internal)] (BlobRef blobRef) => blobRef.Id).ToArray();
				long num3 = 0L;
				for (int num4 = 0; num4 < array2.Length; num4++)
				{
					num3 += 28;
				}
				long num5 = num;
				int num6 = (int)(num5 / 16777216 + ((num5 % 16777216 > 0) ? 1 : 0));
				num5 += 28 * num6;
				long num7 = num5 - num3;
				long remaining = m_sizeTracker.Sizes.Remaining;
				if (remaining < 0 || num7 > remaining)
				{
					throw new IOException($"Can not write {num} + {8 * num6} bytes to '{relativePath.ToString()}' because there is only {remaining} bytes of free space.");
				}
				array = new ulong[num6];
				BlobRef[] array3 = new BlobRef[array.Length];
				for (int num8 = 0; num8 < num6; num8++)
				{
					array[num8] = GenerateNewId();
				}
				long num9 = 0L;
				for (int num10 = 0; num10 < num6; num10++)
				{
					int length = (int)((num10 != num6 - 1) ? 16777216 : (num - num9));
					using RefCountedBuffer refCountedBuffer = RefCountedBuffer.CreatePooled(length);
					Span<byte> span = refCountedBuffer.Span;
					int num11 = 0;
					while (num11 < span.Length)
					{
						int num12 = inputStream.Read(span.Slice(num11, span.Length - num11));
						if (num12 <= 0)
						{
							throw new IOException($"Reached end of stream after {num9} bytes saved, expected a total of {num} bytes.");
						}
						num11 += num12;
						num9 += num12;
					}
					ulong num13 = array[num10];
					lock (m_blobCacheLock)
					{
						array3[num10] = m_blobCache.Set(num13, refCountedBuffer);
						m_blobCacheQueue.Remove(num13);
					}
					SetBlob(num13, refCountedBuffer);
					num2++;
				}
				orCreateNode.LastWriteTimeUtc = DateTime.UtcNow;
				orCreateNode.SetBlobRefs(array3);
				array = null;
				flag = true;
			}
		}
		finally
		{
			if (!flag && array != null && array.Length != 0)
			{
				for (int num14 = 0; num14 < num2; num14++)
				{
					try
					{
						DeleteBlob(IdToString(array[num14]));
					}
					catch (IOException)
					{
					}
				}
			}
		}
		WriteMFT();
		ulong[] array4 = array2;
		foreach (ulong id in array4)
		{
			DeleteBlob(IdToString(id));
		}
	}

	public void FileDelete(StringSpan relativePath)
	{
		Node node = GetNode(relativePath);
		if (node == null)
		{
			return;
		}
		if (!node.IsFile())
		{
			throw new IOException(SpanUtils.Concat("Node at ", relativePath, "' is not a file."));
		}
		if (node.Children.Count > 0)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has children so is not a file that can be deleted."));
		}
		if (node.Parent == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has no parent so might be a root node which can not be deleted."));
		}
		node.Parent.DeleteChildNode(node.Name);
		WriteMFT();
		foreach (BlobRef blobRef in node.BlobRefs)
		{
			DeleteBlob(IdToString(blobRef.Id));
		}
		node.Dispose();
	}

	public long FileLength(StringSpan relativePath)
	{
		Node node = GetNode(relativePath);
		if (node == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' does not exist."));
		}
		long num = 0L;
		lock (node.m_blobLock)
		{
			foreach (BlobRef blobRef in node.BlobRefs)
			{
				XGameSaveBlobInfo blobInfoCached = GetBlobInfoCached(blobRef.Id);
				num += blobInfoCached.Size;
			}
			return num;
		}
	}

	public void FileMove(StringSpan sourceRelativePath, StringSpan destRelativePath)
	{
		Node node = GetNode(sourceRelativePath);
		if (node == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", sourceRelativePath, "' does not exist."));
		}
		if (!node.IsFile())
		{
			throw new IOException(SpanUtils.Concat("Node at '", sourceRelativePath, "' is not a file."));
		}
		if (node.Children.Count > 0)
		{
			throw new IOException(SpanUtils.Concat("Node at '", sourceRelativePath, "' has children so is not a file that can be moved."));
		}
		if (node.Parent == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", sourceRelativePath, "' has no parent so might be a root node which can not be moved."));
		}
		if (GetNode(destRelativePath) != null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", destRelativePath, "' exists, so can't be moved to."));
		}
		Node nodeParent = GetNodeParent(destRelativePath);
		if (nodeParent == null)
		{
			throw new IOException(SpanUtils.Concat("Parent of '", destRelativePath, "' does not exist, so can't be moved to."));
		}
		StringSpan newName = destRelativePath.Substring(destRelativePath.LastIndexOf('/') + 1);
		lock (m_mftLock)
		{
			nodeParent.MoveChild(node, newName);
			WriteMFT();
		}
	}

	public bool DirectoryExists(StringSpan relativePath)
	{
		return GetNode(relativePath)?.IsDirectory() ?? false;
	}

	public void DirectoryCreate(StringSpan relativePath)
	{
		if (!GetOrCreateNode(relativePath, createDirectory: true, createParents: true, out var wasCreated).IsDirectory())
		{
			throw new IOException(SpanUtils.Concat("A non-directory node already exists at '", relativePath, "'."));
		}
		if (wasCreated)
		{
			WriteMFT();
		}
	}

	public void DirectoryDelete(StringSpan relativePath, bool recursive)
	{
		Node node = GetNode(relativePath);
		if (node == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' does not exist."));
		}
		if (!node.IsDirectory())
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' is not a directory."));
		}
		if (node.BlobRefs.Count > 0)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' contains blobIds so is considered a file."));
		}
		if (node.Children.Count > 0 && !recursive)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has children so is not a directory that can be deleted without recursive = true."));
		}
		if (node.Parent == null)
		{
			throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' has no parent so might be a root node which can not be deleted."));
		}
		node.Parent.DeleteChildNode(node.Name);
		WriteMFT();
		if (recursive)
		{
			int num = 0;
			foreach (Node item in node.Enumerate(includeSelf: false, recursive: true))
			{
				foreach (BlobRef blobRef in item.BlobRefs)
				{
					DeleteBlob(IdToString(blobRef.Id));
					num++;
				}
			}
		}
		node.Dispose();
	}

	public IEnumerable<string> DirectoryEnumerate(string relativePath, string searchPattern, bool recursive, bool includeDirectories, bool includeFiles)
	{
		return from x in DirectoryEnumerate(relativePath, searchPattern, recursive)
			where (x.IsDirectory && includeDirectories) || (x.IsFile && includeFiles)
			select x.RelativePath;
	}

	public IEnumerable<PathEnumerationInfo> DirectoryEnumerate(string relativePath, string searchPattern, bool recursive)
	{
		Node baseNode;
		if (string.IsNullOrEmpty(relativePath))
		{
			baseNode = m_containerData.RootNode;
		}
		else
		{
			baseNode = GetNode(relativePath);
		}
		if (baseNode == null)
		{
			throw new IOException("Node at '" + relativePath + "' does not exist.");
		}
		Func<Node, bool> predicate;
		if (searchPattern != "*")
		{
			string pattern = "^" + Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			Regex regex = new Regex(pattern, RegexOptions.Compiled);
			predicate = [PublicizedFrom(EAccessModifier.Internal)] (Node node) => regex.IsMatch(node.Name);
		}
		else
		{
			predicate = [PublicizedFrom(EAccessModifier.Internal)] (Node _) => true;
		}
		string baseRelativePath = GetRelativePathFromNode(baseNode, m_containerData.RootNode);
		if (baseRelativePath.Length > 0)
		{
			baseRelativePath += "/";
		}
		return from n in baseNode.Enumerate(includeSelf: false, recursive).Where(predicate)
			select new PathEnumerationInfo(baseRelativePath + GetRelativePathFromNode(n, baseNode), n.IsDirectory(), n.IsFile());
		[PublicizedFrom(EAccessModifier.Internal)]
		static string GetRelativePathFromNode(Node currentNode, Node node)
		{
			List<Node> list = new List<Node>();
			while (currentNode.Parent != null && currentNode != node)
			{
				list.Add(currentNode);
				currentNode = currentNode.Parent;
			}
			return string.Join('/', list.Select([PublicizedFrom(EAccessModifier.Internal)] (Node n) => n.Name).Reverse());
		}
	}

	public void SetCreationTimeUtc(StringSpan relativePath, DateTime lastWriteTimeUtc)
	{
		(GetNode(relativePath) ?? throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' does not exist."))).CreationTimeUtc = lastWriteTimeUtc;
		WriteMFT();
	}

	public DateTime GetCreationTimeUtc(StringSpan relativePath)
	{
		return GetNode(relativePath)?.CreationTimeUtc ?? DateTime.FromFileTimeUtc(0L);
	}

	public void SetLastWriteTimeUtc(StringSpan relativePath, DateTime lastWriteTimeUtc)
	{
		(GetNode(relativePath) ?? throw new IOException(SpanUtils.Concat("Node at '", relativePath, "' does not exist."))).LastWriteTimeUtc = lastWriteTimeUtc;
		WriteMFT();
	}

	public DateTime GetLastWriteTimeUtc(StringSpan relativePath)
	{
		return GetNode(relativePath)?.LastWriteTimeUtc ?? DateTime.FromFileTimeUtc(0L);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Node GetNode(StringSpan relativePath)
	{
		Node node = m_containerData.RootNode;
		StringSpan.CharSplitEnumerator enumerator = relativePath.GetSplitEnumerator('/').GetEnumerator();
		while (enumerator.MoveNext())
		{
			StringSpan current = enumerator.Current;
			if (current.Length != 0)
			{
				node = node.GetChildNode(current);
				if (node == null)
				{
					return null;
				}
			}
		}
		return node;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Node GetNodeParent(StringSpan relativePath)
	{
		if (relativePath.Length <= 0)
		{
			return null;
		}
		int num = relativePath.LastIndexOf('/');
		if (num < 0)
		{
			return m_containerData.RootNode;
		}
		StringSpan stringSpan = relativePath;
		return GetNode(stringSpan.Slice(0, num));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Node GetOrCreateNode(StringSpan relativePath, bool createDirectory, bool createParents, out bool wasCreated)
	{
		Node result;
		if (relativePath.Length <= 0)
		{
			result = m_containerData.RootNode;
			wasCreated = false;
		}
		else
		{
			int num = relativePath.LastIndexOf('/');
			Node node;
			StringSpan name;
			if (num < 0)
			{
				node = m_containerData.RootNode;
				name = relativePath;
			}
			else
			{
				StringSpan stringSpan = relativePath;
				StringSpan relativePath2 = stringSpan.Slice(0, num);
				stringSpan = relativePath;
				int num2 = num + 1;
				name = stringSpan.Slice(num2, stringSpan.Length - num2);
				if (!createParents)
				{
					node = GetNode(relativePath2);
				}
				else
				{
					Node node2 = m_containerData.RootNode;
					StringSpan.CharSplitEnumerator enumerator = relativePath2.GetSplitEnumerator('/').GetEnumerator();
					while (enumerator.MoveNext())
					{
						StringSpan current = enumerator.Current;
						if (current.Length != 0)
						{
							node2 = node2.GetOrCreateChildNode(current, createDirectory: true, out var _);
							if (!node2.IsDirectory())
							{
								throw new IOException(SpanUtils.Concat("Parent '", node2.Name, "' of '", relativePath, "' is not a directory."));
							}
						}
					}
					node = node2;
				}
			}
			if (node == null)
			{
				throw new IOException(SpanUtils.Concat("Parent of '", relativePath, "' does not exist."));
			}
			if (!node.IsDirectory())
			{
				throw new IOException(SpanUtils.Concat("Parent of '", relativePath, "' is not a directory."));
			}
			result = node.GetOrCreateChildNode(name, createDirectory, out wasCreated);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XGameSaveBlobInfo GetBlobInfoCached(ulong blobId)
	{
		lock (m_blobIdToInfo)
		{
			if (m_blobIdToInfo.TryGetValue(blobId, out var value) && value != null)
			{
				return value;
			}
		}
		string text = IdToString(blobId);
		XGameSaveBlobInfo blobInfo = m_saveStorageContainer.GetBlobInfo(text);
		lock (m_blobIdToInfo)
		{
			if (m_blobIdToInfo.TryGetValue(blobId, out var value2) && value2 != null)
			{
				return value2;
			}
			if (blobInfo != null)
			{
				m_blobIdToInfo[blobId] = blobInfo;
				return blobInfo;
			}
		}
		throw new IOException("Expected to find a blob with the name '" + text + "'");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBlob(ulong blobId, RefCountedBuffer blobData)
	{
		string text = IdToString(blobId);
		m_saveStorageContainer.SetBlob(text, blobData);
		XGameSaveBlobInfo value = new XGameSaveBlobInfo
		{
			Name = text,
			Size = (uint)blobData.Length
		};
		lock (m_blobIdToInfo)
		{
			if (m_blobIdToInfo.TryGetValue(blobId, out var value2))
			{
				m_sizeTracker.UpdateUsedEstimate(blobData.Length - value2.Size);
			}
			else
			{
				m_sizeTracker.UpdateUsedEstimate(blobData.Length);
			}
			m_blobIdToInfo[blobId] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteBlob(string blobName)
	{
		m_saveStorageContainer.DeleteBlob(blobName);
		if (!TryConvertToId(blobName, out var id))
		{
			return;
		}
		lock (m_blobIdToInfo)
		{
			if (m_blobIdToInfo.TryGetValue(id, out var value))
			{
				m_sizeTracker.UpdateUsedEstimate(0L - (long)value.Size);
			}
			m_blobIdToInfo.Remove(id);
			lock (m_blobCacheLock)
			{
				m_blobCache?.Invalidate(id);
				m_blobCacheQueue?.Remove(id);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteMFT()
	{
		lock (m_mftLock)
		{
			m_mftMemoryStream.Reset();
			m_containerData.Write(m_mftBinaryWriter);
			if (m_mftMemoryStream.Length > 16777216)
			{
				throw new IOException("The MFT has grown too large to be persisted.");
			}
			int length = (int)m_mftMemoryStream.Length;
			using RefCountedBuffer refCountedBuffer = RefCountedBuffer.CreatePooled(length);
			m_mftMemoryStream.GetBuffer().AsSpan(0, length).CopyTo(refCountedBuffer.Span);
			SetBlob(18364758544493064720uL, refCountedBuffer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadMFT(RefCountedBuffer buffer)
	{
		using MemoryStream inputStream = new MemoryStream(buffer.BufferRaw, buffer.Offset, buffer.Length, writable: false);
		Migrator.ReadMigrate(inputStream, m_containerData, "ContainerData", Migrator.s_containerDataMigrators);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong GenerateNewId()
	{
		ulong reference = 0uL;
		Span<byte> buffer = MemoryMarshal.Cast<ulong, byte>(MemoryMarshal.CreateSpan(ref reference, 1));
		Random value = Platform.Shared.Utils.RandLocal.Value;
		lock (m_blobIdToInfo)
		{
			do
			{
				value.NextBytes(buffer);
			}
			while (m_blobIdToInfo.ContainsKey(reference));
			return reference;
		}
	}

	public static string IdToString(ulong id)
	{
		return FormattableString.Invariant($"{id:x16}");
	}

	public static bool TryConvertToId(string input, out ulong id)
	{
		id = 0uL;
		if (input.Length == 16 && input.Equals(input.ToLowerInvariant(), StringComparison.InvariantCulture))
		{
			return ulong.TryParse(input, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out id);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async Task BlobCacheQueueTask(CancellationToken cancellationToken)
	{
		LogInfo(string.Format("{0} Started with {1} blobs in queue.", "BlobCacheQueueTask", m_blobCacheQueue.Count));
		List<ulong> blobIdsToCache = new List<ulong>();
		TimeSpan totalActiveTime = TimeSpan.Zero;
		ulong totalBytesCached = 0uL;
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					if (!(await InnerLoop()))
					{
						break;
					}
				}
				catch (Exception ex)
				{
					if (!(ex is TaskCanceledException) && !(ex is OperationCanceledException))
					{
						LogError(string.Format("{0} Exception: {1}", "BlobCacheQueueTask", ex));
					}
				}
			}
		}
		finally
		{
			LogInfo(string.Format("{0} Finished {1} with total active time {2:F3} s and {3} cached.", "BlobCacheQueueTask", (m_blobCacheQueue.Count > 0) ? $"with {m_blobCacheQueue.Count} remaining blobs in queue" : "", totalActiveTime.TotalSeconds, totalBytesCached.FormatSize()));
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		async ValueTask<bool> InnerLoop()
		{
			if (DateTime.Now - m_saveStorageContainer.LastAccessed < BlobCacheInactivityThreshold)
			{
				await Task.Delay(BlobCacheInactivityThreshold, cancellationToken);
				return true;
			}
			ulong num;
			lock (m_blobCacheLock)
			{
				if (m_blobCacheQueue.Count <= 0)
				{
					return false;
				}
				num = 0uL;
				blobIdsToCache.Clear();
				foreach (ulong key in m_blobCacheQueue.Keys)
				{
					uint num2 = m_blobCacheQueue[key];
					if (num != 0 && num + num2 > 1048576)
					{
						break;
					}
					blobIdsToCache.Add(key);
					num += num2;
				}
			}
			string[] blobNames;
			lock (m_blobIdToInfo)
			{
				blobNames = blobIdsToCache.Select([PublicizedFrom(EAccessModifier.Internal)] (ulong blobId) => (!m_blobIdToInfo.TryGetValue(blobId, out var value)) ? IdToString(blobId) : value.Name).ToArray();
			}
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			RefCountedBuffer[] blobs;
			try
			{
				blobs = m_saveStorageContainer.GetBlobs(blobNames, "BlobCacheQueueTask");
			}
			catch (IOException)
			{
				bool flag = false;
				lock (m_blobCacheLock)
				{
					foreach (ulong item in blobIdsToCache)
					{
						if (!m_blobCacheQueue.ContainsKey(item))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					throw;
				}
				return true;
			}
			try
			{
				lock (m_blobCacheLock)
				{
					for (int num3 = 0; num3 < blobIdsToCache.Count; num3++)
					{
						ulong num4 = blobIdsToCache[num3];
						if (m_blobCacheQueue.Remove(num4))
						{
							m_blobCache.Set(num4, blobs[num3]);
						}
					}
				}
				TimeSpan elapsed = microStopwatch.Elapsed;
				totalActiveTime += elapsed;
				totalBytesCached += num;
				LogInfo(string.Format("{0} Cached {1} blobs totaling {2} in {3:F3} ms.", "BlobCacheQueueTask", blobIdsToCache.Count, num.FormatSize(), elapsed.TotalMilliseconds));
			}
			finally
			{
				RefCountedBuffer[] array = blobs;
				for (int num5 = 0; num5 < array.Length; num5++)
				{
					array[num5].Dispose();
				}
			}
			return true;
		}
	}
}
