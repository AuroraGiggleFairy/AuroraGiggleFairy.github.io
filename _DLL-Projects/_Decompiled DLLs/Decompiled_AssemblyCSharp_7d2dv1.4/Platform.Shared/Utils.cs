using System;
using System.IO;
using System.Text;
using System.Threading;
using InControl;
using UnityEngine;

namespace Platform.Shared;

public class Utils : IUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int Seed = Environment.TickCount;

	public static readonly ThreadLocal<System.Random> RandLocal = new ThreadLocal<System.Random>([PublicizedFrom(EAccessModifier.Internal)] () => new System.Random(Interlocked.Increment(ref Seed)));

	[PublicizedFrom(EAccessModifier.Private)]
	public string platformLanguageCache;

	public virtual void Init(IPlatform _owner)
	{
	}

	public virtual bool OpenBrowser(string _url)
	{
		return global::Utils.OpenSystemBrowser(_url);
	}

	public void ControllerDisconnected(InputDevice inputDevice)
	{
	}

	public virtual string GetPlatformLanguage()
	{
		if (platformLanguageCache == null)
		{
			string text = Application.systemLanguage.ToStringCached().ToLower();
			platformLanguageCache = text switch
			{
				"chinesesimplified" => "schinese", 
				"chinesetraditional" => "tchinese", 
				"korean" => "koreana", 
				_ => text, 
			};
		}
		return platformLanguageCache;
	}

	public virtual string GetAppLanguage()
	{
		return GetPlatformLanguage();
	}

	public virtual string GetCountry()
	{
		return "??";
	}

	public virtual void ClearTempFiles()
	{
		TryDeleteTempCacheContents();
	}

	public virtual string GetTempFileName(string prefix = "", string suffix = "")
	{
		return GetRandomTempCacheFileName(prefix, suffix);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void TryDeleteTempCacheContents()
	{
		TryDeleteTempDirectoryContentsExceptCrashes(PlatformApplicationManager.Application.temporaryCachePath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TryDeleteTempDirectoryContentsExceptCrashes(string path)
	{
		try
		{
			if (!Directory.Exists(path))
			{
				return;
			}
			foreach (FileSystemInfo item in new DirectoryInfo(path).EnumerateFileSystemInfos())
			{
				try
				{
					if (!item.Name.EqualsCaseInsensitive("Crashes"))
					{
						if (item is DirectoryInfo directoryInfo)
						{
							directoryInfo.Delete(recursive: true);
						}
						else
						{
							item.Delete();
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning("[Platform.Shared.Utils] Could not delete '" + item.Name + "' from temp cache. " + ex.GetType().FullName + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Warning("[Platform.Shared.Utils] Could not delete contents of temp cache. " + ex2.GetType().FullName + ": " + ex2.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string GetRandomTempCacheFileName(string prefix, string suffix)
	{
		return GetRandomFileName(PlatformApplicationManager.Application.temporaryCachePath, prefix, suffix);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetRandomFileName(string parentDir, string prefix, string suffix)
	{
		for (int i = 0; i < 100; i++)
		{
			string randomName = GetRandomName(prefix, suffix);
			string text = Path.Join(parentDir, randomName);
			if (!File.Exists(text))
			{
				using (File.Open(text, FileMode.OpenOrCreate))
				{
					return text;
				}
			}
		}
		throw new IOException($"Failed to create a temporary file after {100} attempts.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetRandomName(string prefix, string suffix)
	{
		System.Random value = RandLocal.Value;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix);
		for (int i = 0; i < 16; i++)
		{
			stringBuilder.Append("0123456789ABCDEFGHIJKLMNOPabcdefghijklmnop"[value.Next("0123456789ABCDEFGHIJKLMNOPabcdefghijklmnop".Length)]);
		}
		stringBuilder.Append(suffix);
		return stringBuilder.ToString();
	}

	public virtual string GetCrossplayPlayerIcon(EPlayGroup _playGroup, bool _fetchGenericIcons, EPlatformIdentifier _nativePlatform)
	{
		return string.Empty;
	}
}
