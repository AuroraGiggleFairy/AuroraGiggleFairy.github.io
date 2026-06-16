using System.Collections.Generic;
using System.IO;

public class SaveDataManager_Minimal : SaveDataManagerBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool concurrencyCheckEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataManagedPath, int> openCountMap = new Dictionary<SaveDataManagedPath, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Stream, SaveDataManagedPath> streamToPathMap = new Dictionary<Stream, SaveDataManagedPath>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataManager_Minimal instance;

	public static SaveDataManager_Minimal Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SaveDataManager_Minimal();
			}
			return instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		lock (lockObj)
		{
			Stream stream = new FileStream(path.GetOriginalPath(), mode, access, share);
			if (concurrencyCheckEnabled)
			{
				streamToPathMap[stream] = path;
				if (!openCountMap.ContainsKey(path))
				{
					openCountMap[path] = 1;
				}
				else
				{
					openCountMap[path]++;
					if (openCountMap[path] > 1)
					{
						Log.Error($"[SaveDataManager] Detected {openCountMap[path]} concurrent operations on file: {path}");
					}
				}
			}
			return stream;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReturnStream(Stream stream)
	{
		lock (lockObj)
		{
			if (concurrencyCheckEnabled)
			{
				SaveDataManagedPath key = streamToPathMap[stream];
				stream.Dispose();
				openCountMap[key] -= 1;
			}
			else
			{
				stream.Dispose();
			}
		}
	}
}
