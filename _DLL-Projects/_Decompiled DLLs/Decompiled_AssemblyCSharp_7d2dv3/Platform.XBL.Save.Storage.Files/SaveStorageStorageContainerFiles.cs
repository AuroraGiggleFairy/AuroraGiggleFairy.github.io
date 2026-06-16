using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.XGamingRuntime;

namespace Platform.XBL.Save.Storage.Files;

public sealed class SaveStorageStorageContainerFiles : ISaveStorageContainer, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LastAccessHelper m_lastAccessHelper = new LastAccessHelper();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_containerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_containerPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_disposed;

	public bool IsDisposed => m_disposed;

	public string Name => m_containerName;

	public DateTime LastAccessed => m_lastAccessHelper.Time;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: SaveStorageStorageContainerFiles] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveStorageStorageContainerFiles] " + text);
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: SaveStorageStorageContainerFiles] " + text);
	}

	public SaveStorageStorageContainerFiles(string gameSaveFolder, string containerName)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			bool flag = false;
			try
			{
				m_containerName = containerName;
				m_containerPath = Path.GetFullPath(Path.Join(gameSaveFolder, m_containerName));
				Directory.CreateDirectory(m_containerPath);
				if (!Directory.Exists(m_containerPath))
				{
					LogError("Container '" + containerName + "' failed to create directory at: " + m_containerPath);
					return;
				}
				LogInfo("Container '" + containerName + "' exists at: " + m_containerPath);
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
	}

	public void Dispose()
	{
		m_disposed = true;
	}

	public void Flush(bool waitForFlush)
	{
	}

	public bool TryEnumerateBlobInfos(out XGameSaveBlobInfo[] blobInfos)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			try
			{
				blobInfos = (from fi in new DirectoryInfo(m_containerPath).EnumerateFiles()
					select new XGameSaveBlobInfo
					{
						Name = fi.Name,
						Size = (uint)fi.Length
					}).ToArray();
				return true;
			}
			catch (IOException arg)
			{
				LogError(string.Format("{0} failed: {1}", "TryEnumerateBlobInfos", arg));
				blobInfos = null;
				return false;
			}
		}
	}

	public XGameSaveBlobInfo GetBlobInfo(string blobName)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			FileInfo fileInfo = new FileInfo(Path.Join(m_containerPath, blobName));
			if (!fileInfo.Exists)
			{
				return null;
			}
			return new XGameSaveBlobInfo
			{
				Name = blobName,
				Size = (uint)fileInfo.Length
			};
		}
	}

	public RefCountedBuffer[] GetBlobs(string[] blobNames, StringSpan debugIdentifier)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			bool flag = false;
			RefCountedBuffer[] array = new RefCountedBuffer[blobNames.Length];
			try
			{
				for (int i = 0; i < blobNames.Length; i++)
				{
					string blobName = blobNames[i];
					using FileStream fileStream = File.Open(GetBlobPath(blobName), FileMode.Open, FileAccess.Read, FileShare.Read);
					int num = (int)fileStream.Length;
					using RefCountedBuffer refCountedBuffer = RefCountedBuffer.CreatePooled(num);
					int num2;
					for (int j = 0; j < num; j += num2)
					{
						num2 = fileStream.Read(refCountedBuffer.Span.Slice(j, num - j));
						if (num2 <= 0)
						{
							throw new IOException("Unexpected end of file stream.");
						}
					}
					array[i] = refCountedBuffer.CreateRef();
				}
				flag = true;
				return array;
			}
			finally
			{
				if (!flag)
				{
					RefCountedBuffer[] array2 = array;
					for (int k = 0; k < array2.Length; k++)
					{
						array2[k]?.Dispose();
					}
				}
			}
		}
	}

	public void SetBlob(string blobName, RefCountedBuffer blobData)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			using FileStream fileStream = File.Open(GetBlobPath(blobName), FileMode.Create, FileAccess.Write);
			fileStream.Write(blobData.Span);
		}
	}

	public void DeleteBlob(string blobName)
	{
		using (m_lastAccessHelper.CreateScope())
		{
			File.Delete(GetBlobPath(blobName));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetBlobPath(string blobName)
	{
		return Path.Join(m_containerPath, blobName);
	}
}
