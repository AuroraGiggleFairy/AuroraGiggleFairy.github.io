using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;

namespace Platform.EOS;

public static class EosHelpers
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class EosConnectionTestInfo
	{
		public Action<bool> Callback;

		public bool Result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<ExternalAccountType, EPlatformIdentifier> accountTypeMappings;

	public static readonly ReadOnlyDictionary<ExternalAccountType, EPlatformIdentifier> AccountTypeMappings;

	public static readonly ReadOnlyDictionary<EPlatformIdentifier, ExternalAccountType> PlatformIdentifierMappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform> deviceTypeToAntiCheatPlatformMappings;

	public static readonly ReadOnlyDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform> DeviceTypeToAntiCheatPlatformMappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosApiUrl = "https://api.epicgames.dev/sdk/v1/default";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int eosApiTestTimeout = 5000;

	[PublicizedFrom(EAccessModifier.Private)]
	static EosHelpers()
	{
		accountTypeMappings = new EnumDictionary<ExternalAccountType, EPlatformIdentifier>
		{
			{
				ExternalAccountType.Epic,
				EPlatformIdentifier.EGS
			},
			{
				ExternalAccountType.Psn,
				EPlatformIdentifier.PSN
			},
			{
				ExternalAccountType.Steam,
				EPlatformIdentifier.Steam
			},
			{
				ExternalAccountType.Xbl,
				EPlatformIdentifier.XBL
			}
		};
		AccountTypeMappings = new ReadOnlyDictionary<ExternalAccountType, EPlatformIdentifier>(accountTypeMappings);
		deviceTypeToAntiCheatPlatformMappings = new EnumDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform>
		{
			{
				ClientInfo.EDeviceType.Unknown,
				AntiCheatCommonClientPlatform.Unknown
			},
			{
				ClientInfo.EDeviceType.Linux,
				AntiCheatCommonClientPlatform.Linux
			},
			{
				ClientInfo.EDeviceType.Mac,
				AntiCheatCommonClientPlatform.Mac
			},
			{
				ClientInfo.EDeviceType.Windows,
				AntiCheatCommonClientPlatform.Windows
			},
			{
				ClientInfo.EDeviceType.PlayStation,
				AntiCheatCommonClientPlatform.PlayStation
			},
			{
				ClientInfo.EDeviceType.Xbox,
				AntiCheatCommonClientPlatform.Xbox
			}
		};
		DeviceTypeToAntiCheatPlatformMappings = new ReadOnlyDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform>(deviceTypeToAntiCheatPlatformMappings);
		EnumDictionary<EPlatformIdentifier, ExternalAccountType> enumDictionary = new EnumDictionary<EPlatformIdentifier, ExternalAccountType>();
		foreach (KeyValuePair<ExternalAccountType, EPlatformIdentifier> accountTypeMapping in accountTypeMappings)
		{
			enumDictionary.Add(accountTypeMapping.Value, accountTypeMapping.Key);
		}
		PlatformIdentifierMappings = new ReadOnlyDictionary<EPlatformIdentifier, ExternalAccountType>(enumDictionary);
	}

	public static void TestEosConnection(Action<bool> _callback)
	{
		ThreadManager.StartThread("TestEosConnection", workerFunc, new EosConnectionTestInfo
		{
			Callback = _callback
		}, null, _useRealThread: false, _isSilent: true);
		[PublicizedFrom(EAccessModifier.Internal)]
		static void mainThreadSyncFunc(object _parameter)
		{
			EosConnectionTestInfo eosConnectionTestInfo = (EosConnectionTestInfo)_parameter;
			eosConnectionTestInfo.Callback(eosConnectionTestInfo.Result);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void workerFunc(ThreadManager.ThreadInfo _info)
		{
			try
			{
				HttpWebRequest obj = (HttpWebRequest)WebRequest.Create("https://api.epicgames.dev/sdk/v1/default");
				obj.Timeout = 5000;
				obj.KeepAlive = false;
				using ((HttpWebResponse)obj.GetResponse())
				{
					((EosConnectionTestInfo)_info.parameter).Result = true;
				}
			}
			catch (Exception ex)
			{
				Log.Out("[EOS] Connection test failed: " + ex.Message);
				((EosConnectionTestInfo)_info.parameter).Result = false;
			}
			ThreadManager.AddSingleTaskMainThread("TestEosConnectionResult", mainThreadSyncFunc, _info.parameter);
		}
	}

	public static void AssertMainThread(string _id)
	{
		if (!ThreadManager.IsMainThread())
		{
			Log.Warning("[EOSH] Called EOS code from secondary thread: " + _id);
		}
	}

	public static ClientInfo.EDeviceType GetDeviceTypeFromPlatform(string platform)
	{
		switch (platform)
		{
		case "other":
		case "steam":
			return ClientInfo.EDeviceType.Unknown;
		case "playstation":
			return ClientInfo.EDeviceType.PlayStation;
		case "xbox":
			return ClientInfo.EDeviceType.Xbox;
		default:
			Log.Error("[EOS] [Auth] GetDeviceTypeFromPlatform: Unknown platform: " + platform);
			return ClientInfo.EDeviceType.Unknown;
		}
	}

	public static bool RequiresAntiCheat(this ClientInfo.EDeviceType deviceType)
	{
		if (deviceType != ClientInfo.EDeviceType.PlayStation)
		{
			return deviceType != ClientInfo.EDeviceType.Xbox;
		}
		return false;
	}
}
