using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Sanctions;
using UnityEngine;

namespace Platform.EOS;

public class Api : IPlatformApi
{
	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PlatformTickInterval = 0.01f;

	public static readonly EDebugLevel DebugLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	public PlatformInterface PlatformInterface;

	public ConnectInterface ConnectInterface;

	public SanctionsInterface SanctionsInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public float platformTickTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(_bStart: false);

	[PublicizedFrom(EAccessModifier.Internal)]
	public readonly SanctionsCheck eosSanctionsCheck = new SanctionsCheck();

	[PublicizedFrom(EAccessModifier.Private)]
	public Action clientApiInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int httpWarningLimit = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float httpWarningTimeout = 600f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int httpWarningCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float httpNextTime;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EApiStatus ClientApiStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EApiStatus.Uninitialized;

	public event Action ClientApiInitialized
	{
		add
		{
			lock (this)
			{
				clientApiInitialized = (Action)Delegate.Combine(clientApiInitialized, value);
				if (ClientApiStatus == EApiStatus.Ok)
				{
					value();
				}
			}
		}
		remove
		{
			lock (this)
			{
				clientApiInitialized = (Action)Delegate.Remove(clientApiInitialized, value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Api()
	{
		DebugLevel = EDebugLevel.Off;
		string launchArgument = GameUtils.GetLaunchArgument("debugeos");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				DebugLevel = EDebugLevel.Verbose;
			}
			else
			{
				DebugLevel = EDebugLevel.Normal;
			}
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationStateChanged(ApplicationState _applicationState)
	{
		if (PlatformInterface == null)
		{
			return;
		}
		ApplicationStatus applicationStatus = _applicationState switch
		{
			ApplicationState.Foreground => ApplicationStatus.Foreground, 
			ApplicationState.Suspended => ApplicationStatus.BackgroundSuspended, 
			_ => throw new ArgumentOutOfRangeException("_applicationState", _applicationState, "[EOS] OnApplicationStateChanged: ApplicationState is missing a conversion to a EOS.ApplicationStatus"), 
		};
		lock (AntiCheatCommon.LockObject)
		{
			PlatformInterface.SetApplicationStatus(applicationStatus);
		}
	}

	public bool InitClientApis()
	{
		if (ClientApiStatus == EApiStatus.Ok)
		{
			return true;
		}
		EosCreds eosCreds = (GameManager.IsDedicatedServer ? EosCreds.ServerCredentials : EosCreds.ClientCredentials);
		initPlatform(eosCreds, eosCreds.ServerMode);
		return ClientApiStatus == EApiStatus.Ok;
	}

	public bool InitServerApis()
	{
		return InitClientApis();
	}

	public void ServerApiLoaded()
	{
	}

	public void Update()
	{
		if (ClientApiStatus != EApiStatus.Ok)
		{
			return;
		}
		platformTickTimer += Time.unscaledDeltaTime;
		if (GameManager.IsDedicatedServer || platformTickTimer >= 0.01f)
		{
			platformTickTimer = 0f;
			tickDurationStopwatch.Restart();
			lock (AntiCheatCommon.LockObject)
			{
				PlatformInterface.Tick();
			}
			long num = tickDurationStopwatch.ElapsedMicroseconds / 1000;
			if (DebugLevel != EDebugLevel.Off && num > 5)
			{
				Log.Warning($"[EOS] Tick took exceptionally long: {num} ms");
			}
		}
	}

	public void Destroy()
	{
		if (ClientApiStatus != EApiStatus.Ok)
		{
			return;
		}
		ConnectInterface = null;
		lock (AntiCheatCommon.LockObject)
		{
			PlatformInterface.Release();
		}
		PlatformInterface = null;
		lock (AntiCheatCommon.LockObject)
		{
			PlatformInterface.Shutdown();
		}
	}

	public float GetScreenBoundsValueFromSystem()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPlatform(EosCreds _creds, bool _serverMode)
	{
		InitializeOptions options = new InitializeOptions
		{
			ProductName = "7 Days To Die",
			ProductVersion = Constants.cVersionInformation.SerializableString
		};
		Result result = Result.NotFound;
		try
		{
			lock (AntiCheatCommon.LockObject)
			{
				result = PlatformInterface.Initialize(ref options);
			}
		}
		catch (DllNotFoundException e)
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[EOS] Native library or one of its dependencies not found (e.g. no Microsoft Visual C Redistributables 2022)");
			Log.Exception(e);
			Application.Quit(1);
		}
		Log.Out($"[EOS] Initialize: {result}");
		LogLevel logLevel = DebugLevel switch
		{
			EDebugLevel.Off => LogLevel.Warning, 
			EDebugLevel.Normal => LogLevel.Info, 
			EDebugLevel.Verbose => LogLevel.VeryVerbose, 
			_ => throw new ArgumentOutOfRangeException("DebugLevel"), 
		};
		lock (AntiCheatCommon.LockObject)
		{
			LoggingInterface.SetLogLevel(LogCategory.AllCategories, logLevel);
			string launchArgument = GameUtils.GetLaunchArgument("debugeac");
			if (launchArgument != null)
			{
				LoggingInterface.SetLogLevel(LogCategory.AntiCheat, (launchArgument == "verbose") ? LogLevel.Verbose : LogLevel.Info);
			}
			else
			{
				LoggingInterface.SetLogLevel(LogCategory.AntiCheat, LogLevel.Warning);
			}
			if (logLevel == LogLevel.VeryVerbose)
			{
				LoggingInterface.SetLogLevel(LogCategory.Http, LogLevel.Verbose);
			}
			LoggingInterface.SetLogLevel(LogCategory.Analytics, LogLevel.Error);
			LoggingInterface.SetLogLevel(LogCategory.Messaging, LogLevel.Warning);
			LoggingInterface.SetLogLevel(LogCategory.Ecom, LogLevel.Error);
			LoggingInterface.SetLogLevel(LogCategory.Auth, LogLevel.Error);
			LoggingInterface.SetLogLevel(LogCategory.Presence, LogLevel.Warning);
			LoggingInterface.SetLogLevel(LogCategory.Overlay, LogLevel.Warning);
			LoggingInterface.SetLogLevel(LogCategory.Ui, LogLevel.Warning);
			LoggingInterface.SetCallback(logCallback);
		}
		PlatformInterface = createPlatformInterface(_creds, _serverMode);
		if (PlatformInterface == null)
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[EOS] Failed to create platform");
			return;
		}
		if (PlatformManager.NativePlatform?.ApplicationState != null)
		{
			PlatformManager.NativePlatform.ApplicationState.OnApplicationStateChanged += OnApplicationStateChanged;
		}
		lock (AntiCheatCommon.LockObject)
		{
			ConnectInterface = PlatformInterface.GetConnectInterface();
		}
		if (ConnectInterface == null)
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[EOS] Failed to get connect interface");
			return;
		}
		lock (AntiCheatCommon.LockObject)
		{
			SanctionsInterface = PlatformInterface.GetSanctionsInterface();
		}
		if (SanctionsInterface == null)
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[EOS] Failed to get sanctions interface");
		}
		else
		{
			ClientApiStatus = EApiStatus.Ok;
			clientApiInitialized?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformInterface createPlatformInterface(EosCreds _creds, bool _serverMode)
	{
		WindowsOptions options = new WindowsOptions
		{
			ProductId = _creds.ProductId,
			SandboxId = _creds.SandboxId,
			ClientCredentials = new ClientCredentials
			{
				ClientId = _creds.ClientId,
				ClientSecret = _creds.ClientSecret
			},
			DeploymentId = _creds.DeploymentId,
			EncryptionKey = "0000000000000000000000000000000000000000000000000000000000000000",
			IsServer = _serverMode,
			Flags = PlatformFlags.DisableOverlay
		};
		options.Flags |= PlatformFlags.DisableSocialOverlay;
		options.RTCOptions = null;
		options.RTCOptions = new WindowsRTCOptions
		{
			PlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions
			{
				XAudio29DllPath = GameIO.GetGameDir("7DaysToDie_Data/Plugins/x86_64/xaudio2_9redist.dll")
			}
		};
		options.CacheDirectory = GameIO.GetUserGameDataDir();
		if (!Directory.Exists(options.CacheDirectory))
		{
			Directory.CreateDirectory(options.CacheDirectory);
		}
		lock (AntiCheatCommon.LockObject)
		{
			return PlatformInterface.Create(ref options);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logCallback(ref LogMessage _message)
	{
		if (_message.Level == LogLevel.Warning && _message.Category == (Utf8String)"LogHttp")
		{
			httpWarningCount++;
			if (httpWarningCount == 50)
			{
				httpNextTime = Time.unscaledTime + 600f;
				return;
			}
			if (httpWarningCount > 50)
			{
				if (!(Time.unscaledTime >= httpNextTime))
				{
					return;
				}
				Log.Out($"[EOS] [LogHttp - Warning] Skipped {httpWarningCount - 50} warnings within the last {600f} seconds!");
				httpWarningCount = 0;
			}
		}
		if (DebugLevel != EDebugLevel.Off || _message.Level != LogLevel.Warning || !(_message.Category == (Utf8String)"LogEOSRTC") || !((string)_message.Message).StartsWith("TickTracker Tick is delayed.", StringComparison.Ordinal))
		{
			string txt = $"[EOS] [{_message.Category} - {_message.Level.ToStringCached()}] {_message.Message}";
			switch (_message.Level)
			{
			case LogLevel.Off:
				Log.Error(txt);
				throw new ArgumentOutOfRangeException();
			case LogLevel.Info:
			case LogLevel.Verbose:
			case LogLevel.VeryVerbose:
				Log.Out(txt);
				break;
			case LogLevel.Warning:
				Log.Warning(txt);
				break;
			case LogLevel.Fatal:
			case LogLevel.Error:
				Log.Error(txt);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
