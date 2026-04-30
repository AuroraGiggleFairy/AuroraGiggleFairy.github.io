using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

namespace Platform;

public class DLCTitleStorageManager
{
	public struct CatalogEntry
	{
		public DLCEnvironmentFlags environments;

		public DateTime? retailDate;
	}

	public const string RFSUri = "DLCConfiguration";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DLCTitleStorageManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<EntitlementSetEnum, CatalogEntry> dlcPurchasability = new Dictionary<EntitlementSetEnum, CatalogEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fetchAttempted;

	public static DLCTitleStorageManager Instance => instance ?? (instance = new DLCTitleStorageManager());

	[PublicizedFrom(EAccessModifier.Private)]
	public DLCTitleStorageManager()
	{
	}

	public bool IsDLCPurchasable(EntitlementSetEnum _dlcSet, DLCEnvironmentFlags _dlcEnvironments)
	{
		if (_dlcSet == EntitlementSetEnum.None)
		{
			return false;
		}
		if (!dlcPurchasability.TryGetValue(_dlcSet, out var value))
		{
			return false;
		}
		if ((value.environments & _dlcEnvironments) == 0)
		{
			return false;
		}
		if (_dlcEnvironments.HasFlag(DLCEnvironmentFlags.Retail) && value.retailDate.HasValue && value.retailDate.Value > DateTime.Now)
		{
			return false;
		}
		return true;
	}

	public void FetchFromSource()
	{
		if (fetchAttempted)
		{
			return;
		}
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		if (crossplatformPlatform != null && crossplatformPlatform.PlatformIdentifier == EPlatformIdentifier.EOS)
		{
			fetchAttempted = true;
			PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Private)] (IPlatform _) =>
			{
				ThreadManager.StartCoroutine(RequestDataCo());
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RequestDataCo()
	{
		IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
		if (storage == null)
		{
			Log.Warning("[DLCTitleStorageManager] No remote file storage implementation available.");
			yield break;
		}
		bool loggedSlow = false;
		float startTime = Time.time;
		while (!storage.IsReady)
		{
			if (storage.Unavailable)
			{
				Log.Warning("[DLCTitleStorageManager] Remote Storage is unavailable");
				yield break;
			}
			yield return null;
			if (!loggedSlow && Time.time > startTime + 30f)
			{
				loggedSlow = true;
				Log.Warning("[DLCTitleStorageManager] Waiting for DLC configuration from remote storage exceeded 30s");
			}
		}
		bool fileDownloadComplete = false;
		storage.GetFile("DLCConfiguration", fileDownloadedCallback);
		while (!fileDownloadComplete)
		{
			yield return null;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void fileDownloadedCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
		{
			try
			{
				if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
				{
					Log.Warning("[DLCTitleStorageManager] Retrieving DLC configuration file failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
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
							Log.Error("[DLCTitleStorageManager] Failed loading DLC configuration XML:");
							Log.Exception(e);
							return;
						}
					}
					XElement xElement = xmlFile?.XmlDoc.Root;
					string localPlatformNetworkString = GetLocalPlatformNetworkString();
					if (xElement != null && localPlatformNetworkString != null)
					{
						Log.Out("[DLCTitleStorageManager] Successfully retrieved DLC configuration.");
						{
							foreach (XElement item in xElement.Elements("platform"))
							{
								if (localPlatformNetworkString.Equals(item.Attribute("name")?.Value, StringComparison.OrdinalIgnoreCase))
								{
									foreach (XElement item2 in item.Elements("entitlement"))
									{
										if (Enum.TryParse<EntitlementSetEnum>(item2.Attribute("name").Value, out var result) && result != EntitlementSetEnum.None)
										{
											CatalogEntry value = default(CatalogEntry);
											string[] array = item2.Attribute("environments")?.Value.Split(',');
											if (array != null && array.Length != 0)
											{
												string[] array2 = array;
												foreach (string text in array2)
												{
													switch (text)
													{
													case "dev":
														value.environments |= DLCEnvironmentFlags.Dev;
														break;
													case "cert":
														value.environments |= DLCEnvironmentFlags.Cert;
														break;
													case "retail":
														value.environments |= DLCEnvironmentFlags.Retail;
														break;
													default:
														Log.Warning("[DLCTitleStoreManager] Unrecognized environment " + text + " in DLC config");
														break;
													}
												}
											}
											string text2 = item2.Attribute("retaildate")?.Value;
											if (text2 != null && DateTime.TryParseExact(text2, "u", null, DateTimeStyles.AssumeUniversal, out var result2))
											{
												value.retailDate = result2;
											}
											dlcPurchasability[result] = value;
										}
									}
									break;
								}
							}
							return;
						}
					}
					if (xElement == null)
					{
						Log.Error("[DLCTitleStorageManager] Failed to load root from DLC configuration XML.");
					}
					else
					{
						Log.Error("[DLCTitleStorageManager] Could not parse local platform/network for DLC configuration.");
					}
				}
			}
			finally
			{
				fileDownloadComplete = true;
			}
		}
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
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				return "XboxSeries_XBL";
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				return "PS5_PSN";
			}
		}
		return null;
	}
}
