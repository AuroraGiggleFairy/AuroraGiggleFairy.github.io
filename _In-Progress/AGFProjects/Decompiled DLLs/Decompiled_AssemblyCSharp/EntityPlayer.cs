using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityPlayer : EntityAlive
{
	public delegate void OnPlayerTeleportDelegate();

	public enum TwitchActionsStates
	{
		Disabled,
		Enabled,
		TempDisabled,
		TempDisabledEnding
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly FastTags<TagGroup.Global> STAMINA_LOSS_TAGS = FastTags<TagGroup.Global>.GetTag("Athletics");

	public float jumpStrength = 0.451f;

	public SpawnPosition lastSpawnPosition = SpawnPosition.Undef;

	public PlayerProfile playerProfile;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerName cachedPlayerName;

	public List<EntityAlive> aiClosest = new List<EntityAlive>();

	public AIDirectorBloodMoonParty bloodMoonParty;

	public bool IsBloodMoonDead;

	public PlayerStealth Stealth;

	public const long cSpawnPointKeyInvalid = -1L;

	public long selectedSpawnPointKey = -1L;

	public ulong LastZombieAttackTime;

	public bool IsFriendOfLocalPlayer;

	public bool IsInPartyOfLocalPlayer;

	public uint totalItemsCrafted;

	public float longestLife;

	public float currentLife;

	public float totalTimePlayed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int longestLifeLived;

	public ChunkManager.ChunkObserver ChunkObserver;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RespawnType lastRespawnReason = RespawnType.Unknown;

	public int SpawnedTicks;

	public ulong gameStageBornAtWorldTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_MarkerPosition = Vector3i.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject navMarker;

	public bool navMarkerHidden;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject navVending;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastTimePrefabChecked;

	public PrefabInstance prefab;

	public PrefabInstance enteredPrefab;

	public bool prefabInfoEntered;

	public float prefabTimeIn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bModelVisible = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchSafe;

	public bool IsInTrader;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public TwitchVoteLockTypes twitchVoteLock;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool twitchVisionDisabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public TwitchActionsStates twitchActionsEnabled = TwitchActionsStates.Enabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isSpectator;

	public WaypointCollection Waypoints = new WaypointCollection();

	public List<Waypoint> WaypointInvites = new List<Waypoint>();

	public QuestJournal QuestJournal = new QuestJournal();

	public List<ushort> favoriteCreativeStacks = new List<ushort>();

	public List<string> favoriteShapes = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_rentedVMPosition = Vector3i.zero;

	public ulong RentalEndTime;

	public int RentalEndDay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 averageVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 averagVelLastPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBreadcrumbMask = 31;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] breadcrumbs = new Vector3[32];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 breadcrumbLastPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int breadcrumbIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAdmin;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lastChunkPos = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

	public bool HasUpdated;

	public FastTags<TagGroup.Global> generalTags = FastTags<TagGroup.Global>.none;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 attachedModelPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int visiblityCheckTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastVehiclePositionOnDismount = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeOfVehicleDismount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float vehicleTeleportThresholdSeconds = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool forcedDetach;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Party party;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CompanionGroup companions;

	public List<EntityPlayer> partyInvites = new List<EntityPlayer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool laserSightActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 laserSightPosition;

	public string PlayerDisplayName
	{
		get
		{
			if (cachedPlayerName != null)
			{
				return cachedPlayerName.DisplayName;
			}
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
			if (playerDataFromEntityID == null)
			{
				return null;
			}
			cachedPlayerName = playerDataFromEntityID.PlayerName;
			return cachedPlayerName.DisplayName;
		}
	}

	public bool TwitchEnabled
	{
		get
		{
			return twitchEnabled;
		}
		set
		{
			if (value != twitchEnabled)
			{
				twitchEnabled = value;
				bPlayerTwitchChanged |= !isEntityRemote;
				if (TwitchManager.HasInstance && TwitchManager.Current.extensionManager != null)
				{
					TwitchManager.Current.extensionManager.TwitchEnabledChanged(this);
				}
			}
		}
	}

	public bool TwitchSafe
	{
		get
		{
			return twitchSafe;
		}
		set
		{
			if (value != twitchSafe)
			{
				twitchSafe = value;
				bPlayerTwitchChanged |= !isEntityRemote;
				if (twitchSafe)
				{
					Buffs.AddBuff("twitch_safe");
				}
				else
				{
					Buffs.RemoveBuff("twitch_safe");
				}
			}
		}
	}

	public TwitchVoteLockTypes TwitchVoteLock
	{
		get
		{
			return twitchVoteLock;
		}
		set
		{
			if (value != twitchVoteLock)
			{
				twitchVoteLock = value;
				bPlayerTwitchChanged |= !isEntityRemote;
			}
		}
	}

	public bool TwitchVisionDisabled
	{
		get
		{
			return twitchVisionDisabled;
		}
		set
		{
			if (value != twitchVisionDisabled)
			{
				twitchVisionDisabled = value;
				bPlayerTwitchChanged |= !isEntityRemote;
			}
		}
	}

	public TwitchActionsStates TwitchActionsEnabled
	{
		get
		{
			return twitchActionsEnabled;
		}
		set
		{
			if (value != twitchActionsEnabled)
			{
				twitchActionsEnabled = value;
				bPlayerTwitchChanged |= !isEntityRemote;
			}
		}
	}

	public bool IsSpectator
	{
		get
		{
			return isSpectator;
		}
		set
		{
			isSpectator = value;
			isIgnoredByAI = isSpectator;
			SetVisible(bModelVisible);
			bPlayerStatsChanged |= !isEntityRemote;
		}
	}

	public Vector3i markerPosition
	{
		get
		{
			return m_MarkerPosition;
		}
		set
		{
			if (isEntityRemote)
			{
				return;
			}
			if (value.Equals(Vector3i.zero))
			{
				if (navMarker != null)
				{
					NavObjectManager.Instance.UnRegisterNavObject(navMarker);
					navMarker = null;
				}
			}
			else if (navMarker == null)
			{
				navMarker = NavObjectManager.Instance.RegisterNavObject("quick_waypoint", value.ToVector3(), "", navMarkerHidden);
			}
			else
			{
				navMarker.TrackedPosition = value.ToVector3();
				navMarker.hiddenOnCompass = navMarkerHidden;
			}
			m_MarkerPosition = value;
		}
	}

	public Vector3i RentedVMPosition
	{
		get
		{
			return m_rentedVMPosition;
		}
		set
		{
			if (isEntityRemote)
			{
				return;
			}
			if (value.Equals(Vector3i.zero))
			{
				if (navVending != null)
				{
					NavObjectManager.Instance.UnRegisterNavObject(navVending);
					navVending = null;
				}
			}
			else if (navVending == null)
			{
				navVending = NavObjectManager.Instance.RegisterNavObject("vending_machine", value.ToVector3());
			}
			else
			{
				navVending.TrackedPosition = value.ToVector3();
			}
			m_rentedVMPosition = value;
		}
	}

	public bool IsAdmin
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (!isEntityRemote)
				{
					return true;
				}
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId);
				return (GameManager.Instance.adminTools?.Users.GetUserPermissionLevel(clientInfo) ?? 1000) == 0;
			}
			return isAdmin;
		}
		set
		{
			if (value != isAdmin)
			{
				isAdmin = value;
			}
		}
	}

	public PlayerEntityStats PlayerStats
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (PlayerEntityStats)entityStats;
		}
	}

	public int unModifiedGameStage
	{
		get
		{
			float num = Mathf.Clamp((long)(world.worldTime - gameStageBornAtWorldTime) / 24000L, 0f, Progression.Level);
			float num2 = GameStats.GetFloat(EnumGameStats.GameDifficultyBonus);
			return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)Progression.Level + num) * num2, this));
		}
	}

	public int gameStage
	{
		get
		{
			float num = Mathf.Clamp((long)(world.worldTime - gameStageBornAtWorldTime) / 24000L, 0f, Progression.Level);
			float num2 = GameStats.GetFloat(EnumGameStats.GameDifficultyBonus);
			if (biomeStandingOn != null)
			{
				float num3 = 0f;
				float num4 = 0f;
				if (QuestJournal.ActiveQuest != null)
				{
					num3 = QuestJournal.ActiveQuest.QuestClass.GameStageMod;
					num4 = QuestJournal.ActiveQuest.QuestClass.GameStageBonus;
				}
				return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)Progression.Level * (1f + biomeStandingOn.GameStageMod + num3) + num + biomeStandingOn.GameStageBonus + num4) * num2, this));
			}
			return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)Progression.Level + num) * num2, this));
		}
	}

	public int HighestPartyGameStage
	{
		get
		{
			if (Party != null)
			{
				return Party.HighestGameStage;
			}
			return gameStage;
		}
	}

	public int PartyGameStage
	{
		get
		{
			if (Party != null)
			{
				return Party.GameStage;
			}
			return gameStage;
		}
	}

	public override float MaxVelocity
	{
		get
		{
			if (MovementRunning)
			{
				return 0.35f;
			}
			return 0.17999f;
		}
	}

	public override bool IsImmuneToLegDamage => false;

	public override bool IsAlert => true;

	public Party Party
	{
		get
		{
			return party;
		}
		set
		{
			if (party != null && value == null && this is EntityPlayerLocal)
			{
				party.ClearAllNavObjectColors();
			}
			party = value;
			if (party == null && this is EntityPlayerLocal)
			{
				QuestJournal.RemoveAllSharedQuests();
			}
		}
	}

	public CompanionGroup Companions
	{
		get
		{
			if (companions == null)
			{
				companions = new CompanionGroup();
			}
			return companions;
		}
	}

	public event OnPlayerTeleportDelegate PlayerTeleportedDelegates;

	public event QuestJournal_QuestEvent QuestAccepted;

	public event QuestJournal_QuestEvent QuestChanged;

	public event QuestJournal_QuestEvent QuestRemoved;

	public event QuestJournal_QuestSharedEvent SharedQuestAdded;

	public event QuestJournal_QuestSharedEvent SharedQuestRemoved;

	public event OnPartyChanged PartyJoined;

	public event OnPartyChanged PartyChanged;

	public event OnPartyChanged PartyLeave;

	public event OnPartyChanged InvitedToParty;

	public void TriggerQuestAddedEvent(Quest _q)
	{
		this.QuestAccepted?.Invoke(_q);
	}

	public void TriggerQuestChangedEvent(Quest _q)
	{
		this.QuestChanged?.Invoke(_q);
	}

	public void TriggerQuestRemovedEvent(Quest _q)
	{
		this.QuestRemoved?.Invoke(_q);
	}

	public void TriggerSharedQuestAddedEvent(SharedQuestEntry _entry)
	{
		if (this.SharedQuestAdded != null)
		{
			this.SharedQuestAdded(_entry);
		}
		else
		{
			Log.Warning($"No SharedQuestAdded listeners! Player: {this}");
		}
	}

	public void TriggerSharedQuestRemovedEvent(SharedQuestEntry _entry)
	{
		this.SharedQuestRemoved?.Invoke(_entry);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		Progression = new Progression(this);
		bWillRespawn = true;
	}

	public override void Init(int _entityClass)
	{
		gameStageBornAtWorldTime = ulong.MaxValue;
		if (playerProfile == null)
		{
			playerProfile = PlayerProfile.LoadLocalProfile();
		}
		Stealth.Init(this);
		alertEnabled = false;
		base.Init(_entityClass);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitStats()
	{
		entityStats = new PlayerEntityStats(this);
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		SetVisible(Spawned);
	}

	public override void SetAlive()
	{
		bool num = IsDead();
		base.SetAlive();
		if (num)
		{
			long num2 = GameStageDefinition.DaysAliveChangeWhenKilled * 24000;
			if ((long)(world.worldTime - gameStageBornAtWorldTime) < num2)
			{
				gameStageBornAtWorldTime = world.worldTime;
			}
			else
			{
				gameStageBornAtWorldTime += (ulong)num2;
			}
		}
	}

	public override void SetDead()
	{
		base.SetDead();
		if (world.aiDirector != null)
		{
			IsBloodMoonDead = world.aiDirector.BloodMoonComponent.BloodMoonActive;
		}
	}

	public int GetTraderStage(int tier)
	{
		GameStats.GetFloat(EnumGameStats.GameDifficultyBonus);
		int a = Mathf.Max(0, tier - 1);
		float num = TraderManager.QuestTierMod[Mathf.Min(a, TraderManager.QuestTierMod.Length - 1)];
		return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.TraderStage, null, (float)Progression.Level * (1f + num), this));
	}

	public int GetLootStage(float containerMod, float containerBonus)
	{
		float num = 0f;
		float num2 = 0f;
		if (prefab != null && prefab.prefab.DifficultyTier > 0)
		{
			int a = Mathf.Max(0, prefab.prefab.DifficultyTier - 1);
			num = LootManager.POITierMod[Mathf.Min(a, LootManager.POITierMod.Length - 1)];
			num2 = LootManager.POITierBonus[Mathf.Min(a, LootManager.POITierBonus.Length - 1)];
		}
		if (biomeStandingOn != null)
		{
			int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStage, null, (float)Progression.Level * (1f + num + biomeStandingOn.LootStageMod + containerMod) + (num2 + biomeStandingOn.LootStageBonus + containerBonus), this));
			if (biomeStandingOn.LootStageMin != -1)
			{
				num3 = Utils.FastMax(num3, biomeStandingOn.LootStageMin);
			}
			if (GameStats.GetBool(EnumGameStats.BiomeProgression) && biomeStandingOn.LootStageMax != -1)
			{
				num3 = Utils.FastMin(num3, Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStageMax, null, biomeStandingOn.LootStageMax, this)));
			}
			return num3;
		}
		return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStage, null, (float)Progression.Level * (1f + num + containerMod) + (num2 + containerBonus), this));
	}

	public int GetHighestPartyLootStage(float containerMod, float containerBonus)
	{
		if (Party != null)
		{
			return Party.GetHighestLootStage(containerMod, containerBonus);
		}
		return GetLootStage(containerMod, containerBonus);
	}

	public void TurnOffLightFlares()
	{
		inventory.TurnOffLightFlares();
	}

	public override float GetSeeDistance()
	{
		return 80f;
	}

	public float DetectUsScale(EntityAlive _entity)
	{
		if (prefab != null && prefab.prefab.DifficultyTier >= 1 && Time.time - prefabTimeIn > 60f && _entity.GetSpawnerSource() == EnumSpawnerSource.Biome && _entity is EntityEnemy)
		{
			return 0.3f;
		}
		return 1f;
	}

	public override Vector3 getHeadPosition()
	{
		if (!(emodel != null) || !(emodel.GetHeadTransform() != null))
		{
			return base.transform.position + new Vector3(0f, base.height - 0.15f, 0f) + Origin.position;
		}
		return emodel.GetHeadTransform().position + Origin.position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		generalTags = MinEventContext.Tags;
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		float num = totalTimePlayed + Time.unscaledDeltaTime / 60f;
		if (this is EntityPlayerLocal)
		{
			int num2 = (int)totalTimePlayed;
			int num3 = (int)num;
			if (num2 != num3 && num3 % 60 == 0)
			{
				int num4 = num3 / 60;
				if (num4 < 301)
				{
					GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.PlayerLevelAtHour, num4.ToString(), Progression.Level);
				}
			}
		}
		totalTimePlayed = num;
		if (ChunkObserver != null)
		{
			ChunkObserver.SetPosition(GetPosition());
			if (ChunkObserver.mapDatabase != null && IsSpawned() && chunkPosAddedEntityTo != lastChunkPos)
			{
				lastChunkPos = chunkPosAddedEntityTo;
				ChunkObserver.mapDatabase.Add(chunkPosAddedEntityTo, world);
			}
		}
		if (emodel.avatarController != null)
		{
			emodel.avatarController.SetHeadAngles(rotation.x, 0f);
			if (inventory.holdingItem != null && inventory.holdingItem.CanHold())
			{
				emodel.avatarController.SetArmsAngles(rotation.x + 90f, 0f);
			}
			else
			{
				emodel.avatarController.SetArmsAngles(0f, 0f);
			}
		}
		if (!IsDead())
		{
			currentLife += Time.deltaTime / 60f;
			if (currentLife > longestLife)
			{
				longestLife = currentLife;
				if ((int)longestLife > longestLifeLived)
				{
					longestLifeLived = (int)longestLife;
					if (this is EntityPlayerLocal)
					{
						QuestEventManager.Current.TimeSurvived(longestLifeLived);
						PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.LongestLifeLived, longestLifeLived);
					}
				}
			}
		}
		HasUpdated = true;
	}

	public override float GetSpeedModifier()
	{
		float num2;
		float num;
		if (base.IsCrouching)
		{
			if (MovementRunning)
			{
				num = Constants.cPlayerSpeedModifierWalking;
				num2 = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, num, this);
			}
			else
			{
				num = Constants.cPlayerSpeedModifierCrouching;
				num2 = EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, num, this);
			}
		}
		else if (MovementRunning)
		{
			num = Constants.cPlayerSpeedModifierRunning;
			num2 = EffectManager.GetValue(PassiveEffects.RunSpeed, null, num, this);
		}
		else
		{
			num = Constants.cPlayerSpeedModifierWalking;
			num2 = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, num, this);
		}
		num *= 0.35f;
		if (num2 < num)
		{
			num2 = num;
		}
		return num2;
	}

	public override Vector3 GetVelocityPerSecond()
	{
		if ((bool)AttachedToEntity)
		{
			return AttachedToEntity.GetVelocityPerSecond();
		}
		return averageVel * 20f;
	}

	public Color GetTeamColor()
	{
		return Constants.cTeamColors[TeamNumber];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpMotion()
	{
		base.StartJumpMotion();
		motion.y = EffectManager.GetValue(PassiveEffects.JumpStrength, null, jumpStrength, this) * base.Stats.Stamina.ValuePercent;
	}

	public override void OnUpdateLive()
	{
		base.Stats.Stamina.RegenerationAmount = 0f;
		base.OnUpdateLive();
		GetEntitySenses().Clear();
		CheckSleeperTriggers();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CheckSleeperTriggers()
	{
		if (!world.IsRemote() && IsAlive())
		{
			world.CheckSleeperVolumeTouching(this);
			world.CheckTriggerVolumeTrigger(this);
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
	{
		if (GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled))
		{
			return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
		}
		return 0;
	}

	public override void CheckDismember(ref DamageResponse _dmResponse, float damagePer)
	{
	}

	public override void PlayOneShot(string clipName, bool sound_in_head = false, bool serverSignalOnly = false, bool isUnique = false, AnimationEvent _animEvent = null)
	{
		if (!isSpectator || sound_in_head)
		{
			base.PlayOneShot(clipName, sound_in_head, serverSignalOnly, isUnique, _animEvent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetSoundHurt(DamageSource _damageSource, int _damageStrength)
	{
		string result;
		if (_damageSource.GetDamageType() == EnumDamageTypes.Suffocation && (result = GetSoundDrownPain()) != null)
		{
			return result;
		}
		if (_damageStrength > 15 || GetSoundHurtSmall() == null)
		{
			return GetSoundHurt();
		}
		return GetSoundHurtSmall();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetSoundDeath(DamageSource _damageSource)
	{
		if (soundDrownDeath == null || _damageSource.GetDamageType() != EnumDamageTypes.Suffocation)
		{
			return base.GetSoundDeath(_damageSource);
		}
		return soundDrownDeath;
	}

	public bool CanHeal()
	{
		if (Health > 0)
		{
			return Health < GetMaxHealth();
		}
		return false;
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override bool IsSavedToNetwork()
	{
		return false;
	}

	public virtual void EnableCamera(bool _b)
	{
	}

	public virtual void Respawn(RespawnType _reason)
	{
		lastRespawnReason = _reason;
		emodel.DisableRagdoll(isSetAlive: true);
		InitBreadcrumbs();
	}

	public virtual void Teleport(Vector3 _pos, float _dir = float.MinValue)
	{
		if ((bool)AttachedToEntity)
		{
			AttachedToEntity.SetPosition(_pos);
		}
		else
		{
			SetPosition(_pos);
			if (_dir > -999999f)
			{
				SetRotation(new Vector3(0f, _dir, 0f));
			}
		}
		GameEventManager.Current.HandleForceBossDespawn(this);
		Respawn(RespawnType.Teleport);
		InvokeTeleportDelegates();
	}

	public void InvokeTeleportDelegates()
	{
		this.PlayerTeleportedDelegates?.Invoke();
	}

	public virtual void BeforePlayerRespawn(RespawnType _type)
	{
	}

	public virtual void AfterPlayerRespawn(RespawnType _type)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onSpawnStateChanged()
	{
		base.onSpawnStateChanged();
		SetVisible(Spawned);
		if (!Spawned)
		{
			return;
		}
		SpawnedTicks = 0;
		switch (lastRespawnReason)
		{
		case RespawnType.NewGame:
		case RespawnType.Died:
		case RespawnType.EnterMultiplayer:
			if (!world.IsRemote() && !world.IsEditor() && IsSafeZoneActive())
			{
				world.LockAreaMasterChunksAround(World.worldToBlockPos(GetPosition()), world.worldTime + (ulong)(GamePrefs.GetInt(EnumGamePrefs.PlayerSafeZoneHours) * 1000));
			}
			break;
		}
		if (lastRespawnReason != RespawnType.Teleport)
		{
			lastRespawnReason = RespawnType.Unknown;
		}
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			Transform modelTransformParent = emodel.GetModelTransformParent();
			attachedModelPos = modelTransformParent.localPosition;
			modelTransformParent.localPosition = Vector3.zero;
		}
		return slot;
	}

	public override void Detach()
	{
		base.Detach();
		emodel.GetModelTransformParent().localPosition = attachedModelPos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onNewPrefabEntered(PrefabInstance _prefabInstance)
	{
		if (_prefabInstance != null && _prefabInstance.prefab.bTraderArea && this is EntityPlayerLocal entityPlayerLocal)
		{
			Waypoint waypoint = new Waypoint();
			waypoint.pos = World.worldToBlockPos(_prefabInstance.boundingBoxPosition + _prefabInstance.boundingBoxSize / 2);
			waypoint.icon = "ui_game_symbol_map_trader";
			waypoint.name.Update(_prefabInstance.prefab.PrefabName, PlatformManager.MultiPlatform.User.PlatformUserId);
			waypoint.ownerId = null;
			waypoint.lastKnownPositionEntityId = -1;
			waypoint.bIsAutoWaypoint = true;
			waypoint.bUsingLocalizationId = true;
			if (!entityPlayerLocal.Waypoints.ContainsWaypoint(waypoint))
			{
				NavObject navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos, waypoint.icon, hiddenOnCompass: true);
				navObject.UseOverrideColor = true;
				navObject.OverrideColor = Color.white;
				navObject.IsActive = false;
				navObject.name = waypoint.name.Text;
				navObject.usingLocalizationId = true;
				waypoint.navObject = navObject;
				entityPlayerLocal.Waypoints.Collection.Add(waypoint);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpSwimMotion()
	{
		motion.y += 0.04f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	public override int GetLayerForMapIcon()
	{
		return 19;
	}

	public override bool CanMapIconBeSelected()
	{
		return GameStats.GetBool(EnumGameStats.IsSpawnNearOtherPlayer);
	}

	public override bool IsDrawMapIcon()
	{
		if (IsSpawned())
		{
			if ((!IsFriendOfLocalPlayer || !GameStats.GetBool(EnumGameStats.ShowFriendPlayerOnMap)) && !GameStats.GetBool(EnumGameStats.ShowAllPlayersOnMap))
			{
				return IsInPartyOfLocalPlayer;
			}
			return true;
		}
		return false;
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}

	public override Vector3 GetMapIconScale()
	{
		return new Vector3(1.5f, 1.5f, 1.5f);
	}

	public override bool IsClientControlled()
	{
		return true;
	}

	public bool IsFriendsWith(EntityPlayer _other)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
		PersistentPlayerData playerDataFromEntityID2 = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_other.entityId);
		if (playerDataFromEntityID != null && playerDataFromEntityID2 != null && playerDataFromEntityID2.ACL != null && playerDataFromEntityID2.ACL.Contains(playerDataFromEntityID.PrimaryId))
		{
			return true;
		}
		return false;
	}

	public bool IsSafeZoneActive()
	{
		if (Progression.Level <= GamePrefs.GetInt(EnumGamePrefs.PlayerSafeZoneLevel) && spawnPoints.Count == 0)
		{
			return true;
		}
		return false;
	}

	public override void OnEntityUnload()
	{
		if (!world.IsEditor() && prefab != null)
		{
			world.triggerManager.RemovePlayer(prefab, entityId);
		}
		base.OnEntityUnload();
		ChunkObserver = null;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		SpawnedTicks++;
		Vector3 vector = position - averagVelLastPos;
		averagVelLastPos = position;
		if (vector.sqrMagnitude < 25f)
		{
			averageVel = averageVel * 0.7f + vector * 0.3f;
		}
		if (Health <= 0)
		{
			lastRespawnReason = RespawnType.Died;
			List<Transform> found = new List<Transform>();
			GameUtils.FindDeepChildWithPartialName(base.transform, "temp_Projectile", ref found);
			for (int i = 0; i < found.Count; i++)
			{
				UnityEngine.Object.Destroy(found[i].gameObject);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Vector3 vector2 = position;
			if ((vector2 - breadcrumbLastPos).sqrMagnitude >= 0.9025f)
			{
				breadcrumbLastPos = vector2;
				breadcrumbIndex = (breadcrumbIndex + 1) & 0x1F;
				breadcrumbs[breadcrumbIndex] = vector2;
			}
			Stealth.TickServer();
		}
		else if (!isEntityRemote)
		{
			Stealth.TickLocalClient();
		}
		PrefabTick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabTick()
	{
		if (!(Time.time - lastTimePrefabChecked > 1f))
		{
			return;
		}
		lastTimePrefabChecked = Time.time;
		PrefabInstance pOIAtPosition = world.GetPOIAtPosition(position);
		if (pOIAtPosition != prefab)
		{
			if (!world.IsEditor())
			{
				if (prefab != null)
				{
					world.triggerManager.RemovePlayer(prefab, entityId);
				}
				if (pOIAtPosition != null)
				{
					world.triggerManager.AddPrefabData(pOIAtPosition, entityId);
				}
			}
			prefab = pOIAtPosition;
			prefabTimeIn = Time.time;
			prefabInfoEntered = false;
			onNewPrefabEntered(prefab);
		}
		if (prefab != null && !prefabInfoEntered && !world.IsEditor())
		{
			if (this is EntityPlayerLocal)
			{
				if (prefab.IsWithinInfoArea(position))
				{
					if (prefab.prefab.InfoVolumes.Count > 0 || prefab.prefab.DifficultyTier >= 0)
					{
						enteredPrefab = prefab;
					}
					prefabInfoEntered = true;
				}
			}
			else
			{
				prefabInfoEntered = true;
			}
		}
		Vector3i blockPosition = GetBlockPosition();
		IsInTrader = world.GetTraderAreaAt(blockPosition) != null;
		if (TwitchEnabled || HasTwitchMember())
		{
			TwitchSafe = !world.CanPlaceBlockAt(blockPosition, null) || IsInTrader;
		}
		else if (twitchSafe)
		{
			TwitchSafe = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitBreadcrumbs()
	{
		breadcrumbs.Fill(position);
	}

	public Vector3 GetBreadcrumbPos(float distance)
	{
		int num = (int)(distance + 0.5f);
		int num2 = breadcrumbIndex;
		num2 = ((num < 31) ? (num2 - num) : (num2 + 1));
		return breadcrumbs[num2 & 0x1F];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
		if (this is EntityPlayerLocal && !isSpectator)
		{
			base.updateStepSound(_distX, _distZ, _rotYDelta);
		}
	}

	public override float GetBlockDamageScale()
	{
		return (float)GameStats.GetInt(EnumGameStats.BlockDamagePlayer) * 0.01f;
	}

	public override void SetDamagedTarget(EntityAlive _attackTarget)
	{
		base.SetDamagedTarget(_attackTarget);
		if (_attackTarget is EntityEnemy)
		{
			LastZombieAttackTime = world.worldTime;
		}
		IsBloodMoonDead = false;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (Spawned && --visiblityCheckTicks <= 0)
		{
			visiblityCheckTicks = 5;
			int num = Utils.FastMin(12, GameUtils.GetViewDistance()) * 16;
			num--;
			bModelVisible = _distanceSqr < (float)(num * num);
			if (!IsDead() && GetDeathTime() == 0)
			{
				SetVisible(bModelVisible);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _isVisible)
	{
		if (isSpectator)
		{
			emodel.SetVisible(_bVisible: false);
		}
		else
		{
			emodel.SetVisible(_isVisible, !world.IsRemote());
		}
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		base.Kill(_dmResponse);
		currentLife = 0f;
	}

	public virtual void OnHUD()
	{
	}

	public void ServerNetSendRangeCheckedDamage(Vector3 _origin, float _maxRange, DamageSourceEntity _damageSource, int _strength, bool _isCritical, List<string> _buffActions, string _buffActionsContext, ParticleEffect particleEffect)
	{
		NetPackageRangeCheckDamageEntity package = NetPackageManager.GetPackage<NetPackageRangeCheckDamageEntity>().Setup(entityId, _origin, _maxRange, _damageSource, _strength, _isCritical, _buffActions, _buffActionsContext, particleEffect);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
	}

	public override AttachedToEntitySlotExit FindValidExitPosition(List<AttachedToEntitySlotExit> candidatePositions)
	{
		lastVehiclePositionOnDismount = position;
		timeOfVehicleDismount = Time.time;
		forcedDetach = false;
		return base.FindValidExitPosition(candidatePositions);
	}

	public override void CheckPosition()
	{
		base.CheckPosition();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || IsFlyMode.Value || !Spawned || position.y >= 0f)
		{
			return;
		}
		if (AttachedToEntity != null)
		{
			Detach();
			forcedDetach = true;
			return;
		}
		Log.Out($"[FELLTHROUGHWORLD] Player is under the world, starting teleport respawn from {position}");
		Vector3 fallingSavePosition = GetFallingSavePosition();
		if (isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(fallingSavePosition, null, _onlyIfNotFlying: true), _onlyClientsAttachedToAnEntity: false, entityId);
			return;
		}
		Log.Out($"[FELLTHROUGHWORLD] Attempting teleport to {fallingSavePosition}");
		Teleport(fallingSavePosition);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetFallingSavePosition()
	{
		if (!forcedDetach && Time.time - timeOfVehicleDismount < vehicleTeleportThresholdSeconds)
		{
			return lastVehiclePositionOnDismount;
		}
		Vector3 result = position;
		IChunk chunkFromWorldPos = world.GetChunkFromWorldPos((int)result.x, (int)result.z);
		if (chunkFromWorldPos == null || chunkFromWorldPos.IsEmpty())
		{
			Log.Out($"[FELLTHROUGHWORLD] GetFallingSavePosition - CurrentChunk {chunkFromWorldPos}");
			IChunk chunk = null;
			Vector2 vector = new Vector2(result.x, result.z);
			float num = float.PositiveInfinity;
			foreach (long item in ChunkObserver.chunksAround.list)
			{
				IChunk chunkSync = world.GetChunkSync(item);
				if (chunkSync != null && !chunkSync.IsEmpty())
				{
					Vector3i worldPos = chunkSync.GetWorldPos();
					float sqrMagnitude = (new Vector2((float)worldPos.x + 8f, (float)worldPos.z + 8f) - vector).sqrMagnitude;
					if (chunk == null || !(sqrMagnitude >= num))
					{
						chunk = chunkSync;
						num = sqrMagnitude;
					}
				}
			}
			Log.Out($"[FELLTHROUGHWORLD] GetFallingSavePosition - closestChunk {chunk}");
			if (chunk != null)
			{
				Vector3i worldPos2 = chunk.GetWorldPos();
				result.x = Math.Clamp(result.x, (float)worldPos2.x + 0.5f, (float)(worldPos2.x + 16) - 1f);
				result.z = Math.Clamp(result.z, (float)worldPos2.z + 0.5f, (float)(worldPos2.z + 16) - 1f);
			}
		}
		result.y = (float)(int)GameManager.Instance.World.GetTerrainHeight((int)result.x, (int)result.z) + 0.5f;
		return result;
	}

	public override bool FriendlyFireCheck(EntityAlive other)
	{
		bool result = true;
		try
		{
			EntityPlayer entityPlayer = other as EntityPlayer;
			if (entityPlayer != null)
			{
				if (entityId == entityPlayer.entityId)
				{
					return true;
				}
				int num = GameStats.GetInt(EnumGameStats.PlayerKillingMode);
				switch (num)
				{
				case 0:
					result = false;
					break;
				case 1:
				case 2:
				{
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
					PersistentPlayerData playerDataFromEntityID2 = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId);
					if (playerDataFromEntityID != null && playerDataFromEntityID2 != null)
					{
						bool num2 = playerDataFromEntityID2.ACL != null && playerDataFromEntityID2.ACL.Contains(playerDataFromEntityID.PrimaryId);
						bool flag = Party != null && Party.MemberList.Contains(entityPlayer);
						result = (num2 || flag) ^ (num == 2);
					}
					break;
				}
				}
			}
		}
		catch
		{
			result = true;
		}
		return result;
	}

	public bool IsInParty()
	{
		return Party != null;
	}

	public bool IsPartyLead()
	{
		if (Party != null)
		{
			return Party.Leader == this;
		}
		return false;
	}

	public bool HasTwitchMember()
	{
		if (Party != null)
		{
			return Party.HasTwitchMember;
		}
		return false;
	}

	public TwitchVoteLockTypes HasTwitchVoteLockMember()
	{
		if (Party != null)
		{
			return Party.HasTwitchVoteLock;
		}
		return TwitchVoteLockTypes.None;
	}

	public void CreateParty()
	{
		Party = new Party();
		Party.AddPlayer(this);
		Party.LeaderIndex = 0;
		HandleOnPartyJoined();
	}

	public void LeaveParty()
	{
		Party oldParty = Party;
		if (Party != null)
		{
			Party.MemberList.Remove(this);
			if (this is EntityPlayerLocal)
			{
				for (int i = 0; i < Party.MemberList.Count; i++)
				{
					if (Party.MemberList[i].NavObject != null)
					{
						Party.MemberList[i].NavObject.UseOverrideColor = false;
					}
				}
			}
		}
		Party = null;
		HandleOnPartyLeave(oldParty);
	}

	public void RemovePartyInvite(int playerEntityID)
	{
		EntityPlayer item = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		if (partyInvites.Contains(item))
		{
			partyInvites.Remove(item);
		}
	}

	public void RemoveAllPartyInvites()
	{
		partyInvites.Clear();
	}

	public void AddPartyInvite(int playerEntityID)
	{
		EntityPlayer item = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		if (!partyInvites.Contains(item))
		{
			partyInvites.Add(item);
			if (this.InvitedToParty != null)
			{
				this.InvitedToParty(null, this);
			}
		}
	}

	public bool HasPendingPartyInvite(int playerEntityID)
	{
		EntityPlayer item = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		return partyInvites.Contains(item);
	}

	public void HandleOnPartyJoined()
	{
		this.PartyJoined?.Invoke(party, this);
	}

	public void HandleOnPartyChanged()
	{
		this.PartyChanged?.Invoke(party, this);
	}

	public void HandleOnPartyLeave(Party _oldParty)
	{
		this.PartyLeave?.Invoke(_oldParty, this);
	}

	public void PartyDisconnect()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Party.ServerHandleDisconnectParty(this);
		}
	}

	public void SetPrefabsAroundNear(Dictionary<int, PrefabInstance> _prefabsAround)
	{
		prefabsAroundNear.Clear();
		foreach (KeyValuePair<int, PrefabInstance> item in _prefabsAround)
		{
			prefabsAroundNear.Add(item.Key, item.Value);
		}
	}

	public Dictionary<int, PrefabInstance> GetPrefabsAroundNear()
	{
		return prefabsAroundNear;
	}

	public void AddKillXP(EntityAlive killedEntity, float xpModifier = 1f)
	{
		int experienceValue = EntityClass.list[killedEntity.entityClass].ExperienceValue;
		experienceValue = (int)EffectManager.GetValue(PassiveEffects.ExperienceGain, killedEntity.inventory.holdingItemItemValue, experienceValue, killedEntity);
		if (xpModifier != 1f)
		{
			experienceValue = (int)((float)experienceValue * xpModifier + 0.5f);
		}
		if (IsInParty())
		{
			experienceValue = Party.GetPartyXP(this, experienceValue);
		}
		if (!isEntityRemote)
		{
			Progression.AddLevelExp(experienceValue, "_xpFromKill", Progression.XPTypes.Kill, useBonus: true, notifyUI: true, entityId);
			bPlayerStatsChanged = true;
		}
		else
		{
			NetPackageEntityAddExpClient package = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityId, experienceValue, Progression.XPTypes.Kill);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
		}
		if (xpModifier == 1f)
		{
			GameManager.Instance.SharedKillServer(killedEntity.entityId, entityId, xpModifier);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClientDeath(Vector3i attackPos)
	{
		base.HandleClientDeath(attackPos);
		TwitchManager.Current.CheckKiller(this, entityThatKilledMe, attackPos);
		switch ((EnumDeathPenalty)GameStats.GetInt(EnumGameStats.DeathPenalty))
		{
		case EnumDeathPenalty.None:
			GameEventManager.Current.HandleAction("game_on_death_none", this, this, twitchActivated: false);
			break;
		case EnumDeathPenalty.XPOnly:
			GameEventManager.Current.HandleAction("game_on_death_default", this, this, twitchActivated: false);
			break;
		case EnumDeathPenalty.Injured:
			GameEventManager.Current.HandleAction("game_on_death_injured", this, this, twitchActivated: false);
			break;
		case EnumDeathPenalty.Permadeath:
			GameEventManager.Current.HandleAction("game_on_death_permanent", this, this, twitchActivated: false);
			break;
		}
	}

	public void HandleTwitchActionsTempEnabled(TwitchActionsStates newState)
	{
		if (twitchActionsEnabled != TwitchActionsStates.Disabled)
		{
			TwitchActionsEnabled = newState;
		}
	}

	public bool IsReloadCancelled()
	{
		if (inventory.holdingItemData.actionData != null)
		{
			foreach (ItemActionData actionDatum in inventory.holdingItemData.actionData)
			{
				if (actionDatum is ItemActionRanged.ItemActionDataRanged { isReloadCancelled: not false })
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetLaserSightData(bool _laserSightActive, Vector3 _laserSightPosition)
	{
		if (_laserSightActive != laserSightActive || !((laserSightPosition - _laserSightPosition).sqrMagnitude < 0.0016f))
		{
			laserSightPosition = _laserSightPosition;
			laserSightActive = _laserSightActive;
			if (world.entityDistributer != null)
			{
				world.entityDistributer.SendPacketToTrackedPlayers(entityId, (world.GetPrimaryPlayer() != null) ? world.GetPrimaryPlayer().entityId : (-1), NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(entityId, _laserSightActive, _laserSightPosition), _inRangeOnly: true);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(entityId, _laserSightActive, _laserSightPosition));
			}
		}
	}
}
