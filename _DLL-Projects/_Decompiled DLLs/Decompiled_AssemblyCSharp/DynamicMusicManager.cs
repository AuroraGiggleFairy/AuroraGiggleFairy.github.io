using System;
using DynamicMusic.Legacy;
using DynamicMusic.Legacy.ObjectModel;

public class DynamicMusicManager : IGamePrefsChangedListener
{
	public EntityPlayerLocal PrimaryLocalPlayer;

	public ThreatLevelTracker ThreatLevelTracker;

	public FrequencyManager FrequencyManager;

	public TransitionManager TransitionManager;

	public StreamerMaster StreamerMaster;

	public bool IsMusicPlayingThisTick;

	public bool WasMusicPlayingLastTick;

	public static readonly float PlayBanThreshold = 1f;

	public static readonly float deadWindow = 1f / 6f;

	public static GameRandom Random;

	public DMSUpdateConditions UpdateConditions;

	public bool MusicStarted
	{
		get
		{
			if (IsMusicPlayingThisTick)
			{
				return !WasMusicPlayingLastTick;
			}
			return false;
		}
	}

	public bool MusicStopped
	{
		get
		{
			if (!IsMusicPlayingThisTick)
			{
				return WasMusicPlayingLastTick;
			}
			return false;
		}
	}

	public bool IsDynamicMusicPlaying => IsMusicPlayingThisTick;

	public bool IsBeforeDuskPlayBan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() < SkyManager.GetDuskTimeAsMinutes() - PlayBanThreshold;
		}
	}

	public bool IsAfterDusk => SkyManager.TimeOfDay() > SkyManager.GetDuskTime();

	public bool IsAfterDuskWindow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() > SkyManager.GetDuskTimeAsMinutes() + deadWindow;
		}
	}

	public bool IsBeforeDawnPlayBan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() < SkyManager.GetDawnTimeAsMinutes() - PlayBanThreshold;
		}
	}

	public bool IsAfterDawn => SkyManager.TimeOfDay() > SkyManager.GetDawnTime();

	public bool IsAfterDawnWindow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() > SkyManager.GetDawnTimeAsMinutes() + deadWindow;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInDeadWindow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlayAllowed
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float DistanceFromDeadWindow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlayerInTraderStation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float TimeToNextDayEvent
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (SkyManager.IsDark() ? SkyManager.GetDawnTimeAsMinutes() : SkyManager.GetDuskTimeAsMinutes()) - SkyManager.GetTimeOfDayAsMinutes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager()
	{
		UpdateConditions = default(DMSUpdateConditions);
		UpdateConditions.IsDMSEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled);
		UpdateConditions.IsGameUnPaused = true;
	}

	public static void Init(EntityPlayerLocal _epLocal)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Initializing Dynamic Music System");
		_epLocal.DynamicMusicManager = new DynamicMusicManager();
		GamePrefs.AddChangeListener(_epLocal.DynamicMusicManager);
		_epLocal.DynamicMusicManager.PrimaryLocalPlayer = _epLocal;
		Random = GameRandomManager.Instance.CreateGameRandom();
		ThreatLevelTracker.Init(_epLocal.DynamicMusicManager);
		FrequencyManager.Init(_epLocal.DynamicMusicManager);
		StreamerMaster.Init(_epLocal.DynamicMusicManager);
		TransitionManager.Init(_epLocal.DynamicMusicManager);
		_epLocal.DynamicMusicManager.UpdateConditions.IsDMSInitialized = true;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Finished initializing Dynamic Music System");
	}

	public void Tick()
	{
		if (!UpdateConditions.CanUpdate)
		{
			return;
		}
		if (StreamerMaster.currentStreamer != null)
		{
			IsMusicPlayingThisTick = StreamerMaster.currentStreamer.IsPlaying;
		}
		if (IsAfterDusk)
		{
			bool flag = (IsPlayAllowed = IsAfterDuskWindow);
			IsInDeadWindow = !flag;
			DistanceFromDeadWindow = (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) - SkyManager.GetTimeOfDayAsMinutes() + SkyManager.GetDawnTimeAsMinutes();
		}
		else if (IsAfterDawn)
		{
			if (IsAfterDawnWindow)
			{
				DistanceFromDeadWindow = Utils.FastMax(SkyManager.GetDuskTimeAsMinutes() - deadWindow - SkyManager.GetTimeOfDayAsMinutes(), 0f);
				if (IsBeforeDuskPlayBan)
				{
					IsInDeadWindow = false;
					IsPlayAllowed = true;
				}
				else
				{
					IsPlayAllowed = false;
					IsInDeadWindow = DistanceFromDeadWindow == 0f;
				}
			}
		}
		else
		{
			DistanceFromDeadWindow = Utils.FastMax(SkyManager.GetDawnTimeAsMinutes() - deadWindow - SkyManager.GetTimeOfDayAsMinutes(), 0f);
			if (IsBeforeDawnPlayBan)
			{
				IsPlayAllowed = true;
				IsInDeadWindow = false;
			}
			else
			{
				IsPlayAllowed = false;
				IsInDeadWindow = DistanceFromDeadWindow == 0f;
			}
		}
		IsPlayerInTraderStation = IsPrimaryPlayerInTraderStation();
		ThreatLevelTracker.Tick();
		FrequencyManager.Tick();
		TransitionManager.Tick();
		StreamerMaster.Tick();
		WasMusicPlayingLastTick = IsMusicPlayingThisTick;
	}

	public void CleanUpDynamicMembers()
	{
		if (StreamerMaster != null)
		{
			StreamerMaster.Cleanup();
		}
		UpdateConditions.IsDMSInitialized = false;
	}

	public static void Cleanup()
	{
		MusicGroup.Cleanup();
		ConfigSet.Cleanup();
	}

	public void Event(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (_eventType <= MinEventTypes.onSelfDied)
		{
			switch (_eventType)
			{
			case MinEventTypes.onOtherDamagedSelf:
				ThreatLevelTracker.Event(_eventType, _eventParms);
				break;
			case MinEventTypes.onOtherAttackedSelf:
				ThreatLevelTracker.Event(_eventType, _eventParms);
				break;
			case MinEventTypes.onSelfDamagedOther:
				ThreatLevelTracker.Event(_eventType, _eventParms);
				break;
			case MinEventTypes.onSelfAttackedOther:
				ThreatLevelTracker.Event(_eventType, _eventParms);
				break;
			case MinEventTypes.onSelfDied:
				Log.Out("DMS Died!");
				UpdateConditions.DoesPlayerExist = false;
				break;
			}
		}
		else
		{
			switch (_eventType)
			{
			default:
				_ = 106;
				break;
			case MinEventTypes.onSelfRespawn:
				Log.Out("DMS Respawn!");
				UpdateConditions.DoesPlayerExist = true;
				break;
			case MinEventTypes.onSelfLeaveGame:
				Log.Out("DMS Left Game!");
				break;
			case MinEventTypes.onSelfEnteredGame:
				Log.Out("DMS Entered Game!");
				UpdateConditions.DoesPlayerExist = true;
				break;
			case MinEventTypes.onSelfEnteredBiome:
				break;
			}
		}
	}

	public void OnPlayerDeath()
	{
		StreamerMaster.Stop();
	}

	public void OnPlayerFirstSpawned()
	{
		FrequencyManager.OnPlayerFirstSpawned();
	}

	public void Pause()
	{
		UpdateConditions.IsGameUnPaused = false;
		StreamerMaster.Pause();
		FrequencyManager.OnPause();
	}

	public void UnPause()
	{
		UpdateConditions.IsGameUnPaused = true;
		StreamerMaster.UnPause();
		FrequencyManager.OnUnPause();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPrimaryPlayerInTraderStation()
	{
		return GameManager.Instance.World.IsWithinTraderArea(PrimaryLocalPlayer.GetBlockPosition());
	}

	public bool IsInDawnOrDuskRange(float _dawnOrDuskTime, float _currentTime)
	{
		return DistanceFromDawnOrDusk(_dawnOrDuskTime, _currentTime) <= deadWindow;
	}

	public float DistanceFromDawnOrDusk(float _dawnOrDuskTime, float _currentTime)
	{
		return Math.Abs(_dawnOrDuskTime - _currentTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void IGamePrefsChangedListener.OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsDynamicMusicEnabled && !(UpdateConditions.IsDMSEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled)))
		{
			StreamerMaster.Stop();
		}
	}
}
