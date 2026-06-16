using System;
using System.Collections;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class TitleStorageOverridesManager
{
	public struct TSOverrides
	{
		public bool Crossplay;
	}

	public const string RFSUri = "PlatformOverrides";

	[PublicizedFrom(EAccessModifier.Private)]
	public static TitleStorageOverridesManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastSuccess = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object fetchLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fetching;

	[PublicizedFrom(EAccessModifier.Private)]
	public TSOverrides overrides;

	public static TitleStorageOverridesManager Instance => instance ?? (instance = new TitleStorageOverridesManager());

	[method: PublicizedFrom(EAccessModifier.Private)]
	public event Action<TSOverrides> fetchFinished;

	public void FetchFromSource(Action<TSOverrides> _callback)
	{
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		if (crossplatformPlatform == null || crossplatformPlatform.PlatformIdentifier != EPlatformIdentifier.EOS)
		{
			_callback?.Invoke(new TSOverrides
			{
				Crossplay = true
			});
			return;
		}
		bool flag = false;
		lock (fetchLock)
		{
			if (!fetching)
			{
				flag = true;
				fetching = true;
			}
			fetchFinished += _callback;
		}
		if (flag)
		{
			ThreadManager.StartCoroutine(RequestDataCo());
		}
	}

	public void ClearOverrides()
	{
		overrides = default(TSOverrides);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalPlatformNetworkString()
	{
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.Steam)
			{
				return "Standalone_Steam";
			}
			if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
			{
				return "Standalone_XBL";
			}
		}
		else
		{
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				return "XboxSeriesX_XBL";
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				return "XboxSeriesS_XBL";
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				return "PS5_PSN";
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RequestDataCo()
	{
		bool fileDownloadComplete;
		try
		{
			if ((DateTime.Now - lastSuccess).TotalMinutes < 5.0)
			{
				Log.Out("[TitleStorageOverridesManager] Using cached last success.");
				yield break;
			}
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				Log.Warning("[TitleStorageOverridesManager] No remote file storage implementation available.");
				yield break;
			}
			bool loggedSlow = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("[TitleStorageOverridesManager] Remote Storage is unavailable");
					ClearOverrides();
					yield break;
				}
				yield return null;
				if (!loggedSlow && Time.time > startTime + 30f)
				{
					loggedSlow = true;
					Log.Warning("[TitleStorageOverridesManager] Waiting for title storage overrides from remote storage exceeded 30s");
				}
			}
			fileDownloadComplete = false;
			storage.GetFile("PlatformOverrides", fileDownloadedCallback);
			while (!fileDownloadComplete)
			{
				yield return null;
			}
		}
		finally
		{
			TitleStorageOverridesManager titleStorageOverridesManager = this;
			lock (titleStorageOverridesManager.fetchLock)
			{
				titleStorageOverridesManager.fetchFinished?.Invoke(titleStorageOverridesManager.overrides);
				titleStorageOverridesManager.fetchFinished = null;
				titleStorageOverridesManager.fetching = false;
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void fileDownloadedCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
		{
			try
			{
				if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
				{
					Log.Warning("[TitleStorageOverridesManager] Retrieving title storage overrides file failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
					ClearOverrides();
				}
				else
				{
					XmlFile xmlFile = null;
					if (_data != null && _data.Length != 0)
					{
						try
						{
							xmlFile = new XmlFile(_data, _throwExc: true);
						}
						catch (Exception e)
						{
							Log.Error("[TitleStorageOverridesManager] Failed loading title storage overrides XML:");
							Log.Exception(e);
							ClearOverrides();
							return;
						}
					}
					XElement xElement = xmlFile?.XmlDoc.Root;
					string localPlatformNetworkString = GetLocalPlatformNetworkString();
					if (xElement != null && localPlatformNetworkString != null)
					{
						lastSuccess = DateTime.Now;
						Log.Out("[TitleStorageOverridesManager] Successfully retrieved overrides.");
						{
							foreach (XElement item in xElement.Elements("platform"))
							{
								if (localPlatformNetworkString.Equals(item.Attribute("name")?.Value, StringComparison.OrdinalIgnoreCase))
								{
									bool crossplay = bool.Parse(item.Element("crossplay")?.Value ?? "false");
									overrides.Crossplay = crossplay;
									break;
								}
							}
							return;
						}
					}
					if (xElement == null)
					{
						Log.Error("[TitleStorageOverridesManager] Failed to load root from title storage override XML.");
					}
					else
					{
						Log.Error("[TitleStorageOverridesManager] Could not parse local platform/network for title storage overrides.");
					}
					ClearOverrides();
				}
			}
			finally
			{
				fileDownloadComplete = true;
			}
		}
	}
}
