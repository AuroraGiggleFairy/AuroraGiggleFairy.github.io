using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Platform.MultiPlatform;
using UnityEngine;

namespace Platform;

public static class PlatformManager
{
	public const string PlatformConfigFileName = "platform.cfg";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<EPlatformIdentifier, IPlatform> serverPlatforms = new EnumDictionary<EPlatformIdentifier, IPlatform>();

	public static readonly ReadOnlyDictionary<EPlatformIdentifier, IPlatform> ServerPlatforms = new ReadOnlyDictionary<EPlatformIdentifier, IPlatform>(serverPlatforms);

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<EPlatformIdentifier, Type> supportedPlatforms = new EnumDictionary<EPlatformIdentifier, Type>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string supportedPlatformsString;

	public static readonly Dictionary<EPlatformIdentifier, AbsUserIdentifierFactory> UserIdentifierFactories = new EnumDictionary<EPlatformIdentifier, AbsUserIdentifierFactory>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static EDeviceType DeviceType
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static IPlatform MultiPlatform
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static IPlatform NativePlatform
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static IPlatform CrossplatformPlatform
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ClientLobbyManager ClientLobbyManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static PlatformUserIdentifierAbs InternalLocalUserIdentifier => CrossplatformPlatform?.User?.PlatformUserId ?? NativePlatform.User.PlatformUserId;

	public static bool Init()
	{
		if (initialized)
		{
			return true;
		}
		DeviceType = EDeviceType.PC;
		try
		{
			initialized = true;
			Log.Out("[Platform] Init");
			FindSupportedPlatforms();
			PlatformConfiguration platformConfiguration = DetectPlatform();
			GetCommandLineOverrides(platformConfiguration);
			initPlatformFromIdentifier(platformConfiguration.NativePlatform, "Native", out var _target);
			NativePlatform = _target;
			if (platformConfiguration.CrossPlatform != EPlatformIdentifier.None)
			{
				initPlatformFromIdentifier(platformConfiguration.CrossPlatform, "Cross", out _target);
				_target.IsCrossplatform = true;
				CrossplatformPlatform = _target;
			}
			MultiPlatform = new Factory();
			foreach (EPlatformIdentifier serverPlatform in platformConfiguration.ServerPlatforms)
			{
				if (initPlatformFromIdentifier(serverPlatform, "Server", out _target))
				{
					_target.AsServerOnly = true;
				}
			}
			ClientLobbyManager = new ClientLobbyManager();
			NativePlatform.CreateInstances();
			CrossplatformPlatform?.CreateInstances();
			MultiPlatform.CreateInstances();
			List<EPlatformIdentifier> list = new List<EPlatformIdentifier>();
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform2 in serverPlatforms)
			{
				if (serverPlatform2.Value.AsServerOnly)
				{
					try
					{
						serverPlatform2.Value.CreateInstances();
					}
					catch (NotSupportedException ex)
					{
						Log.Error($"[Platform] Platform {serverPlatform2.Key} Errored on init, removing from the list of server platforms. Error: {ex.Message}.");
						list.Add(serverPlatform2.Key);
					}
				}
			}
			foreach (EPlatformIdentifier item in list)
			{
				if (serverPlatforms.TryGetValue(item, out var value))
				{
					platformConfiguration.ServerPlatforms.Remove(item);
					value.Destroy();
					serverPlatforms.Remove(item);
				}
			}
			list.Clear();
			if (CrossplatformPlatform?.User != null)
			{
				CrossplatformPlatform.User.UserLoggedIn += BacktraceUtils.BacktraceUserLoggedIn;
			}
			else if (NativePlatform.User != null)
			{
				NativePlatform.User.UserLoggedIn += BacktraceUtils.BacktraceUserLoggedIn;
			}
			NativePlatform.Init();
			CrossplatformPlatform?.Init();
			MultiPlatform.Init();
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform3 in serverPlatforms)
			{
				if (serverPlatform3.Value.AsServerOnly)
				{
					serverPlatform3.Value.Init();
				}
			}
			PlatformUserManager.Init();
		}
		catch (Exception e)
		{
			Log.Error("[Platform] Error while initializing platform code, shutting down.");
			Log.Exception(e);
			Application.Quit(1);
			return false;
		}
		return true;
	}

	public static void Update()
	{
		NativePlatform?.Update();
		CrossplatformPlatform?.Update();
		MultiPlatform?.Update();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in serverPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly)
			{
				serverPlatform.Value.Update();
			}
		}
		PlatformUserManager.Update();
	}

	public static void LateUpdate()
	{
		NativePlatform?.LateUpdate();
		CrossplatformPlatform?.LateUpdate();
		MultiPlatform?.LateUpdate();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in serverPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly)
			{
				serverPlatform.Value.LateUpdate();
			}
		}
	}

	public static void Destroy()
	{
		PlatformUserManager.Destroy();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in serverPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly)
			{
				serverPlatform.Value.Destroy();
			}
		}
		MultiPlatform?.Destroy();
		CrossplatformPlatform?.Destroy();
		NativePlatform?.Destroy();
		serverPlatforms.Clear();
		MultiPlatform = null;
		CrossplatformPlatform = null;
		NativePlatform = null;
	}

	public static string PlatformStringFromEnum(EPlatformIdentifier _platformIdentifier)
	{
		return _platformIdentifier.ToStringCached();
	}

	public static bool TryPlatformIdentifierFromString(string _platformName, out EPlatformIdentifier _platformIdentifier)
	{
		return EnumUtils.TryParse<EPlatformIdentifier>(_platformName, out _platformIdentifier, _ignoreCase: true);
	}

	public static IPlatform InstanceForPlatformIdentifier(EPlatformIdentifier _platformIdentifier)
	{
		if (!serverPlatforms.TryGetValue(_platformIdentifier, out var value))
		{
			return null;
		}
		return value;
	}

	public static bool IsPlatformLoaded(EPlatformIdentifier _platformIdentifier)
	{
		return serverPlatforms.ContainsKey(_platformIdentifier);
	}

	public static string GetPlatformDisplayName(EPlatformIdentifier _platformIdentifier)
	{
		return Localization.Get("platformName" + _platformIdentifier.ToStringCached());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initPlatformFromIdentifier(EPlatformIdentifier _platformIdentifier, string _logName, out IPlatform _target)
	{
		if (!supportedPlatforms.TryGetValue(_platformIdentifier, out var value))
		{
			throw new NotSupportedException("[Platform] " + _logName + " platform " + _platformIdentifier.ToStringCached() + " not supported. Supported: " + supportedPlatformsString);
		}
		Log.Out("[Platform] Using " + _logName.ToLowerInvariant() + " platform: " + _platformIdentifier.ToStringCached());
		if (serverPlatforms.ContainsKey(_platformIdentifier))
		{
			_target = null;
			return false;
		}
		_target = ReflectionHelpers.Instantiate<IPlatform>(value);
		serverPlatforms.Add(_target.PlatformIdentifier, _target);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FindSupportedPlatforms()
	{
		supportedPlatforms.Clear();
		supportedPlatformsString = "";
		Type typeFromHandle = typeof(IPlatform);
		Type attrType = typeof(PlatformFactoryAttribute);
		ReflectionHelpers.FindTypesImplementingBase(typeFromHandle, [PublicizedFrom(EAccessModifier.Internal)] (Type _type) =>
		{
			object[] customAttributes = _type.GetCustomAttributes(attrType, inherit: false);
			if (customAttributes.Length == 1)
			{
				PlatformFactoryAttribute platformFactoryAttribute = (PlatformFactoryAttribute)customAttributes[0];
				if (supportedPlatforms.TryGetValue(platformFactoryAttribute.TargetPlatform, out var value))
				{
					Log.Error("[Platform] Multiple platform providers for platform " + platformFactoryAttribute.TargetPlatform.ToStringCached() + ": Loaded '" + value.FullName + "', found '" + _type.FullName + "'");
				}
				else
				{
					supportedPlatforms.Add(platformFactoryAttribute.TargetPlatform, _type);
					if (supportedPlatformsString.Length > 0)
					{
						supportedPlatformsString += ", ";
					}
					supportedPlatformsString += platformFactoryAttribute.TargetPlatform.ToStringCached();
				}
			}
		});
		UserIdentifierFactories.Clear();
		Type typeFromHandle2 = typeof(AbsUserIdentifierFactory);
		Type attrType2 = typeof(UserIdentifierFactoryAttribute);
		ReflectionHelpers.FindTypesImplementingBase(typeFromHandle2, [PublicizedFrom(EAccessModifier.Internal)] (Type _type) =>
		{
			object[] customAttributes = _type.GetCustomAttributes(attrType2, inherit: false);
			if (customAttributes.Length == 1)
			{
				UserIdentifierFactoryAttribute userIdentifierFactoryAttribute = (UserIdentifierFactoryAttribute)customAttributes[0];
				if (UserIdentifierFactories.TryGetValue(userIdentifierFactoryAttribute.TargetPlatform, out var value))
				{
					Log.Error("[Platform] Multiple user identifier factories for platform " + userIdentifierFactoryAttribute.TargetPlatform.ToStringCached() + ": Loaded '" + value.GetType().FullName + "', found '" + _type.FullName + "'");
				}
				else
				{
					AbsUserIdentifierFactory absUserIdentifierFactory = ReflectionHelpers.Instantiate<AbsUserIdentifierFactory>(_type);
					if (absUserIdentifierFactory != null)
					{
						UserIdentifierFactories.Add(userIdentifierFactoryAttribute.TargetPlatform, absUserIdentifierFactory);
					}
				}
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetCommandLineOverrides(PlatformConfiguration _platforms)
	{
		string launchArgument = GameUtils.GetLaunchArgument("platform");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			_platforms.ParsePlatform("platform", launchArgument);
		}
		launchArgument = GameUtils.GetLaunchArgument("crossplatform");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			_platforms.ParsePlatform("crossplatform", launchArgument);
		}
		launchArgument = GameUtils.GetLaunchArgument("serverplatforms");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			_platforms.ParsePlatform("serverplatforms", launchArgument);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlatformConfiguration DetectPlatform()
	{
		PlatformConfiguration _result = null;
		if (PlatformConfiguration.ReadFile(ref _result))
		{
			return _result;
		}
		PlatformConfiguration platformConfiguration = new PlatformConfiguration();
		Log.Warning(string.Format("[Platform] No platform config file ({0}) found, defaulting to {1} / {2} without additional server platforms.", "platform.cfg", platformConfiguration.NativePlatform, platformConfiguration.CrossPlatform));
		return platformConfiguration;
	}
}
