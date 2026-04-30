using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Audio;
using Challenges;
using Newtonsoft.Json.Linq;
using Twitch.PubSub;
using UniLinq;
using UnityEngine;

namespace Twitch;

public class TwitchManager
{
	public enum PimpPotSettings
	{
		Disabled,
		EnabledSP,
		EnabledPP
	}

	public enum IntegrationSettings
	{
		ExtensionOnly,
		Both
	}

	public enum CooldownTypes
	{
		None,
		Startup,
		Time,
		MaxReached,
		MaxReachedWaiting,
		BloodMoonDisabled,
		BloodMoonCooldown,
		QuestDisabled,
		QuestCooldown,
		SafeCooldown,
		SafeCooldownExit
	}

	public enum InitStates
	{
		Setup,
		None,
		WaitingForPermission,
		PermissionDenied,
		WaitingForOAuth,
		Authenticating,
		Authenticated,
		CheckingForExtension,
		Ready,
		ExtensionNotInstalled,
		Failed
	}

	public class TwitchPartyMemberInfo
	{
		public bool LastOptedOut;

		public bool LastAlive = true;

		public float Cooldown;

		public bool NeedsRespawnEvent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchManager instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SAVE_TIME_SEC = 30f;

	public float saveTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo dataSaveThreadInfo;

	public static byte FileVersion = 26;

	public static byte MainFileVersion = 3;

	public TwitchIRCClient ircClient;

	public ExtensionManager extensionManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetClientAttempts;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool overrideProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public int commandsAvailable = -1;

	public Dictionary<string, TwitchAction> AvailableCommands = new Dictionary<string, TwitchAction>();

	public Dictionary<string, string> AlternateCommands = new Dictionary<string, string>();

	public PimpPotSettings PimpPotType = PimpPotSettings.EnabledSP;

	public static int PimpPotDefault = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public IntegrationSettings integrationSetting = IntegrationSettings.Both;

	[PublicizedFrom(EAccessModifier.Private)]
	public float actionCooldownModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HistoryItemMax = 500;

	public float ActionPotPercentage = 0.15f;

	public float BitPotPercentage = 0.25f;

	public int RewardPot = PimpPotDefault;

	public int BitPot;

	public int PartyKillRewardMax = 250;

	public EntityPlayerLocal LocalPlayer;

	public bool LocalPlayerInLandClaim;

	public CooldownTypes CooldownType = CooldownTypes.Startup;

	public float CooldownTime = 300f;

	public float CurrentCooldownFill;

	public float CooldownFillMax = 50f;

	public int NextCooldownTime = 180;

	public bool AllowCrateSharing;

	public bool AllowBitEvents = true;

	public bool AllowSubEvents = true;

	public bool AllowGiftSubEvents = true;

	public bool AllowCharityEvents = true;

	public bool AllowRaidEvents = true;

	public bool AllowHypeTrainEvents = true;

	public bool AllowCreatorGoalEvents = true;

	public bool AllowChannelPointRedemptions = true;

	public List<CooldownPreset> CooldownPresets = new List<CooldownPreset>();

	public int CooldownPresetIndex;

	public CooldownPreset CurrentCooldownPreset;

	public List<TwitchActionPreset> ActionPresets = new List<TwitchActionPreset>();

	public List<TwitchVotePreset> VotePresets = new List<TwitchVotePreset>();

	public List<TwitchEventPreset> EventPresets = new List<TwitchEventPreset>();

	public int ActionPresetIndex;

	public int VotePresetIndex;

	public int EventPresetIndex;

	public TwitchActionPreset CurrentActionPreset;

	public TwitchVotePreset CurrentVotePreset;

	public TwitchEventPreset CurrentEventPreset;

	public bool UIDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime = 1f;

	public float ExtensionCheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	public int lastGameDay = -1;

	public int currentBMDayEnd = -1;

	public int nextBMDay = -1;

	public int BMCooldownStart;

	public int BMCooldownEnd;

	public int BitPointModifier = 1;

	public int SubPointModifier = 1;

	public int GiftSubPointModifier = 2;

	public int RaidPointAdd = 1000;

	public int RaidViewerMinimum = 10;

	public int HypeTrainLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bitPriceMultiplier = 1f;

	public static TwitchLeaderboardStats LeaderboardStats = new TwitchLeaderboardStats();

	public bool isBMActive;

	public TwitchVoteLockTypes VoteLockedLevel;

	public List<TwitchActionHistoryEntry> ActionHistory = new List<TwitchActionHistoryEntry>();

	public List<TwitchActionHistoryEntry> VoteHistory = new List<TwitchActionHistoryEntry>();

	public List<TwitchActionHistoryEntry> EventHistory = new List<TwitchActionHistoryEntry>();

	public List<TwitchLeaderboardEntry> Leaderboard = new List<TwitchLeaderboardEntry>();

	public List<TwitchRespawnEntry> RespawnEntries = new List<TwitchRespawnEntry>();

	public int UseActionsDuringBloodmoon = 2;

	public int UseActionsDuringQuests = 2;

	public bool InitialCooldownSet;

	public List<EntityPlayer> twitchPlayerDeathsThisFrame = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public InitStates initState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLoaded;

	public XUi LocalPlayerXUi;

	public float CurrentUnityTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resetCommandsNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool respawnEventNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkingExtensionInstalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public string broadcasterType = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchMessageEntry> inGameChatQueue = new List<TwitchMessageEntry>();

	public TwitchAuthentication Authentication;

	public EventSubClient EventSub;

	public bool HasViewedSettings;

	public string DeniedCrateEvent = "";

	public string StealingCrateEvent = "";

	public string PartyRespawnEvent = "";

	public string OnPlayerDeathEvent = "";

	public string OnPlayerRespawnEvent = "";

	public List<string> tipTitleList = new List<string>();

	public List<string> tipDescriptionList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int extensionActiveCheckFailures;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_ActivatedAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_ActivatedBitAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_BitCredits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_BitEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_BitPotBalance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_ChannelPointEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_CharityEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_CooldownComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_CooldownStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_CooldownTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_Commands;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_CreatorGoalEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_DonateBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_DonateCharity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_Gamestage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_GiftSubEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_GiftSubs;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_HypeTrainEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_KilledParty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_KilledStreamer;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_KilledByBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_KilledByHypeTrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_KilledByVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_NewActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_PimpPotBalance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_PointsWithSpecial;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_PointsWithoutSpecial;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_QueuedBitAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_RaidEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_RaidPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_SubEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_Subscribed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_ActivatedAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_BitRespawns;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_DonateBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_DonateCharity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_GiftSubs;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_KilledParty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_KilledStreamer;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_KilledByBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_KilledByHypeTrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_KilledByVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_RaidPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_RefundedAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_Subscribed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameDeathScreen_Message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameBitsDeathScreen_Message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameHypeTrainDeathScreen_Message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameVoteDeathScreen_Message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string subPointDisplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TwitchRandomActionGroup> randomGroups = new Dictionary<string, TwitchRandomActionGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<TwitchAction>> randomKeys = new Dictionary<string, List<TwitchAction>>();

	public List<BaseTwitchCommand> TwitchCommandList = new List<BaseTwitchCommand>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<EntityPlayer, TwitchPartyMemberInfo> PartyInfo = new Dictionary<EntityPlayer, TwitchPartyMemberInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastAlive;

	public static string DeathText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchActive = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionEntry> QueuedActionEntries = new List<TwitchActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionEntry> LiveActionEntries = new List<TwitchActionEntry>();

	public List<TwitchEventActionEntry> EventQueue = new List<TwitchEventActionEntry>();

	public List<TwitchEventActionEntry> LiveEvents = new List<TwitchEventActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchSpawnedEntityEntry> liveList = new List<TwitchSpawnedEntityEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchSpawnedBlocksEntry> liveBlockList = new List<TwitchSpawnedBlocksEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchRecentlyRemovedEntityEntry> recentlyDeadList = new List<TwitchRecentlyRemovedEntityEntry>();

	public List<TwitchSpawnedEntityEntry> actionSpawnLiveList = new List<TwitchSpawnedEntityEntry>();

	public bool HasDataChanges;

	public List<string> changedActionList = new List<string>();

	public List<string> changedEnabledVoteList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> ActionMessages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextDisplayCommandsTime;

	public static TwitchManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new TwitchManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentMainFileVersion { get; set; }

	public bool OverrideProgession
	{
		get
		{
			return overrideProgression;
		}
		set
		{
			if (overrideProgression != value)
			{
				overrideProgression = value;
				if (InitState == InitStates.Ready)
				{
					resetCommandsNeeded = true;
				}
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool UseProgression
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int HighestGameStage
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool BossHordeActive
	{
		get
		{
			if (!HasInstance)
			{
				return false;
			}
			return Current.IsBossHordeActive;
		}
	}

	public IntegrationSettings IntegrationSetting
	{
		get
		{
			return integrationSetting;
		}
		set
		{
			if (integrationSetting != value)
			{
				integrationSetting = value;
				IntegrationTypeChanged();
			}
		}
	}

	public float ActionCooldownModifier
	{
		get
		{
			return actionCooldownModifier;
		}
		set
		{
			if (value != actionCooldownModifier)
			{
				actionCooldownModifier = value;
				UpdateActionCooldowns(value);
			}
		}
	}

	public bool AllowActions => !CurrentActionPreset.IsEmpty;

	public bool AllowEvents => !CurrentEventPreset.IsEmpty;

	public bool OnCooldown
	{
		get
		{
			if (!(CooldownTime > 0f))
			{
				return CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Always;
			}
			return true;
		}
	}

	public float BitPriceMultiplier
	{
		get
		{
			return bitPriceMultiplier;
		}
		set
		{
			if (bitPriceMultiplier != value)
			{
				bitPriceMultiplier = value;
				ResetPrices();
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TwitchVotingManager VotingManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TwitchViewerData ViewerData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string BroadcasterType
	{
		get
		{
			return broadcasterType;
		}
		set
		{
			broadcasterType = value;
			UIDirty = true;
			if (this.CommandsChanged != null)
			{
				this.CommandsChanged();
			}
		}
	}

	public InitStates InitState
	{
		get
		{
			return initState;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (initState != value)
			{
				InitStates oldState = initState;
				initState = value;
				if (this.ConnectionStateChanged != null)
				{
					this.ConnectionStateChanged(oldState, initState);
				}
			}
		}
	}

	public string StateText
	{
		get
		{
			switch (initState)
			{
			case InitStates.Setup:
			case InitStates.WaitingForOAuth:
			case InitStates.Authenticating:
			case InitStates.Authenticated:
				return Localization.Get("xuiTwitchStatus_Connecting");
			case InitStates.WaitingForPermission:
				return Localization.Get("xuiTwitchStatus_RequestPermission");
			case InitStates.Ready:
				return string.Format(Localization.Get("xuiTwitchStatus_Connected"), Authentication.userName);
			case InitStates.Failed:
				return Localization.Get("xuiTwitchStatus_ConnectionFailed");
			case InitStates.ExtensionNotInstalled:
				return Localization.Get("xuiTwitchStatus_ExtensionDenied");
			case InitStates.PermissionDenied:
				return Localization.Get("xuiTwitchStatus_PermissionDenied");
			default:
				return "";
			}
		}
	}

	public bool IsReady => initState == InitStates.Ready;

	public bool IsVoting
	{
		get
		{
			if (initState == InitStates.Ready)
			{
				return VotingManager.VotingIsActive;
			}
			return false;
		}
	}

	public bool IsBossHordeActive
	{
		get
		{
			if (initState == InitStates.Ready)
			{
				if (VotingManager.CurrentVoteState != TwitchVotingManager.VoteStateTypes.EventActive)
				{
					return VotingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForActive;
				}
				return true;
			}
			return false;
		}
	}

	public bool ReadyForVote => actionSpawnLiveList.Count == 0;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsSafe
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool HasCustomEvents => EventPresets.Count > 0;

	public bool TwitchActive => twitchActive;

	public event OnTwitchConnectionStateChange ConnectionStateChanged;

	public event OnCommandsChanged CommandsChanged;

	public event OnHistoryAdded ActionHistoryAdded;

	public event OnHistoryAdded VoteHistoryAdded;

	public event OnHistoryAdded EventHistoryAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager()
	{
		ViewerData = new TwitchViewerData(this);
		VotingManager = new TwitchVotingManager(this);
		HighestGameStage = -1;
		UseProgression = true;
	}

	public void Cleanup()
	{
		Disconnect();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && InitState == InitStates.Ready)
		{
			SaveViewerData();
		}
		instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupLocalization()
	{
		chatOutput_ActivatedAction = Localization.Get("TwitchChat_ActivatedAction");
		chatOutput_ActivatedBitAction = Localization.Get("TwitchChat_ActivatedBitAction");
		chatOutput_BitCredits = Localization.Get("TwitchChat_BitCredits");
		chatOutput_BitEvent = Localization.Get("TwitchChat_BitEvent");
		chatOutput_BitPotBalance = Localization.Get("TwitchChat_BitPotBalance");
		chatOutput_ChannelPointEvent = Localization.Get("TwitchChat_ChannelPointEvent");
		chatOutput_CharityEvent = Localization.Get("TwitchChat_CharityEvent");
		chatOutput_Commands = Localization.Get("TwitchChat_Commands");
		chatOutput_CooldownComplete = Localization.Get("TwitchChat_CooldownComplete");
		chatOutput_CooldownStarted = Localization.Get("TwitchChat_CooldownStarted");
		chatOutput_CooldownTime = Localization.Get("TwitchChat_CooldownTime");
		chatOutput_CreatorGoalEvent = Localization.Get("TwitchChat_CreatorGoalEvent");
		chatOutput_DonateBits = Localization.Get("TwitchChat_DonateBits");
		chatOutput_DonateCharity = Localization.Get("TwitchChat_DonateCharity");
		chatOutput_Gamestage = Localization.Get("TwitchChat_Gamestage");
		chatOutput_GiftSubEvent = Localization.Get("TwitchChat_GiftedSubEvent");
		chatOutput_GiftSubs = Localization.Get("TwitchChat_GiftedSubs");
		chatOutput_HypeTrainEvent = Localization.Get("TwitchChat_HypeTrainEvent");
		chatOutput_KilledParty = Localization.Get("TwitchChat_KilledParty");
		chatOutput_KilledStreamer = Localization.Get("TwitchChat_KilledStreamer");
		chatOutput_KilledByBits = Localization.Get("TwitchChat_KilledByBits");
		chatOutput_KilledByHypeTrain = Localization.Get("TwitchChat_KilledByHypeTrain");
		chatOutput_KilledByVote = Localization.Get("TwitchChat_KilledByVote");
		chatOutput_NewActions = Localization.Get("TwitchChat_NewActions");
		chatOutput_PimpPotBalance = Localization.Get("TwitchChat_PimpPotBalance");
		chatOutput_PointsWithSpecial = Localization.Get("TwitchChat_PointsWithSpecial");
		chatOutput_PointsWithoutSpecial = Localization.Get("TwitchChat_PointsWithoutSpecial");
		chatOutput_QueuedBitAction = Localization.Get("TwitchChat_QueuedBitAction");
		chatOutput_RaidEvent = Localization.Get("TwitchChat_RaidEvent");
		chatOutput_RaidPoints = Localization.Get("TwitchChat_RaidPoints");
		chatOutput_SubEvent = Localization.Get("TwitchChat_SubEvent");
		chatOutput_Subscribed = Localization.Get("TwitchChat_Subscribed");
		ingameOutput_ActivatedAction = Localization.Get("TwitchInGame_ActivatedAction");
		ingameOutput_BitRespawns = Localization.Get("TwitchInGame_BitRespawns");
		ingameOutput_DonateBits = Localization.Get("TwitchInGame_DonateBits");
		ingameOutput_DonateCharity = Localization.Get("TwitchInGame_DonateCharity");
		ingameOutput_GiftSubs = Localization.Get("TwitchInGame_GiftedSubs");
		ingameOutput_KilledParty = Localization.Get("TwitchInGame_KilledParty");
		ingameOutput_KilledStreamer = Localization.Get("TwitchInGame_KilledStreamer");
		ingameOutput_KilledByBits = Localization.Get("TwitchInGame_KilledByBits");
		ingameOutput_KilledByHypeTrain = Localization.Get("TwitchInGame_KilledByHypeTrain");
		ingameOutput_KilledByVote = Localization.Get("TwitchInGame_KilledByVote");
		ingameOutput_RaidPoints = Localization.Get("TwitchInGame_RaidPoints");
		ingameOutput_RefundedAction = Localization.Get("TwitchInGame_RefundedAction");
		ingameOutput_Subscribed = Localization.Get("TwitchInGame_Subscribed");
		ingameDeathScreen_Message = Localization.Get("TwitchDeathMessage");
		ingameBitsDeathScreen_Message = Localization.Get("TwitchBitsDeathMessage");
		ingameHypeTrainDeathScreen_Message = Localization.Get("TwitchHypeTrainDeathMessage");
		ingameVoteDeathScreen_Message = Localization.Get("TwitchVoteDeathMessage");
		subPointDisplay = Localization.Get("xuiOptionsTwitchSubPointDisplay");
		ViewerData.SetupLocalization();
		VotingManager.SetupLocalization();
		LeaderboardStats.SetupLocalization();
	}

	public void SetupClient(string twitchChannel, string password)
	{
		ircClient = new TwitchIRCClient("irc.twitch.tv", 6667, twitchChannel, password);
		if (EventSub == null)
		{
			EventSub = new EventSubClient(Authentication.userID, Authentication.oauth.Substring(6), TwitchAuthentication.client_id);
		}
	}

	public void IntegrationTypeChanged()
	{
		if (IsReady && extensionManager == null)
		{
			extensionManager = new ExtensionManager();
			extensionManager.Init();
		}
	}

	public void CleanupData()
	{
		BaseTwitchCommand.ClearCommandPermissionOverrides();
		VotingManager.CleanupData();
		CooldownPresets.Clear();
		tipTitleList.Clear();
		tipDescriptionList.Clear();
		ActionPresets.Clear();
		VotePresets.Clear();
		CurrentActionPreset = null;
		CurrentVotePreset = null;
		CleanupEventData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveChannelPointRedeems()
	{
		if (CurrentEventPreset != null)
		{
			CurrentEventPreset.RemoveChannelPointRedemptions();
		}
	}

	public void CleanupEventData()
	{
		RemoveChannelPointRedeems();
		CurrentEventPreset = null;
		EventPresets.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Disconnect()
	{
		RemoveChannelPointRedeems();
		if (EventSub != null)
		{
			EventSub.Disconnect();
			EventSub.Cleanup();
			EventSub = null;
		}
		if (ircClient != null)
		{
			ircClient.Disconnect();
			ircClient = null;
		}
		if (extensionManager != null)
		{
			extensionManager.Cleanup();
			extensionManager = null;
		}
		Authentication = null;
		if (LocalPlayer != null && LocalPlayer.PlayerUI != null && LocalPlayer.PlayerUI.windowManager.IsWindowOpen("twitch"))
		{
			LocalPlayer.PlayerUI.windowManager.Close("twitch");
		}
	}

	public void AddRandomGroup(string name, int randomCount)
	{
		if (!randomGroups.ContainsKey(name))
		{
			randomGroups.Add(name, new TwitchRandomActionGroup
			{
				Name = name,
				RandomCount = randomCount
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetDailyCommands(int currentDay, int lastGameStage = -1)
	{
		randomKeys.Clear();
		foreach (string key in AvailableCommands.Keys)
		{
			TwitchAction twitchAction = AvailableCommands[key];
			if (!twitchAction.IsInPreset(CurrentActionPreset))
			{
				continue;
			}
			if (twitchAction.RandomDaily)
			{
				if (!randomKeys.ContainsKey(twitchAction.RandomGroup))
				{
					randomKeys.Add(twitchAction.RandomGroup, new List<TwitchAction>());
				}
				randomKeys[twitchAction.RandomGroup].Add(twitchAction);
			}
			else if (twitchAction.SingleDayUse)
			{
				twitchAction.AllowedDay = currentDay;
			}
		}
		foreach (string key2 in randomKeys.Keys)
		{
			int num = 1;
			if (randomGroups.ContainsKey(key2))
			{
				num = randomGroups[key2].RandomCount;
			}
			List<TwitchAction> list = randomKeys[key2];
			if (lastGameStage != -1)
			{
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].StartGameStage > lastGameStage)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				int index2 = UnityEngine.Random.Range(0, list.Count);
				TwitchAction value = list[index];
				list[index] = list[index2];
				list[index2] = value;
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].AllowedDay = ((k < num) ? currentDay : (-1));
			}
		}
		if (this.CommandsChanged != null)
		{
			this.CommandsChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTwitchCommands()
	{
		TwitchCommandList.Clear();
		TwitchCommandList.Add(new TwitchCommandAddBitCredit());
		TwitchCommandList.Add(new TwitchCommandAddPoints());
		TwitchCommandList.Add(new TwitchCommandAddSpecialPoints());
		TwitchCommandList.Add(new TwitchCommandCheckCredit());
		TwitchCommandList.Add(new TwitchCommandCheckPoints());
		TwitchCommandList.Add(new TwitchCommandCommands());
		TwitchCommandList.Add(new TwitchCommandDebug());
		TwitchCommandList.Add(new TwitchCommandDisableCommand());
		TwitchCommandList.Add(new TwitchCommandGamestage());
		TwitchCommandList.Add(new TwitchCommandPauseCommand());
		TwitchCommandList.Add(new TwitchCommandUnpauseCommand());
		TwitchCommandList.Add(new TwitchCommandRemoveViewer());
		TwitchCommandList.Add(new TwitchCommandResetCooldowns());
		TwitchCommandList.Add(new TwitchCommandSetBitPot());
		TwitchCommandList.Add(new TwitchCommandSetCooldown());
		TwitchCommandList.Add(new TwitchCommandSetPot());
		TwitchCommandList.Add(new TwitchCommandTeleportBackpack());
		if (CurrentEventPreset.HasBitEvents && AllowBitEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemBits());
		}
		if (CurrentEventPreset.HasSubEvents && AllowSubEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemSub());
		}
		if (CurrentEventPreset.HasGiftSubEvents && AllowGiftSubEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemGiftSub());
		}
		if (CurrentEventPreset.HasRaidEvents && AllowRaidEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemRaid());
		}
		if (CurrentEventPreset.HasCharityEvents && AllowCharityEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemCharity());
		}
		if (CurrentEventPreset.HasHypeTrainEvents && AllowHypeTrainEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemHypeTrain());
		}
		if (CurrentEventPreset.HasCreatorGoalEvents && AllowCreatorGoalEvents)
		{
			TwitchCommandList.Add(new TwitchCommandRedeemCreatorGoal());
		}
		TwitchCommandList.Add(new TwitchCommandUseProgression());
	}

	public void StartTwitchIntegration()
	{
		updateTime = 60f;
		try
		{
			if (Authentication == null)
			{
				Authentication = new TwitchAuthentication();
			}
			Authentication.StopListener();
			Authentication.GetToken();
		}
		catch (Exception ex)
		{
			Log.Out("Twitch integration failed to start with message " + ex.Message);
			updateTime = 5f;
		}
		InitialCooldownSet = false;
		InitState = InitStates.WaitingForOAuth;
	}

	public void StopTwitchIntegration(InitStates initState = InitStates.None)
	{
		resetClientAttempts = 0;
		Disconnect();
		TwitchDisconnectPartyUpdate();
		ClearEventHandlers();
		if (LocalPlayer != null)
		{
			LocalPlayer.TwitchEnabled = false;
			LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
		}
		if (Authentication != null)
		{
			Authentication.StopListener();
		}
		InitState = initState;
	}

	public void WaitForOAuth()
	{
		updateTime = 10f;
		InitState = InitStates.WaitingForOAuth;
	}

	public void WaitForPermission()
	{
		updateTime = 10f;
		InitState = InitStates.WaitingForPermission;
	}

	public void DeniedPermission()
	{
		InitState = InitStates.PermissionDenied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearEventHandlers()
	{
		GameEventManager current = GameEventManager.Current;
		current.GameEntitySpawned -= Current_GameEntitySpawned;
		current.GameEntityDespawned -= Current_GameEntityDespawned;
		current.GameEntityKilled -= Current_GameEntityKilled;
		current.GameBlocksAdded -= Current_GameBlocksAdded;
		current.GameBlocksRemoved -= Current_GameBlocksRemoved;
		current.GameBlockRemoved -= Current_GameBlockRemoved;
		current.GameEventApproved -= Current_GameEventApproved;
		current.TwitchPartyGameEventApproved -= Current_TwitchPartyGameEventApproved;
		current.TwitchRefundNeeded -= Current_TwitchRefundNeeded;
		current.GameEventDenied -= Current_GameEventDenied;
		current.GameEventCompleted -= Current_GameEventCompleted;
		if (LocalPlayer != null)
		{
			LocalPlayer.PartyLeave -= LocalPlayer_PartyLeave;
			LocalPlayer.PartyJoined -= LocalPlayer_PartyJoined;
			LocalPlayer.PartyChanged -= LocalPlayer_PartyChanged;
			if (LocalPlayer.Party != null)
			{
				LocalPlayer.Party.PartyMemberAdded -= Party_PartyMemberAdded;
				LocalPlayer.Party.PartyMemberRemoved -= Party_PartyMemberRemoved;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_TwitchPartyGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		foreach (TwitchAction value in TwitchActionManager.TwitchActions.Values)
		{
			if (value.IsInPreset(CurrentActionPreset) && value.EventName == gameEventID)
			{
				AddCooldownForAction(value);
				break;
			}
		}
	}

	public void Update(float deltaTime)
	{
		GameManager gameManager = GameManager.Instance;
		if (gameManager.World == null || gameManager.World.Players == null || gameManager.World.Players.Count == 0)
		{
			return;
		}
		CurrentUnityTime = Time.time;
		switch (InitState)
		{
		case InitStates.Setup:
		{
			GameEventManager current2 = GameEventManager.Current;
			current2.GameEventAccessApproved -= Current_GameEventAccessApproved;
			current2.GameEventAccessApproved += Current_GameEventAccessApproved;
			SetupLocalization();
			if (!isLoaded)
			{
				isLoaded = true;
				LoadViewerData();
			}
			if (!LoadLatestMainViewerData() && !LoadMainViewerData())
			{
				LoadSpecialViewerData();
			}
			InitState = InitStates.None;
			break;
		}
		case InitStates.WaitingForPermission:
			updateTime -= deltaTime;
			if (updateTime <= 0f)
			{
				StopTwitchIntegration();
				Log.Warning("Twitch: login failed in " + InitState.ToString() + " state");
				InitState = InitStates.Failed;
				return;
			}
			break;
		case InitStates.WaitingForOAuth:
			updateTime -= deltaTime;
			if (updateTime <= 0f)
			{
				StopTwitchIntegration();
				Log.Warning("Twitch: Login failed in " + InitState.ToString() + " state");
				InitState = InitStates.Failed;
				return;
			}
			if (Authentication.oauth != "" && Authentication.userName != "" && Authentication.userID != "")
			{
				SetupClient(Authentication.userName, Authentication.oauth);
				EventSub.OnEventReceived -= EventSubMessageReceived;
				EventSub.OnEventReceived += EventSubMessageReceived;
				EventSub.Connect();
				updateTime = 3f;
				Log.Out("retrieved oauth. Waiting for IRC to post auth...");
				InitState = InitStates.Authenticating;
			}
			break;
		case InitStates.Authenticating:
			updateTime -= deltaTime;
			if (updateTime <= 0f)
			{
				if (!(Authentication.oauth != "") || !(Authentication.userName != "") || !(Authentication.userID != "") || resetClientAttempts++ >= 5)
				{
					Log.Warning("Twitch: Login failed in " + InitState.ToString() + " state");
					StopTwitchIntegration(InitStates.Failed);
					InitState = InitStates.Failed;
					return;
				}
				SetupClient(Authentication.userName, Authentication.oauth);
				updateTime = 2.5f;
				Log.Out("attempting to reset client...");
			}
			break;
		case InitStates.CheckingForExtension:
			if (!AllowActions)
			{
				InitState = InitStates.Authenticated;
			}
			else
			{
				if (checkingExtensionInstalled)
				{
					break;
				}
				checkingExtensionInstalled = true;
				ExtensionManager.CheckExtensionInstalled([PublicizedFrom(EAccessModifier.Private)] (bool IsInstalled) =>
				{
					checkingExtensionInstalled = false;
					if (!IsInstalled)
					{
						XUiC_MessageBoxWindowGroup.ShowMessageBox(LocalPlayerXUi, Localization.Get("xuiTwitchPopup_ExtensionNeededHeader"), Localization.Get("xuiTwitchPopup_ExtensionNeeded"));
						Application.OpenURL("https://dashboard.twitch.tv/extensions/k6ji189bf7i4ge8il4iczzw7kpgmjt");
					}
					InitState = InitStates.Authenticated;
				});
			}
			break;
		case InitStates.Authenticated:
		{
			ClearEventHandlers();
			GameEventManager current = GameEventManager.Current;
			current.GameEntitySpawned += Current_GameEntitySpawned;
			current.GameEntityDespawned += Current_GameEntityDespawned;
			current.GameEntityKilled += Current_GameEntityKilled;
			current.GameBlocksAdded += Current_GameBlocksAdded;
			current.GameBlockRemoved += Current_GameBlockRemoved;
			current.GameBlocksRemoved += Current_GameBlocksRemoved;
			current.GameEventApproved += Current_GameEventApproved;
			current.TwitchPartyGameEventApproved += Current_TwitchPartyGameEventApproved;
			current.TwitchRefundNeeded += Current_TwitchRefundNeeded;
			current.GameEventDenied += Current_GameEventDenied;
			current.GameEventCompleted += Current_GameEventCompleted;
			world = gameManager.World;
			if (extensionManager == null)
			{
				extensionManager = new ExtensionManager();
				extensionManager.Init();
			}
			InitState = InitStates.Ready;
			QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.Enabled, "");
			break;
		}
		case InitStates.Ready:
			if (LocalPlayer == null)
			{
				SetupTwitchCommands();
				LocalPlayer = XUiM_Player.GetPlayer() as EntityPlayerLocal;
				RefreshPartyInfo();
				HighestGameStage = LocalPlayer.unModifiedGameStage;
				ActionMessages.Clear();
				GetCooldownMax();
				SetCooldown(CurrentCooldownPreset.NextCooldownTime, CooldownTypes.Startup, displayToChannel: false, playCooldownSound: false);
				LocalPlayer.PartyLeave += LocalPlayer_PartyLeave;
				LocalPlayer.PartyJoined += LocalPlayer_PartyJoined;
				LocalPlayer.PartyChanged += LocalPlayer_PartyChanged;
				if (LocalPlayer.Party != null)
				{
					LocalPlayer.Party.PartyMemberAdded += Party_PartyMemberAdded;
					LocalPlayer.Party.PartyMemberRemoved += Party_PartyMemberRemoved;
				}
				if (!InitialCooldownSet)
				{
					if (CurrentCooldownPreset.StartCooldownTime > 0)
					{
						SetCooldown(100000f, CooldownTypes.Startup);
					}
					if (CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
					{
						SetCooldown(0f, CooldownTypes.None, displayToChannel: false, playCooldownSound: false);
					}
					CurrentActionPreset.HandleCooldowns();
					InitialCooldownSet = true;
				}
			}
			LocalPlayer.TwitchEnabled = true;
			LocalPlayerInLandClaim = GameManager.Instance.World.GetLandClaimOwnerInParty(LocalPlayer, LocalPlayer.persistentPlayerData);
			if (!ircClient.IsConnected)
			{
				Log.Out("Reached 'Ready' but waiting for IRC to post auth message...");
				ircClient.Reconnect();
				InitState = InitStates.Authenticating;
				updateTime = 30f;
			}
			LeaderboardStats.UpdateStats(deltaTime);
			if (resetCommandsNeeded)
			{
				ResetCommands();
			}
			if (!XUi.InGameMenuOpen && AllowActions && ExtensionCheckTime < 0f)
			{
				ExtensionCheckTime = 30f;
				ExtensionManager.CheckExtensionInstalled([PublicizedFrom(EAccessModifier.Private)] (bool IsInstalled) =>
				{
					if (IsInstalled)
					{
						extensionActiveCheckFailures = 0;
					}
					else if (extensionActiveCheckFailures < 3)
					{
						extensionActiveCheckFailures++;
					}
					else
					{
						extensionActiveCheckFailures = 0;
						LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(LocalPlayer);
						ircClient.SendChannelMessage(Localization.Get("TwitchChat_ExtensionNotInstalled"), useQueue: false);
						XUiC_ChatOutput.AddMessage(uIForPlayer.xui, EnumGameMessages.PlainTextLocal, Localization.Get("TwitchChat_ExtensionNotInstalled"));
						StopTwitchIntegration();
						InitState = InitStates.ExtensionNotInstalled;
						Application.OpenURL("https://dashboard.twitch.tv/extensions/k6ji189bf7i4ge8il4iczzw7kpgmjt");
					}
				});
			}
			ExtensionCheckTime -= deltaTime;
			if (!LocalPlayer.Buffs.HasBuff("twitch_extensionneeded"))
			{
				break;
			}
			updateTime -= deltaTime;
			if (!(updateTime <= 0f))
			{
				break;
			}
			ExtensionManager.CheckExtensionInstalled([PublicizedFrom(EAccessModifier.Private)] (bool IsInstalled) =>
			{
				if (IsInstalled)
				{
					LocalPlayer.Buffs.RemoveBuff("twitch_extensionneeded");
				}
			});
			updateTime = 5f;
			break;
		}
		if (extensionManager != null)
		{
			if (extensionManager.HasCommand())
			{
				ExtensionAction command = extensionManager.GetCommand();
				HandleExtensionMessage(int.Parse(command.username), command.command, isRerun: false, command.creditUsed, (command is ExtensionBitAction extensionBitAction) ? extensionBitAction.cost : 0);
			}
			extensionManager.Update();
		}
		if (ircClient != null)
		{
			ircClient.Update(deltaTime);
			if (ircClient.AvailableMessage())
			{
				HandleMessage(ircClient.ReadMessage());
			}
			ViewerData.Update(deltaTime);
			if (LocalPlayer == null)
			{
				return;
			}
			bool flag = false;
			for (int num = LiveActionEntries.Count - 1; num >= 0; num--)
			{
				if (LiveActionEntries[num].ReadyForRemove)
				{
					LiveActionEntries.RemoveAt(num);
				}
				else if (LiveActionEntries[num].Action.CooldownBlocked)
				{
					flag = true;
				}
			}
			for (int num2 = actionSpawnLiveList.Count - 1; num2 >= 0; num2--)
			{
				if (actionSpawnLiveList[num2].SpawnedEntity == null)
				{
					actionSpawnLiveList.RemoveAt(num2);
				}
			}
			for (int num3 = LiveEvents.Count - 1; num3 >= 0; num3--)
			{
				if (LiveEvents[num3].ReadyForRemove)
				{
					LiveEvents.RemoveAt(num3);
				}
			}
			if (LocalPlayer.IsAlive() && CooldownTime > 0f && TwitchActive)
			{
				if (CooldownType == CooldownTypes.MaxReachedWaiting && actionSpawnLiveList.Count == 0 && !flag)
				{
					SetCooldown(CooldownTime, CooldownTypes.MaxReached);
				}
				if (CooldownType == CooldownTypes.MaxReached || CooldownType == CooldownTypes.Time || CooldownType == CooldownTypes.Startup || CooldownType == CooldownTypes.SafeCooldownExit)
				{
					float cooldownTime = CooldownTime;
					CooldownTime -= Time.deltaTime;
					if (cooldownTime >= 15f && CooldownTime < 15f && CooldownTime > 0f && CooldownType != CooldownTypes.SafeCooldownExit)
					{
						LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.TempDisabledEnding;
					}
				}
				if (CooldownTime <= 0f)
				{
					if (CooldownType == CooldownTypes.SafeCooldownExit)
					{
						HandleEndCooldownStateChanging();
					}
					else
					{
						HandleEndCooldown();
					}
					VotingManager.VoteStartDelayTimeRemaining = 10f;
				}
			}
			for (int num4 = liveList.Count - 1; num4 >= 0; num4--)
			{
				if (liveList[num4].SpawnedEntity == null)
				{
					liveList[num4].SpawnedEntity = world.GetEntity(liveList[num4].SpawnedEntityID);
					_ = liveList[num4].SpawnedEntity == null;
				}
			}
			for (int num5 = recentlyDeadList.Count - 1; num5 >= 0; num5--)
			{
				recentlyDeadList[num5].TimeRemaining -= deltaTime;
				if (recentlyDeadList[num5].TimeRemaining <= 0f)
				{
					recentlyDeadList.RemoveAt(num5);
				}
			}
			for (int num6 = liveBlockList.Count - 1; num6 >= 0; num6--)
			{
				TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = liveBlockList[num6];
				if (twitchSpawnedBlocksEntry.TimeRemaining > 0f)
				{
					twitchSpawnedBlocksEntry.TimeRemaining -= deltaTime;
					if (twitchSpawnedBlocksEntry.TimeRemaining <= 0f)
					{
						liveBlockList.RemoveAt(num6);
					}
				}
			}
			int num7 = GameUtils.WorldTimeToDays(world.worldTime);
			if (num7 != lastGameDay)
			{
				SetupAvailableCommands();
				ResetDailyCommands(num7);
				HandleCooldownActionLocking();
				lastGameDay = num7;
			}
			if (CooldownType != CooldownTypes.Startup && TwitchActive && !gameManager.IsPaused() && InitState == InitStates.Ready)
			{
				VotingManager.Update(deltaTime);
			}
			HandleEventQueue();
			if (LocalPlayer.IsAlive() && CooldownType != CooldownTypes.Time)
			{
				for (int num8 = 0; num8 < QueuedActionEntries.Count; num8++)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
					{
						if (!QueuedActionEntries[num8].IsSent)
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventRequest>().Setup(QueuedActionEntries[num8].Action.EventName, QueuedActionEntries[num8].Target.entityId, _isTwitchEvent: true, Vector3.zero, null, QueuedActionEntries[num8].UserName, "action", AllowCrateSharing, _allowRefunds: true, null));
							QueuedActionEntries[num8].IsSent = true;
							break;
						}
						continue;
					}
					if (GameEventManager.Current.HandleAction(QueuedActionEntries[num8].Action.EventName, LocalPlayer, QueuedActionEntries[num8].Target, twitchActivated: true, QueuedActionEntries[num8].UserName, "action", AllowCrateSharing))
					{
						if (LocalPlayer.Party != null)
						{
							for (int num9 = 0; num9 < LocalPlayer.Party.MemberList.Count; num9++)
							{
								EntityPlayer entityPlayer = LocalPlayer.Party.MemberList[num9];
								if (entityPlayer != LocalPlayer && entityPlayer.TwitchEnabled)
								{
									SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(QueuedActionEntries[num8].Action.EventName, QueuedActionEntries[num8].Target.entityId, QueuedActionEntries[num8].UserName, "action", NetPackageGameEventResponse.ResponseTypes.TwitchPartyActionApproved), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
								}
							}
						}
						GameEventManager.Current.HandleGameEventApproved(QueuedActionEntries[num8].Action.EventName, QueuedActionEntries[num8].Target.entityId, QueuedActionEntries[num8].UserName, "action");
					}
					else
					{
						TwitchActionEntry twitchActionEntry = QueuedActionEntries[num8];
						ViewerEntry viewerEntry = ViewerData.GetViewerEntry(twitchActionEntry.UserName);
						AddActionHistory(twitchActionEntry, viewerEntry, TwitchActionHistoryEntry.EntryStates.Reimbursed);
						ShowReimburseMessage(twitchActionEntry, viewerEntry);
						ViewerData.ReimburseAction(twitchActionEntry);
						QueuedActionEntries.RemoveAt(num8);
					}
					break;
				}
			}
			saveTime -= Time.deltaTime;
			if (saveTime <= 0f && (dataSaveThreadInfo == null || dataSaveThreadInfo.HasTerminated()))
			{
				saveTime = 30f;
				if (HasDataChanges)
				{
					SaveViewerData();
					HasDataChanges = false;
				}
			}
			updateTime -= deltaTime;
			if (updateTime <= 0f)
			{
				updateTime = 2f;
				if (UseProgression && !OverrideProgession && commandsAvailable == -1)
				{
					commandsAvailable = GetCommandCount();
				}
				RefreshCommands(displayMessage: true);
				int num10 = GameStats.GetInt(EnumGameStats.BloodMoonDay);
				if (nextBMDay != num10)
				{
					nextBMDay = num10;
					if (num7 != currentBMDayEnd)
					{
						currentBMDayEnd = nextBMDay + 1;
					}
					SetupBloodMoonData();
				}
				RefreshVoteLockedLevel();
				IsSafe = LocalPlayer.TwitchSafe;
				isBMActive = false;
				if (CooldownType != CooldownTypes.Time && !IsVoting)
				{
					if (UseActionsDuringBloodmoon != 1)
					{
						if (WithinBloodMoonPeriod())
						{
							isBMActive = true;
							if (CooldownType != CooldownTypes.BloodMoonDisabled && CooldownType != CooldownTypes.BloodMoonCooldown)
							{
								SetCooldown(5f, (UseActionsDuringBloodmoon == 0) ? CooldownTypes.BloodMoonDisabled : CooldownTypes.BloodMoonCooldown);
							}
						}
						else if (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.BloodMoonCooldown)
						{
							SetCooldown(5f, CooldownTypes.Time);
							currentBMDayEnd = nextBMDay + 1;
							VotingManager.VoteStartDelayTimeRemaining += 35f;
						}
					}
					else if (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.BloodMoonCooldown)
					{
						SetCooldown(5f, CooldownTypes.Time);
					}
					if (AllowActions && CooldownType != CooldownTypes.BloodMoonDisabled && CooldownType != CooldownTypes.BloodMoonCooldown)
					{
						if (UseActionsDuringQuests != 1 && CooldownType != CooldownTypes.Time)
						{
							if (QuestEventManager.Current.QuestBounds.width != 0f)
							{
								if (UseActionsDuringQuests == 0 && CooldownType != CooldownTypes.QuestDisabled)
								{
									SetCooldown(5f, CooldownTypes.QuestDisabled, displayToChannel: false, playCooldownSound: false);
								}
								else if (UseActionsDuringQuests == 2 && CooldownType != CooldownTypes.QuestCooldown)
								{
									SetCooldown(5f, CooldownTypes.QuestCooldown, displayToChannel: false, playCooldownSound: false);
								}
							}
							else if (CooldownType == CooldownTypes.QuestCooldown || CooldownType == CooldownTypes.QuestDisabled)
							{
								SetCooldown(60f, CooldownTypes.Time);
								CurrentCooldownFill = 0f;
							}
						}
						else if (UseActionsDuringQuests == 1 && (CooldownType == CooldownTypes.QuestCooldown || CooldownType == CooldownTypes.QuestDisabled))
						{
							SetCooldown(60f, CooldownTypes.Time);
						}
						if (CooldownType != CooldownTypes.Time && CooldownType != CooldownTypes.Startup && CooldownType != CooldownTypes.MaxReached && CooldownType != CooldownTypes.MaxReachedWaiting && CooldownType != CooldownTypes.QuestCooldown && CooldownType != CooldownTypes.QuestDisabled)
						{
							if (CooldownType != CooldownTypes.SafeCooldown && LocalPlayer.TwitchSafe)
							{
								SetCooldown(5f, CooldownTypes.SafeCooldown, displayToChannel: false, playCooldownSound: false);
							}
							else if (CooldownType == CooldownTypes.SafeCooldown && !LocalPlayer.TwitchSafe)
							{
								SetCooldown(5f, CooldownTypes.SafeCooldownExit);
							}
						}
					}
					else if (CooldownType == CooldownTypes.QuestCooldown || CooldownType == CooldownTypes.QuestDisabled)
					{
						SetCooldown(60f, CooldownTypes.Time);
					}
				}
				for (int num11 = RespawnEntries.Count - 1; num11 >= 0; num11--)
				{
					TwitchRespawnEntry twitchRespawnEntry = RespawnEntries[num11];
					if (twitchRespawnEntry.CanRespawn(this))
					{
						EntityPlayer target = twitchRespawnEntry.Target;
						if (!PartyInfo.ContainsKey(target) || !(PartyInfo[target].Cooldown > 0f))
						{
							if (target.Buffs.HasBuff("twitch_pausedspawns"))
							{
								target.Buffs.RemoveBuff("twitch_pausedspawns");
							}
							target.PlayOneShot("twitch_unpause");
							QueuedActionEntries.Add(twitchRespawnEntry.RespawnAction());
						}
					}
				}
			}
			if (lastAlive && LocalPlayer.IsDead())
			{
				CurrentCooldownFill = 0f;
				if (CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
				{
					SetCooldown(CurrentCooldownPreset.AfterDeathCooldownTime, CooldownTypes.Time);
				}
				KillAllSpawnsForPlayer(LocalPlayer);
			}
			if (lastAlive && LocalPlayer.IsAlive())
			{
				DeathText = "";
			}
			if (!lastAlive && LocalPlayer.IsAlive())
			{
				respawnEventNeeded = true;
			}
			if (respawnEventNeeded && CheckCanRespawnEvent(LocalPlayer))
			{
				if (OnPlayerRespawnEvent != "")
				{
					GameEventManager.Current.HandleAction(OnPlayerRespawnEvent, LocalPlayer, LocalPlayer, twitchActivated: false);
				}
				respawnEventNeeded = false;
			}
			lastAlive = LocalPlayer.IsAlive();
			twitchPlayerDeathsThisFrame.Clear();
			UpdatePartyInfo(deltaTime);
		}
		HandleInGameChatQueue();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEndCooldown()
	{
		CurrentCooldownFill = 0f;
		Manager.BroadcastPlayByLocalPlayer(LocalPlayer.position, "twitch_cooldown_ended");
		ircClient.SendChannelMessage(chatOutput_CooldownComplete, useQueue: true);
		HandleEndCooldownStateChanging();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEndCooldownStateChanging()
	{
		CooldownType = CooldownTypes.None;
		if (LocalPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabled || LocalPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabledEnding)
		{
			LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
		}
		if (this.ConnectionStateChanged != null)
		{
			this.ConnectionStateChanged(initState, initState);
		}
		HandleCooldownActionLocking();
	}

	public void AddToInGameChatQueue(string msg, string sound = null)
	{
		inGameChatQueue.Add(new TwitchMessageEntry(msg, sound));
	}

	public void HandleInGameChatQueue()
	{
		if (inGameChatQueue.Count > 0 && LocalPlayer != null && LocalPlayer.IsAlive())
		{
			TwitchMessageEntry twitchMessageEntry = inGameChatQueue[0];
			XUiC_ChatOutput.AddMessage(LocalPlayerXUi, EnumGameMessages.PlainTextLocal, twitchMessageEntry.Message, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
			if (twitchMessageEntry.Sound != null)
			{
				LocalPlayer.PlayOneShot(twitchMessageEntry.Sound);
			}
			inGameChatQueue.RemoveAt(0);
		}
	}

	public void RefreshVoteLockedLevel()
	{
		VoteLockedLevel = LocalPlayer.HasTwitchVoteLockMember();
	}

	public void SetupBloodMoonData()
	{
		(int duskHour, int dawnHour) tuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		int item = tuple.duskHour;
		int item2 = tuple.dawnHour;
		BMCooldownStart = item - CurrentCooldownPreset.BMStartOffset;
		BMCooldownEnd = item2 + CurrentCooldownPreset.BMEndOffset;
	}

	public bool WithinBloodMoonPeriod()
	{
		ulong worldTime = world.worldTime;
		int num = GameUtils.WorldTimeToDays(worldTime);
		int num2 = GameUtils.WorldTimeToHours(worldTime);
		if (num == nextBMDay)
		{
			if (num2 >= BMCooldownStart)
			{
				return true;
			}
		}
		else if (num > 1 && num == currentBMDayEnd && num2 < BMCooldownEnd)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyMemberAdded(EntityPlayer player)
	{
		GetCooldownMax();
		if (!PartyInfo.ContainsKey(player))
		{
			PartyInfo.Add(player, new TwitchPartyMemberInfo());
			extensionManager?.OnPartyChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyMemberRemoved(EntityPlayer player)
	{
		GetCooldownMax();
		if (PartyInfo.ContainsKey(player))
		{
			PartyInfo.Remove(player);
			extensionManager?.OnPartyChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RefreshPartyInfo()
	{
		if (LocalPlayer == null)
		{
			return;
		}
		if (LocalPlayer.Party == null)
		{
			PartyInfo.Clear();
			return;
		}
		PartyInfo.Clear();
		for (int i = 0; i < LocalPlayer.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = LocalPlayer.Party.MemberList[i];
			if (!(entityPlayer == LocalPlayer) && !PartyInfo.ContainsKey(entityPlayer))
			{
				PartyInfo.Add(entityPlayer, new TwitchPartyMemberInfo());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdatePartyInfo(float deltaTime)
	{
		bool flag = false;
		foreach (EntityPlayer key in PartyInfo.Keys)
		{
			TwitchPartyMemberInfo twitchPartyMemberInfo = PartyInfo[key];
			if (!twitchPartyMemberInfo.LastAlive && key.IsAlive() && PartyRespawnEvent != "")
			{
				GameEventManager.Current.HandleAction(PartyRespawnEvent, LocalPlayer, key, twitchActivated: false);
			}
			if (twitchPartyMemberInfo.LastOptedOut != (key.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled))
			{
				twitchPartyMemberInfo.LastOptedOut = key.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled;
				flag = true;
			}
			if (twitchPartyMemberInfo.Cooldown > 0f)
			{
				twitchPartyMemberInfo.Cooldown -= deltaTime;
			}
			if (twitchPartyMemberInfo.LastAlive && !key.IsAlive())
			{
				KillAllSpawnsForPlayer(key);
				twitchPartyMemberInfo.Cooldown = 60f;
			}
			if (!twitchPartyMemberInfo.LastAlive && key.IsAlive())
			{
				twitchPartyMemberInfo.NeedsRespawnEvent = true;
			}
			if (twitchPartyMemberInfo.NeedsRespawnEvent && CheckCanRespawnEvent(key) && twitchPartyMemberInfo.Cooldown <= 0f)
			{
				if (OnPlayerRespawnEvent != "")
				{
					GameEventManager.Current.HandleAction(OnPlayerRespawnEvent, LocalPlayer, key, twitchActivated: false);
				}
				twitchPartyMemberInfo.NeedsRespawnEvent = false;
			}
			twitchPartyMemberInfo.LastAlive = key.IsAlive();
		}
		if (flag)
		{
			GetCooldownMax();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TwitchDisconnectPartyUpdate()
	{
		RespawnEntries.Clear();
		if (OnPlayerRespawnEvent != "")
		{
			GameEventManager.Current.HandleAction(OnPlayerRespawnEvent, LocalPlayer, LocalPlayer, twitchActivated: false);
		}
		if (LocalPlayer != null && LocalPlayer.Buffs.HasBuff("twitch_pausedspawns"))
		{
			LocalPlayer.Buffs.RemoveBuff("twitch_pausedspawns");
		}
		foreach (EntityPlayer key in PartyInfo.Keys)
		{
			TwitchPartyMemberInfo twitchPartyMemberInfo = PartyInfo[key];
			if (twitchPartyMemberInfo.NeedsRespawnEvent)
			{
				if (OnPlayerRespawnEvent != "")
				{
					GameEventManager.Current.HandleAction(OnPlayerRespawnEvent, LocalPlayer, key, twitchActivated: false);
				}
				twitchPartyMemberInfo.NeedsRespawnEvent = false;
			}
			if (key.Buffs.HasBuff("twitch_pausedspawns"))
			{
				key.Buffs.RemoveBuff("twitch_pausedspawns");
			}
		}
	}

	public bool CheckCanRespawnEvent(EntityPlayer player)
	{
		if (player != null && player.IsAlive() && player.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled)
		{
			return !player.TwitchSafe;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void KillAllSpawnsForPlayer(EntityPlayer player)
	{
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = RespawnEntries.Count - 1; num >= 0; num--)
		{
			TwitchRespawnEntry twitchRespawnEntry = RespawnEntries[num];
			if (twitchRespawnEntry != null && twitchRespawnEntry.Target == player)
			{
				twitchRespawnEntry.NeedsRespawn = twitchRespawnEntry.SpawnedEntities.Count > twitchRespawnEntry.Action.RespawnThreshold;
				if (twitchRespawnEntry.NeedsRespawn)
				{
					if (flag)
					{
						stringBuilder.Append(", ");
					}
					flag = true;
					stringBuilder.Append(twitchRespawnEntry.Action.Command);
				}
				else
				{
					Debug.LogWarning($"Respawn Entry removed '{twitchRespawnEntry.Action.Command}' because count {twitchRespawnEntry.SpawnedEntities.Count} was less than {twitchRespawnEntry.Action.RespawnThreshold}");
					RespawnEntries.RemoveAt(num);
				}
			}
		}
		if (flag)
		{
			if (!player.Buffs.HasBuff("twitch_pausedspawns"))
			{
				player.Buffs.AddBuff("twitch_pausedspawns");
			}
			string text = stringBuilder.ToString();
			string msg = string.Format(ingameOutput_BitRespawns, text);
			AddToInGameChatQueue(msg, "twitch_pause");
			Debug.LogWarning($"Respawns Found for {player.EntityName}: {text}");
		}
		else
		{
			Debug.LogWarning("No Respawns Found!");
			if (player.Buffs.HasBuff("twitch_pausedspawns"))
			{
				player.Buffs.RemoveBuff("twitch_pausedspawns");
			}
		}
		if (OnPlayerDeathEvent != "")
		{
			GameEventManager.Current.HandleAction(OnPlayerDeathEvent, LocalPlayer, player, twitchActivated: false);
		}
		for (int num2 = LiveActionEntries.Count - 1; num2 >= 0; num2--)
		{
			if (LiveActionEntries[num2] != null && LiveActionEntries[num2].Target == player)
			{
				TwitchActionEntry twitchActionEntry = LiveActionEntries[num2];
				twitchActionEntry.ReadyForRemove = true;
				if (twitchActionEntry.HistoryEntry != null)
				{
					twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalPlayer_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		if (extensionManager != null)
		{
			extensionManager.OnPartyChanged();
		}
		GetCooldownMax();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalPlayer_PartyLeave(Party _affectedParty, EntityPlayer _player)
	{
		if (extensionManager != null)
		{
			extensionManager.OnPartyChanged();
		}
		GetCooldownMax();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalPlayer_PartyJoined(Party _affectedParty, EntityPlayer _player)
	{
		if (LocalPlayer.Party != null)
		{
			LocalPlayer.Party.PartyMemberAdded += Party_PartyMemberAdded;
			LocalPlayer.Party.PartyMemberRemoved += Party_PartyMemberRemoved;
		}
		if (extensionManager != null)
		{
			extensionManager.OnPartyChanged();
		}
		GetCooldownMax();
		RefreshPartyInfo();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEntityKilled(int entityID)
	{
		for (int num = liveList.Count - 1; num >= 0; num--)
		{
			TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = liveList[num];
			if (twitchSpawnedEntityEntry.SpawnedEntityID == entityID)
			{
				if (twitchSpawnedEntityEntry.RespawnEntry != null)
				{
					TwitchRespawnEntry respawnEntry = twitchSpawnedEntityEntry.RespawnEntry;
					if (respawnEntry.RemoveSpawnedEntry(entityID, checkForRemove: true) && respawnEntry.ReadyForRemove)
					{
						RespawnEntries.Remove(respawnEntry);
					}
				}
				actionSpawnLiveList.Remove(twitchSpawnedEntityEntry);
				recentlyDeadList.Add(new TwitchRecentlyRemovedEntityEntry(twitchSpawnedEntityEntry));
				liveList.RemoveAt(num);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEntityDespawned(int entityID)
	{
		for (int num = liveList.Count - 1; num >= 0; num--)
		{
			if (liveList[num].SpawnedEntityID == entityID)
			{
				TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = liveList[num];
				if (twitchSpawnedEntityEntry.Action != null && twitchSpawnedEntityEntry.Action.UserName != null)
				{
					ViewerData.AddPoints(twitchSpawnedEntityEntry.Action.UserName, (int)((float)twitchSpawnedEntityEntry.Action.ActionCost * 0.25f), isSpecial: false, displayNewTotal: false);
					if (twitchSpawnedEntityEntry.Action.HistoryEntry != null)
					{
						twitchSpawnedEntityEntry.Action.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
					}
					if (twitchSpawnedEntityEntry.RespawnEntry != null)
					{
						TwitchRespawnEntry respawnEntry = twitchSpawnedEntityEntry.RespawnEntry;
						if (respawnEntry.RemoveSpawnedEntry(entityID, checkForRemove: false) && respawnEntry.RespawnsLeft == 0)
						{
							RespawnEntries.Remove(respawnEntry);
						}
					}
				}
				else if (twitchSpawnedEntityEntry.Event != null && twitchSpawnedEntityEntry.Event.HistoryEntry != null)
				{
					twitchSpawnedEntityEntry.Event.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
				}
				actionSpawnLiveList.Remove(twitchSpawnedEntityEntry);
				recentlyDeadList.Add(new TwitchRecentlyRemovedEntityEntry(twitchSpawnedEntityEntry));
				liveList.RemoveAt(num);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventAccessApproved()
	{
		if (InitState == InitStates.None || InitState == InitStates.WaitingForPermission)
		{
			StartTwitchIntegration();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventApproved(string gameEventID, int targetEntityID, string viewerName, string tag)
	{
		if (tag == "action")
		{
			for (int i = 0; i < QueuedActionEntries.Count; i++)
			{
				if (QueuedActionEntries[i].Action.EventName == gameEventID && QueuedActionEntries[i].Target.entityId == targetEntityID && QueuedActionEntries[i].UserName == viewerName)
				{
					ConfirmAction(QueuedActionEntries[i]);
					LiveActionEntries.Add(QueuedActionEntries[i]);
					QueuedActionEntries.RemoveAt(i);
					break;
				}
			}
		}
		else
		{
			if (!(tag == "event"))
			{
				return;
			}
			for (int j = 0; j < EventQueue.Count; j++)
			{
				if (EventQueue[j].UserName == viewerName && EventQueue[j].Event.EventName == gameEventID)
				{
					LiveEvents.Add(EventQueue[j]);
					EventQueue.RemoveAt(j);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventDenied(string gameEventID, int targetEntityID, string viewerName, string tag)
	{
		if (!(tag == "action"))
		{
			return;
		}
		for (int i = 0; i < QueuedActionEntries.Count; i++)
		{
			if (QueuedActionEntries[i].Action.EventName == gameEventID && QueuedActionEntries[i].Target.entityId == targetEntityID && QueuedActionEntries[i].UserName == viewerName)
			{
				ViewerData.ReimburseAction(QueuedActionEntries[i]);
				QueuedActionEntries.RemoveAt(i);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_TwitchRefundNeeded(string gameEventID, int targetEntityID, string viewerName, string tag)
	{
		if (tag == "action")
		{
			for (int i = 0; i < LiveActionEntries.Count; i++)
			{
				TwitchActionEntry twitchActionEntry = LiveActionEntries[i];
				if (twitchActionEntry.Action.EventName == gameEventID && twitchActionEntry.Target.entityId == targetEntityID && twitchActionEntry.UserName == viewerName)
				{
					ViewerData.ReimburseAction(LiveActionEntries[i]);
					ShowReimburseMessage(twitchActionEntry);
					Debug.LogWarning($"TwitchAction {twitchActionEntry.Action.Name} refunded for {viewerName}.");
					if (twitchActionEntry.HistoryEntry != null)
					{
						twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Reimbursed;
					}
					if (twitchActionEntry.Action.tempCooldown > 30f)
					{
						twitchActionEntry.Action.SetCooldown(CurrentUnityTime, 30f);
					}
					LiveActionEntries.RemoveAt(i);
					break;
				}
			}
		}
		else
		{
			if (!(tag == "event"))
			{
				return;
			}
			for (int j = 0; j < LiveEvents.Count; j++)
			{
				TwitchEventActionEntry twitchEventActionEntry = LiveEvents[j];
				if (twitchEventActionEntry.Event.EventName == gameEventID && twitchEventActionEntry.UserName == viewerName)
				{
					Debug.LogWarning($"Twitch Debug: Live Event: Refunded {twitchEventActionEntry.Event.EventTitle} for {viewerName}.");
					if (twitchEventActionEntry.HistoryEntry != null)
					{
						twitchEventActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Reimbursed;
					}
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventCompleted(string gameEventID, int targetEntityID, string viewerName, string tag)
	{
		if (tag == "action")
		{
			for (int i = 0; i < LiveActionEntries.Count; i++)
			{
				TwitchActionEntry twitchActionEntry = LiveActionEntries[i];
				if (!twitchActionEntry.ReadyForRemove && twitchActionEntry.Action.EventName == gameEventID && twitchActionEntry.Target.entityId == targetEntityID && twitchActionEntry.UserName == viewerName)
				{
					twitchActionEntry.ReadyForRemove = true;
					if (twitchActionEntry.HistoryEntry != null)
					{
						twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
					}
					break;
				}
			}
		}
		else
		{
			if (!(tag == "event"))
			{
				return;
			}
			for (int j = 0; j < LiveEvents.Count; j++)
			{
				TwitchEventActionEntry twitchEventActionEntry = LiveEvents[j];
				if (!twitchEventActionEntry.ReadyForRemove && twitchEventActionEntry.UserName == viewerName && twitchEventActionEntry.Event.EventName == gameEventID)
				{
					if (twitchEventActionEntry.HistoryEntry != null)
					{
						twitchEventActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
					}
					twitchEventActionEntry.ReadyForRemove = true;
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEntitySpawned(string gameEventID, int entityID, string tag)
	{
		switch (tag)
		{
		case "action":
		{
			for (int j = 0; j < LiveActionEntries.Count; j++)
			{
				if (LiveActionEntries[j].ReadyForRemove || !(LiveActionEntries[j].Action.EventName == gameEventID))
				{
					continue;
				}
				Entity entity3 = null;
				if (LiveActionEntries[j].Action.AddsToCooldown)
				{
					TwitchSpawnedEntityEntry twitchSpawnedEntityEntry3 = new TwitchSpawnedEntityEntry
					{
						Action = LiveActionEntries[j],
						SpawnedEntityID = entityID
					};
					liveList.Add(twitchSpawnedEntityEntry3);
					if (entity3 == null)
					{
						entity3 = world.GetEntity(entityID);
					}
					twitchSpawnedEntityEntry3.SpawnedEntity = entity3;
					if (twitchSpawnedEntityEntry3.Action.Action.RespawnCountType != TwitchAction.RespawnCountTypes.None)
					{
						TwitchRespawnEntry respawnEntry = GetRespawnEntry(twitchSpawnedEntityEntry3.Action.UserName, twitchSpawnedEntityEntry3.Action.Target, twitchSpawnedEntityEntry3.Action.Action);
						respawnEntry.SpawnedEntities.Add(entityID);
						twitchSpawnedEntityEntry3.RespawnEntry = respawnEntry;
					}
					actionSpawnLiveList.Add(twitchSpawnedEntityEntry3);
				}
				break;
			}
			break;
		}
		case "event":
		{
			for (int i = 0; i < LiveEvents.Count; i++)
			{
				if (!LiveEvents[i].ReadyForRemove && LiveEvents[i].Event.EventName == gameEventID)
				{
					Entity entity2 = null;
					if (entity2 == null)
					{
						entity2 = world.GetEntity(entityID);
					}
					if (entity2 is EntityAlive)
					{
						TwitchSpawnedEntityEntry twitchSpawnedEntityEntry2 = new TwitchSpawnedEntityEntry
						{
							Event = LiveEvents[i],
							SpawnedEntityID = entityID
						};
						liveList.Add(twitchSpawnedEntityEntry2);
						twitchSpawnedEntityEntry2.SpawnedEntity = entity2;
						actionSpawnLiveList.Add(twitchSpawnedEntityEntry2);
					}
					break;
				}
			}
			break;
		}
		case "vote":
			if (VotingManager.CurrentEvent != null && VotingManager.CurrentEvent.VoteClass.GameEvent == gameEventID)
			{
				Entity entity = null;
				if (entity == null)
				{
					entity = world.GetEntity(entityID);
				}
				if (entity is EntityAlive)
				{
					TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = new TwitchSpawnedEntityEntry
					{
						Vote = VotingManager.CurrentEvent,
						SpawnedEntityID = entityID
					};
					liveList.Add(twitchSpawnedEntityEntry);
					twitchSpawnedEntityEntry.SpawnedEntity = entity;
					VotingManager.CurrentEvent.ActiveSpawns.Add(entityID);
					actionSpawnLiveList.Add(twitchSpawnedEntityEntry);
				}
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameBlocksAdded(string gameEventID, int blockGroupID, List<Vector3i> blockList, string tag)
	{
		switch (tag)
		{
		case "action":
		{
			for (int j = 0; j < LiveActionEntries.Count; j++)
			{
				if (LiveActionEntries[j].ReadyForRemove || !(LiveActionEntries[j].Action.EventName == gameEventID))
				{
					continue;
				}
				if (LiveActionEntries[j].Action.AddsToCooldown)
				{
					TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = new TwitchSpawnedBlocksEntry
					{
						BlockGroupID = blockGroupID,
						Action = LiveActionEntries[j],
						blocks = blockList.ToList()
					};
					liveBlockList.Add(twitchSpawnedBlocksEntry);
					if (twitchSpawnedBlocksEntry.Action.Action.RespawnCountType != TwitchAction.RespawnCountTypes.None)
					{
						TwitchRespawnEntry respawnEntry = GetRespawnEntry(twitchSpawnedBlocksEntry.Action.UserName, twitchSpawnedBlocksEntry.Action.Target, twitchSpawnedBlocksEntry.Action.Action);
						respawnEntry.SpawnedBlocks.AddRange(blockList);
						twitchSpawnedBlocksEntry.RespawnEntry = respawnEntry;
					}
				}
				break;
			}
			break;
		}
		case "event":
		{
			for (int i = 0; i < LiveEvents.Count; i++)
			{
				if (!LiveEvents[i].ReadyForRemove && LiveEvents[i].Event.EventName == gameEventID)
				{
					TwitchSpawnedBlocksEntry item2 = new TwitchSpawnedBlocksEntry
					{
						BlockGroupID = blockGroupID,
						Event = LiveEvents[i],
						blocks = blockList.ToList()
					};
					liveBlockList.Add(item2);
					break;
				}
			}
			break;
		}
		case "vote":
			if (VotingManager.CurrentEvent != null && VotingManager.CurrentEvent.VoteClass.GameEvent == gameEventID)
			{
				TwitchSpawnedBlocksEntry item = new TwitchSpawnedBlocksEntry
				{
					BlockGroupID = blockGroupID,
					Vote = VotingManager.CurrentEvent,
					blocks = blockList.ToList()
				};
				liveBlockList.Add(item);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameBlocksRemoved(int blockGroupID, bool isDespawn)
	{
		for (int i = 0; i < liveBlockList.Count; i++)
		{
			if (liveBlockList[i].BlockGroupID != blockGroupID)
			{
				continue;
			}
			if (liveBlockList[i].RespawnEntry != null)
			{
				TwitchRespawnEntry respawnEntry = liveBlockList[i].RespawnEntry;
				if (respawnEntry.RemoveAllSpawnedBlock(!isDespawn) && respawnEntry.ReadyForRemove)
				{
					RespawnEntries.Remove(respawnEntry);
				}
			}
			liveBlockList.RemoveAt(i);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameBlockRemoved(Vector3i blockRemoved)
	{
		for (int i = 0; i < liveBlockList.Count; i++)
		{
			if (liveBlockList[i].RemoveBlock(blockRemoved))
			{
				liveBlockList[i].TimeRemaining = 5f;
				break;
			}
		}
	}

	public void HandleConsoleAction(List<string> consoleParams)
	{
		for (int i = 0; i < TwitchCommandList.Count; i++)
		{
			for (int j = 0; j < TwitchCommandList[i].CommandText.Length; j++)
			{
				if (consoleParams[0].StartsWith(TwitchCommandList[i].CommandText[j]))
				{
					TwitchCommandList[i].ExecuteConsole(consoleParams);
					return;
				}
			}
		}
	}

	public bool IsActionAvailable(string actionName)
	{
		if (!VotingManager.VotingIsActive)
		{
			if (actionName[0] != '#')
			{
				actionName = "#" + actionName.ToLower();
			}
			if (twitchActive && VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked && AllowActions && AvailableCommands.ContainsKey(actionName))
			{
				TwitchAction twitchAction = AvailableCommands[actionName];
				if (!twitchAction.IgnoreCooldown)
				{
					if (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.Time || CooldownType == CooldownTypes.QuestDisabled)
					{
						return false;
					}
					if ((CooldownType == CooldownTypes.MaxReachedWaiting || CooldownType == CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked)
					{
						return false;
					}
				}
				if (UseProgression && !OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > HighestGameStage)
				{
					return false;
				}
				if (!twitchAction.CanUse)
				{
					return false;
				}
				if ((twitchAction.IgnoreCooldown || !twitchAction.CooldownBlocked || !OnCooldown) && twitchAction.IsReady(this))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void HandleExtensionMessage(int userId, string message, bool isRerun, int creditUsed, int bitsUsed)
	{
		if (!ViewerData.IdToUsername.TryGetValue(userId, out var value))
		{
			return;
		}
		bool flag = false;
		if (creditUsed < 0)
		{
			creditUsed = 0;
		}
		string[] array = message.Split(' ');
		string text = array[0];
		TwitchAction twitchAction = null;
		if (AvailableCommands.ContainsKey(text))
		{
			twitchAction = AvailableCommands[text];
		}
		else
		{
			foreach (TwitchAction value2 in TwitchActionManager.TwitchActions.Values)
			{
				if (value2.IsInPreset(CurrentActionPreset) && value2.Command == text)
				{
					twitchAction = value2;
					break;
				}
			}
		}
		if (twitchAction == null)
		{
			return;
		}
		bool flag2 = twitchAction.PointType == TwitchAction.PointTypes.Bits;
		if (!VotingManager.VotingIsActive && twitchActive && (flag2 || (VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked && AllowActions)))
		{
			if (!isRerun && !flag2 && ((!twitchAction.IgnoreCooldown && (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.Time || CooldownType == CooldownTypes.QuestDisabled || ((CooldownType == CooldownTypes.MaxReachedWaiting || CooldownType == CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked))) || (UseProgression && !OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > HighestGameStage) || !twitchAction.CanUse))
			{
				return;
			}
			if (flag2 || isRerun || twitchAction.IgnoreCooldown || !twitchAction.CooldownBlocked || !OnCooldown || ((CooldownType == CooldownTypes.MaxReachedWaiting || CooldownType == CooldownTypes.SafeCooldown) && !twitchAction.WaitingBlocked))
			{
				TwitchActionEntry actionEntry = null;
				if ((isRerun || twitchAction.IsReady(this)) && ViewerData.HandleInitialActionEntrySetup(value, twitchAction, isRerun, flag2, out actionEntry))
				{
					actionEntry.UserName = value;
					actionEntry.ChannelNotify = twitchAction.PointType == TwitchAction.PointTypes.Bits;
					actionEntry.IsBitAction = flag2;
					actionEntry.IsReRun = isRerun;
					actionEntry.Action = twitchAction;
					EntityPlayer entityPlayer = LocalPlayer;
					if (array.Length > 1 && LocalPlayer.Party != null)
					{
						string text2 = message.Substring(array[0].Length + 1).ToLower();
						for (int i = 0; i < LocalPlayer.Party.MemberList.Count; i++)
						{
							if (LocalPlayer.Party.MemberList[i].EntityName.ToLower() == text2)
							{
								entityPlayer = LocalPlayer.Party.MemberList[i];
								break;
							}
						}
						if (entityPlayer != LocalPlayer && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
						{
							if (flag2)
							{
								ViewerEntry viewerEntry = ViewerData.GetViewerEntry(actionEntry.UserName);
								actionEntry.Target = entityPlayer;
								AddActionHistory(actionEntry, viewerEntry, TwitchActionHistoryEntry.EntryStates.Reimbursed);
								ShowReimburseMessage(actionEntry, viewerEntry);
							}
							ViewerData.ReimburseAction(actionEntry);
							return;
						}
						if (PartyInfo.ContainsKey(entityPlayer) && PartyInfo[entityPlayer].Cooldown > 0f)
						{
							if (flag2)
							{
								ViewerEntry viewerEntry2 = ViewerData.GetViewerEntry(actionEntry.UserName);
								actionEntry.Target = entityPlayer;
								AddActionHistory(actionEntry, viewerEntry2, TwitchActionHistoryEntry.EntryStates.Reimbursed);
								ShowReimburseMessage(actionEntry, viewerEntry2);
							}
							ViewerData.ReimburseAction(actionEntry);
							return;
						}
					}
					actionEntry.Target = entityPlayer;
					if (actionEntry.CreditsUsed != creditUsed)
					{
						Debug.LogWarning($"Twitch Bit Credit usage is invalid: {actionEntry.UserName} used {creditUsed} when their balance was {actionEntry.CreditsUsed}. They were credited the amount they spent in bits.");
						ViewerData.AddCredit(value, creditUsed + bitsUsed, displayNewTotal: false);
						ViewerEntry viewerEntry3 = ViewerData.GetViewerEntry(actionEntry.UserName);
						AddActionHistory(actionEntry, viewerEntry3, TwitchActionHistoryEntry.EntryStates.Reimbursed);
						ShowReimburseMessage(actionEntry, viewerEntry3);
						return;
					}
					if (twitchAction.ModifiedCooldown > 0f)
					{
						twitchAction.tempCooldownSet = CurrentUnityTime;
						twitchAction.tempCooldown = 1f;
					}
					if (actionEntry.CreditsUsed > 0)
					{
						ViewerEntry viewerEntry4 = ViewerData.GetViewerEntry(actionEntry.UserName);
						PushBalanceToExtensionQueue(viewerEntry4.UserID.ToString(), viewerEntry4.BitCredits);
					}
					QueuedActionEntries.Add(actionEntry);
					flag = true;
				}
			}
		}
		if (!flag && flag2)
		{
			ViewerEntry viewerEntry5 = ViewerData.AddCredit(value, twitchAction.CurrentCost - creditUsed, displayNewTotal: false);
			if (viewerEntry5 != null)
			{
				ShowReimburseMessage(value, creditUsed, twitchAction, viewerEntry5);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMessage(TwitchIRCClient.TwitchChatMessage message)
	{
		switch (message.MessageType)
		{
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Invalid:
			break;
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Message:
		{
			if (InitState != InitStates.Ready)
			{
				break;
			}
			string text = message.Message.ToLower();
			ViewerEntry entry = ViewerData.UpdateViewerEntry(message.UserID, message.UserName, message.UserNameColor, message.isSub);
			if (message.isBroadcaster)
			{
				if (text.StartsWith("#cooldowninfo"))
				{
					ircClient.SendChannelMessage($"[7DTD]: Cooldown is at {CurrentCooldownFill}/{CurrentCooldownPreset.CooldownFillMax}.", useQueue: true);
				}
				else if (text.StartsWith("#reset "))
				{
					actionSpawnLiveList.Clear();
					LiveActionEntries.Clear();
					ircClient.SendChannelMessage("[7DTD]: Action Live list Cleared!", useQueue: true);
				}
			}
			for (int i = 0; i < TwitchCommandList.Count; i++)
			{
				for (int j = 0; j < TwitchCommandList[i].CommandTextList.Count; j++)
				{
					if (text.StartsWith(TwitchCommandList[i].CommandTextList[j]) && TwitchCommandList[i].CheckAllowed(message))
					{
						TwitchCommandList[i].Execute(entry, message);
						break;
					}
				}
			}
			VotingManager.HandleMessage(message);
			if (VotingManager.VotingIsActive || IntegrationSetting == IntegrationSettings.ExtensionOnly)
			{
				break;
			}
			string[] array = text.Split(' ');
			string key = array[0];
			if (AlternateCommands.ContainsKey(key))
			{
				key = AlternateCommands[key];
			}
			if (!twitchActive || VoteLockedLevel == TwitchVoteLockTypes.ActionsLocked || !AllowActions || !AvailableCommands.ContainsKey(key))
			{
				break;
			}
			TwitchAction twitchAction = AvailableCommands[key];
			if (twitchAction.PointType == TwitchAction.PointTypes.Bits || (!twitchAction.IgnoreCooldown && (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.Time || CooldownType == CooldownTypes.Startup || CooldownType == CooldownTypes.QuestDisabled || ((CooldownType == CooldownTypes.MaxReachedWaiting || CooldownType == CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked))) || (UseProgression && !OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > HighestGameStage) || !twitchAction.CanUse || !twitchAction.CheckUsable(message) || (!twitchAction.IgnoreCooldown && twitchAction.CooldownBlocked && OnCooldown && ((CooldownType != CooldownTypes.MaxReachedWaiting && CooldownType != CooldownTypes.SafeCooldown) || twitchAction.WaitingBlocked)))
			{
				break;
			}
			TwitchActionEntry actionEntry = null;
			if (!twitchAction.IsReady(this) || !ViewerData.HandleInitialActionEntrySetup(message.UserName, twitchAction, isRerun: false, isBitAction: false, out actionEntry))
			{
				break;
			}
			actionEntry.UserName = message.UserName;
			EntityPlayer entityPlayer = LocalPlayer;
			if (array.Length > 1 && LocalPlayer.Party != null)
			{
				int _result = -1;
				if (StringParsers.TryParseSInt32(array[1], out _result))
				{
					entityPlayer = LocalPlayer.Party.GetMemberAtIndex(_result, LocalPlayer);
					if (entityPlayer == null)
					{
						ViewerData.ReimburseAction(actionEntry);
						break;
					}
					if (entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
					{
						ViewerData.ReimburseAction(actionEntry);
						break;
					}
				}
				else
				{
					string text2 = text.Substring(array[0].Length + 1);
					bool flag = false;
					for (int k = 0; k < LocalPlayer.Party.MemberList.Count; k++)
					{
						if (LocalPlayer.Party.MemberList[k].EntityName.ToLower() == text2)
						{
							entityPlayer = LocalPlayer.Party.MemberList[k];
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						ViewerData.ReimburseAction(actionEntry);
						break;
					}
					if (entityPlayer != LocalPlayer && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
					{
						ViewerData.ReimburseAction(actionEntry);
						break;
					}
					if (PartyInfo.ContainsKey(entityPlayer) && PartyInfo[entityPlayer].Cooldown > 0f)
					{
						ViewerData.ReimburseAction(actionEntry);
						break;
					}
				}
			}
			if (twitchAction.StreamerOnly && entityPlayer != LocalPlayer)
			{
				ViewerData.ReimburseAction(actionEntry);
				break;
			}
			actionEntry.Target = entityPlayer;
			actionEntry.Action = twitchAction;
			QueuedActionEntries.Add(actionEntry);
			break;
		}
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Authenticated:
			if (InitState != InitStates.Ready)
			{
				List<string> list = new List<string>();
				list.Add("CAP REQ :twitch.tv/membership");
				list.Add("CAP REQ :twitch.tv/tags");
				list.Add("CAP REQ :twitch.tv/commands");
				ircClient.SendIrcMessages(list, useQueue: false);
				InitState = InitStates.CheckingForExtension;
				TwitchAuthentication.bFirstLogin = false;
			}
			break;
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Raid:
		{
			int viewerAmount = StringParsers.ParseSInt32(message.Message);
			HandleRaid(message.UserName.ToLower(), message.UserID, viewerAmount);
			break;
		}
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Charity:
		{
			int charityAmount = StringParsers.ParseSInt32(message.Message);
			HandleCharity(message.UserName.ToLower(), message.UserID, charityAmount);
			break;
		}
		case TwitchIRCClient.TwitchChatMessage.MessageTypes.Output:
			break;
		}
	}

	public void DisplayDebug(string message)
	{
		Debug.LogWarning("Called: " + message);
		Debug.LogWarning($"[7DTD]: Spawns Alive: {actionSpawnLiveList.Count}  Blocks Alive: {liveBlockList.Count}  ActionLiveList: {LiveActionEntries.Count}.");
		for (int i = 0; i < actionSpawnLiveList.Count; i++)
		{
			if (actionSpawnLiveList[i].SpawnedEntity != null)
			{
				Debug.LogWarning($"Spawn Alive: {actionSpawnLiveList[i].SpawnedEntity.name}");
			}
		}
		for (int j = 0; j < LiveActionEntries.Count; j++)
		{
			Debug.LogWarning($"Action: {LiveActionEntries[j].Action.Name} Target: {LiveActionEntries[j].Target.EntityName} Viewer: {LiveActionEntries[j].UserName}");
		}
		for (int k = 0; k < EventQueue.Count; k++)
		{
			Debug.LogWarning($"Event: {EventQueue[k].Event.EventTitle} User: {EventQueue[k].UserName} Sent: {EventQueue[k].IsSent}");
		}
		ircClient.SendChannelMessage("[7DTD]: Debug Complete!", useQueue: true);
	}

	public void AddToPot(int amount)
	{
		RewardPot += amount;
		if (RewardPot < 0)
		{
			RewardPot = 0;
		}
		if (RewardPot > LeaderboardStats.LargestPimpPot)
		{
			LeaderboardStats.LargestPimpPot = RewardPot;
		}
	}

	public void AddToBitPot(int amount)
	{
		BitPot += amount;
		if (BitPot < 0)
		{
			BitPot = 0;
		}
		if (BitPot > LeaderboardStats.LargestBitPot)
		{
			LeaderboardStats.LargestBitPot = BitPot;
		}
	}

	public void SetPot(int newPot)
	{
		if (newPot < 0)
		{
			newPot = 0;
		}
		RewardPot = newPot;
		ircClient.SendChannelMessage(string.Format(chatOutput_PimpPotBalance, RewardPot), useQueue: true);
		if (RewardPot > LeaderboardStats.LargestPimpPot)
		{
			LeaderboardStats.LargestPimpPot = RewardPot;
		}
	}

	public void SetBitPot(int newPot)
	{
		if (newPot < 0)
		{
			newPot = 0;
		}
		BitPot = newPot;
		ircClient.SendChannelMessage(string.Format(chatOutput_BitPotBalance, BitPot), useQueue: true);
		if (BitPot > LeaderboardStats.LargestBitPot)
		{
			LeaderboardStats.LargestBitPot = BitPot;
		}
	}

	public void SetCooldown(float newCooldownTime, CooldownTypes newCooldownType, bool displayToChannel = false, bool playCooldownSound = true)
	{
		if (!(LocalPlayer == null) && (CooldownType != newCooldownType || CooldownTime != newCooldownTime))
		{
			if (newCooldownType != CooldownTypes.MaxReachedWaiting && newCooldownType != CooldownTypes.SafeCooldown && newCooldownType != CooldownTypes.SafeCooldownExit && newCooldownType != CooldownTypes.None)
			{
				LocalPlayer.HandleTwitchActionsTempEnabled((newCooldownTime > 15f) ? EntityPlayer.TwitchActionsStates.TempDisabled : EntityPlayer.TwitchActionsStates.TempDisabledEnding);
			}
			else if (newCooldownType == CooldownTypes.None)
			{
				LocalPlayer.HandleTwitchActionsTempEnabled(EntityPlayer.TwitchActionsStates.Enabled);
			}
			CooldownType = newCooldownType;
			CooldownTime = newCooldownTime;
			if (displayToChannel)
			{
				ircClient.SendChannelMessage(string.Format(chatOutput_CooldownTime, newCooldownTime), useQueue: true);
			}
			HandleCooldownActionLocking();
			if (playCooldownSound && LocalPlayer != null && newCooldownType != CooldownTypes.None)
			{
				Manager.BroadcastPlayByLocalPlayer(LocalPlayer.position, "twitch_cooldown_started");
			}
		}
	}

	public bool ForceEndCooldown(bool playEndSound = true)
	{
		if (IsReady && (CooldownType == CooldownTypes.MaxReached || CooldownType == CooldownTypes.Time))
		{
			SetCooldown(0f, CooldownTypes.None);
			CurrentCooldownFill = 0f;
			if (playEndSound)
			{
				Manager.BroadcastPlayByLocalPlayer(LocalPlayer.position, "twitch_end_cooldown");
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfirmAction(TwitchActionEntry entry)
	{
		TwitchAction action = entry.Action;
		if (!entry.IsRespawn)
		{
			action.SetQueued();
		}
		if (!entry.IsRespawn)
		{
			if (!entry.IsBitAction && PimpPotType != PimpPotSettings.Disabled)
			{
				int num = (int)EffectManager.GetValue(PassiveEffects.TwitchAddPimpPot, null, (float)action.ModifiedCost * ActionPotPercentage, LocalPlayer);
				if (num > 0)
				{
					RewardPot += num;
					if (RewardPot > LeaderboardStats.LargestPimpPot)
					{
						LeaderboardStats.LargestPimpPot = RewardPot;
					}
				}
			}
			if (entry.IsBitAction && entry.BitsUsed > 0)
			{
				int num2 = (int)((float)(entry.BitsUsed - entry.CreditsUsed) * BitPotPercentage);
				if (num2 > 0)
				{
					BitPot += num2;
					if (BitPot > LeaderboardStats.LargestBitPot)
					{
						LeaderboardStats.LargestBitPot = BitPot;
					}
				}
			}
		}
		AddCooldownForAction(action);
		if (entry.IsRespawn)
		{
			return;
		}
		ViewerEntry viewerEntry = ViewerData.GetViewerEntry(entry.UserName);
		if (EffectManager.GetValue(PassiveEffects.DisableGameEventNotify, null, 0f, LocalPlayer) == 0f)
		{
			if (action.DelayNotify)
			{
				GameManager.Instance.StartCoroutine(onDelayedActionNotify(entry, viewerEntry));
			}
			else
			{
				DisplayActionNotification(entry, viewerEntry);
			}
		}
		AddActionHistory(entry, viewerEntry);
	}

	public void ShowReimburseMessage(string userName, int bitsUsed, TwitchAction action, ViewerEntry viewerEntry = null)
	{
		if (bitsUsed > 0)
		{
			if (viewerEntry == null)
			{
				viewerEntry = ViewerData.GetViewerEntry(userName);
			}
			if (viewerEntry != null)
			{
				string msg = string.Format(ingameOutput_RefundedAction, viewerEntry.UserColor, userName, bitsUsed, action.Command);
				Debug.LogWarning($"{userName} has been refunded {bitsUsed} bits for {action.Command}");
				AddToInGameChatQueue(msg, "twitch_refund");
			}
		}
	}

	public void ShowReimburseMessage(TwitchActionEntry entry, ViewerEntry viewerEntry = null)
	{
		ShowReimburseMessage(entry.UserName, entry.BitsUsed, entry.Action, viewerEntry);
	}

	public TwitchActionHistoryEntry AddActionHistory(TwitchActionEntry entry, ViewerEntry viewerEntry, TwitchActionHistoryEntry.EntryStates startState = TwitchActionHistoryEntry.EntryStates.Waiting)
	{
		if (!entry.IsReRun)
		{
			if (entry.HistoryEntry == null)
			{
				TwitchActionHistoryEntry twitchActionHistoryEntry = entry.SetupHistoryEntry(viewerEntry);
				twitchActionHistoryEntry.EntryState = startState;
				entry.HistoryEntry = twitchActionHistoryEntry;
				ActionHistory.Insert(0, twitchActionHistoryEntry);
				if (ActionHistory.Count > 500)
				{
					ActionHistory.RemoveAt(ActionHistory.Count - 1);
				}
				if (this.ActionHistoryAdded != null)
				{
					this.ActionHistoryAdded();
				}
				return twitchActionHistoryEntry;
			}
			entry.HistoryEntry.EntryState = startState;
			return entry.HistoryEntry;
		}
		return null;
	}

	public void AddVoteHistory(TwitchVote vote)
	{
		TwitchActionHistoryEntry twitchActionHistoryEntry = new TwitchActionHistoryEntry("Vote", "FFFFFF", null, vote, null);
		VoteHistory.Insert(0, twitchActionHistoryEntry);
		twitchActionHistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
		if (VoteHistory.Count > 500)
		{
			VoteHistory.RemoveAt(VoteHistory.Count - 1);
		}
		if (this.VoteHistoryAdded != null)
		{
			this.VoteHistoryAdded();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddCooldownForAction(TwitchAction action)
	{
		if (action.AddsToCooldown)
		{
			int num = (int)EffectManager.GetValue(PassiveEffects.TwitchAddCooldown, null, action.CooldownAddAmount, LocalPlayer);
			if (num > 0)
			{
				AddCooldownAmount(num);
			}
		}
	}

	public void AddCooldownAmount(int amount)
	{
		if (CurrentCooldownPreset == null)
		{
			GetCooldownMax();
		}
		if (CooldownType == CooldownTypes.QuestCooldown || CooldownType == CooldownTypes.QuestDisabled || CooldownType == CooldownTypes.BloodMoonCooldown || CooldownType == CooldownTypes.BloodMoonDisabled || IsVoting)
		{
			return;
		}
		if (CurrentCooldownPreset != null && CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
		{
			if (CurrentCooldownFill < CurrentCooldownPreset.CooldownFillMax)
			{
				CurrentCooldownFill += amount;
				if (CurrentCooldownFill >= CurrentCooldownPreset.CooldownFillMax)
				{
					SetCooldown(CurrentCooldownPreset.NextCooldownTime, CooldownTypes.MaxReachedWaiting);
					if (ircClient != null)
					{
						ircClient.SendChannelMessage(chatOutput_CooldownStarted, useQueue: true);
					}
				}
			}
			else if (CooldownType != CooldownTypes.MaxReachedWaiting && CooldownType != CooldownTypes.MaxReached)
			{
				SetCooldown(CurrentCooldownPreset.NextCooldownTime, CooldownTypes.MaxReachedWaiting);
				if (ircClient != null)
				{
					ircClient.SendChannelMessage(chatOutput_CooldownStarted, useQueue: true);
				}
			}
		}
		UIDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator onDelayedActionNotify(TwitchActionEntry entry, ViewerEntry viewerEntry)
	{
		yield return new WaitForSeconds(5f);
		if (ircClient != null)
		{
			DisplayActionNotification(entry, viewerEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayActionNotification(TwitchActionEntry entry, ViewerEntry viewerEntry)
	{
		if (entry.HistoryEntry != null && entry.HistoryEntry.EntryState == TwitchActionHistoryEntry.EntryStates.Reimbursed)
		{
			return;
		}
		TwitchAction action = entry.Action;
		if (entry.ChannelNotify && action.TwitchNotify)
		{
			string text = "";
			if (action.PointType == TwitchAction.PointTypes.Bits)
			{
				text = string.Format(chatOutput_ActivatedBitAction, entry.UserName, action.Command, viewerEntry.CombinedPoints, entry.Target.EntityName, entry.Action.CurrentCost);
				if (action.PlayBitSound)
				{
					Manager.PlayInsidePlayerHead(action.IsPositive ? "twitch_donation" : "twitch_donation_bad", LocalPlayer.entityId, 0f, false, false);
				}
			}
			else
			{
				text = string.Format(chatOutput_ActivatedAction, entry.UserName, action.Command, viewerEntry.CombinedPoints, entry.Target.EntityName);
			}
			ircClient.SendChannelMessage(text, useQueue: true);
		}
		string text2 = string.Format(ingameOutput_ActivatedAction, viewerEntry.UserColor, entry.UserName, action.Command, entry.Target.EntityName);
		SendServerChatMessage(text2);
		AddToInGameChatQueue(text2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendServerChatMessage(string serverMsg)
	{
		if (!LocalPlayer.IsInParty())
		{
			return;
		}
		List<int> memberIdList = LocalPlayer.Party.GetMemberIdList(LocalPlayer);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (memberIdList == null)
			{
				return;
			}
			{
				foreach (int item in memberIdList)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(item)?.SendPackage(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(serverMsg));
				}
				return;
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(serverMsg, memberIdList));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshCommands(bool displayMessage)
	{
		if (LocalPlayer.unModifiedGameStage != HighestGameStage)
		{
			int highestGameStage = HighestGameStage;
			HighestGameStage = LocalPlayer.unModifiedGameStage;
			GetCooldownMax();
			if (UseProgression && !OverrideProgession)
			{
				ActionMessages.Clear();
				SetupAvailableCommandsWithOutput(highestGameStage, displayMessage);
				ResetDailyCommands(lastGameDay, highestGameStage);
				HandleCooldownActionLocking();
				commandsAvailable = AvailableCommands.Count;
			}
		}
	}

	public void ToggleTwitchActive()
	{
		twitchActive = !twitchActive;
		ActionMessages.Clear();
		HandleCooldownActionLocking();
	}

	public void SetTwitchActive(bool newActive)
	{
		if (twitchActive != newActive)
		{
			twitchActive = newActive;
			ActionMessages.Clear();
			HandleCooldownActionLocking();
		}
	}

	public void ResetPrices()
	{
		bool flag = false;
		foreach (TwitchAction value in TwitchActionManager.TwitchActions.Values)
		{
			if (value.UpdateCost(BitPriceMultiplier))
			{
				flag = true;
			}
		}
		if (flag)
		{
			resetCommandsNeeded = true;
		}
	}

	public void ResetPricesToDefault()
	{
		bool flag = false;
		foreach (TwitchAction value in TwitchActionManager.TwitchActions.Values)
		{
			value.ResetToDefaultCost();
			if (value.UpdateCost(BitPriceMultiplier))
			{
				flag = true;
			}
		}
		if (flag)
		{
			resetCommandsNeeded = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetCommands()
	{
		ActionMessages.Clear();
		SetupAvailableCommands();
		if (UseProgression && !OverrideProgession)
		{
			ResetDailyCommands(lastGameDay);
		}
		HandleCooldownActionLocking();
		resetCommandsNeeded = false;
	}

	public void SetUseProgression(bool useProgression)
	{
		if (UseProgression != useProgression)
		{
			UseProgression = useProgression;
			if (InitState == InitStates.Ready)
			{
				resetCommandsNeeded = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetCommandCount()
	{
		int num = 0;
		if (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.Time || CooldownType == CooldownTypes.QuestDisabled)
		{
			return 0;
		}
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchAction twitchAction = TwitchActionManager.TwitchActions[key];
			if (!twitchAction.IsInPreset(CurrentActionPreset))
			{
				continue;
			}
			if (CooldownType == CooldownTypes.MaxReachedWaiting)
			{
				if (twitchAction.WaitingBlocked)
				{
					continue;
				}
			}
			else if (twitchAction.CooldownBlocked && CooldownTime > 0f)
			{
				continue;
			}
			int startGameStage = twitchAction.StartGameStage;
			if (startGameStage == -1 || startGameStage <= HighestGameStage)
			{
				num++;
			}
		}
		return num;
	}

	public TwitchRespawnEntry GetRespawnEntry(string username, EntityPlayer target, TwitchAction action)
	{
		for (int i = 0; i < RespawnEntries.Count; i++)
		{
			if (RespawnEntries[i].CheckRespawn(username, target, action))
			{
				return RespawnEntries[i];
			}
		}
		TwitchRespawnEntry twitchRespawnEntry = new TwitchRespawnEntry(username, GameEventManager.Current.Random.RandomRange(action.MinRespawnCount, action.MaxRespawnCount), target, action);
		RespawnEntries.Add(twitchRespawnEntry);
		return twitchRespawnEntry;
	}

	public void CheckKiller(EntityPlayer player, EntityAlive killer, Vector3i pos)
	{
		if (LocalPlayer == null || twitchPlayerDeathsThisFrame.Contains(player))
		{
			return;
		}
		if (player == LocalPlayer && VotingManager != null && VotingManager.VotingEnabled && VotingManager.CurrentEvent != null)
		{
			HandleVoteKill(null);
			VotingManager.ResetVoteOnDeath();
			twitchPlayerDeathsThisFrame.Add(player);
			return;
		}
		bool flag = false;
		if (player == LocalPlayer)
		{
			flag = true;
		}
		else
		{
			if (LocalPlayer.Party == null || !LocalPlayer.Party.ContainsMember(player))
			{
				return;
			}
			flag = false;
		}
		TwitchActionEntry twitchActionEntry = null;
		TwitchEventActionEntry twitchEventActionEntry = null;
		TwitchVoteEntry twitchVoteEntry = null;
		if (killer == null)
		{
			for (int num = liveBlockList.Count - 1; num >= 0; num--)
			{
				if (liveBlockList[num].CheckPos(pos))
				{
					TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = liveBlockList[num];
					twitchActionEntry = twitchSpawnedBlocksEntry.Action;
					twitchEventActionEntry = twitchSpawnedBlocksEntry.Event;
					twitchVoteEntry = twitchSpawnedBlocksEntry.Vote;
					if (twitchSpawnedBlocksEntry.RespawnEntry != null && (flag || twitchActionEntry.Target == player))
					{
						RespawnEntries.Remove(twitchSpawnedBlocksEntry.RespawnEntry);
					}
					break;
				}
			}
		}
		else
		{
			for (int num2 = liveList.Count - 1; num2 >= 0; num2--)
			{
				if (liveList[num2].SpawnedEntity == killer)
				{
					TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = liveList[num2];
					twitchActionEntry = twitchSpawnedEntityEntry.Action;
					twitchEventActionEntry = twitchSpawnedEntityEntry.Event;
					twitchVoteEntry = twitchSpawnedEntityEntry.Vote;
					if (twitchSpawnedEntityEntry.RespawnEntry != null && (flag || twitchSpawnedEntityEntry.Action.Target == player))
					{
						RespawnEntries.Remove(twitchSpawnedEntityEntry.RespawnEntry);
					}
					break;
				}
			}
		}
		if (twitchActionEntry == null && twitchEventActionEntry == null && twitchVoteEntry == null)
		{
			for (int num3 = recentlyDeadList.Count - 1; num3 >= 0; num3--)
			{
				if (recentlyDeadList[num3].SpawnedEntity == killer)
				{
					twitchActionEntry = recentlyDeadList[num3].Action;
					twitchEventActionEntry = recentlyDeadList[num3].Event;
					twitchVoteEntry = recentlyDeadList[num3].Vote;
					break;
				}
			}
		}
		if (twitchActionEntry != null || twitchEventActionEntry != null)
		{
			string text = ((twitchActionEntry != null) ? twitchActionEntry.UserName : twitchEventActionEntry.UserName);
			string text2 = ((twitchActionEntry != null) ? twitchActionEntry.Action.Command : twitchEventActionEntry.Event.EventTitle);
			if (twitchEventActionEntry != null && (twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.HypeTrain || twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.CreatorGoal))
			{
				if (flag)
				{
					int num4 = 100;
					bool flag2 = true;
					if (twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.HypeTrain)
					{
						TwitchHypeTrainEventEntry obj = (TwitchHypeTrainEventEntry)twitchEventActionEntry.Event;
						num4 = obj.RewardAmount;
						flag2 = obj.RewardType != TwitchAction.PointTypes.PP;
					}
					else
					{
						TwitchCreatorGoalEventEntry obj2 = (TwitchCreatorGoalEventEntry)twitchEventActionEntry.Event;
						num4 = obj2.RewardAmount;
						flag2 = obj2.RewardType != TwitchAction.PointTypes.PP;
					}
					ViewerData.AddPointsAll((!flag2) ? num4 : 0, flag2 ? num4 : 0, announceToChat: false);
					string arg = (flag2 ? Localization.Get("TwitchPoints_SP") : Localization.Get("TwitchPoints_PP"));
					string text3 = string.Format(ingameOutput_KilledByHypeTrain, num4, arg, LocalPlayer.EntityName);
					GameManager.ShowTooltip(LocalPlayer, text3);
					GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(Authentication.userName, text3), null, EMessageSender.None);
					ircClient.SendChannelMessage(string.Format(chatOutput_KilledByHypeTrain, num4, arg, LocalPlayer.EntityName), useQueue: true);
					DeathText = string.Format(ingameHypeTrainDeathScreen_Message, num4, arg);
				}
				twitchPlayerDeathsThisFrame.Add(player);
				return;
			}
			if (flag)
			{
				ViewerEntry viewerEntry = ViewerData.GetViewerEntry(text);
				if ((twitchActionEntry != null && twitchActionEntry.IsBitAction) || (twitchEventActionEntry != null && twitchEventActionEntry.Event.RewardsBitPot))
				{
					if (BitPot > 0)
					{
						viewerEntry.BitCredits += BitPot;
					}
					string text4 = ((PimpPotType == PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP") : Localization.Get("TwitchPoints_PP"));
					if (PimpPotType != PimpPotSettings.Disabled)
					{
						if (PimpPotType == PimpPotSettings.EnabledSP)
						{
							viewerEntry.SpecialPoints += RewardPot;
						}
						else
						{
							viewerEntry.StandardPoints += RewardPot;
						}
					}
					ircClient.SendChannelMessage(string.Format(chatOutput_KilledByBits, text, viewerEntry.BitCredits, BitPot, player.EntityName, RewardPot, text4), useQueue: true);
					string text5 = string.Format(ingameOutput_KilledByBits, viewerEntry.UserColor, text, BitPot, player.EntityName, RewardPot, text4);
					GameManager.ShowTooltip(LocalPlayer, text5);
					GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(Authentication.userName, text5), null, EMessageSender.None);
					DeathText = string.Format(ingameBitsDeathScreen_Message, viewerEntry.UserColor, text, text2, BitPot, RewardPot, text4);
					QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.BitPot, "");
					BitPot = 0;
					RewardPot = PimpPotDefault;
				}
				else
				{
					string text6 = ((PimpPotType == PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP") : Localization.Get("TwitchPoints_PP"));
					if (PimpPotType != PimpPotSettings.Disabled)
					{
						if (PimpPotType == PimpPotSettings.EnabledSP)
						{
							viewerEntry.SpecialPoints += RewardPot;
						}
						else
						{
							viewerEntry.StandardPoints += RewardPot;
						}
						ircClient.SendChannelMessage(string.Format(chatOutput_KilledStreamer, text, viewerEntry.CombinedPoints, RewardPot, text6, player.EntityName), useQueue: true);
						string text7 = string.Format(ingameOutput_KilledStreamer, viewerEntry.UserColor, text, RewardPot, text6, player.EntityName);
						GameManager.ShowTooltip(Current.LocalPlayer, text7);
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(Authentication.userName, text7), null, EMessageSender.None);
						QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.PimpPot, "");
					}
					DeathText = string.Format(ingameDeathScreen_Message, viewerEntry.UserColor, text, text2, RewardPot, text6);
					RewardPot = PimpPotDefault;
				}
				LeaderboardStats.CheckTopKiller(LeaderboardStats.AddKill(text, viewerEntry.UserColor));
				AddKillToLeaderboard(text, viewerEntry.UserColor);
				twitchPlayerDeathsThisFrame.Add(player);
			}
			else
			{
				if (PimpPotType != PimpPotSettings.Disabled)
				{
					ViewerEntry viewerEntry2 = ViewerData.GetViewerEntry(text);
					int num5 = Mathf.Min(PartyKillRewardMax, RewardPot);
					viewerEntry2.StandardPoints += num5;
					ircClient.SendChannelMessage(string.Format(chatOutput_KilledParty, text, viewerEntry2.CombinedPoints, num5, player.EntityName), useQueue: true);
					string text8 = string.Format(ingameOutput_KilledParty, viewerEntry2.UserColor, text, num5, player.EntityName);
					GameManager.ShowTooltip(Current.LocalPlayer, text8);
					GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(Authentication.userName, text8), null, EMessageSender.None);
				}
				twitchPlayerDeathsThisFrame.Add(player);
			}
		}
		else if (twitchVoteEntry != null && flag && VotingManager != null && !twitchVoteEntry.Complete)
		{
			HandleVoteKill(twitchVoteEntry);
		}
		if (flag)
		{
			VotingManager.ResetVoteOnDeath();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleVoteKill(TwitchVoteEntry voteEntry)
	{
		List<string> list = VotingManager.HandleKiller(voteEntry);
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ViewerData.GetViewerEntry(list[i]).StandardPoints += VotingManager.ViewerDefeatReward;
			}
			string text = string.Format(ingameOutput_KilledByVote, VotingManager.ViewerDefeatReward, LocalPlayer.EntityName);
			GameManager.ShowTooltip(Current.LocalPlayer, text);
			GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(Authentication.userName, text), null, EMessageSender.None);
			ircClient.SendChannelMessage(string.Format(chatOutput_KilledByVote, VotingManager.ViewerDefeatReward, LocalPlayer.EntityName), useQueue: true);
			DeathText = string.Format(ingameVoteDeathScreen_Message, VotingManager.ViewerDefeatReward);
			list.Clear();
		}
		if (voteEntry != null)
		{
			voteEntry.Complete = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddKillToLeaderboard(string username, string usercolor)
	{
		bool flag = false;
		for (int i = 0; i < Leaderboard.Count; i++)
		{
			if (Leaderboard[i].UserName == username)
			{
				Leaderboard[i].Kills++;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Leaderboard.Add(new TwitchLeaderboardEntry(username, usercolor, 1));
		}
	}

	public void ClearLeaderboard()
	{
		Leaderboard.Clear();
	}

	public void AddCooldownPreset(CooldownPreset preset)
	{
		if (CooldownPresets == null)
		{
			CooldownPresets = new List<CooldownPreset>();
		}
		if (preset.IsDefault)
		{
			CooldownPresetIndex = CooldownPresets.Count;
		}
		CooldownPresets.Add(preset);
	}

	public void SetCooldownPreset(int index)
	{
		if (InitState == InitStates.Ready)
		{
			bool num = CurrentCooldownPreset.CooldownType != CooldownPresets[index].CooldownType;
			CooldownPresetIndex = index;
			GetCooldownMax();
			if (num)
			{
				resetCommandsNeeded = true;
			}
		}
		else
		{
			CooldownPresetIndex = index;
		}
	}

	public void SetToDefaultCooldown()
	{
		for (int i = 0; i < CooldownPresets.Count; i++)
		{
			if (CooldownPresets[i].IsDefault)
			{
				SetCooldownPreset(i);
				break;
			}
		}
	}

	public void GetCooldownMax()
	{
		CurrentCooldownPreset = CooldownPresets[CooldownPresetIndex];
		CurrentCooldownPreset.SetupCooldownInfo(HighestGameStage, LocalPlayer);
		SetupBloodMoonData();
	}

	public void AddTwitchActionPreset(TwitchActionPreset preset)
	{
		if (ActionPresets == null)
		{
			ActionPresets = new List<TwitchActionPreset>();
		}
		ActionPresets.Add(preset);
		if (preset.IsDefault)
		{
			ActionPresetIndex = ActionPresets.Count - 1;
			CurrentActionPreset = ActionPresets[ActionPresetIndex];
		}
	}

	public void SetTwitchActionPreset(int index)
	{
		if (ActionPresetIndex != index)
		{
			ActionPresets[ActionPresetIndex].AddedActions.Clear();
			ActionPresets[ActionPresetIndex].RemovedActions.Clear();
			ActionPresetIndex = index;
			CurrentActionPreset = ActionPresets[ActionPresetIndex];
			CurrentActionPreset.HandleCooldowns();
			if (InitState == InitStates.Ready)
			{
				resetCommandsNeeded = true;
			}
		}
	}

	public void SetToDefaultActionPreset()
	{
		for (int i = 0; i < ActionPresets.Count; i++)
		{
			if (ActionPresets[i].IsDefault)
			{
				SetTwitchActionPreset(i);
				break;
			}
		}
	}

	public void AddTwitchVotePreset(TwitchVotePreset preset)
	{
		if (VotePresets == null)
		{
			VotePresets = new List<TwitchVotePreset>();
		}
		VotePresets.Add(preset);
		if (preset.IsDefault)
		{
			VotePresetIndex = VotePresets.Count - 1;
			CurrentVotePreset = VotePresets[VotePresetIndex];
		}
	}

	public void SetTwitchVotePreset(int index)
	{
		if (VotePresetIndex != index)
		{
			VotePresetIndex = index;
			CurrentVotePreset = VotePresets[VotePresetIndex];
			if (CurrentVotePreset.IsEmpty)
			{
				VotingManager.ForceEndVote();
			}
			SetupAvailableCommands();
		}
	}

	public void SetToDefaultVotePreset()
	{
		for (int i = 0; i < VotePresets.Count; i++)
		{
			if (VotePresets[i].IsDefault)
			{
				SetTwitchVotePreset(i);
				break;
			}
		}
	}

	public void AddTwitchEventPreset(TwitchEventPreset preset)
	{
		if (EventPresets == null)
		{
			EventPresets = new List<TwitchEventPreset>();
		}
		EventPresets.Add(preset);
		if (preset.IsDefault)
		{
			EventPresetIndex = EventPresets.Count - 1;
			CurrentEventPreset = EventPresets[EventPresetIndex];
		}
	}

	public void SetTwitchEventPreset(int index, bool oldAllowChannelPointRedeems)
	{
		TwitchEventPreset currentEventPreset = CurrentEventPreset;
		EventPresetIndex = index;
		CurrentEventPreset = EventPresets[EventPresetIndex];
		currentEventPreset?.RemoveChannelPointRedemptions(AllowChannelPointRedemptions ? CurrentEventPreset : null);
		SetupTwitchCommands();
		if (AllowChannelPointRedemptions && CurrentEventPreset != null)
		{
			CurrentEventPreset.AddChannelPointRedemptions();
		}
	}

	public TwitchEventPreset GetEventPreset(string name)
	{
		for (int i = 0; i < EventPresets.Count; i++)
		{
			if (EventPresets[i].Name.EqualsCaseInsensitive(name))
			{
				return EventPresets[i];
			}
		}
		return null;
	}

	public void SetToDefaultEventPreset()
	{
		for (int i = 0; i < EventPresets.Count; i++)
		{
			if (EventPresets[i].IsDefault)
			{
				SetTwitchEventPreset(i, AllowChannelPointRedemptions);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventSub_OnSubscriptionRedeemed(SubscriptionEventBase e)
	{
		if (e.UserName != null)
		{
			string text = e.UserName.ToLower();
			TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.GetSubTier(e.Tier);
			ViewerEntry viewerEntry = ViewerData.GetViewerEntry(text);
			viewerEntry.UserID = StringParsers.ParseSInt32(e.UserId);
			int num = ViewerData.GetSubTierPoints(subTier) * SubPointModifier;
			if (num > 0)
			{
				viewerEntry.SpecialPoints += num;
				ircClient.SendChannelMessage(string.Format(chatOutput_Subscribed, text, viewerEntry.CombinedPoints, GetTierName(subTier), num), useQueue: true);
				string msg = string.Format(ingameOutput_Subscribed, text, GetTierName(subTier), num);
				AddToInGameChatQueue(msg);
			}
			if (e is SubscriptionMessageEvent subscriptionMessageEvent)
			{
				HandleSubEvent(text, subscriptionMessageEvent.CumulativeMonths, subTier);
			}
			else
			{
				HandleSubEvent(text, 1, subTier);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventSub_OnSubGifted(SubscriptionGiftEvent e)
	{
		if (e.UserName != null && !e.IsAnonymous)
		{
			ViewerData.AddGiftSubEntry(e.UserName.ToLower(), StringParsers.ParseSInt32(e.UserId), TwitchSubEventEntry.GetSubTier(e.Tier), e.Total);
		}
	}

	public string GetTierName(TwitchSubEventEntry.SubTierTypes tier)
	{
		return tier switch
		{
			TwitchSubEventEntry.SubTierTypes.Prime => "Prime", 
			TwitchSubEventEntry.SubTierTypes.Tier1 => "1", 
			TwitchSubEventEntry.SubTierTypes.Tier2 => "2", 
			TwitchSubEventEntry.SubTierTypes.Tier3 => "3", 
			_ => "1", 
		};
	}

	public string GetSubTierRewards(int subModifier)
	{
		if (subModifier == 0)
		{
			return Localization.Get("xuiLightPropShadowsNone");
		}
		return string.Format(subPointDisplay, ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier1) * subModifier, ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier2) * subModifier, ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier3) * subModifier);
	}

	public string GetGiftSubTierRewards(int subModifier)
	{
		if (subModifier == 0)
		{
			return Localization.Get("xuiLightPropShadowsNone");
		}
		return string.Format(subPointDisplay, ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier1) * subModifier, ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier2) * subModifier, ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier3) * subModifier);
	}

	public void HandleSubEvent(string username, int months, TwitchSubEventEntry.SubTierTypes tier)
	{
		if (AllowEvents)
		{
			TwitchSubEventEntry twitchSubEventEntry = CurrentEventPreset.HandleSubEvent(months, tier);
			if (twitchSubEventEntry != null)
			{
				ViewerEntry viewerEntry = ViewerData.GetViewerEntry(username);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = username;
				twitchEventActionEntry.Event = twitchSubEventEntry;
				EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(username, this);
				ircClient.SendChannelMessage(string.Format(chatOutput_SubEvent, twitchSubEventEntry.EventTitle, username, viewerEntry.CombinedPoints), useQueue: true);
			}
		}
	}

	public void HandleGiftSubEvent(string username, int giftCounts, TwitchSubEventEntry.SubTierTypes tier)
	{
		if (AllowEvents)
		{
			TwitchSubEventEntry twitchSubEventEntry = CurrentEventPreset.HandleGiftSubEvent(giftCounts, tier);
			if (twitchSubEventEntry != null)
			{
				ViewerEntry viewerEntry = ViewerData.GetViewerEntry(username);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = username;
				twitchEventActionEntry.Event = twitchSubEventEntry;
				EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(username, this);
				ircClient.SendChannelMessage(string.Format(chatOutput_GiftSubEvent, twitchSubEventEntry.EventTitle, username, viewerEntry.CombinedPoints), useQueue: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventSubMessageReceived(JObject payload)
	{
		string text = payload["subscription"]?["type"]?.ToString() ?? "unknown";
		switch (text)
		{
		case "channel.channel_points_custom_reward_redemption.add":
		{
			ChannelPointsRedemptionEvent channelPointsRedemptionEvent = payload["event"].ToObject<ChannelPointsRedemptionEvent>();
			if (channelPointsRedemptionEvent != null)
			{
				EventSub_OnChannelPointsRedeemed(channelPointsRedemptionEvent);
			}
			break;
		}
		case "channel.subscribe":
		{
			SubscriptionEvent subscriptionEvent = payload["event"].ToObject<SubscriptionEvent>();
			if (subscriptionEvent != null && !subscriptionEvent.IsGift)
			{
				EventSub_OnSubscriptionRedeemed(subscriptionEvent);
			}
			break;
		}
		case "channel.subscription.message":
		{
			SubscriptionMessageEvent subscriptionMessageEvent = payload["event"].ToObject<SubscriptionMessageEvent>();
			if (subscriptionMessageEvent != null)
			{
				EventSub_OnSubscriptionRedeemed(subscriptionMessageEvent);
			}
			break;
		}
		case "channel.subscription.gift":
		{
			SubscriptionGiftEvent e = payload["event"].ToObject<SubscriptionGiftEvent>();
			EventSub_OnSubGifted(e);
			break;
		}
		case "channel.bits.use":
		{
			BitsUsedEvent bitsUsedEvent = payload["event"].ToObject<BitsUsedEvent>();
			if (bitsUsedEvent != null)
			{
				EventSub_OnBitsRedeemed(bitsUsedEvent);
			}
			break;
		}
		case "channel.hype_train.begin":
			if (HypeTrainLevel == 0)
			{
				StartHypeTrain();
			}
			break;
		case "channel.hype_train.progress":
		{
			HypeTrainProgressEvent hypeTrainProgressEvent = payload["event"].ToObject<HypeTrainProgressEvent>();
			if (HypeTrainLevel < hypeTrainProgressEvent.Level)
			{
				IncrementHypeTrainLevel();
			}
			break;
		}
		case "channel.hype_train.end":
			EndHypeTrain();
			break;
		case "channel.raid":
		{
			RaidEvent raidEvent = payload["event"].ToObject<RaidEvent>();
			if (int.TryParse(raidEvent.RaiderID, out var result))
			{
				HandleRaid(raidEvent.RaiderUserName.ToLower(), result, raidEvent.viewerCount);
			}
			else
			{
				Log.Warning("Failed to run Raid Event because RaiderID could not be parsed as an int");
			}
			break;
		}
		default:
			Log.Warning("Unhandled event: " + text);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PubSub_OnBitsRedeemed(object sender, PubSubBitRedemptionMessage.BitRedemptionData e)
	{
		if (e.user_name != null)
		{
			ViewerEntry viewerEntry = ViewerData.GetViewerEntry(e.user_name);
			int num = e.bits_used * BitPointModifier;
			viewerEntry.UserID = StringParsers.ParseSInt32(e.user_id);
			viewerEntry.SpecialPoints += num;
			ircClient.SendChannelMessage(string.Format(chatOutput_DonateBits, e.user_name, viewerEntry.CombinedPoints, e.bits_used, num), useQueue: true);
			string msg = string.Format(ingameOutput_DonateBits, e.user_name, e.bits_used, num);
			AddToInGameChatQueue(msg);
			HandleBitRedeem(e.user_name, e.bits_used, viewerEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventSub_OnBitsRedeemed(BitsUsedEvent e)
	{
		if (e.UserName != null)
		{
			string text = e.UserName.ToLower();
			ViewerEntry viewerEntry = ViewerData.GetViewerEntry(text);
			int num = e.Bits * BitPointModifier;
			viewerEntry.UserID = StringParsers.ParseSInt32(e.UserId);
			viewerEntry.SpecialPoints += num;
			ircClient.SendChannelMessage(string.Format(chatOutput_DonateBits, text, viewerEntry.CombinedPoints, e.Bits, num), useQueue: true);
			string msg = string.Format(ingameOutput_DonateBits, text, e.Bits, num);
			AddToInGameChatQueue(msg);
			HandleBitRedeem(text, e.Bits, viewerEntry);
		}
	}

	public void HandleBitRedeem(string userName, int bitAmount, ViewerEntry viewerEntry = null)
	{
		if (!AllowEvents)
		{
			return;
		}
		TwitchEventEntry twitchEventEntry = CurrentEventPreset.HandleBitRedeem(bitAmount);
		if (twitchEventEntry != null)
		{
			if (viewerEntry == null)
			{
				viewerEntry = ViewerData.GetViewerEntry(userName);
			}
			TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
			twitchEventActionEntry.UserName = userName;
			twitchEventActionEntry.Event = twitchEventEntry;
			EventQueue.Add(twitchEventActionEntry);
			twitchEventActionEntry.Event.HandleInstant(userName, this);
			ircClient.SendChannelMessage(string.Format(chatOutput_BitEvent, twitchEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), useQueue: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointMessage.ChannelRedemptionData e)
	{
		HandleChannelPointsRedeem(e.redemption.reward.title, e.redemption.user.display_name.ToLower());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventSub_OnChannelPointsRedeemed(ChannelPointsRedemptionEvent e)
	{
		HandleChannelPointsRedeem(e.Reward.Title, e.UserLogin.ToLower());
	}

	public void HandleChannelPointsRedeem(string title, string userName)
	{
		if (AllowEvents)
		{
			TwitchChannelPointEventEntry twitchChannelPointEventEntry = CurrentEventPreset.HandleChannelPointsRedeem(title);
			if (twitchChannelPointEventEntry != null)
			{
				ViewerEntry viewerEntry = ViewerData.GetViewerEntry(userName);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = userName;
				twitchEventActionEntry.Event = twitchChannelPointEventEntry;
				EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(userName, this);
				ircClient.SendChannelMessage(string.Format(chatOutput_ChannelPointEvent, twitchChannelPointEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), useQueue: true);
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.ChannelPointRedeems, twitchChannelPointEventEntry.EventName);
			}
		}
	}

	public void HandleRaid(string userName, int userID, int viewerAmount)
	{
		ViewerEntry viewerEntry = ViewerData.GetViewerEntry(userName);
		if (viewerAmount >= RaidViewerMinimum && RaidPointAdd > 0)
		{
			viewerEntry.UserID = userID;
			viewerEntry.SpecialPoints += RaidPointAdd;
			ircClient.SendChannelMessage(string.Format(chatOutput_RaidPoints, userName, viewerEntry.CombinedPoints, viewerAmount, RaidPointAdd), useQueue: true);
			string msg = string.Format(ingameOutput_RaidPoints, userName, viewerAmount, RaidPointAdd);
			AddToInGameChatQueue(msg);
		}
		HandleRaidRedeem(userName, viewerAmount, viewerEntry);
	}

	public void HandleRaidRedeem(string userName, int viewerAmount, ViewerEntry viewerEntry = null)
	{
		if (!AllowEvents)
		{
			return;
		}
		TwitchEventEntry twitchEventEntry = CurrentEventPreset.HandleRaid(viewerAmount);
		if (twitchEventEntry != null)
		{
			if (viewerEntry == null)
			{
				viewerEntry = ViewerData.GetViewerEntry(userName);
			}
			TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
			twitchEventActionEntry.UserName = userName;
			twitchEventActionEntry.Event = twitchEventEntry;
			EventQueue.Add(twitchEventActionEntry);
			twitchEventActionEntry.Event.HandleInstant(userName, this);
			ircClient.SendChannelMessage(string.Format(chatOutput_RaidEvent, twitchEventEntry.EventTitle, userName, viewerEntry.CombinedPoints, viewerAmount), useQueue: true);
		}
	}

	public void HandleCharity(string userName, int userID, int charityAmount)
	{
		ViewerEntry viewerEntry = ViewerData.GetViewerEntry(userName);
		int num = charityAmount * BitPointModifier;
		viewerEntry.UserID = userID;
		viewerEntry.SpecialPoints += num;
		ircClient.SendChannelMessage(string.Format(chatOutput_DonateCharity, userName, viewerEntry.CombinedPoints, charityAmount, num), useQueue: true);
		string msg = string.Format(ingameOutput_DonateCharity, userName, charityAmount, num);
		AddToInGameChatQueue(msg);
		HandleCharityRedeem(userName, charityAmount, viewerEntry);
	}

	public void HandleCharityRedeem(string userName, int charityAmount, ViewerEntry viewerEntry = null)
	{
		if (!AllowEvents)
		{
			return;
		}
		TwitchEventEntry twitchEventEntry = CurrentEventPreset.HandleCharityRedeem(charityAmount);
		if (twitchEventEntry != null)
		{
			if (viewerEntry == null)
			{
				viewerEntry = ViewerData.GetViewerEntry(userName);
			}
			TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
			twitchEventActionEntry.UserName = userName;
			twitchEventActionEntry.Event = twitchEventEntry;
			EventQueue.Add(twitchEventActionEntry);
			twitchEventActionEntry.Event.HandleInstant(userName, this);
			ircClient.SendChannelMessage(string.Format(chatOutput_CharityEvent, twitchEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), useQueue: true);
		}
	}

	public void StartHypeTrain()
	{
		HypeTrainLevel = 1;
		HandleHypeTrainRedeem(HypeTrainLevel);
	}

	public void IncrementHypeTrainLevel()
	{
		HypeTrainLevel++;
		HandleHypeTrainRedeem(HypeTrainLevel);
	}

	public void EndHypeTrain()
	{
		HypeTrainLevel = 0;
	}

	public void HandleHypeTrainRedeem(int hypeTrainLevel)
	{
		if (AllowEvents)
		{
			TwitchEventEntry twitchEventEntry = CurrentEventPreset.HandleHypeTrainRedeem(hypeTrainLevel);
			if (twitchEventEntry != null)
			{
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = " ";
				twitchEventActionEntry.Event = twitchEventEntry;
				EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(twitchEventActionEntry.UserName, this);
				ircClient.SendChannelMessage(string.Format(chatOutput_HypeTrainEvent, twitchEventEntry.EventTitle, hypeTrainLevel), useQueue: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PubSub_OnGoalAchieved(object sender, PubSubGoalMessage.Goal e)
	{
		HandleCreatorGoalRedeem(e.contributionType.ToLower());
	}

	public void HandleCreatorGoalRedeem(string goalType)
	{
		if (AllowEvents)
		{
			TwitchCreatorGoalEventEntry twitchCreatorGoalEventEntry = CurrentEventPreset.HandleCreatorGoalEvent(goalType);
			if (twitchCreatorGoalEventEntry != null)
			{
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = " ";
				twitchEventActionEntry.Event = twitchCreatorGoalEventEntry;
				EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(twitchEventActionEntry.UserName, this);
				ircClient.SendChannelMessage(string.Format(chatOutput_CreatorGoalEvent, twitchCreatorGoalEventEntry.EventTitle), useQueue: true);
			}
		}
	}

	public void HandleEventQueue()
	{
		if (!twitchActive)
		{
			return;
		}
		for (int i = 0; i < EventQueue.Count; i++)
		{
			TwitchEventActionEntry twitchEventActionEntry = EventQueue[i];
			if (twitchEventActionEntry.IsSent || !twitchEventActionEntry.HandleEvent(this))
			{
				continue;
			}
			Manager.BroadcastPlayByLocalPlayer(LocalPlayer.position, "twitch_custom_event");
			if (!twitchEventActionEntry.IsRetry)
			{
				TwitchActionHistoryEntry twitchActionHistoryEntry = new TwitchActionHistoryEntry(twitchEventActionEntry.UserName, "FFFFFF", null, null, twitchEventActionEntry);
				twitchActionHistoryEntry.EventEntry = twitchEventActionEntry;
				twitchEventActionEntry.HistoryEntry = twitchActionHistoryEntry;
				EventHistory.Insert(0, twitchActionHistoryEntry);
				if (EventHistory.Count > 500)
				{
					EventHistory.RemoveAt(EventHistory.Count - 1);
				}
				if (this.EventHistoryAdded != null)
				{
					this.EventHistoryAdded();
				}
			}
			break;
		}
	}

	public void SaveExportViewerData()
	{
		string arg = GameIO.GetUserGameDataDir() + "/Twitch/" + MainFileVersion;
		string savePath = string.Format("{0}/{1}", arg, "twitchexport.txt");
		ViewerData.WriteExport(savePath);
	}

	public void LoadExportViewerData()
	{
		string arg = GameIO.GetUserGameDataDir() + "/Twitch/" + MainFileVersion;
		string path = string.Format("{0}/{1}", arg, "twitchexport.txt");
		if (SdFile.Exists(path))
		{
			using (StreamReader tr = File.OpenText(path))
			{
				ViewerData.LoadExport(tr);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int saveViewerDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream[] array = (PooledExpandableMemoryStream[])_threadInfo.parameter;
		string arg = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GameIO.GetSaveGameLocalDir() : GameIO.GetSaveGameDir());
		string text = string.Format("{0}/{1}", arg, "twitch.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", arg, "twitch.dat.bak"), overwrite: true);
		}
		array[0].Position = 0L;
		StreamUtils.WriteStreamToFile(array[0], text);
		MemoryPools.poolMemoryStream.FreeSync(array[0]);
		string arg2 = GameIO.GetUserGameDataDir() + "/Twitch/" + MainFileVersion;
		text = string.Format("{0}/{1}", arg2, "twitch_main.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", arg2, "twitch_main.dat.bak"), overwrite: true);
		}
		array[1].Position = 0L;
		StreamUtils.WriteStreamToFile(array[1], text);
		MemoryPools.poolMemoryStream.FreeSync(array[1]);
		return -1;
	}

	public void LoadViewerData()
	{
		string arg = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GameIO.GetSaveGameLocalDir() : GameIO.GetSaveGameDir());
		string path = string.Format("{0}/{1}", arg, "twitch.dat");
		if (!SdFile.Exists(path))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			Read(pooledBinaryReader);
		}
		catch (Exception)
		{
			path = string.Format("{0}/{1}", arg, "twitch.dat.bak");
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream2 = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(baseStream2);
			Read(pooledBinaryReader2);
		}
	}

	public void LoadSpecialViewerData()
	{
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_special.dat");
		if (!SdFile.Exists(path))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			ReadSpecial(pooledBinaryReader);
		}
		catch (Exception)
		{
			path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_special.dat.bak");
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream2 = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(baseStream2);
			ReadSpecial(pooledBinaryReader2);
		}
	}

	public bool LoadMainViewerData()
	{
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_main.dat");
		if (SdFile.Exists(path))
		{
			try
			{
				using Stream baseStream = SdFile.OpenRead(path);
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(baseStream);
				ReadMain(pooledBinaryReader);
			}
			catch (Exception)
			{
				path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_main.dat.bak");
				if (SdFile.Exists(path))
				{
					using Stream baseStream2 = SdFile.OpenRead(path);
					using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
					pooledBinaryReader2.SetBaseStream(baseStream2);
					ReadMain(pooledBinaryReader2);
				}
			}
			return true;
		}
		return false;
	}

	public bool LoadLatestMainViewerData()
	{
		for (int num = MainFileVersion; num >= 2; num--)
		{
			if (LoadLatestMainViewerData(num))
			{
				return true;
			}
		}
		return false;
	}

	public bool LoadLatestMainViewerData(int version)
	{
		string text = GameIO.GetUserGameDataDir() + "/Twitch/" + version;
		if (!SdDirectory.Exists(text))
		{
			SdDirectory.CreateDirectory(text);
		}
		string path = string.Format("{0}/{1}", text, "twitch_main.dat");
		if (SdFile.Exists(path))
		{
			try
			{
				using Stream baseStream = SdFile.OpenRead(path);
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(baseStream);
				ReadMain(pooledBinaryReader);
			}
			catch (Exception)
			{
				path = string.Format("{0}/{1}", text, "twitch_main.dat.bak");
				if (SdFile.Exists(path))
				{
					using Stream baseStream2 = SdFile.OpenRead(path);
					using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
					pooledBinaryReader2.SetBaseStream(baseStream2);
					ReadMain(pooledBinaryReader2);
				}
			}
			return true;
		}
		return false;
	}

	public void SaveViewerData()
	{
		if (dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("viewerDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				Write(pooledBinaryWriter);
			}
			PooledExpandableMemoryStream pooledExpandableMemoryStream2 = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter2.SetBaseStream(pooledExpandableMemoryStream2);
				WriteMain(pooledBinaryWriter2);
			}
			dataSaveThreadInfo = ThreadManager.StartThread("viewerDataSave", null, saveViewerDataThreaded, null, new PooledExpandableMemoryStream[2] { pooledExpandableMemoryStream, pooledExpandableMemoryStream2 }, null, _useRealThread: false, _isSilent: true);
		}
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(FileVersion);
		bw.Write(UseProgression);
		ViewerData.Write(bw);
		bw.Write(Leaderboard.Count);
		for (int i = 0; i < Leaderboard.Count; i++)
		{
			TwitchLeaderboardEntry twitchLeaderboardEntry = Leaderboard[i];
			bw.Write(twitchLeaderboardEntry.UserName);
			bw.Write(twitchLeaderboardEntry.Kills);
			bw.Write(twitchLeaderboardEntry.UserColor);
		}
		bw.Write(UseActionsDuringBloodmoon);
		bw.Write(RewardPot);
		bw.Write(ViewerData.PointRate);
		bw.Write(CooldownPresetIndex);
		bw.Write((byte)PimpPotType);
		bw.Write(AllowCrateSharing);
		bw.Write(BitPointModifier);
		bw.Write(RaidPointAdd);
		bw.Write(RaidViewerMinimum);
		bw.Write(SubPointModifier);
		bw.Write(GiftSubPointModifier);
		bw.Write((byte)VotingManager.MaxDailyVotes);
		bw.Write(VotingManager.VoteTime);
		bw.Write((byte)VotingManager.CurrentVoteDayTimeRange);
		bw.Write(VotingManager.ViewerDefeatReward);
		bw.Write(VotingManager.AllowVotesDuringBloodmoon);
		bw.Write(ViewerData.ActionSpamDelay);
		bw.Write(ViewerData.StartingPoints);
		bw.Write(UseActionsDuringQuests);
		bw.Write(VotingManager.AllowVotesDuringQuests);
		bw.Write(VotingManager.AllowVotesInSafeZone);
		bw.Write(changedEnabledVoteList.Count);
		for (int j = 0; j < changedEnabledVoteList.Count; j++)
		{
			bw.Write(changedEnabledVoteList[j]);
			bw.Write(TwitchActionManager.TwitchVotes[changedEnabledVoteList[j]].Enabled);
		}
		bw.Write((byte)integrationSetting);
		bw.Write(EventPresetIndex);
		bw.Write(ActionPresetIndex);
		bw.Write(VotePresetIndex);
		bw.Write(AllowBitEvents);
		bw.Write(AllowSubEvents);
		bw.Write(AllowGiftSubEvents);
		bw.Write(AllowCharityEvents);
		bw.Write(AllowRaidEvents);
		bw.Write(AllowHypeTrainEvents);
		bw.Write(AllowChannelPointRedemptions);
		bw.Write(LeaderboardStats.GoodRewardTime);
		bw.Write(LeaderboardStats.GoodRewardAmount);
		int num = 0;
		for (int k = 0; k < ActionPresets.Count; k++)
		{
			TwitchActionPreset twitchActionPreset = ActionPresets[k];
			if (twitchActionPreset.AddedActions.Count > 0 || twitchActionPreset.RemovedActions.Count > 0)
			{
				num++;
			}
		}
		bw.Write(num);
		for (int l = 0; l < ActionPresets.Count; l++)
		{
			TwitchActionPreset twitchActionPreset2 = ActionPresets[l];
			if (twitchActionPreset2.AddedActions.Count > 0 || twitchActionPreset2.RemovedActions.Count > 0)
			{
				bw.Write(twitchActionPreset2.Name);
				bw.Write(twitchActionPreset2.AddedActions.Count);
				for (int m = 0; m < twitchActionPreset2.AddedActions.Count; m++)
				{
					bw.Write(twitchActionPreset2.AddedActions[m]);
				}
				bw.Write(twitchActionPreset2.RemovedActions.Count);
				for (int n = 0; n < twitchActionPreset2.RemovedActions.Count; n++)
				{
					bw.Write(twitchActionPreset2.RemovedActions[n]);
				}
			}
		}
		bw.Write(bitPriceMultiplier);
		bw.Write(AllowCreatorGoalEvents);
		bw.Write(changedActionList.Count);
		for (int num2 = 0; num2 < changedActionList.Count; num2++)
		{
			bw.Write(changedActionList[num2]);
			bw.Write(TwitchActionManager.TwitchActions[changedActionList[num2]].ModifiedCost);
		}
		bw.Write(BitPot);
		bw.Write(BitPotPercentage);
	}

	public void HandleChangedPropertyList()
	{
		changedActionList.Clear();
		changedEnabledVoteList.Clear();
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchAction twitchAction = TwitchActionManager.TwitchActions[key];
			if (twitchAction.DefaultCost != twitchAction.ModifiedCost)
			{
				changedActionList.Add(key);
			}
		}
		foreach (string key2 in TwitchActionManager.TwitchVotes.Keys)
		{
			TwitchVote twitchVote = TwitchActionManager.TwitchVotes[key2];
			if (twitchVote.Enabled != twitchVote.OriginalEnabled)
			{
				changedEnabledVoteList.Add(key2);
			}
		}
	}

	public void WriteSpecial(BinaryWriter bw)
	{
		ViewerData.WriteSpecial(bw);
	}

	public void WriteMain(BinaryWriter bw)
	{
		bw.Write(MainFileVersion);
		bw.Write(HasViewedSettings);
		ViewerData.WriteSpecial(bw);
	}

	public void Read(BinaryReader br)
	{
		CurrentFileVersion = br.ReadByte();
		if (CurrentFileVersion > 1)
		{
			UseProgression = br.ReadBoolean();
		}
		ViewerData.Read(br, CurrentFileVersion);
		if (CurrentFileVersion > 3)
		{
			int num = br.ReadInt32();
			Leaderboard.Clear();
			for (int i = 0; i < num; i++)
			{
				string text = "";
				string text2 = "";
				int num2 = 1;
				if (CurrentFileVersion > 10)
				{
					text = br.ReadString();
					num2 = br.ReadInt32();
					text2 = br.ReadString();
				}
				else
				{
					text = br.ReadString();
					text2 = br.ReadString();
					num2 = br.ReadInt32();
				}
				Leaderboard.Add(new TwitchLeaderboardEntry(text, text2, num2));
			}
		}
		if (CurrentFileVersion > 4)
		{
			UseActionsDuringBloodmoon = br.ReadInt32();
		}
		if (CurrentFileVersion > 5)
		{
			RewardPot = br.ReadInt32();
			if (RewardPot <= 0)
			{
				RewardPot = 0;
			}
			if (RewardPot > LeaderboardStats.LargestPimpPot)
			{
				LeaderboardStats.LargestPimpPot = RewardPot;
			}
		}
		if (CurrentFileVersion > 6)
		{
			ViewerData.PointRate = br.ReadSingle();
			CooldownPresetIndex = br.ReadInt32();
			if (CurrentFileVersion <= 18)
			{
				ActionCooldownModifier = br.ReadSingle();
			}
			PimpPotType = (PimpPotSettings)br.ReadByte();
			AllowCrateSharing = br.ReadBoolean();
		}
		if (CurrentFileVersion > 7)
		{
			if (CurrentFileVersion <= 17)
			{
				br.ReadBoolean();
				br.ReadBoolean();
				br.ReadBoolean();
			}
			BitPointModifier = br.ReadInt32();
			RaidPointAdd = br.ReadInt32();
			RaidViewerMinimum = br.ReadInt32();
			SubPointModifier = br.ReadInt32();
			GiftSubPointModifier = br.ReadInt32();
			if (CurrentFileVersion <= 20)
			{
				int num3 = br.ReadInt32();
				changedActionList.Clear();
				for (int j = 0; j < num3; j++)
				{
					string text3 = br.ReadString();
					bool enabled = br.ReadBoolean();
					if (TwitchActionManager.TwitchActions.ContainsKey(text3))
					{
						changedActionList.Add(text3);
						TwitchActionManager.TwitchActions[text3].Enabled = enabled;
					}
				}
			}
		}
		if (CurrentFileVersion > 8)
		{
			VotingManager.MaxDailyVotes = br.ReadByte();
			VotingManager.VoteTime = br.ReadSingle();
			VotingManager.CurrentVoteDayTimeRange = br.ReadByte();
			VotingManager.ViewerDefeatReward = br.ReadInt32();
			VotingManager.AllowVotesDuringBloodmoon = br.ReadBoolean();
		}
		if (CurrentFileVersion > 9)
		{
			ViewerData.ActionSpamDelay = br.ReadSingle();
		}
		if (CurrentFileVersion > 10)
		{
			ViewerData.StartingPoints = br.ReadInt32();
		}
		if (CurrentFileVersion > 11)
		{
			UseActionsDuringQuests = br.ReadInt32();
			VotingManager.AllowVotesDuringQuests = br.ReadBoolean();
		}
		if (CurrentFileVersion > 12)
		{
			VotingManager.AllowVotesInSafeZone = br.ReadBoolean();
			if (CurrentFileVersion <= 17)
			{
				br.ReadByte();
			}
		}
		if (CurrentFileVersion > 13)
		{
			int num4 = br.ReadInt32();
			changedEnabledVoteList.Clear();
			for (int k = 0; k < num4; k++)
			{
				string text4 = br.ReadString();
				changedEnabledVoteList.Add(text4);
				TwitchActionManager.TwitchVotes[text4].Enabled = br.ReadBoolean();
			}
		}
		if (CurrentFileVersion >= 16)
		{
			byte b = br.ReadByte();
			if (b > 1)
			{
				b = 1;
			}
			IntegrationSetting = (IntegrationSettings)b;
		}
		if (CurrentFileVersion >= 17)
		{
			EventPresetIndex = br.ReadInt32();
			CurrentEventPreset = EventPresets[EventPresetIndex];
		}
		if (CurrentFileVersion >= 18)
		{
			ActionPresetIndex = br.ReadInt32();
			CurrentActionPreset = ActionPresets[ActionPresetIndex];
			VotePresetIndex = br.ReadInt32();
			CurrentVotePreset = VotePresets[VotePresetIndex];
			AllowBitEvents = br.ReadBoolean();
			AllowSubEvents = br.ReadBoolean();
			AllowGiftSubEvents = br.ReadBoolean();
			AllowCharityEvents = br.ReadBoolean();
			AllowRaidEvents = br.ReadBoolean();
			AllowHypeTrainEvents = br.ReadBoolean();
			AllowChannelPointRedemptions = br.ReadBoolean();
		}
		if (CurrentFileVersion >= 20)
		{
			LeaderboardStats.GoodRewardTime = br.ReadInt32();
			LeaderboardStats.GoodRewardAmount = br.ReadInt32();
		}
		if (CurrentFileVersion >= 21)
		{
			int num5 = br.ReadInt32();
			for (int l = 0; l < num5; l++)
			{
				string text5 = br.ReadString();
				TwitchActionPreset twitchActionPreset = null;
				for (int m = 0; m < ActionPresets.Count; m++)
				{
					if (ActionPresets[m].Name == text5)
					{
						twitchActionPreset = ActionPresets[m];
						break;
					}
				}
				int num6 = br.ReadInt32();
				if (twitchActionPreset != null)
				{
					twitchActionPreset.AddedActions.Clear();
					twitchActionPreset.RemovedActions.Clear();
				}
				for (int n = 0; n < num6; n++)
				{
					twitchActionPreset?.AddedActions.Add(br.ReadString());
				}
				num6 = br.ReadInt32();
				for (int num7 = 0; num7 < num6; num7++)
				{
					twitchActionPreset?.RemovedActions.Add(br.ReadString());
				}
			}
		}
		if (CurrentFileVersion >= 22)
		{
			BitPriceMultiplier = br.ReadSingle();
		}
		if (CurrentFileVersion >= 23)
		{
			AllowCreatorGoalEvents = br.ReadBoolean();
		}
		if (CurrentFileVersion >= 24)
		{
			int num8 = br.ReadInt32();
			changedActionList.Clear();
			for (int num9 = 0; num9 < num8; num9++)
			{
				string text6 = br.ReadString();
				int modifiedCost = br.ReadInt32();
				if (TwitchActionManager.TwitchActions.ContainsKey(text6))
				{
					changedActionList.Add(text6);
					TwitchActionManager.TwitchActions[text6].ModifiedCost = modifiedCost;
				}
			}
		}
		if (CurrentFileVersion >= 25)
		{
			BitPot = br.ReadInt32();
			if (BitPot <= 0)
			{
				BitPot = 0;
			}
			if (BitPot > LeaderboardStats.LargestBitPot)
			{
				LeaderboardStats.LargestBitPot = BitPot;
			}
		}
		if (CurrentFileVersion >= 26)
		{
			BitPotPercentage = br.ReadSingle();
		}
	}

	public void ReadSpecial(BinaryReader br)
	{
		ViewerData.ReadSpecial(br, 1);
	}

	public void ReadMain(BinaryReader br)
	{
		CurrentMainFileVersion = br.ReadByte();
		HasViewedSettings = br.ReadBoolean();
		ViewerData.ReadSpecial(br, CurrentMainFileVersion);
	}

	public void SendChannelPointOutputMessage(string name, ViewerEntry entry)
	{
		if (entry.SpecialPoints == 0f)
		{
			ircClient.SendChannelMessage(string.Format(chatOutput_PointsWithoutSpecial, name, entry.CombinedPoints), useQueue: true);
		}
		else
		{
			ircClient.SendChannelMessage(string.Format(chatOutput_PointsWithSpecial, name, entry.CombinedPoints, entry.SpecialPoints), useQueue: true);
		}
	}

	public void SendChannelCreditOutputMessage(string name, ViewerEntry entry)
	{
		ircClient.SendChannelMessage(string.Format(chatOutput_BitCredits, name, entry.BitCredits), useQueue: true);
	}

	public void SendChannelPointOutputMessage(string name)
	{
		SendChannelPointOutputMessage(name, ViewerData.GetViewerEntry(name));
	}

	public void SendChannelCreditOutputMessage(string name)
	{
		SendChannelCreditOutputMessage(name, ViewerData.GetViewerEntry(name));
	}

	public void SendChannelMessage(string message, bool useQueue = true)
	{
		ircClient.SendChannelMessage(message, useQueue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateActionCooldowns(float modifier)
	{
		foreach (TwitchAction value in TwitchActionManager.TwitchActions.Values)
		{
			if (value.IsInPreset(CurrentActionPreset))
			{
				value.UpdateModifiedCooldown(modifier);
			}
		}
	}

	public void SetupAvailableCommands()
	{
		AvailableCommands.Clear();
		AlternateCommands.Clear();
		TwitchAction[] array = (from a in TwitchActionManager.TwitchActions.Values
			where a.CanUse && a.IsInPreset(CurrentActionPreset)
			orderby a.Command
			orderby a.PointType
			select a).ToArray();
		List<string> list = null;
		for (int num = 0; num < array.Count(); num++)
		{
			TwitchAction twitchAction = array[num];
			if (UseProgression && !OverrideProgession)
			{
				int startGameStage = twitchAction.StartGameStage;
				if (startGameStage != -1 && startGameStage > HighestGameStage)
				{
					continue;
				}
				if (twitchAction.Replaces != "")
				{
					string item = twitchAction.Replaces;
					if (AlternateCommands.ContainsKey(twitchAction.Replaces))
					{
						item = AlternateCommands[twitchAction.Replaces];
					}
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(item);
				}
				if (AvailableCommands.ContainsKey(twitchAction.Command))
				{
					AvailableCommands[twitchAction.Command] = twitchAction;
				}
				else
				{
					AvailableCommands.Add(twitchAction.Command, twitchAction);
				}
				if (!AlternateCommands.ContainsKey(twitchAction.BaseCommand))
				{
					AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
				}
				continue;
			}
			if (twitchAction.RandomDaily)
			{
				twitchAction.AllowedDay = lastGameDay;
			}
			if (twitchAction.Replaces != "")
			{
				string item2 = twitchAction.Replaces;
				if (AlternateCommands.ContainsKey(twitchAction.Replaces))
				{
					item2 = AlternateCommands[twitchAction.Replaces];
				}
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(item2);
			}
			if (AvailableCommands.ContainsKey(twitchAction.Command))
			{
				AvailableCommands[twitchAction.Command] = twitchAction;
			}
			else
			{
				AvailableCommands.Add(twitchAction.Command, twitchAction);
			}
			if (!AlternateCommands.ContainsKey(twitchAction.BaseCommand))
			{
				AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
			}
		}
		if (list == null)
		{
			return;
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			string key = list[num2];
			if (AvailableCommands.ContainsKey(key))
			{
				AvailableCommands.Remove(key);
			}
		}
	}

	public void SetupAvailableCommandsWithOutput(int lastGameStage, bool displayMessage)
	{
		AvailableCommands.Clear();
		AlternateCommands.Clear();
		StringBuilder stringBuilder = null;
		TwitchAction[] array = (from a in TwitchActionManager.TwitchActions.Values
			where a.CanUse && a.IsInPreset(CurrentActionPreset)
			orderby a.Command
			orderby a.PointType
			select a).ToArray();
		List<string> list = null;
		for (int num = 0; num < array.Count(); num++)
		{
			TwitchAction twitchAction = array[num];
			if (UseProgression && !OverrideProgession)
			{
				int startGameStage = twitchAction.StartGameStage;
				if (startGameStage != -1 && startGameStage > HighestGameStage)
				{
					continue;
				}
				if (twitchAction.Replaces != "")
				{
					string item = twitchAction.Replaces;
					if (AlternateCommands.ContainsKey(twitchAction.Replaces))
					{
						item = AlternateCommands[twitchAction.Replaces];
					}
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(item);
				}
				if (AvailableCommands.ContainsKey(twitchAction.Command))
				{
					AvailableCommands[twitchAction.Command] = twitchAction;
					if (startGameStage > lastGameStage)
					{
						if (stringBuilder == null)
						{
							stringBuilder = new StringBuilder();
							stringBuilder.Append("*" + twitchAction.Command);
						}
						else
						{
							stringBuilder.Append(", " + twitchAction.Command);
						}
					}
				}
				else
				{
					AvailableCommands.Add(twitchAction.Command, twitchAction);
					if (startGameStage > lastGameStage)
					{
						if (stringBuilder == null)
						{
							stringBuilder = new StringBuilder();
							stringBuilder.Append(twitchAction.Command);
						}
						else
						{
							stringBuilder.Append(", " + twitchAction.Command);
						}
					}
				}
				if (!AlternateCommands.ContainsKey(twitchAction.BaseCommand))
				{
					AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
				}
			}
			else
			{
				if (twitchAction.RandomDaily)
				{
					twitchAction.AllowedDay = lastGameDay;
				}
				if (AvailableCommands.ContainsKey(twitchAction.Command))
				{
					AvailableCommands[twitchAction.Command] = twitchAction;
				}
				else
				{
					AvailableCommands.Add(twitchAction.Command, twitchAction);
				}
				if (!AlternateCommands.ContainsKey(twitchAction.BaseCommand))
				{
					AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
				}
			}
		}
		if (list != null)
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				string key = list[num2];
				if (AvailableCommands.ContainsKey(key))
				{
					AvailableCommands.Remove(key);
				}
			}
		}
		if (displayMessage && stringBuilder != null && AllowActions && CurrentActionPreset.ShowNewCommands)
		{
			ircClient.SendChannelMessage(string.Format(chatOutput_NewActions, stringBuilder), useQueue: true);
			Manager.BroadcastPlayByLocalPlayer(LocalPlayer.position, "twitch_new_commands");
		}
	}

	public void HandleCooldownActionLocking()
	{
		foreach (string key in AvailableCommands.Keys)
		{
			TwitchAction twitchAction = AvailableCommands[key];
			if (!twitchAction.IsInPreset(CurrentActionPreset))
			{
				continue;
			}
			if (OnCooldown)
			{
				if (CooldownType == CooldownTypes.BloodMoonDisabled || CooldownType == CooldownTypes.Time)
				{
					twitchAction.OnCooldown = true;
				}
				else if (CooldownType == CooldownTypes.MaxReachedWaiting || CooldownType == CooldownTypes.SafeCooldown)
				{
					twitchAction.OnCooldown = twitchAction.WaitingBlocked;
				}
				else
				{
					twitchAction.OnCooldown = twitchAction.CooldownBlocked;
				}
			}
			else
			{
				twitchAction.OnCooldown = false;
			}
		}
		if (this.CommandsChanged != null)
		{
			this.CommandsChanged();
		}
	}

	public void PushBalanceToExtensionQueue(string userID, int creditBalance)
	{
		if (extensionManager != null)
		{
			extensionManager.PushUserBalance((userID, creditBalance));
		}
	}

	public void DisplayActions()
	{
		_ = ActionMessages.Count;
		if (CurrentUnityTime > nextDisplayCommandsTime)
		{
			nextDisplayCommandsTime = CurrentUnityTime + 15f;
			ircClient.SendChannelMessages(ActionMessages, useQueue: true);
		}
	}

	public void DisplayCommands(bool isBroadcaster, bool isMod, bool isVIP, bool isSub)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < TwitchCommandList.Count; i++)
		{
			if (TwitchCommandList[i] is TwitchCommandCommands)
			{
				continue;
			}
			bool flag = false;
			switch (BaseTwitchCommand.GetPermission(TwitchCommandList[i]))
			{
			case BaseTwitchCommand.PermissionLevels.Broadcaster:
				flag = isBroadcaster;
				break;
			case BaseTwitchCommand.PermissionLevels.Mod:
				flag = isMod;
				break;
			case BaseTwitchCommand.PermissionLevels.VIP:
				flag = isVIP;
				break;
			case BaseTwitchCommand.PermissionLevels.Sub:
				flag = isSub;
				break;
			case BaseTwitchCommand.PermissionLevels.Everyone:
				flag = true;
				break;
			}
			if (!flag)
			{
				continue;
			}
			for (int j = 0; j < TwitchCommandList[i].LocalizedCommandNames.Length; j++)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(TwitchCommandList[i].LocalizedCommandNames[j]);
			}
		}
		ircClient.SendChannelMessage(string.Format(chatOutput_Commands, stringBuilder.ToString()), useQueue: true);
	}

	public void AddTip(string tipname)
	{
		string text = Localization.Get(tipname);
		string item = Localization.Get(tipname + "Desc");
		if (text != "")
		{
			tipTitleList.Add(text);
			tipDescriptionList.Add(item);
		}
	}

	public void DisplayGameStage()
	{
		if (LocalPlayer != null)
		{
			ircClient.SendChannelMessage(string.Format(chatOutput_Gamestage, LocalPlayer.unModifiedGameStage), useQueue: true);
		}
	}

	public bool CheckIfTwitchKill(EntityPlayer player)
	{
		return twitchPlayerDeathsThisFrame.Contains(player);
	}

	public bool LiveListContains(int entityID)
	{
		for (int i = 0; i < liveList.Count; i++)
		{
			if (liveList[i].SpawnedEntityID == entityID)
			{
				return true;
			}
		}
		return false;
	}
}
