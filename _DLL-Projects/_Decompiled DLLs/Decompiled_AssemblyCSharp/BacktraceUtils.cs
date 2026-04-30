using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Backtrace.Unity;
using Backtrace.Unity.Model;
using Backtrace.Unity.Types;
using Platform;
using UnityEngine;

public static class BacktraceUtils
{
	public enum SaveArchiveResult
	{
		FailureToArchive,
		Success,
		MissingRegions
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long BacktraceFileSizeLimitMebibyte = 30L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BacktraceClient s_BacktraceClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string s_PlayerLogAttachmentPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string s_svncommit = "81384";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string s_platformString;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName enabled = "enabled";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName excludedversions = "excludedversions";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName sampling = "sampling";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName deduplicationStrategy = "deduplicationstrategy";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName minidumptype = "minidumptype";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName enablemetricssupport = "enablemetricssupport";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName bugreporting = "bugreportfeature";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName attachsaves = "bugreportattachsaves";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName attachentireworld = "bugreportattachentireworld";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName errorreportdisabledmessagetypes = "errorreportdisabledmessagetypes";

	[PublicizedFrom(EAccessModifier.Private)]
	public static BacktraceConfiguration s_Configuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BacktraceEnabled = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_VersionEnabled = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportFeature = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportAttachSaveFeature = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportAttachWholeWorldFeature = false;

	public static List<string> ErrorReportingDisabledMessageTypes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string userIdString = string.Empty;

	public static bool Initialized => s_Configuration != null;

	public static bool Enabled
	{
		get
		{
			if (s_BacktraceEnabled)
			{
				return s_VersionEnabled;
			}
			return false;
		}
	}

	public static bool BugReportFeature
	{
		get
		{
			if (Enabled)
			{
				return s_BugReportFeature;
			}
			return false;
		}
	}

	public static bool BugReportAttachSaveFeature
	{
		get
		{
			if (Enabled && s_BugReportFeature)
			{
				return s_BugReportAttachSaveFeature;
			}
			return false;
		}
	}

	public static bool BugReportAttachWholeWorldFeature
	{
		get
		{
			if (s_BugReportFeature && s_BugReportAttachSaveFeature)
			{
				return s_BugReportAttachWholeWorldFeature;
			}
			return false;
		}
	}

	public static void InitializeBacktrace()
	{
		InitializeConfiguration();
		InitializeBacktraceClient();
		Log.Out("Backtrace Initialized");
	}

	public static void BacktraceUserLoggedIn(IPlatform platform)
	{
		Log.Out($"[BACKTRACE] Attempting to get User ID from platform: {platform?.PlatformIdentifier}");
		if (platform?.User?.PlatformUserId == null)
		{
			Log.Out($"[BACKTRACE] {platform?.PlatformIdentifier} PlatformUserId missing at this time");
			return;
		}
		userIdString = platform.User.PlatformUserId.PlatformIdentifierString + "-" + platform.User.PlatformUserId.ReadablePlatformUserIdentifier;
		RefreshUserIdAttribute();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RefreshUserIdAttribute()
	{
		if (!(s_BacktraceClient == null) && !string.IsNullOrEmpty(userIdString))
		{
			s_BacktraceClient.SetAttributes(new Dictionary<string, string> { { "gamestats.platformuserid", userIdString } });
			Log.Out("[BACKTRACE] Platform ID set to: \"" + userIdString + "\"");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Reset()
	{
		s_BacktraceEnabled = false;
		s_VersionEnabled = false;
		s_Configuration.HandleUnhandledExceptions = false;
		s_Configuration.Sampling = 0.01;
		s_Configuration.CaptureNativeCrashes = true;
		s_Configuration.MinidumpType = MiniDumpType.Normal;
		s_Configuration.DeduplicationStrategy = DeduplicationStrategy.Default;
	}

	public static void SendBugReport(string message, string screenshotPath = null, SaveInfoProvider.SaveEntryInfo saveEntry = null, bool sendSave = false, Action<BacktraceResult> callback = null)
	{
		if (BugReportFeature)
		{
			if (s_BacktraceClient == null)
			{
				DebugEnableBacktrace();
			}
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(screenshotPath))
			{
				list.Add(screenshotPath);
			}
			if (saveEntry != null)
			{
				if (BugReportAttachSaveFeature && sendSave)
				{
					bool flag = false;
					string archivePath;
					SaveArchiveResult saveArchiveResult = TryCreateSaveArchive(saveEntry, out archivePath);
					switch (saveArchiveResult)
					{
					case SaveArchiveResult.Success:
						Log.Out("[BACKTRACE] Save file path: " + archivePath);
						list.Add(archivePath);
						break;
					case SaveArchiveResult.MissingRegions:
					{
						list.Add(archivePath);
						if (TryCreateRegionArchives(saveEntry, out var archivePaths))
						{
							list.AddRange(archivePaths);
							Log.Out("[BACKTRACE] region file paths: " + string.Join(", ", archivePaths));
							saveArchiveResult = SaveArchiveResult.Success;
							flag = true;
						}
						break;
					}
					}
					if (saveArchiveResult != SaveArchiveResult.FailureToArchive && TryCreateWorldArchive(saveEntry.WorldEntry, out var archivePath2) && archivePath2 != null)
					{
						list.Add(archivePath2);
						Log.Out("[BACKTRACE] World file path: " + archivePath2);
						saveArchiveResult = SaveArchiveResult.Success;
						flag = true;
					}
					if (saveArchiveResult == SaveArchiveResult.Success && flag)
					{
						list = CondenseZipFiles(saveEntry, list);
					}
					Log.Out("[BACKTRACE] File Sizes:");
					foreach (string item in list)
					{
						Log.Out($"[BACKTRACE] {item}: {(double)new SdFileInfo(item).Length / 1024.0 / 1024.0:N3}MB");
					}
				}
				string text = Path.Combine(saveEntry.WorldEntry.Location.FullPath, "checksums.txt");
				if (SdFile.Exists(text))
				{
					string text2 = Path.Combine(PlatformApplicationManager.Application.temporaryCachePath, "backtraceTemp", "checksums.txt");
					Log.Out("[BACKTRACE] World file path: " + saveEntry.WorldEntry.Location.FullPath);
					if (!SdDirectory.Exists(text2))
					{
						SdDirectory.CreateDirectory(Path.GetDirectoryName(text2));
					}
					SdFile.Copy(text, text2, overwrite: true);
					list.Add(text2);
				}
				string text3 = Path.Combine(saveEntry.WorldEntry.Location.FullPath, "map_info.xml");
				if (SdFile.Exists(text3))
				{
					string text4 = Path.Combine(PlatformApplicationManager.Application.temporaryCachePath, "backtraceTemp", "map_info.xml");
					if (!SdDirectory.Exists(text4))
					{
						SdDirectory.CreateDirectory(Path.GetDirectoryName(text4));
					}
					SdFile.Copy(text3, text4, overwrite: true);
					list.Add(text4);
				}
			}
			callback = ((callback == null) ? new Action<BacktraceResult>(ClearMessageTypeAttribute) : ((Action<BacktraceResult>)Delegate.Combine(callback, new Action<BacktraceResult>(ClearMessageTypeAttribute))));
			SetAttribute("messagetype", "bugreport");
			s_BacktraceClient?.Send(message, callback, list);
		}
		else
		{
			Log.Out("[BACKTRACE] Backtrace bug reporting disabled by platform");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ClearMessageTypeAttribute(BacktraceResult _result)
	{
		SetAttribute("messagetype", null);
	}

	public static void SendErrorReport(string messageType, string message, List<string> files = null)
	{
		if (ErrorReportingDisabledMessageTypes.Contains(messageType))
		{
			Log.Out("Opted out of reporting error of type " + messageType);
			return;
		}
		if (s_BacktraceClient == null)
		{
			DebugEnableBacktrace();
		}
		SetAttribute("messagetype", messageType);
		if (files != null)
		{
			s_BacktraceClient?.Send(message, ClearMessageTypeAttribute, files);
		}
		else
		{
			s_BacktraceClient?.Send(message, ClearMessageTypeAttribute);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> CondenseZipFiles(SaveInfoProvider.SaveEntryInfo saveEntry, List<string> attachmentPaths)
	{
		string text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "/combined_" + saveEntry.WorldEntry.Name + "_" + saveEntry.Name + "/");
		string text2 = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "/combined_" + saveEntry.WorldEntry.Name + "_" + saveEntry.Name + ".zip");
		if (SdDirectory.Exists(text))
		{
			SdDirectory.Delete(text, recursive: true);
		}
		SdDirectory.CreateDirectory(text);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>(attachmentPaths);
		foreach (string attachmentPath in attachmentPaths)
		{
			if (Path.GetFileName(attachmentPath).ContainsCaseInsensitive("zip"))
			{
				string text3 = Path.Join(text, Path.GetFileNameWithoutExtension(attachmentPath));
				char altDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
				string text4 = text3 + altDirectorySeparatorChar;
				SdDirectory.CreateDirectory(text4);
				ZipFile.ExtractToDirectory(attachmentPath, text4);
				list.Add(attachmentPath);
				list2.Remove(attachmentPath);
			}
		}
		using (Stream stream = SdFile.Open(text2, FileMode.Create, FileAccess.Write))
		{
			using ZipArchive archiveZip = new ZipArchive(stream, ZipArchiveMode.Create);
			archiveZip.CreateFromDirectory(text);
		}
		SdDirectory.Delete(text, recursive: true);
		if ((double)new SdFileInfo(text2).Length / 1024.0 / 1024.0 <= 30.0)
		{
			Log.Out("[BACKTRACE] Created combined archive: " + text2);
			Log.Out("[BACKTRACE] Remove the following: " + string.Join(", ", list));
			attachmentPaths = list2;
			attachmentPaths.Add(text2);
			foreach (string item in list)
			{
				SdFile.Delete(item);
			}
		}
		else
		{
			Log.Out("[BACKTRACE] Combined archive is too large to upload: " + text2);
			SdFile.Delete(text2);
		}
		return attachmentPaths;
	}

	public static SaveArchiveResult TryCreateSaveArchive(SaveInfoProvider.SaveEntryInfo saveEntry, out string archivePath)
	{
		try
		{
			string saveGameDir = GameIO.GetSaveGameDir(saveEntry.WorldEntry.Name, saveEntry.Name);
			archivePath = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "Save_" + saveEntry.WorldEntry.Name + "_" + saveEntry.Name + ".zip");
			if (SdFile.Exists(archivePath))
			{
				Log.Out("[BACKTRACE] Old save archive path: {0} exists, deleting...", archivePath);
				SdFile.Delete(archivePath);
			}
			using (Stream stream = SdFile.OpenWrite(archivePath))
			{
				using ZipArchive archiveZip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false, null);
				archiveZip.CreateFromDirectory(saveGameDir);
			}
			double num = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
			if (SdFile.Exists(archivePath) && num <= 30.0)
			{
				if (new SdFileInfo(archivePath).Length != 0L)
				{
					Log.Out("[BACKTRACE] Save archive path: {0} exists, size is {1}MB, success!", archivePath, num.ToString("N3"));
					return SaveArchiveResult.Success;
				}
				Log.Warning("[BACKTRACE] Save archive exists: {0}, but is empty, retry", archivePath);
				SdFile.Delete(archivePath);
			}
			else if (SdFile.Exists(archivePath))
			{
				Log.Out("[BACKTRACE] Save archive path: {0} exists, size is too big, {1}MB, deleting...", archivePath, num.ToString("N3"));
				SdFile.Delete(archivePath);
			}
			using (ZipArchive destination = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				SdDirectoryInfo directoryInfo = new SdDirectoryInfo(saveGameDir);
				destination.AddSearchPattern(directoryInfo, "*.txt", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.sdf", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.ttw", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.xml", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.7dt", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.nim", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.dat", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.ttp", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.ttp.meta", SearchOption.AllDirectories);
				destination.AddSearchPattern(directoryInfo, "*.7rm", SearchOption.AllDirectories);
			}
			if (SdFile.Exists(archivePath))
			{
				return SaveArchiveResult.MissingRegions;
			}
			return SaveArchiveResult.FailureToArchive;
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE]  Exception: Could not create save archive: {0}", ex.Message);
			archivePath = null;
			return SaveArchiveResult.FailureToArchive;
		}
	}

	public static bool TryCreateRegionArchives(SaveInfoProvider.SaveEntryInfo saveEntry, out List<string> archivePaths)
	{
		try
		{
			string saveGameDir = GameIO.GetSaveGameDir(saveEntry.WorldEntry.Name, saveEntry.Name);
			archivePaths = new List<string>();
			foreach (SdFileSystemInfo item in from info in new SdDirectoryInfo(PlatformApplicationManager.Application.temporaryCachePath).EnumerateFileSystemInfos()
				where info.Name.EndsWith("_Region.zip", StringComparison.InvariantCulture)
				select info)
			{
				try
				{
					SdFile.Delete(item.FullName);
				}
				catch (Exception arg)
				{
					Log.Warning($"[BACKTRACE] Failed To Delete {item.FullName}, Reason {arg}");
				}
			}
			int num = 0;
			string text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, $"Save_{saveEntry.WorldEntry.Name}_{saveEntry.Name}_{num}_Region.zip");
			Log.Out("[BACKTRACE] World archive path: {0}", text);
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(saveGameDir);
			IEnumerable<SdFileSystemInfo> collection = (from fileSystemInfo in sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories)
				orderby fileSystemInfo.LastWriteTime.ToFileTimeUtc()
				select fileSystemInfo).Reverse();
			if (!(from fileSystemInfo in sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories)
				orderby fileSystemInfo.LastWriteTime.ToFileTimeUtc()
				select fileSystemInfo).Reverse().Any())
			{
				collection = sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories);
			}
			Queue<SdFileSystemInfo> queue = new Queue<SdFileSystemInfo>(collection);
			while (queue.Any())
			{
				collection = queue;
				foreach (SdFileSystemInfo item2 in collection)
				{
					SdFileInfo sdFileInfo = new SdFileInfo(item2.FullName);
					Log.Out($"{sdFileInfo.FullName} File size: {(float)sdFileInfo.Length / 1024f * 1024f} MiB");
					if (sdFileInfo.Length > 15728640)
					{
						Log.Warning("File too big! " + text);
						continue;
					}
					using (Stream stream = SdFile.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{
						using ZipArchive destination = new ZipArchive(stream, ZipArchiveMode.Update, leaveOpen: false, null);
						destination.CreateEntryFromFile(item2, item2.Name, System.IO.Compression.CompressionLevel.Optimal);
					}
					if (new SdFileInfo(text).Length > 31457280)
					{
						Log.Warning("Archive too big! " + text);
						archivePaths.Add(text);
						break;
					}
					queue.Dequeue();
				}
				archivePaths.Add(text);
				num++;
				text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, $"Save_{saveEntry.WorldEntry.Name}_{saveEntry.Name}_{num}_Region.zip");
			}
			archivePaths = new List<string>(archivePaths.Distinct());
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE] Exception: Could not create save region archive: {0}", ex.Message);
			archivePaths = new List<string>();
			return false;
		}
	}

	public static bool TryCreateWorldArchive(SaveInfoProvider.WorldEntryInfo worldEntry, out string archivePath)
	{
		try
		{
			string fullPath = Path.GetFullPath(GameIO.GetWorldDir(worldEntry.Name));
			if (PathAbstractions.WorldsSearchPaths.GetLocation(fullPath).Type == PathAbstractions.EAbstractedLocationType.GameData)
			{
				Log.Out("[BACKTRACE] World {0} was found in the GameData, do not archive.", worldEntry.Name);
				archivePath = string.Empty;
				return false;
			}
			string fullPath2 = Path.GetFullPath(GameIO.GetUserGameDataDir());
			if (!fullPath.Contains(fullPath2))
			{
				Log.Out("[BACKTRACE] Not creating archive for world {0} not in the User Game Data Directory", worldEntry.Name);
				archivePath = null;
				return false;
			}
			Log.Out("[BACKTRACE] Creating archive for GeneratedWorld {0}", worldEntry.Name);
			archivePath = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "/world_" + worldEntry.Name + ".zip");
			if (SdFile.Exists(archivePath))
			{
				Log.Out("[BACKTRACE] Old World archive path: {0} exists, deleting...", archivePath);
			}
			Log.Out("[BACKTRACE] World archive path: {0}. Creating archive...", archivePath);
			if (BugReportAttachWholeWorldFeature)
			{
				using (Stream stream = SdFile.Open(archivePath, FileMode.Create, FileAccess.ReadWrite))
				{
					using ZipArchive archiveZip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false, null);
					archiveZip.CreateFromDirectory(fullPath);
				}
				double num = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
				if (SdFile.Exists(archivePath) && (BugReportAttachWholeWorldFeature || num <= 30.0))
				{
					if (new SdFileInfo(archivePath).Length != 0L)
					{
						Log.Out("[BACKTRACE] World archive path: {0} exists, size is {1}MB, success!", archivePath, num.ToString("N3"));
						return true;
					}
					Log.Warning("[BACKTRACE] World archive exists: {0}, but is empty, retry", archivePath);
					SdFile.Delete(archivePath);
				}
				else if (SdFile.Exists(archivePath))
				{
					Log.Out("[BACKTRACE] World archive path: {0} exists, size is too big, {1}MB, deleting...", archivePath, num.ToString("N3"));
					SdFile.Delete(archivePath);
				}
			}
			Log.Out("[BACKTRACE] Attempting to archive only required elements...", archivePath);
			using (Stream stream2 = SdFile.Open(archivePath, FileMode.Create, FileAccess.ReadWrite))
			{
				using ZipArchive destination = new ZipArchive(stream2, ZipArchiveMode.Update, leaveOpen: false, null);
				SdDirectoryInfo directoryInfo = new SdDirectoryInfo(fullPath);
				destination.AddSearchPattern(directoryInfo, "*.ttw", SearchOption.TopDirectoryOnly);
				destination.AddSearchPattern(directoryInfo, "*.xml", SearchOption.TopDirectoryOnly);
				destination.AddSearchPattern(directoryInfo, "*.txt", SearchOption.TopDirectoryOnly);
			}
			if (SdFile.Exists(archivePath))
			{
				double num2 = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
				Log.Out("[BACKTRACE] World archive path: {0}", archivePath);
				Log.Out("[BACKTRACE] World archive File size: {0}MB", num2.ToString("N3"));
				if (!BugReportAttachWholeWorldFeature && num2 > 30.0)
				{
					SdFile.Delete(archivePath);
					archivePath = null;
					Log.Error("[BACKTRACE] Exception: World archive too big, deleted");
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE] Exception: Could not create world archive: {0}", ex.Message);
			archivePath = null;
			return false;
		}
	}

	public static void DebugEnableBacktrace()
	{
		s_BacktraceEnabled = true;
		s_VersionEnabled = true;
		InitializeBacktraceClient();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitializeBacktraceClient()
	{
		if (Enabled)
		{
			if (s_Configuration == null)
			{
				InitializeConfiguration();
			}
			s_BacktraceClient = BacktraceClient.Initialize(s_Configuration);
			SetAttributes(new Dictionary<string, string>
			{
				{ "svn.commit", s_svncommit },
				{
					"game.version",
					Constants.cVersionInformation.SerializableString
				}
			});
			BacktraceClient backtraceClient = s_BacktraceClient;
			backtraceClient.OnServerError = (Action<Exception>)Delegate.Combine(backtraceClient.OnServerError, (Action<Exception>)([PublicizedFrom(EAccessModifier.Internal)] (Exception e) =>
			{
				Log.Error("[BACKTRACE] Error response: " + e.Message);
			}));
			BacktraceClient backtraceClient2 = s_BacktraceClient;
			backtraceClient2.OnClientReportLimitReached = (Action<BacktraceReport>)Delegate.Combine(backtraceClient2.OnClientReportLimitReached, (Action<BacktraceReport>)([PublicizedFrom(EAccessModifier.Internal)] (BacktraceReport e) =>
			{
				Log.Error("[BACKTRACE] Report Limit Reached Error: " + e.Message);
			}));
			RefreshUserIdAttribute();
		}
		else
		{
			s_BacktraceClient = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitializeConfiguration()
	{
		s_Configuration = ScriptableObject.CreateInstance<BacktraceConfiguration>();
		s_Configuration.ServerUrl = "https://thefunpimps.sp.backtrace.io:6098/post?format=json&token=4deafd275ace1a865cc35c882f48a2d1f848c59fabea40227c1bfd84d9c794d9";
		s_Configuration.Enabled = true;
		s_platformString = DeviceFlag.StandaloneWindows.ToString().ToUpper();
		Reset();
		lock (_lock)
		{
			s_PlayerLogAttachmentPath = Path.Join(Application.temporaryCachePath, "Player.log");
			Log.AddOutputPath(s_PlayerLogAttachmentPath);
			s_Configuration.AttachmentPaths = new string[1] { s_PlayerLogAttachmentPath };
		}
	}

	public static void StartStatisticsUpdate()
	{
		DebugGameStats.StartStatisticsUpdate(SetAttributes);
	}

	public static void SetAttributes(Dictionary<string, string> _attributesDictionary)
	{
		s_BacktraceClient?.SetAttributes(_attributesDictionary);
	}

	public static void SetAttribute(string _attributeName, string _attributeValue)
	{
		s_BacktraceClient?.SetAttributes(new Dictionary<string, string> { { _attributeName, _attributeValue } });
	}

	public static void UpdateConfig(XmlFile _xmlFile)
	{
		if (s_Configuration == null)
		{
			return;
		}
		Reset();
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			Log.Out("Could not load Backtrace Config from file " + _xmlFile.Filename + ".");
			return;
		}
		s_VersionEnabled = true;
		foreach (XElement item in root.Elements("platform"))
		{
			ParsePlatform(item);
		}
		InitializeBacktraceClient();
		Log.Out($"Backtrace Configuration refreshed from XML: Enabled {Enabled}");
		Log.Out("[BACKTRACE] Bug reporting: " + (s_BugReportFeature ? "Enabled" : "Disabled"));
		Log.Out("[BACKTRACE] Bug reporting attach save feature: " + (s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
		Log.Out("[BACKTRACE] Bug reporting attach entire world feature: " + (s_BugReportAttachWholeWorldFeature ? "Enabled" : "Disabled"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePlatform(XElement _element)
	{
		if (!_element.TryGetAttribute("name", out var _result))
		{
			throw new XmlLoadException("BacktraceConfig", _element, "Platform node attribute 'name' missing");
		}
		if (!(_result.ToUpper() == "DEFAULT") && !(_result.ToUpper() == s_platformString))
		{
			return;
		}
		foreach (XElement item in _element.Elements())
		{
			ParsePlatformElement(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePlatformElement(XElement _element)
	{
		if (_element.Name == enabled)
		{
			s_BacktraceEnabled = _element.TryGetAttribute("value", out var _result) && _result.Equals("true");
		}
		else if (_element.Name == excludedversions)
		{
			_element.TryGetAttribute("value", out var _result2);
			if (!s_VersionEnabled)
			{
				return;
			}
			string[] array = _result2.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				s_VersionEnabled = !Constants.cVersionInformation.SerializableString.ContainsCaseInsensitive(text);
				if (!s_VersionEnabled)
				{
					Log.Out("[BACKTRACE] version " + text + " is Excluded, and matches current version, backtrace disabled");
					break;
				}
			}
		}
		else if (_element.Name == sampling)
		{
			_element.TryGetAttribute("value", out var _result3);
			if (double.TryParse(_result3, out var result))
			{
				s_Configuration.Sampling = result;
			}
		}
		else if (_element.Name == deduplicationStrategy)
		{
			_element.TryGetAttribute("value", out var _result4);
			if (Enum.TryParse<DeduplicationStrategy>(_result4, out var result2))
			{
				s_Configuration.DeduplicationStrategy = result2;
			}
		}
		else if (_element.Name == minidumptype)
		{
			_element.TryGetAttribute("value", out var _result5);
			if (Enum.TryParse<MiniDumpType>(_result5, out var result3))
			{
				s_Configuration.MinidumpType = result3;
			}
		}
		else if (_element.Name == enablemetricssupport)
		{
			s_Configuration.EnableMetricsSupport = _element.TryGetAttribute("value", out var _result6) && _result6.Equals("true");
		}
		else if (_element.Name == bugreporting)
		{
			s_BugReportFeature = _element.TryGetAttribute("value", out var _result7) && _result7.Equals("true");
			Log.Out("[BACKTRACE] Bug reporting " + (s_BugReportFeature ? "Enabled" : "Disabled") + " with save uploading: " + (s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
		}
		else if (_element.Name == attachsaves)
		{
			s_BugReportAttachSaveFeature = _element.TryGetAttribute("value", out var _result8) && _result8.Equals("true");
			Log.Out("[BACKTRACE] Save Attaching Feature: " + (s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
		}
		else if (_element.Name == attachentireworld)
		{
			s_BugReportAttachWholeWorldFeature = _element.TryGetAttribute("value", out var _result9) && _result9.Equals("true");
			Log.Out("[BACKTRACE] Entire World Attaching Feature: " + (s_BugReportAttachWholeWorldFeature ? "Enabled" : "Disabled"));
		}
		else if (_element.Name == errorreportdisabledmessagetypes)
		{
			_element.TryGetAttribute("value", out var _result10);
			string[] array2 = _result10.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			Log.Out("[BACKTRACE] Disabling Error Report Message Types: " + string.Join(", ", array2));
			ErrorReportingDisabledMessageTypes.AddRange(array2);
		}
	}
}
