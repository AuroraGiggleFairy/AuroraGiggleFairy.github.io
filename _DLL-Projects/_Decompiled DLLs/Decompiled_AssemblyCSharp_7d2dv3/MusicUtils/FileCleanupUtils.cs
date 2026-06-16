using System.Collections.Generic;

namespace MusicUtils;

public static class FileCleanupUtils
{
	public static List<string> paths = new List<string>();

	public static void CleanUpAllWaveFiles()
	{
		for (int i = 0; i < paths.Count; i++)
		{
			CleanUpWaveFile(paths[i]);
		}
	}

	public static void CleanUpWaveFile(string file)
	{
		WaveCleanUp.Create().GetComponent<WaveCleanUp>().FilePath = file;
	}
}
