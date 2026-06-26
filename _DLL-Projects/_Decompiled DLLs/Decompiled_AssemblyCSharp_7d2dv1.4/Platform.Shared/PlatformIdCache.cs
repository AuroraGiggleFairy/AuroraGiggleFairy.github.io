using System.IO;

namespace Platform.Shared;

public static class PlatformIdCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string idCacheFile = "PlatformIdCache.txt";

	public static string IdFilePath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Path.Combine(GameIO.GetUserGameDataDir(), "PlatformIdCache.txt");
		}
	}

	public static bool TryGetCachedId<T>(out T _platformUserIdentifier) where T : PlatformUserIdentifierAbs
	{
		string idFilePath = IdFilePath;
		if (SdFile.Exists(idFilePath))
		{
			using (Stream stream = SdFile.OpenRead(idFilePath))
			{
				using StreamReader streamReader = new StreamReader(stream);
				string text = streamReader.ReadLine();
				if (text != null)
				{
					PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(text);
					if (!(platformUserIdentifierAbs is T))
					{
						Log.Error($"[PlatformIdCache] cannot retrieved cached id {text} as {typeof(T)}");
						_platformUserIdentifier = null;
						return false;
					}
					_platformUserIdentifier = (T)platformUserIdentifierAbs;
					return true;
				}
				Log.Out("[PlatformIdCache] no cached user id");
				_platformUserIdentifier = null;
				return false;
			}
		}
		Log.Out("[PlatformIdCache] no id cache file at " + idFilePath);
		_platformUserIdentifier = null;
		return false;
	}

	public static void SetCachedId(PlatformUserIdentifierAbs _platformUserIdentifier)
	{
		using Stream stream = SdFile.OpenWrite(IdFilePath);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.WriteLine(_platformUserIdentifier.CombinedString);
	}
}
