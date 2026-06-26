using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public static class SaveDataUtils
{
	public const string BACKUP_FILE_EXTENSION = "bup";

	public const string BACKUP_FILE_EXTENSION_WITH_DOT = ".bup";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataManager s_saveDataManager;

	public static string s_saveDataRootPathPrefix;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataPrefs s_saveDataPrefs = SaveDataPrefsUninitialized.INSTANCE;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex s_managedPathRegex = new Regex("$^", RegexOptions.Compiled);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex s_managedPathRegexWithoutGroups = new Regex("$^", RegexOptions.Compiled);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataManager SaveDataManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataPrefs SaveDataPrefs
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = SaveDataPrefsUninitialized.INSTANCE;

	public static IEnumerator InitStaticCoroutine()
	{
		if (!s_initStatic)
		{
			s_initStatic = true;
			Log.Out("[SaveDataUtils] InitStatic Begin");
			UpdatePaths();
			s_saveDataManager = SaveDataManager_Placeholder.Instance;
			SaveDataManager = s_saveDataManager;
			SaveDataManager.Init();
			SdDirectory.CreateDirectory(GameIO.GetUserGameDataDir());
			if (LaunchPrefs.PlayerPrefsFile.Value)
			{
				Log.Out("[SaveDataUtils] SdPlayerPrefs -> SdFile");
				SaveDataPrefs = (s_saveDataPrefs = SaveDataPrefsFile.INSTANCE);
			}
			else
			{
				Log.Out("[SaveDataUtils] SdPlayerPrefs -> Unity");
				SaveDataPrefs = (s_saveDataPrefs = SaveDataPrefsUnity.INSTANCE);
			}
			Log.Out("[SaveDataUtils] InitStatic Complete");
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetSaveDataManagerOverride(ISaveDataManager saveDataManagerOverride)
	{
		if (saveDataManagerOverride == s_saveDataManager)
		{
			Log.Error("SetSaveDataManagerOverride failed: Cannot override default Save Data Manager with itself.");
		}
		else
		{
			SaveDataManager = saveDataManagerOverride;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ClearSaveDataManagerOverride()
	{
		if (SaveDataManager == s_saveDataManager)
		{
			Log.Error("ClearSaveDataManagerOverride failed: Save Data Manager override was not set or has already been cleared.");
			return;
		}
		SaveDataManager.Cleanup();
		SaveDataManager = null;
		SaveDataManager = s_saveDataManager;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetSaveDataPrefsOverride(ISaveDataPrefs saveDataPrefsOverride)
	{
		SaveDataPrefs = saveDataPrefsOverride;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ClearSaveDataPrefsOverride()
	{
		SaveDataPrefs = s_saveDataPrefs;
	}

	public static bool IsManaged(string path)
	{
		return false;
	}

	public static bool TryGetManagedPath(string path, out SaveDataManagedPath managedPath)
	{
		managedPath = null;
		return false;
	}

	public static SaveDataManagedPath GetBackupPath(SaveDataManagedPath restorePath)
	{
		return new SaveDataManagedPath(restorePath.PathRelativeToRoot + ".bup");
	}

	public static SaveDataManagedPath GetRestorePath(SaveDataManagedPath backupPath)
	{
		string pathRelativeToRoot = backupPath.PathRelativeToRoot;
		if (!pathRelativeToRoot.EndsWith(".bup"))
		{
			throw new ArgumentException(string.Format("Expected \"{0}\" to end with \"{1}\".", backupPath, ".bup"));
		}
		return new SaveDataManagedPath(pathRelativeToRoot.AsSpan(0, pathRelativeToRoot.Length - ".bup".Length));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdatePaths()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.Append("^(?:");
		stringBuilder2.Append("^(?:");
		string normalizedPath = GameIO.GetNormalizedPath(GameIO.GetUserGameDataDir());
		stringBuilder.Append("(?<1>");
		string value = Regex.Escape(normalizedPath);
		stringBuilder.Append(value);
		stringBuilder2.Append(value);
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder2.Append(')');
		stringBuilder.Append("(?:$|[\\\\/](?<2>.*)$)");
		stringBuilder2.Append("(?:$|[\\\\/])");
		s_managedPathRegex = new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		s_managedPathRegexWithoutGroups = new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		s_saveDataRootPathPrefix = normalizedPath;
	}

	public static void Destroy()
	{
		SdPlayerPrefs.Save();
		SaveDataManager.Cleanup();
		SaveDataPrefs = (s_saveDataPrefs = SaveDataPrefsUninitialized.INSTANCE);
		SaveDataManager = (s_saveDataManager = null);
	}
}
