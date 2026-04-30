using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using GamePath;
using Platform;
using UAI;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAlive : Entity
{
	public enum JumpState
	{
		Off,
		Climb,
		Leap,
		Air,
		Land,
		SwimStart,
		Swim
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class FallBehavior
	{
		public enum Op
		{
			None,
			Land,
			LandLow,
			LandHard,
			Stumble,
			Ragdoll
		}

		public string Name;

		public readonly Op ResponseOp;

		public readonly FloatRange Height;

		public readonly float Weight;

		public readonly FloatRange RagePer;

		public readonly FloatRange RageTime;

		public readonly IntRange Difficulty;

		public FallBehavior(string name, Op type, FloatRange height, float weight, FloatRange ragePer, FloatRange rageTime, IntRange difficulty)
		{
			Name = name;
			ResponseOp = type;
			Height = height;
			Weight = weight;
			RagePer = ragePer;
			RageTime = rageTime;
			Difficulty = difficulty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class DestroyBlockBehavior
	{
		public enum Op
		{
			None,
			Ragdoll,
			Stumble
		}

		public string Name;

		public readonly Op ResponseOp;

		public readonly float Weight;

		public readonly FloatRange RagePer;

		public readonly FloatRange RageTime;

		public readonly IntRange Difficulty = new IntRange(int.MinValue, int.MaxValue);

		public DestroyBlockBehavior(string name, Op type, float weight, FloatRange ragePer, FloatRange rageTime, IntRange difficulty)
		{
			Name = name;
			ResponseOp = type;
			Weight = weight;
			RagePer = ragePer;
			RageTime = rageTime;
			Difficulty = difficulty;
		}
	}

	public enum EnumApproachState
	{
		Ok,
		TooFarAway,
		BlockedByWorldMesh,
		BlockedByEntity,
		Unknown
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct WeightBehavior
	{
		public float weight;

		public int index;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ImpactData
	{
		public int count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NetworkStatChange
	{
		public EntityNetworkStats m_NetworkStats;

		public EntityNetworkHoldingData m_HoldingData;
	}

	public class EntityNetworkHoldingData
	{
		public ItemStack m_HoldingItemStack;

		public byte m_HoldingItemIndex;
	}

	public class EntityNetworkStats
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int experience;

		[PublicizedFrom(EAccessModifier.Private)]
		public int level;

		[PublicizedFrom(EAccessModifier.Private)]
		public int killed;

		[PublicizedFrom(EAccessModifier.Private)]
		public int killedZombies;

		[PublicizedFrom(EAccessModifier.Private)]
		public int killedPlayers;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemStack holdingItemStack;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte holdingItemIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public int deathHealth;

		[PublicizedFrom(EAccessModifier.Private)]
		public int teamNumber;

		[PublicizedFrom(EAccessModifier.Private)]
		public Equipment equipment;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasProgression;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] progressionsData;

		[PublicizedFrom(EAccessModifier.Private)]
		public int attachedToEntityId;

		[PublicizedFrom(EAccessModifier.Private)]
		public string entityName;

		[PublicizedFrom(EAccessModifier.Private)]
		public float distanceWalked;

		[PublicizedFrom(EAccessModifier.Private)]
		public uint totalItemsCrafted;

		[PublicizedFrom(EAccessModifier.Private)]
		public float longestLife;

		[PublicizedFrom(EAccessModifier.Private)]
		public float currentLife;

		[PublicizedFrom(EAccessModifier.Private)]
		public float totalTimePlayed;

		[PublicizedFrom(EAccessModifier.Private)]
		public int vehiclePose;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isSpectator;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPlayer;

		public void FillFromEntity(EntityAlive _entity)
		{
			killed = _entity.Died;
			holdingItemStack = _entity.inventory.holdingItemStack;
			holdingItemIndex = (byte)_entity.inventory.holdingItemIdx;
			deathHealth = _entity.DeathHealth;
			teamNumber = _entity.TeamNumber;
			equipment = _entity.equipment;
			if (GameManager.Instance.World.GetPrimaryPlayer() == _entity)
			{
				_entity.inventory.TurnOffLightFlares();
			}
			if (_entity.Progression != null && _entity.Progression.bProgressionStatsChanged)
			{
				_entity.Progression.bProgressionStatsChanged = false;
				hasProgression = true;
				progressionsData = _entity.Progression.ToBytes();
			}
			attachedToEntityId = ((_entity.AttachedToEntity != null) ? _entity.AttachedToEntity.entityId : (-1));
			entityName = _entity.EntityName;
			EntityPlayer entityPlayer = _entity as EntityPlayer;
			if (entityPlayer != null)
			{
				isPlayer = true;
				killedPlayers = _entity.KilledPlayers;
				killedZombies = _entity.KilledZombies;
				experience = entityPlayer.Progression.ExpToNextLevel;
				level = entityPlayer.Progression.Level;
				totalItemsCrafted = entityPlayer.totalItemsCrafted;
				distanceWalked = entityPlayer.distanceWalked;
				longestLife = entityPlayer.longestLife;
				currentLife = entityPlayer.currentLife;
				totalTimePlayed = entityPlayer.totalTimePlayed;
				vehiclePose = entityPlayer.GetVehicleAnimation();
				isSpectator = entityPlayer.IsSpectator;
			}
			else
			{
				isPlayer = false;
				experience = 0;
				level = 1;
				distanceWalked = 0f;
				totalItemsCrafted = 0u;
				longestLife = 0f;
				currentLife = 0f;
				totalTimePlayed = 0f;
			}
		}

		public void ToEntity(EntityAlive _entity)
		{
			_entity.Died = killed;
			_entity.DeathHealth = deathHealth;
			_entity.TeamNumber = teamNumber;
			_entity.inventory.bResetLightLevelWhenChanged = true;
			if (!_entity.inventory.GetItem(holdingItemIndex).Equals(holdingItemStack))
			{
				_entity.inventory.SetItem(holdingItemIndex, holdingItemStack);
				_entity.inventory.ForceHoldingItemUpdate();
			}
			if (_entity.inventory.holdingItemIdx != holdingItemIndex)
			{
				_entity.inventory.SetHoldingItemIdxNoHolsterTime(holdingItemIndex);
			}
			_entity.equipment.Apply(equipment, isLocal: false);
			if (hasProgression)
			{
				_entity.Progression = Progression.FromBytes(progressionsData, _entity);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _entity.Progression != null)
				{
					_entity.Progression.bProgressionStatsChanged = true;
				}
			}
			_entity.SetEntityName(entityName);
			EntityPlayer entityPlayer = _entity as EntityPlayer;
			if (entityPlayer != null && isPlayer)
			{
				if (_entity.NavObject != null)
				{
					_entity.NavObject.name = entityName;
				}
				_entity.KilledZombies = killedZombies;
				_entity.KilledPlayers = killedPlayers;
				entityPlayer.Progression.ExpToNextLevel = experience;
				entityPlayer.Progression.Level = level;
				entityPlayer.totalItemsCrafted = totalItemsCrafted;
				entityPlayer.distanceWalked = distanceWalked;
				entityPlayer.longestLife = longestLife;
				entityPlayer.currentLife = currentLife;
				entityPlayer.totalTimePlayed = totalTimePlayed;
				entityPlayer.SetVehiclePoseMode(vehiclePose);
				entityPlayer.IsSpectator = isSpectator;
			}
		}

		public void read(PooledBinaryReader _reader)
		{
			killed = _reader.ReadInt32();
			holdingItemStack = new ItemStack();
			holdingItemStack.Read(_reader);
			holdingItemIndex = _reader.ReadByte();
			deathHealth = _reader.ReadInt32();
			teamNumber = _reader.ReadByte();
			equipment = Equipment.Read(_reader);
			attachedToEntityId = _reader.ReadInt32();
			entityName = _reader.ReadString();
			isPlayer = _reader.ReadBoolean();
			if (isPlayer)
			{
				killedZombies = _reader.ReadInt32();
				killedPlayers = _reader.ReadInt32();
				experience = _reader.ReadInt32();
				level = _reader.ReadInt32();
				totalItemsCrafted = _reader.ReadUInt32();
				distanceWalked = _reader.ReadSingle();
				longestLife = _reader.ReadSingle();
				currentLife = _reader.ReadSingle();
				totalTimePlayed = _reader.ReadSingle();
				vehiclePose = _reader.ReadInt32();
				isSpectator = _reader.ReadBoolean();
			}
			hasProgression = _reader.ReadBoolean();
			if (hasProgression)
			{
				int num = _reader.ReadInt16();
				progressionsData = new byte[num];
				_reader.Read(progressionsData, 0, num);
			}
		}

		public void write(PooledBinaryWriter _writer)
		{
			_writer.Write(killed);
			holdingItemStack.Write(_writer);
			_writer.Write(holdingItemIndex);
			_writer.Write(deathHealth);
			_writer.Write((byte)teamNumber);
			equipment.Write(_writer);
			_writer.Write(attachedToEntityId);
			_writer.Write(entityName);
			_writer.Write(isPlayer);
			if (isPlayer)
			{
				_writer.Write(killedZombies);
				_writer.Write(killedPlayers);
				_writer.Write(experience);
				_writer.Write(level);
				_writer.Write(totalItemsCrafted);
				_writer.Write(distanceWalked);
				_writer.Write(longestLife);
				_writer.Write(currentLife);
				_writer.Write(totalTimePlayed);
				_writer.Write(vehiclePose);
				_writer.Write(isSpectator);
			}
			_writer.Write(hasProgression);
			if (hasProgression)
			{
				_writer.Write((short)progressionsData.Length);
				_writer.Write(progressionsData, 0, progressionsData.Length);
			}
		}

		public void SetName(string name)
		{
			entityName = name;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTraderTeleportCheckTime = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageImmunityOnRespawnSeconds = 1f;

	public static readonly FastTags<TagGroup.Global> DistractionResistanceWithTargetTags = FastTags<TagGroup.Global>.GetTag("with_target");

	public static readonly int FeralTagBit = FastTags<TagGroup.Global>.GetBit("feral");

	public static readonly int FallingBuffTagBit = FastTags<TagGroup.Global>.GetBit("buffPlayerFallingDamage");

	public static readonly FastTags<TagGroup.Global> StanceTagCrouching = FastTags<TagGroup.Global>.GetTag("crouching");

	public static readonly FastTags<TagGroup.Global> StanceTagStanding = FastTags<TagGroup.Global>.GetTag("standing");

	public static readonly FastTags<TagGroup.Global> MovementTagIdle = FastTags<TagGroup.Global>.GetTag("idle");

	public static readonly FastTags<TagGroup.Global> MovementTagWalking = FastTags<TagGroup.Global>.GetTag("walking");

	public static readonly FastTags<TagGroup.Global> MovementTagRunning = FastTags<TagGroup.Global>.GetTag("running");

	public static readonly FastTags<TagGroup.Global> MovementTagFloating = FastTags<TagGroup.Global>.GetTag("floating");

	public static readonly FastTags<TagGroup.Global> MovementTagSwimming = FastTags<TagGroup.Global>.GetTag("swimming");

	public static readonly FastTags<TagGroup.Global> MovementTagSwimmingRun = FastTags<TagGroup.Global>.GetTag("swimmingRun");

	public static readonly FastTags<TagGroup.Global> MovementTagJumping = FastTags<TagGroup.Global>.GetTag("jumping");

	public static readonly FastTags<TagGroup.Global> MovementTagFalling = FastTags<TagGroup.Global>.GetTag("falling");

	public static readonly FastTags<TagGroup.Global> MovementTagClimbing = FastTags<TagGroup.Global>.GetTag("climbing");

	public static readonly FastTags<TagGroup.Global> MovementTagDriving = FastTags<TagGroup.Global>.GetTag("driving");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] moveSpeedRandomness = new float[6] { 0.2f, 1f, 1.1f, 1.2f, 1.35f, 1.5f };

	public const float CLIMB_LADDER_SPEED = 1234f;

	public static ulong HitDelay = 11000uL;

	public static float HitSoundDistance = 10f;

	public MinEventParams MinEventContext = new MinEventParams();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int equippingCount;

	public bool IsSleeper;

	public bool IsSleeping;

	public bool IsSleeperPassive;

	public bool SleeperSupressLivingSounds;

	public Vector3 SleeperSpawnPosition;

	public Vector3 SleeperSpawnLookDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float accumulatedDamageResisted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int pendingSleepTrigger = -1;

	public int lastSleeperPose;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sleeperLookDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float sleeperSightRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float sleeperViewAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 sightLightThreshold;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 sightWakeThresholdAtRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 sightGroanThresholdAtRange;

	public float sleeperNoiseToSense;

	public float sleeperNoiseToWake;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSnore;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGroan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGroanSilent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float sleeperNoiseToSenseSoundChance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int snoreGroanCD;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int kSnoreGroanMinCD = 20;

	public float noisePlayerDistance;

	public float noisePlayerVolume;

	public EntityPlayer noisePlayer;

	public EntityPlayer smellPlayer;

	public float smellPlayerDistance;

	public int smellPlayerTimeoutTicks;

	public EntityItem pendingDistraction;

	public float pendingDistractionDistanceSq;

	public EntityItem distraction;

	public float distractionResistance;

	public float distractionResistanceWithTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimGravityPer = 0.025f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimDragY = 0.91f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimDrag = 0.91f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAnimDelay = 6f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int jumpTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public JumpState jumpState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int jumpStateTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float jumpDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float jumpHeightDiff;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float jumpSwimDurationTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 jumpSwimMotion;

	public float jumpDelay;

	public float jumpMaxDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool jumpIsMoving;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksNoPlayerAdjacent;

	public int hasBeenAttackedTime;

	public float painHitsFelt;

	public float painResistPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int attackingTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive revengeEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int revengeTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool targetAlertChanged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastAliveTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool alertEnabled = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int alertTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string notAlertedId = "_notAlerted";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int notAlertDelayTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAlert;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 investigatePos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int investigatePositionTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInvestigateAlert;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAI;

	public EAIManager aiManager;

	public List<string> AIPackages;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Context utilityAIContext;

	public EntityPlayer aiClosestPlayer;

	public float aiClosestPlayerDistSq;

	public float aiActiveScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float aiActiveDelay;

	public bool IsBloodMoon;

	public bool IsFeral;

	public bool IsBreakingDoors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isBreakingBlocks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEating;

	public Vector3 ChaseReturnLocation;

	public bool IsScoutZombie;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityLookHelper lookHelper;

	public EntityMoveHelper moveHelper;

	public PathNavigate navigator;

	public bool bCanClimbLadders;

	public bool bCanClimbVertical;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive damagedTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive attackTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTargetTime;

	public EntityAlive attackTargetClient;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive attackTargetLast;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySeeCache seeCache;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCoordinates homePosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int maximumHomeDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float jumpMovementFactor = 0.02f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float landMovementFactor = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float jumpMotionYValue = 0.419f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stepSoundDistanceRemaining;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stepSoundRotYRemaining;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSwimDistance;

	public Inventory inventory;

	public Inventory saveInventory;

	public Equipment equipment;

	public Bag bag;

	public ChallengeJournal challengeJournal;

	public int ExperienceValue;

	public int deathUpdateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive entityThatKilledMe;

	public bool bPlayerStatsChanged;

	public bool bEntityAliveFlagsChanged;

	public bool bPlayerTwitchChanged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<EnumDamageSource, ulong> damageSourceTimeouts = new EnumDictionary<EnumDamageSource, ulong>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int traderTeleportStreak = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bJetpackWearing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bJetpackActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bParachuteWearing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAimingGun;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bMovementRunning;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCrouching;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bJumping;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bClimbing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int died;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int score;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int killedZombies;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int killedPlayers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int teamNumber;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string entityName = string.Empty;

	public string DebugNameInfo = string.Empty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int damageLocationBits;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bSpawned;

	public bool bReplicatedAlertFlag;

	public int vehiclePoseMode = -1;

	public byte factionId;

	public byte factionRank;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksToCheckSeenByPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSeenByPlayer;

	public DamageResponse RecordedDamage;

	public float moveSpeed;

	public float moveSpeedNight;

	public float moveSpeedAggro;

	public float moveSpeedAggroMax;

	public float moveSpeedPanic;

	public float moveSpeedPanicMax;

	public float swimSpeed;

	public Vector2 swimStrokeRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue handItem;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundSpawn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundSleeperGroan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundSleeperSnore;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundAlert;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundAttack;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundLiving;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundRandom;

	public string soundSense;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundGiveUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundStepType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundStamina;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundJump;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundLand;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundHurt;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundHurtSmall;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundDrownPain;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundDrownDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundWaterSurface;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int soundDelayTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int soundLivingID = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSoundRandomMaxDist = 20f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int soundAlertTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int soundRandomTicks;

	public int classMaxHealth;

	public int classMaxStamina;

	public int classMaxFood;

	public int classMaxWater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float weight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float pushFactor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxViewAngle;

	public float sightRangeBase;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float sightRange;

	public float senseScale;

	public int timeStayAfterDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue corpseBlockValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float corpseBlockChance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeoutDay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeoutNight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string particleOnDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string particleOnDestroy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityBedrollPositionList spawnPoints;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3i> droppedBackpackPositions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float speedModifier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 accumulatedRootMotion;

	public Vector3 moveDirection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMoveDirAbsolute;

	public Vector3 lookAtPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPosStandingOn;

	public BlockValue blockValueStandingOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool blockStandingOnChanged;

	public BiomeDefinition biomeStandingOn;

	public bool IsMale;

	public int crouchType;

	public float crouchBendPer;

	public float crouchBendPerTarget;

	public const int cWalkTypeSwim = -1;

	public const int cWalkTypeFat = 1;

	public const int cWalkTypeCripple = 5;

	public const int cWalkTypeCrouch = 8;

	public const int cWalkTypeBandit = 15;

	public const int cWalkTypeCrawlFirst = 20;

	public const int cWalkTypeCrawler = 21;

	public const int cWalkTypeSpider = 22;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int walkType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int walkTypeBeforeCrouch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string rightHandTransformName;

	public int pingToServer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ItemStack> itemsOnEnterGame = new List<ItemStack>();

	public Utils.EnumHitDirection lastHitDirection = Utils.EnumHitDirection.None;

	public Vector3 lastHitImpactDir = Vector3.zero;

	public Vector3 lastHitEntityFwd = Vector3.zero;

	public bool lastHitRanged;

	public float lastHitForce;

	public DamageResponse lastDamageResponse;

	public bool canDisintegrate;

	public bool isDisintegrated;

	public float CreationTimeSinceLevelLoad;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityStats entityStats;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float proneRefillRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float kneelRefillRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float proneRefillCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float kneelRefillCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int deathHealth;

	public BodyDamage bodyDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool stompsSpikes;

	public float OverrideSize = 1f;

	public float OverrideHeadSize = 1f;

	public float OverrideHeadDismemberScaleTime = 1.5f;

	public float OverridePitch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDancing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeTraderStationChecked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lerpForwardSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedForwardTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedForwardTargetStep = 1f;

	public EntityBuffs Buffs;

	public Progression Progression;

	public FastTags<TagGroup.Global> CurrentStanceTag = StanceTagStanding;

	public FastTags<TagGroup.Global> CurrentMovementTag = FastTags<TagGroup.Global>.none;

	public float renderFadeMax = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float renderFade;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float renderFadeTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<FallBehavior> fallBehaviors = new List<FallBehavior>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool disableFallBehaviorUntilOnGround;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<DestroyBlockBehavior> _destroyBlockBehaviors = new List<DestroyBlockBehavior>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicRagdollFlags _dynamicRagdoll;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _dynamicRagdollStunTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _dynamicRagdollRootMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3> _ragdollPositionsPrev = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3> _ragdollPositionsCur = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFirstTimeEquipmentReassigned = true;

	public bool CrouchingLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EModelBase.HeadStates currentHeadState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<WeightBehavior> weightBehaviorTemp = new List<WeightBehavior>();

	public static bool ShowDebugDisplayHit = false;

	public static float DebugDisplayHitSize = 0.005f;

	public static float DebugDisplayHitTime = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayHurtSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bBeenWounded;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int woundedStrength;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public DamageSource woundedDamageSource;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int despawnDelayCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDespawnWhenPlayerFar;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasOnGround = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float landWaterLevel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_addedToWorld;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int saveHoldingItemIdxBeforeAttach;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float impactSoundTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Transform, ImpactData> impacts = new Dictionary<Transform, ImpactData>();

	public const string cParticlePrefix = "Ptl_";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Transform> particles = new Dictionary<string, Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Transform> parts = new Dictionary<string, Transform>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<OwnedEntityData> ownedEntities = new List<OwnedEntityData>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetworkStatChange> networkStatsUpdateQueue = new List<NetworkStatChange>();

	public bool IsEquipping
	{
		get
		{
			return equippingCount > 0;
		}
		set
		{
			if (value)
			{
				equippingCount++;
			}
			else if (equippingCount > 0)
			{
				equippingCount--;
			}
		}
	}

	public bool IsDancing
	{
		get
		{
			return isDancing;
		}
		set
		{
			isDancing = value;
			if (value)
			{
				if (emodel != null && emodel.avatarController != null)
				{
					emodel.avatarController.UpdateInt("IsDancing", base.EntityClass.DanceTypeID);
				}
			}
			else if (emodel != null && emodel.avatarController != null)
			{
				emodel.avatarController.UpdateInt("IsDancing", 0);
			}
		}
	}

	public bool sleepingOrWakingUp => IsSleeping;

	public EntityStats Stats
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return entityStats;
		}
	}

	public virtual bool JetpackActive
	{
		get
		{
			return bJetpackActive;
		}
		set
		{
			if (value != bJetpackActive)
			{
				bJetpackActive = value;
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual bool JetpackWearing
	{
		get
		{
			return bJetpackWearing;
		}
		set
		{
			if (value != bJetpackWearing)
			{
				bJetpackWearing = value;
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual bool ParachuteWearing
	{
		get
		{
			return bParachuteWearing;
		}
		set
		{
			if (value != bParachuteWearing)
			{
				bParachuteWearing = value;
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual bool AimingGun
	{
		get
		{
			if (emodel.avatarController != null && emodel.avatarController.TryGetBool(AvatarController.isAimingHash, out bAimingGun))
			{
				return bAimingGun;
			}
			return false;
		}
		set
		{
			bool aimingGun = AimingGun;
			if (value != aimingGun)
			{
				if (emodel.avatarController != null)
				{
					emodel.avatarController.UpdateBool(AvatarController.isAimingHash, value);
				}
				UpdateCameraFOV(_bLerpPosition: true);
			}
			if (this is EntityPlayerLocal && inventory != null)
			{
				inventory.holdingItem.Actions[1]?.AimingSet(inventory.holdingItemData.actionData[1], value, aimingGun);
			}
			EntityPlayerLocal entityPlayerLocal = this as EntityPlayerLocal;
			if ((object)entityPlayerLocal != null && value)
			{
				entityPlayerLocal.StartTPCameraLockTimer();
			}
		}
	}

	public virtual bool MovementRunning
	{
		get
		{
			return bMovementRunning;
		}
		set
		{
			if (value != bMovementRunning)
			{
				bMovementRunning = value;
			}
		}
	}

	public virtual bool Crouching
	{
		get
		{
			return bCrouching;
		}
		set
		{
			if (value != bCrouching)
			{
				bCrouching = value;
				if (emodel.avatarController != null)
				{
					emodel.avatarController.SetCrouching(value);
				}
				CurrentStanceTag = (bCrouching ? StanceTagCrouching : StanceTagStanding);
				Buffs.SetCustomVar("_crouching", bCrouching ? 1 : 0);
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public bool IsCrouching
	{
		get
		{
			if (!Crouching)
			{
				return CrouchingLocked;
			}
			return true;
		}
	}

	public virtual bool Jumping
	{
		get
		{
			if (bJumping)
			{
				return EffectManager.GetValue(PassiveEffects.JumpStrength, null, 1f, this) != 0f;
			}
			return false;
		}
		set
		{
			if (value != bJumping)
			{
				bJumping = value;
				if (Jumping)
				{
					StartJump();
					CurrentMovementTag &= MovementTagIdle;
					CurrentMovementTag |= MovementTagJumping;
				}
				else
				{
					EndJump();
					CurrentMovementTag &= MovementTagJumping;
					bJumping = false;
				}
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public bool Climbing
	{
		get
		{
			return bClimbing;
		}
		set
		{
			if (value != bClimbing)
			{
				bClimbing = value;
				bPlayerStatsChanged |= !isEntityRemote;
				if (bClimbing)
				{
					CurrentMovementTag &= MovementTagIdle;
					CurrentMovementTag |= MovementTagClimbing;
				}
				else
				{
					CurrentMovementTag &= MovementTagClimbing;
				}
			}
		}
	}

	public virtual bool RightArmAnimationAttack
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationAttackPlaying();
			}
			return false;
		}
		set
		{
			if (emodel.avatarController != null && value && !emodel.avatarController.IsAnimationAttackPlaying())
			{
				emodel.avatarController.StartAnimationAttack();
			}
		}
	}

	public virtual bool RightArmAnimationUse
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationUsePlaying();
			}
			return false;
		}
		set
		{
			if (emodel.avatarController != null && value != emodel.avatarController.IsAnimationUsePlaying())
			{
				emodel.avatarController.StartAnimationUse();
			}
		}
	}

	public virtual bool SpecialAttack
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationSpecialAttackPlaying();
			}
			return false;
		}
		set
		{
			if (emodel.avatarController != null && value != emodel.avatarController.IsAnimationSpecialAttackPlaying())
			{
				bPlayerStatsChanged |= !isEntityRemote;
				emodel.avatarController.StartAnimationSpecialAttack(value, 0);
			}
		}
	}

	public virtual bool SpecialAttack2
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationSpecialAttack2Playing();
			}
			return false;
		}
		set
		{
			if (emodel.avatarController != null && value)
			{
				bPlayerStatsChanged |= !isEntityRemote;
				emodel.avatarController.StartAnimationSpecialAttack2();
			}
		}
	}

	public virtual bool Raging
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationRagingPlaying();
			}
			return false;
		}
		set
		{
			if (emodel.avatarController != null && value && !emodel.avatarController.IsAnimationRagingPlaying())
			{
				emodel.avatarController.StartAnimationRaging();
			}
		}
	}

	public virtual bool Electrocuted
	{
		get
		{
			if (emodel != null && emodel.avatarController != null)
			{
				return emodel.avatarController.GetAnimationElectrocuteRemaining() > 0f;
			}
			return false;
		}
		set
		{
			if (emodel != null && emodel.avatarController != null && value != emodel.avatarController.GetAnimationElectrocuteRemaining() > 0.4f)
			{
				bPlayerStatsChanged |= !isEntityRemote;
				if (value)
				{
					emodel.avatarController.StartAnimationElectrocute(0.6f);
					emodel.avatarController.Electrocute(enabled: true);
				}
			}
		}
	}

	public virtual bool HarvestingAnimation
	{
		get
		{
			if (emodel.avatarController != null)
			{
				return emodel.avatarController.IsAnimationHarvestingPlaying();
			}
			return false;
		}
		set
		{
			emodel.avatarController.UpdateBool("Harvesting", value);
		}
	}

	public bool IsEating
	{
		get
		{
			return m_isEating;
		}
		set
		{
			if (value == m_isEating)
			{
				return;
			}
			m_isEating = value;
			bPlayerStatsChanged |= !isEntityRemote;
			if (emodel != null && emodel.avatarController != null)
			{
				if (m_isEating)
				{
					emodel.avatarController.StartEating();
				}
				else
				{
					emodel.avatarController.StopEating();
				}
			}
		}
	}

	public virtual int Died
	{
		get
		{
			return died;
		}
		set
		{
			if (value != died)
			{
				died = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual int Score
	{
		get
		{
			return score;
		}
		set
		{
			if (value != score)
			{
				score = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual int KilledZombies
	{
		get
		{
			return killedZombies;
		}
		set
		{
			if (value != killedZombies)
			{
				killedZombies = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual int KilledPlayers
	{
		get
		{
			return killedPlayers;
		}
		set
		{
			if (value != killedPlayers)
			{
				killedPlayers = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual int TeamNumber
	{
		get
		{
			return teamNumber;
		}
		set
		{
			if (value != teamNumber)
			{
				teamNumber = value;
				bPlayerStatsChanged |= !isEntityRemote;
				if (!isEntityRemote)
				{
					GameManager.Instance.GameMessage(EnumGameMessages.ChangedTeam, this, null);
				}
			}
		}
	}

	public virtual string EntityName => entityName;

	public virtual int DeathHealth
	{
		get
		{
			return deathHealth;
		}
		set
		{
			if (value != deathHealth)
			{
				deathHealth = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual bool Spawned
	{
		get
		{
			return bSpawned;
		}
		set
		{
			if (value != bSpawned)
			{
				bSpawned = value;
				onSpawnStateChanged();
				bEntityAliveFlagsChanged |= !isEntityRemote;
			}
		}
	}

	public bool IsBreakingBlocks
	{
		get
		{
			return m_isBreakingBlocks;
		}
		set
		{
			if (value != m_isBreakingBlocks)
			{
				m_isBreakingBlocks = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
		}
	}

	public virtual EntityBedrollPositionList SpawnPoints => spawnPoints;

	public virtual int Health
	{
		get
		{
			return (int)Stats.Health.Value;
		}
		set
		{
			Stats.Health.Value = value;
		}
	}

	public virtual float Stamina
	{
		get
		{
			return Stats.Stamina.Value;
		}
		set
		{
			Stats.Stamina.Value = value;
		}
	}

	public virtual float Water
	{
		get
		{
			return Stats.Water.Value;
		}
		set
		{
			Stats.Water.Value = value;
		}
	}

	public virtual bool IsValidAimAssistSlowdownTarget => true;

	public virtual bool IsValidAimAssistSnapTarget => true;

	public virtual EModelBase.HeadStates CurrentHeadState
	{
		get
		{
			return currentHeadState;
		}
		set
		{
			if (value != currentHeadState)
			{
				currentHeadState = value;
				bPlayerStatsChanged |= !isEntityRemote;
			}
			emodel.ForceHeadState(value);
		}
	}

	public virtual float MaxVelocity => 5f;

	public virtual bool IsImmuneToLegDamage
	{
		get
		{
			EntityClass entityClass = EntityClass.list[base.entityClass];
			if (GetWalkType() != 21 && bodyDamage.HasLeftLeg && bodyDamage.HasRightLeg)
			{
				if (entityClass.LowerLegDismemberThreshold <= 0f)
				{
					return entityClass.UpperLegDismemberThreshold <= 0f;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasInvestigatePosition => investigatePositionTicks > 0;

	public Vector3 InvestigatePosition => investigatePos;

	public virtual bool IsAlert
	{
		get
		{
			if (isEntityRemote)
			{
				return bReplicatedAlertFlag;
			}
			return isAlert;
		}
	}

	public virtual bool IsRunning
	{
		get
		{
			if (!IsBloodMoon)
			{
				return world.IsDark();
			}
			return true;
		}
	}

	public int OwnedEntityCount => ownedEntities.Count;

	public void BeginDynamicRagdoll(DynamicRagdollFlags flags, FloatRange stunTime)
	{
		_dynamicRagdoll = flags;
		_dynamicRagdollRootMotion = Vector3.zero;
		_dynamicRagdollStunTime = stunTime.Random(rand);
	}

	public void ActivateDynamicRagdoll()
	{
		if (!_dynamicRagdoll.HasFlag(DynamicRagdollFlags.Active))
		{
			return;
		}
		DynamicRagdollFlags dynamicRagdoll = _dynamicRagdoll;
		_dynamicRagdoll = DynamicRagdollFlags.None;
		Vector3 forceVec = _dynamicRagdollRootMotion * 20f;
		bodyDamage.StunDuration = _dynamicRagdollStunTime;
		emodel.DoRagdoll(_dynamicRagdollStunTime, EnumBodyPartHit.None, forceVec, Vector3.zero, isRemote: true);
		if (dynamicRagdoll.HasFlag(DynamicRagdollFlags.UseBoneVelocities) && _ragdollPositionsPrev.Count == _ragdollPositionsCur.Count)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < _ragdollPositionsPrev.Count; i++)
			{
				Vector3 vector = _ragdollPositionsCur[i] - _ragdollPositionsPrev[i];
				list.Add(vector * 20f);
			}
			emodel.ApplyRagdollVelocities(list);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		entityName = GetType().Name;
		MinEventContext.Self = this;
		seeCache = new EntitySeeCache(this);
		maximumHomeDistance = -1;
		homePosition = new ChunkCoordinates(0, 0, 0);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !(this is EntityPlayer))
		{
			hasAI = true;
			navigator = AstarManager.CreateNavigator(this);
			aiManager = new EAIManager(this);
			lookHelper = new EntityLookHelper(this);
			moveHelper = new EntityMoveHelper(this);
		}
		equipment = new Equipment(this);
		InitInventory();
		if (bag == null)
		{
			bag = new Bag(this);
		}
		stepHeight = 0.52f;
		soundDelayTicks = GetSoundRandomTicks() / 3 - 5;
		spawnPoints = new EntityBedrollPositionList(this);
		CreationTimeSinceLevelLoad = Time.timeSinceLevelLoad;
		Buffs = new EntityBuffs(this);
		droppedBackpackPositions = new List<Vector3i>();
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		InitStats();
		switchModelView(EnumEntityModelView.ThirdPerson);
		InitPostCommon();
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
		switchModelView(EnumEntityModelView.ThirdPerson);
		InitPostCommon();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPostCommon()
	{
		if (GameManager.IsDedicatedServer)
		{
			Transform modelTransform = emodel.GetModelTransform();
			if ((bool)modelTransform)
			{
				ServerHelper.SetupForServer(modelTransform.gameObject);
			}
		}
		AddCharacterController();
		wasSeenByPlayer = false;
		ticksToCheckSeenByPlayer = 20;
		if (EntityClass.list[entityClass].UseAIPackages)
		{
			hasAI = true;
			AIPackages = new List<string>();
			AIPackages.AddRange(EntityClass.list[entityClass].AIPackages);
			utilityAIContext = new Context(this);
		}
		List<string> buffs = EntityClass.list[entityClass].Buffs;
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
				string text = buffs[i];
				if (!Buffs.HasBuff(text))
				{
					Buffs.AddBuff(text);
				}
			}
		}
		if ((entityFlags & EntityFlags.AIHearing) != EntityFlags.None)
		{
			emodel.SetVisible(_bVisible: false);
			emodel.SetFade(0f);
		}
	}

	public override void PostInit()
	{
		base.PostInit();
		ApplySpawnState();
		LODGroup componentInChildren = emodel.GetModelTransform().GetComponentInChildren<LODGroup>();
		if ((bool)componentInChildren)
		{
			LOD[] lODs = componentInChildren.GetLODs();
			lODs[^1].screenRelativeTransitionHeight = 0.003f;
			componentInChildren.SetLODs(lODs);
		}
		disableFallBehaviorUntilOnGround = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySpawnState()
	{
		if (Health <= 0 && isEntityRemote)
		{
			ClientKill(DamageResponse.New(_fatal: true));
		}
		ExecuteDismember(restoreState: true);
	}

	public virtual void InitInventory()
	{
		if (inventory == null)
		{
			inventory = new Inventory(GameManager.Instance, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void switchModelView(EnumEntityModelView modelView)
	{
		emodel.SwitchModelAndView(modelView == EnumEntityModelView.FirstPerson, IsMale);
		ReassignEquipmentTransforms();
	}

	public virtual void ReassignEquipmentTransforms()
	{
		if (isFirstTimeEquipmentReassigned)
		{
			Buffs.SetCustomVar("_equipReload", 0f);
			isFirstTimeEquipmentReassigned = false;
		}
		else
		{
			Buffs.SetCustomVar("_equipReload", 1f);
		}
		equipment.InitializeEquipmentTransforms();
		Buffs.SetCustomVar("_equipReload", 0f);
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[base.entityClass];
		string text = entityClass.Properties.GetString(EntityClass.PropHandItem);
		if (text.Length > 0)
		{
			handItem = ItemClass.GetItem(text);
			if (handItem.IsEmpty())
			{
				throw new Exception("Item with name '" + text + "' not found!");
			}
		}
		else
		{
			handItem = ItemClass.GetItem("meleeHandPlayer").Clone();
		}
		if (inventory != null)
		{
			inventory.SetBareHandItem(handItem);
		}
		rightHandTransformName = "Gunjoint";
		if (emodel is EModelSDCS)
		{
			rightHandTransformName = "RightWeapon";
		}
		entityClass.Properties.ParseString(EntityClass.PropRightHandJointName, ref rightHandTransformName);
		if (!(this is EntityPlayer))
		{
			factionId = 0;
			factionRank = 0;
			string text2 = entityClass.Properties.GetString("Faction");
			if (text2.Length > 0)
			{
				Faction factionByName = FactionManager.Instance.GetFactionByName(text2);
				if (factionByName != null)
				{
					factionId = factionByName.ID;
					string text3 = entityClass.Properties.GetString("FactionRank");
					if (text3.Length > 0)
					{
						factionRank = StringParsers.ParseUInt8(text3);
					}
				}
			}
		}
		else if (FactionManager.Instance.GetFaction(factionId).ID == 0)
		{
			factionId = FactionManager.Instance.CreateFaction(entityName).ID;
			factionRank = byte.MaxValue;
		}
		maxViewAngle = 180f;
		entityClass.Properties.ParseFloat(EntityClass.PropMaxViewAngle, ref maxViewAngle);
		sightRangeBase = entityClass.SightRange;
		sightLightThreshold = entityClass.sightLightThreshold;
		SetSleeperSight(-1f, -1f);
		sightWakeThresholdAtRange.x = rand.RandomRange(entityClass.SleeperSightToWakeMin.x, entityClass.SleeperSightToWakeMin.y);
		sightWakeThresholdAtRange.y = rand.RandomRange(entityClass.SleeperSightToWakeMax.y, entityClass.SleeperSightToWakeMax.y);
		sightGroanThresholdAtRange.x = rand.RandomRange(entityClass.SleeperSightToSenseMin.x, entityClass.SleeperSightToSenseMin.y);
		sightGroanThresholdAtRange.y = rand.RandomRange(entityClass.SleeperSightToSenseMax.y, entityClass.SleeperSightToSenseMax.y);
		sleeperNoiseToSense = rand.RandomRange(entityClass.SleeperNoiseToSense.x, entityClass.SleeperNoiseToSense.y);
		sleeperNoiseToSenseSoundChance = entityClass.SleeperNoiseToSenseSoundChance;
		sleeperNoiseToWake = rand.RandomRange(entityClass.SleeperNoiseToWake.x, entityClass.SleeperNoiseToWake.y);
		float optionalValue = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropAttackTimeoutDay, ref optionalValue);
		attackTimeoutDay = (int)(optionalValue * 20f);
		entityClass.Properties.ParseFloat(EntityClass.PropAttackTimeoutNight, ref optionalValue);
		attackTimeoutNight = (int)(optionalValue * 20f);
		entityClass.Properties.ParseBool(EntityClass.PropStompsSpikes, ref stompsSpikes);
		weight = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropWeight, ref weight);
		weight = Utils.FastMax(weight, 0.5f);
		pushFactor = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropPushFactor, ref pushFactor);
		float optionalValue2 = 5f;
		entityClass.Properties.ParseFloat(EntityClass.PropTimeStayAfterDeath, ref optionalValue2);
		timeStayAfterDeath = (int)(optionalValue2 * 20f);
		IsMale = true;
		entityClass.Properties.ParseBool(EntityClass.PropIsMale, ref IsMale);
		IsFeral = entityClass.Tags.Test_Bit(FeralTagBit);
		proneRefillRate = rand.RandomRange(entityClass.KnockdownProneRefillRate.x, entityClass.KnockdownProneRefillRate.y);
		kneelRefillRate = rand.RandomRange(entityClass.KnockdownKneelRefillRate.x, entityClass.KnockdownKneelRefillRate.y);
		moveSpeed = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeed, ref moveSpeed);
		moveSpeedNight = moveSpeed;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeedNight, ref moveSpeedNight);
		moveSpeedAggro = moveSpeed;
		moveSpeedAggroMax = moveSpeed;
		entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedAggro, ref moveSpeedAggro, ref moveSpeedAggroMax);
		moveSpeedPanic = 1f;
		moveSpeedPanicMax = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeedPanic, ref moveSpeedPanic);
		if (moveSpeedPanic != 1f)
		{
			moveSpeedPanicMax = moveSpeedPanic;
		}
		entityClass.Properties.ParseFloat(EntityClass.PropSwimSpeed, ref swimSpeed);
		entityClass.Properties.ParseVec(EntityClass.PropSwimStrokeRate, ref swimStrokeRate);
		Vector2 optionalValue3 = Vector2.negativeInfinity;
		entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedRand, ref optionalValue3);
		if (optionalValue3.x > -1f)
		{
			float num = rand.RandomRange(optionalValue3.x, optionalValue3.y);
			int num2 = GameStats.GetInt(EnumGameStats.GameDifficulty);
			num *= moveSpeedRandomness[num2];
			if (moveSpeedAggro < 1f)
			{
				moveSpeedAggro += num;
				if (moveSpeedAggro < 0.1f)
				{
					moveSpeedAggro = 0.1f;
				}
				if (moveSpeedAggro > moveSpeedAggroMax)
				{
					moveSpeedAggro = moveSpeedAggroMax;
				}
			}
		}
		entityClass.Properties.ParseInt(EntityClass.PropCrouchType, ref crouchType);
		walkType = GetSpawnWalkType(entityClass);
		entityClass.Properties.ParseBool(EntityClass.PropCanClimbLadders, ref bCanClimbLadders);
		entityClass.Properties.ParseBool(EntityClass.PropCanClimbVertical, ref bCanClimbVertical);
		Vector2 optionalValue4 = new Vector2(1.9f, 2.1f);
		entityClass.Properties.ParseVec(EntityClass.PropJumpMaxDistance, ref optionalValue4);
		jumpMaxDistance = rand.RandomRange(optionalValue4.x, optionalValue4.y);
		jumpDelay = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropJumpDelay, ref jumpDelay);
		jumpDelay *= 20f;
		ExperienceValue = 20;
		entityClass.Properties.ParseInt(EntityClass.PropExperienceGain, ref ExperienceValue);
		if (aiManager != null)
		{
			aiManager.CopyPropertiesFromEntityClass(entityClass);
		}
		entityClass.Properties.ParseString(EntityClass.PropSoundSpawn, ref soundSpawn);
		entityClass.Properties.ParseString(EntityClass.PropSoundSleeperSense, ref soundSleeperGroan);
		entityClass.Properties.ParseString(EntityClass.PropSoundSleeperSnore, ref soundSleeperSnore);
		entityClass.Properties.ParseString(EntityClass.PropSoundDeath, ref soundDeath);
		entityClass.Properties.ParseString(EntityClass.PropSoundAlert, ref soundAlert);
		entityClass.Properties.ParseString(EntityClass.PropSoundAttack, ref soundAttack);
		entityClass.Properties.ParseString(EntityClass.PropSoundLiving, ref soundLiving);
		entityClass.Properties.ParseString(EntityClass.PropSoundRandom, ref soundRandom);
		entityClass.Properties.ParseString(EntityClass.PropSoundSense, ref soundSense);
		entityClass.Properties.ParseString(EntityClass.PropSoundGiveUp, ref soundGiveUp);
		soundStepType = "step";
		entityClass.Properties.ParseString(EntityClass.PropSoundStepType, ref soundStepType);
		entityClass.Properties.ParseString(EntityClass.PropSoundStamina, ref soundStamina);
		entityClass.Properties.ParseString(EntityClass.PropSoundJump, ref soundJump);
		entityClass.Properties.ParseString(EntityClass.PropSoundLand, ref soundLand);
		entityClass.Properties.ParseString(EntityClass.PropSoundHurt, ref soundHurt);
		entityClass.Properties.ParseString(EntityClass.PropSoundHurtSmall, ref soundHurtSmall);
		entityClass.Properties.ParseString(EntityClass.PropSoundDrownPain, ref soundDrownPain);
		entityClass.Properties.ParseString(EntityClass.PropSoundDrownDeath, ref soundDrownDeath);
		entityClass.Properties.ParseString(EntityClass.PropSoundWaterSurface, ref soundWaterSurface);
		soundAlertTicks = 25;
		entityClass.Properties.ParseInt(EntityClass.PropSoundAlertTime, ref soundAlertTicks);
		soundAlertTicks *= 20;
		soundRandomTicks = 25;
		entityClass.Properties.ParseInt(EntityClass.PropSoundRandomTime, ref soundRandomTicks);
		soundRandomTicks *= 20;
		entityClass.Properties.ParseString(EntityClass.PropParticleOnDeath, ref particleOnDeath);
		entityClass.Properties.ParseString(EntityClass.PropParticleOnDestroy, ref particleOnDestroy);
		string text4 = entityClass.Properties.GetString(EntityClass.PropCorpseBlock);
		if (text4.Length > 0)
		{
			corpseBlockValue = Block.GetBlockValue(text4);
		}
		corpseBlockChance = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropCorpseBlockChance, ref corpseBlockChance);
		GameMode gameModeForId = GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId));
		if (gameModeForId != null)
		{
			string text5 = entityClass.Properties.GetString(EntityClass.PropItemsOnEnterGame + "." + gameModeForId.GetTypeName());
			if (text5.Length > 0)
			{
				string[] array = text5.Split(',');
				foreach (string text6 in array)
				{
					ItemStack itemStack = ItemStack.FromString(text6.Trim());
					if (itemStack.itemValue.IsEmpty())
					{
						throw new Exception("Item with name '" + text6 + "' not found in class " + EntityClass.list[base.entityClass].entityClassName);
					}
					if (itemStack.itemValue.ItemClass.CreativeMode != EnumCreativeMode.Console || (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
					{
						itemsOnEnterGame.Add(itemStack);
					}
				}
			}
		}
		DynamicProperties dynamicProperties = entityClass.Properties.Classes[EntityClass.PropFallLandBehavior];
		if (dynamicProperties != null)
		{
			foreach (KeyValuePair<string, string> item in dynamicProperties.Data.Dict)
			{
				string key = item.Key;
				DictionarySave<string, string> dictionarySave = dynamicProperties.ParseKeyData(key);
				if (dictionarySave == null)
				{
					continue;
				}
				FloatRange floatRange = default(FloatRange);
				FloatRange ragePer = default(FloatRange);
				FloatRange rageTime = default(FloatRange);
				IntRange difficulty = new IntRange(0, 10);
				if (!dictionarySave.TryGetValue("anim", out var _value) || !Enum.TryParse<FallBehavior.Op>(_value, out var result))
				{
					Log.Error("Expected 'anim' parameter as float for FallBehavior " + key + ", skipping");
					continue;
				}
				float _result = 0f;
				if (!dictionarySave.TryGetValue("weight", out _value) || !StringParsers.TryParseFloat(_value, out _result))
				{
					Log.Error("Expected 'weight' parameter as float for FallBehavior " + key + ", skipping");
				}
				else if (dictionarySave.TryGetValue("height", out _value))
				{
					if (StringParsers.TryParseRange(_value, out var _result2, float.MaxValue))
					{
						floatRange = _result2;
						if (dictionarySave.TryGetValue("ragePer", out _value))
						{
							if (!StringParsers.TryParseRange(_value, out FloatRange _result3, (float?)null))
							{
								Log.Error("Expected 'ragePer' parameter as range(min,min-max) " + key + ", skipping");
								continue;
							}
							ragePer = _result3;
						}
						if (dictionarySave.TryGetValue("rageTime", out _value))
						{
							if (!StringParsers.TryParseRange(_value, out FloatRange _result4, (float?)null))
							{
								Log.Error("Expected 'rageTime' parameter as range(min,min-max) " + key + ", skipping");
								continue;
							}
							rageTime = _result4;
						}
						if (dictionarySave.TryGetValue("difficulty", out _value))
						{
							if (!StringParsers.TryParseRange(_value, out var _result5, null))
							{
								Log.Error("Expected 'difficulty' parameter as range(min,min-max) " + key + ", skipping");
								continue;
							}
							difficulty = _result5;
						}
						fallBehaviors.Add(new FallBehavior(key, result, floatRange, _result, ragePer, rageTime, difficulty));
					}
					else
					{
						Log.Error("Expected 'height' parameter as range(min,min-max) " + key + ", skipping");
					}
				}
				else
				{
					Log.Error("Expected 'height' parameter for FallBehavior " + key + ", skipping");
				}
			}
		}
		DynamicProperties dynamicProperties2 = entityClass.Properties.Classes[EntityClass.PropDestroyBlockBehavior];
		if (dynamicProperties2 != null)
		{
			DestroyBlockBehavior.Op[] array2 = Enum.GetValues(typeof(DestroyBlockBehavior.Op)) as DestroyBlockBehavior.Op[];
			for (int j = 0; j < array2.Length; j++)
			{
				string text7 = array2[j].ToStringCached();
				DictionarySave<string, string> dictionarySave2 = dynamicProperties2.ParseKeyData(array2[j].ToStringCached());
				if (dictionarySave2 == null)
				{
					continue;
				}
				FloatRange ragePer2 = default(FloatRange);
				FloatRange rageTime2 = default(FloatRange);
				IntRange difficulty2 = new IntRange(0, 10);
				if (!dictionarySave2.TryGetValue("weight", out var _value2) || !StringParsers.TryParseFloat(_value2, out var _result6))
				{
					Log.Error($"Expected 'weight' parameter as float for FallBehavior {array2[j]}, skipping");
					continue;
				}
				if (dictionarySave2.TryGetValue("ragePer", out _value2))
				{
					if (!StringParsers.TryParseRange(_value2, out FloatRange _result7, (float?)null))
					{
						Log.Error($"Expected 'ragePer' parameter as range(min,min-max) {array2[j]}, skipping");
						continue;
					}
					ragePer2 = _result7;
				}
				if (dictionarySave2.TryGetValue("rageTime", out _value2))
				{
					if (!StringParsers.TryParseRange(_value2, out FloatRange _result8, (float?)null))
					{
						Log.Error($"Expected 'rageTime' parameter as range(min,min-max) {array2[j]}, skipping");
						continue;
					}
					rageTime2 = _result8;
				}
				if (dictionarySave2.TryGetValue("difficulty", out _value2))
				{
					if (!StringParsers.TryParseRange(_value2, out var _result9, null))
					{
						Log.Error("Expected 'difficulty' parameter as range(min,min-max) " + text7 + ", skipping");
						continue;
					}
					difficulty2 = _result9;
				}
				_destroyBlockBehaviors.Add(new DestroyBlockBehavior(text7, array2[j], _result6, ragePer2, rageTime2, difficulty2));
			}
		}
		distractionResistance = EffectManager.GetValue(PassiveEffects.DistractionResistance, null, 0f, this);
		distractionResistanceWithTarget = EffectManager.GetValue(PassiveEffects.DistractionResistance, null, 0f, this, null, DistractionResistanceWithTargetTags);
	}

	public static int GetSpawnWalkType(EntityClass _entityClass)
	{
		int optionalValue = 0;
		_entityClass.Properties.ParseInt(EntityClass.PropWalkType, ref optionalValue);
		return optionalValue;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _isZoom)
	{
		if ((entityFlags & EntityFlags.AIHearing) != EntityFlags.None)
		{
			if (GameManager.IsDedicatedServer)
			{
				emodel.SetVisible(_bVisible: true);
			}
			else if (_distanceSqr < (float)(_isZoom ? 14400 : 8100))
			{
				renderFadeTarget = renderFadeMax;
			}
			else
			{
				renderFadeTarget = 0f;
			}
		}
	}

	public virtual void SetSleeper()
	{
		IsSleeper = true;
		aiManager.pathCostScale += 0.2f;
	}

	public void SetSleeperSight(float angle, float range)
	{
		if (angle < 0f)
		{
			angle = maxViewAngle;
		}
		sleeperViewAngle = angle;
		if (range < 0f)
		{
			range = Utils.FastMax(3f, sightRangeBase * 0.2f);
		}
		sleeperSightRange = range;
	}

	public void SetSleeperHearing(float percent)
	{
		if (percent < 0.001f)
		{
			percent = 0.001f;
		}
		percent = 1f / percent;
		sleeperNoiseToSense *= percent;
		sleeperNoiseToWake *= percent;
	}

	public int GetSleeperDisturbedLevel(float dist, float lightLevel)
	{
		float num = dist / sightRangeBase;
		if (num <= 1f)
		{
			float num2 = Mathf.Lerp(sightWakeThresholdAtRange.x, sightWakeThresholdAtRange.y, num);
			if (lightLevel > num2)
			{
				return 2;
			}
			float num3 = Mathf.Lerp(sightGroanThresholdAtRange.x, sightGroanThresholdAtRange.y, num);
			if (lightLevel > num3)
			{
				return 1;
			}
		}
		return 0;
	}

	public void GetSleeperDebugScale(float dist, out float wake, out float groan)
	{
		float t = dist / sightRangeBase;
		wake = Mathf.Lerp(sightWakeThresholdAtRange.x, sightWakeThresholdAtRange.y, t);
		groan = Mathf.Lerp(sightGroanThresholdAtRange.x, sightGroanThresholdAtRange.y, t);
	}

	public void TriggerSleeperPose(int _pose, bool _returningToSleep = false)
	{
		if (IsDead())
		{
			return;
		}
		if ((bool)emodel && (bool)emodel.avatarController)
		{
			emodel.avatarController.TriggerSleeperPose(_pose, _returningToSleep);
			pendingSleepTrigger = -1;
			if (_pose != 5)
			{
				physicsHeight = 0.85f;
			}
		}
		else
		{
			pendingSleepTrigger = _pose;
		}
		lastSleeperPose = _pose;
		IsSleeping = true;
		SleeperSupressLivingSounds = true;
		sleeperLookDir = Quaternion.AngleAxis(rotation.y, Vector3.up) * SleeperSpawnLookDir;
	}

	public void ResumeSleeperPose()
	{
		TriggerSleeperPose(lastSleeperPose, _returningToSleep: true);
	}

	public void ConditionalTriggerSleeperWakeUp()
	{
		if (IsSleeping && !IsDead())
		{
			IsSleeping = false;
			IsSleeperPassive = false;
			int pose = ((physicsHeight < 1f && !IsWalkTypeACrawl()) ? (-2) : (-1));
			emodel.avatarController.TriggerSleeperPose(pose);
			if (aiManager != null)
			{
				aiManager.SleeperWokeUp();
			}
			if (!world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSleeperWakeup>().Setup(entityId));
			}
		}
	}

	public void SetSleeperActive()
	{
		if (IsSleeperPassive)
		{
			IsSleeperPassive = false;
			if (!world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSleeperPassiveChange>().Setup(entityId));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitStats()
	{
		entityStats = new EntityStats(this);
	}

	public void SetStats(EntityStats _stats)
	{
		entityStats.CopyFrom(_stats);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemValue GetHandItem()
	{
		return handItem;
	}

	public bool IsHoldingLight()
	{
		return inventory.IsFlashlightOn;
	}

	public void CycleActivatableItems()
	{
	}

	public List<ItemValue> GetActivatableItemPool()
	{
		List<ItemValue> list = new List<ItemValue>();
		CollectActivatableItems(list);
		return list;
	}

	public void CollectActivatableItems(List<ItemValue> _pool)
	{
		if (inventory != null)
		{
			GetActivatableItems(inventory.holdingItemItemValue, _pool);
		}
		if (equipment != null)
		{
			int slotCount = equipment.GetSlotCount();
			for (int i = 0; i < slotCount; i++)
			{
				GetActivatableItems(equipment.GetSlotItemOrNone(i), _pool);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetActivatableItems(ItemValue _item, List<ItemValue> _itemPool)
	{
		ItemClass itemClass = _item.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
		{
			_itemPool.Add(_item);
		}
		for (int i = 0; i < _item.Modifications.Length; i++)
		{
			ItemValue itemValue = _item.Modifications[i];
			if (itemValue != null)
			{
				ItemClass itemClass2 = itemValue.ItemClass;
				if (itemClass2 != null && itemClass2.HasTrigger(MinEventTypes.onSelfItemActivate))
				{
					_itemPool.Add(itemValue);
				}
			}
		}
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		float rotYDelta = Utils.DeltaAngle(rotation.y, prevRotation.y);
		base.OnUpdatePosition(_partialTicks);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < lastTickPos.Length - 1; i++)
		{
			zero.x += lastTickPos[i].x - lastTickPos[i + 1].x;
			zero.z += lastTickPos[i].z - lastTickPos[i + 1].z;
		}
		zero += position - lastTickPos[0];
		zero /= (float)lastTickPos.Length;
		if (AttachedToEntity == null)
		{
			updateStepSound(zero.x, zero.z, rotYDelta);
		}
		if (!RootMotion && !isEntityRemote)
		{
			updateSpeedForwardAndStrafe(zero, _partialTicks);
		}
	}

	public void Snore()
	{
		if (!isSnore && isGroan && snoreGroanCD <= 0)
		{
			isSnore = true;
			isGroan = false;
			snoreGroanCD = rand.RandomRange(20, 21);
			if (soundSleeperSnore != null && !isGroanSilent)
			{
				Manager.BroadcastPlay(this, soundSleeperSnore);
			}
		}
	}

	public void Groan()
	{
		if (isGroan || snoreGroanCD > 0)
		{
			return;
		}
		isGroan = true;
		isSnore = false;
		snoreGroanCD = rand.RandomRange(20, 21);
		if (sleeperNoiseToSenseSoundChance >= 1f || rand.RandomFloat <= sleeperNoiseToSenseSoundChance)
		{
			isGroanSilent = false;
			if (soundSleeperGroan != null)
			{
				Manager.BroadcastPlay(this, soundSleeperGroan);
			}
		}
		else
		{
			isGroanSilent = true;
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		Buffs.SetCustomVar("_underwater", inWaterPercent);
		if (Buffs != null)
		{
			Buffs.Tick();
		}
		OnUpdateLive();
		if (!IsSleeping && (!isEntityRemote || !(this is EntityPlayer)))
		{
			bag.OnUpdate();
			if (inventory != null)
			{
				inventory.OnUpdate();
			}
		}
		if (Health <= 0 && !IsDead() && !isEntityRemote && !IsGodMode.Value)
		{
			if (Buffs.HasBuff("drowning"))
			{
				DamageEntity(DamageSource.suffocating, 1, _criticalHit: false);
			}
			else
			{
				DamageEntity(DamageSource.disease, 1, _criticalHit: false);
			}
		}
		if (IsAlive() && bPlayHurtSound)
		{
			string text = GetSoundHurt(woundedDamageSource, woundedStrength);
			if (text != null)
			{
				PlayOneShot(text);
			}
		}
		bPlayHurtSound = false;
		bBeenWounded = false;
		woundedStrength = 0;
		woundedDamageSource = null;
		if (snoreGroanCD > 0)
		{
			snoreGroanCD--;
		}
		if (!IsDead() && !isEntityRemote)
		{
			if (isRadiationSensitive() && biomeStandingOn != null && biomeStandingOn.m_RadiationLevel > 0 && !IsGodMode.Value && world.worldTime % 20 == 0L)
			{
				DamageEntity(DamageSource.radiation, biomeStandingOn.m_RadiationLevel, _criticalHit: false);
			}
			if (hasAI)
			{
				if (IsSleeping && pendingSleepTrigger > -1)
				{
					TriggerSleeperPose(pendingSleepTrigger);
				}
				soundDelayTicks--;
				if (attackingTime <= 0 && soundDelayTicks <= 0 && aiClosestPlayerDistSq <= 400f && bodyDamage.CurrentStun == EnumEntityStunType.None && !SleeperSupressLivingSounds)
				{
					if (targetAlertChanged)
					{
						targetAlertChanged = false;
						soundDelayTicks = GetSoundAlertTicks();
						if (GetSoundAlert() != null && !IsScoutZombie)
						{
							PlayOneShot(GetSoundAlert());
						}
						OnEntityTargeted(attackTarget);
					}
					else
					{
						soundDelayTicks = GetSoundRandomTicks();
						attackTargetLast = null;
						if (GetSoundRandom() != null)
						{
							PlayOneShot(GetSoundRandom());
						}
					}
				}
			}
		}
		if (hasBeenAttackedTime > 0)
		{
			hasBeenAttackedTime--;
		}
		if (painResistPercent > 0f)
		{
			painResistPercent -= 0.010000001f;
			if (painResistPercent <= 0f)
			{
				painHitsFelt = 0f;
			}
		}
		if (attackingTime > 0)
		{
			attackingTime--;
		}
		if (investigatePositionTicks > 0 && --investigatePositionTicks == 0)
		{
			ClearInvestigatePosition();
		}
		bool flag = IsDead();
		if (alertEnabled)
		{
			isAlert = bReplicatedAlertFlag;
			if (!isEntityRemote)
			{
				if (alertTicks > 0)
				{
					alertTicks--;
				}
				isAlert = !flag && (alertTicks > 0 || (bool)attackTarget || (HasInvestigatePosition && isInvestigateAlert));
				if (bReplicatedAlertFlag != isAlert)
				{
					bReplicatedAlertFlag = isAlert;
					bEntityAliveFlagsChanged = true;
				}
			}
			if (!isAlert && !flag)
			{
				Buffs.SetCustomVar(notAlertedId, 1f);
				notAlertDelayTicks = 4;
			}
			else
			{
				if (notAlertDelayTicks > 0)
				{
					notAlertDelayTicks--;
				}
				if (notAlertDelayTicks == 0)
				{
					Buffs.SetCustomVar(notAlertedId, 0f);
				}
			}
		}
		if (flag)
		{
			OnDeathUpdate();
		}
		if (revengeEntity != null)
		{
			if (!revengeEntity.IsAlive())
			{
				SetRevengeTarget(null);
			}
			else if (revengeTimer > 0)
			{
				revengeTimer--;
			}
			else
			{
				SetRevengeTarget(null);
			}
		}
	}

	public override void KillLootContainer()
	{
		if (!isEntityRemote && IsDead() && !corpseBlockValue.isair && deathUpdateTime < timeStayAfterDeath)
		{
			deathUpdateTime = timeStayAfterDeath - 1;
		}
		base.KillLootContainer();
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		NotifySleeperDeath();
		if (AttachedToEntity != null)
		{
			Detach();
		}
		if (deathUpdateTime == 0)
		{
			string text = GetSoundDeath(_dmResponse.Source);
			if (text != null)
			{
				PlayOneShot(text);
			}
		}
		if (IsDead())
		{
			SetDead();
			return;
		}
		ClientKill(_dmResponse);
		base.Kill(_dmResponse);
	}

	public override void SetDead()
	{
		base.SetDead();
		Stats.Health.Value = 0f;
	}

	public void NotifySleeperDeath()
	{
		if (!isEntityRemote && IsSleeper)
		{
			world.NotifySleeperVolumesEntityDied(this);
		}
	}

	public void ClearEntityThatKilledMe()
	{
		entityThatKilledMe = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ClientKill(DamageResponse _dmResponse)
	{
		lastHitDirection = Utils.EnumHitDirection.Back;
		if (entityThatKilledMe == null && _dmResponse.Source != null)
		{
			Entity entity = ((_dmResponse.Source.getEntityId() != -1) ? world.GetEntity(_dmResponse.Source.getEntityId()) : null);
			if (Spawned && entity is EntityAlive)
			{
				entityThatKilledMe = (EntityAlive)entity;
			}
		}
		if (!IsDead())
		{
			SetDead();
			if (Buffs != null)
			{
				Buffs.OnDeath(entityThatKilledMe, _dmResponse.Source != null && _dmResponse.Source.damageType == EnumDamageTypes.Crushing, (_dmResponse.Source == null) ? FastTags<TagGroup.Global>.Parse("crushing") : _dmResponse.Source.DamageTypeTag);
			}
			if (Progression != null)
			{
				Progression.OnDeath();
			}
			EntityPlayer obj = this as EntityPlayer;
			AnalyticsSendDeath(_dmResponse);
			if (obj == null && entityThatKilledMe is EntityPlayer && EffectManager.GetValue(PassiveEffects.CelebrationKill, null, 0f, entityThatKilledMe) > 0f)
			{
				HandleClientDeath((_dmResponse.Source != null) ? _dmResponse.Source.BlockPosition : GetBlockPosition());
				OnEntityDeath();
				float lightBrightness = world.GetLightBrightness(GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("confetti", position, lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityId, _forceCreation: false, _worldSpawn: true);
				Manager.BroadcastPlayByLocalPlayer(position, "twitch_celebrate");
				GameManager.Instance.World.RemoveEntity(entityId, EnumRemoveEntityReason.Killed);
			}
			else
			{
				HandleClientDeath((_dmResponse.Source != null) ? _dmResponse.Source.BlockPosition : GetBlockPosition());
				OnEntityDeath();
				emodel.OnDeath(_dmResponse, world.ChunkClusters[0]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClientDeath(Vector3i attackPos)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntityTargeted(EntityAlive target)
	{
	}

	public void ForceHoldingWeaponUpdate()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(this), _onlyClientsAttachedToAnEntity: false, -1, entityId);
			}
			else if (entityId > 0 && this as EntityPlayerLocal != null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(this));
			}
		}
	}

	public virtual void SetHoldingItemTransform(Transform _transform)
	{
		emodel.SetInRightHand(_transform);
		ForceHoldingWeaponUpdate();
	}

	public virtual void OnHoldingItemChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCameraFOV(bool _bLerpPosition)
	{
	}

	public virtual int GetCameraFOV()
	{
		return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
	}

	public virtual float GetWetnessRate()
	{
		return inWaterPercent;
	}

	public float GetAmountEnclosed()
	{
		Vector3 worldPos = position;
		worldPos.y += 0.5f;
		Vector3i blockPos = World.worldToBlockPos(worldPos);
		if ((uint)blockPos.y < 255u)
		{
			IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(blockPos);
			if (chunkFromWorldPos != null)
			{
				float v = (int)chunkFromWorldPos.GetLight(blockPos.x, blockPos.y, blockPos.z, Chunk.LIGHT_TYPE.SUN);
				float v2 = (int)chunkFromWorldPos.GetLight(blockPos.x, blockPos.y + 1, blockPos.z, Chunk.LIGHT_TYPE.SUN);
				float num = Utils.FastMax(v, v2) / 15f;
				return 1f - num;
			}
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		base.OnHeadUnderwaterStateChanged(_bUnderwater);
		if (_bUnderwater)
		{
			FireEvent(MinEventTypes.onSelfWaterSubmerge);
		}
		else
		{
			FireEvent(MinEventTypes.onSelfWaterSurface);
		}
	}

	public virtual Vector3 GetChestTransformPosition()
	{
		if (IsCrouching || bodyDamage.CurrentStun == EnumEntityStunType.Kneel || bodyDamage.CurrentStun == EnumEntityStunType.Prone)
		{
			return base.transform.position + new Vector3(0f, GetEyeHeight() * 0.25f, 0f);
		}
		return base.transform.position + new Vector3(0f, GetEyeHeight() * 0.95f, 0f);
	}

	public virtual bool CanNavigatePath()
	{
		if (!onGround && !isSwimming && !bInElevator)
		{
			return Climbing;
		}
		return true;
	}

	public AvatarController.ActionState GetAnimActionState()
	{
		if ((bool)emodel.avatarController)
		{
			return emodel.avatarController.GetActionState();
		}
		return AvatarController.ActionState.None;
	}

	public virtual void StartAnimAction(int _animType)
	{
		if (!emodel.avatarController)
		{
			return;
		}
		emodel.avatarController.GetActionState();
		if (_animType != 9999)
		{
			if (emodel.avatarController.IsActionActive())
			{
				return;
			}
		}
		else if (!emodel.avatarController.IsActionActive())
		{
			return;
		}
		bPlayerStatsChanged |= !isEntityRemote;
		emodel.avatarController.StartAction(_animType);
	}

	public virtual void ContinueAnimAction(int _animType)
	{
		if ((bool)emodel.avatarController)
		{
			bPlayerStatsChanged |= !isEntityRemote;
			emodel.avatarController.StartAction(_animType);
		}
	}

	public virtual void StartHarvestingAnim(float _length, bool _weaponFireTrigger)
	{
		if (emodel != null && emodel.avatarController != null)
		{
			emodel.avatarController.StartAnimationHarvesting(_length, _weaponFireTrigger);
		}
	}

	public virtual void SetVehicleAnimation(int _animHash, int _pose)
	{
		if ((bool)emodel && (bool)emodel.avatarController)
		{
			emodel.avatarController.SetVehicleAnimation(_animHash, _pose);
			bPlayerStatsChanged = !isEntityRemote;
			if (_pose == -1 && emodel.avatarController is AvatarLocalPlayerController avatarLocalPlayerController)
			{
				avatarLocalPlayerController.TPVResetAnimPose();
			}
		}
	}

	public virtual int GetVehicleAnimation()
	{
		if ((bool)emodel && (bool)emodel.avatarController)
		{
			return emodel.avatarController.GetVehicleAnimation();
		}
		return -1;
	}

	public override void SetEntityName(string _name)
	{
		if (!_name.Equals(entityName))
		{
			entityName = _name;
			bPlayerStatsChanged |= !isEntityRemote;
			HandleSetNavName();
		}
	}

	public override bool IsSpawned()
	{
		return bSpawned;
	}

	public virtual void RemoveIKTargets()
	{
		emodel.RemoveIKController();
	}

	public virtual void SetIKTargets(List<IKController.Target> targets)
	{
		IKController iKController = emodel.AddIKController();
		if ((bool)iKController)
		{
			iKController.SetTargets(targets);
		}
	}

	public virtual List<Vector3i> GetDroppedBackpackPositions()
	{
		return droppedBackpackPositions;
	}

	public virtual Vector3i GetLastDroppedBackpackPosition()
	{
		if (droppedBackpackPositions == null)
		{
			return Vector3i.zero;
		}
		if (droppedBackpackPositions.Count == 0)
		{
			return Vector3i.zero;
		}
		List<Vector3i> list = droppedBackpackPositions;
		return list[list.Count - 1];
	}

	public virtual bool EqualsDroppedBackpackPositions(Vector3i position)
	{
		if (droppedBackpackPositions != null)
		{
			foreach (Vector3i droppedBackpackPosition in droppedBackpackPositions)
			{
				if (position.Equals(droppedBackpackPosition))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void SetDroppedBackpackPositions(List<Vector3i> positions)
	{
		droppedBackpackPositions.Clear();
		if (positions != null)
		{
			droppedBackpackPositions.AddRange(positions);
		}
	}

	public virtual void ClearDroppedBackpackPositions()
	{
		droppedBackpackPositions.Clear();
	}

	public virtual int GetMaxHealth()
	{
		return (int)Stats.Health.Max;
	}

	public virtual int GetMaxStamina()
	{
		return (int)Stats.Stamina.Max;
	}

	public virtual int GetMaxWater()
	{
		return (int)Stats.Water.Max;
	}

	public virtual float GetStaminaMultiplier()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetMovementState()
	{
		float num = speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		float num2 = speedForward * speedForward + num * num;
		MovementState = ((num2 > moveSpeedAggro * moveSpeedAggro) ? 3 : ((num2 > moveSpeed * moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)));
	}

	public virtual void OnUpdateLive()
	{
		Stats.Health.RegenerationAmount = 0f;
		if (!isEntityRemote && !IsDead())
		{
			Stats.Tick(world.worldTime);
		}
		if (jumpTicks > 0)
		{
			jumpTicks--;
		}
		if (attackTargetTime > 0)
		{
			attackTargetTime--;
			if (attackTarget != null && attackTargetTime == 0)
			{
				attackTarget = null;
				if (!isEntityRemote)
				{
					world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityId, -1, NetPackageManager.GetPackage<NetPackageSetAttackTarget>().Setup(entityId, -1));
				}
			}
		}
		updateCurrentBlockPosAndValue();
		if (AttachedToEntity == null)
		{
			if (isEntityRemote)
			{
				if (RootMotion)
				{
					MoveEntityHeaded(Vector3.zero, _isDirAbsolute: false);
				}
			}
			else
			{
				if (Health <= 0)
				{
					bJumping = false;
					bClimbing = false;
					moveDirection = Vector3.zero;
					renderFadeMax = 1f;
				}
				else if (!world.IsRemote() && !IsDead() && !IsClientControlled() && hasAI)
				{
					updateTasks();
				}
				noisePlayer = null;
				noisePlayerDistance = 0f;
				noisePlayerVolume = 0f;
				if (bJumping)
				{
					UpdateJump();
				}
				else
				{
					jumpTicks = 0;
				}
				float num = landMovementFactor;
				landMovementFactor *= GetSpeedModifier();
				MoveEntityHeaded(moveDirection, isMoveDirAbsolute);
				landMovementFactor = num;
			}
			if (moveDirection.x > 0f || moveDirection.z > 0f)
			{
				if (bMovementRunning)
				{
					CurrentMovementTag = MovementTagRunning;
				}
				else
				{
					CurrentMovementTag = MovementTagWalking;
				}
			}
			else
			{
				CurrentMovementTag = MovementTagIdle;
			}
		}
		if (bodyDamage.CurrentStun != EnumEntityStunType.None && !emodel.IsRagdollActive && !IsDead())
		{
			if (bodyDamage.CurrentStun == EnumEntityStunType.Getup)
			{
				if (!emodel.avatarController || !emodel.avatarController.IsAnimationStunRunning())
				{
					ClearStun();
				}
			}
			else
			{
				bodyDamage.StunDuration -= 0.05f;
				if (bodyDamage.StunDuration <= 0f)
				{
					SetStun(EnumEntityStunType.Getup);
					if ((bool)emodel.avatarController)
					{
						emodel.avatarController.EndStun();
					}
				}
			}
		}
		proneRefillCounter += 0.05f * proneRefillRate;
		while (proneRefillCounter >= 1f)
		{
			bodyDamage.StunProne = Mathf.Max(0, bodyDamage.StunProne - 1);
			proneRefillCounter -= 1f;
		}
		kneelRefillCounter += 0.05f * kneelRefillRate;
		while (kneelRefillCounter >= 1f)
		{
			bodyDamage.StunKnee = Mathf.Max(0, bodyDamage.StunKnee - 1);
			kneelRefillCounter -= 1f;
		}
		EntityPlayer primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer != null && primaryPlayer != this)
		{
			if (--ticksToCheckSeenByPlayer <= 0)
			{
				wasSeenByPlayer = primaryPlayer.CanSee(this);
				if (wasSeenByPlayer)
				{
					ticksToCheckSeenByPlayer = 200;
				}
				else
				{
					ticksToCheckSeenByPlayer = 20;
				}
			}
			else if (wasSeenByPlayer)
			{
				primaryPlayer.SetCanSee(this);
			}
		}
		if (onGround)
		{
			disableFallBehaviorUntilOnGround = false;
		}
		UpdateDynamicRagdoll();
		checkForTeleportOutOfTraderArea();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void checkForTeleportOutOfTraderArea()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || GameManager.Instance.IsEditMode() || IsGodMode.Value || !(this is EntityPlayer entityPlayer) || !(Time.time - lastTimeTraderStationChecked > 0.1f))
		{
			return;
		}
		lastTimeTraderStationChecked = Time.time;
		Vector3 vector = position;
		vector.y += 0.5f;
		Vector3i vector3i = World.worldToBlockPos(vector);
		TraderArea traderAreaAt = world.GetTraderAreaAt(vector3i);
		if (traderAreaAt != null && traderAreaAt.IsInitialized)
		{
			Vector3 vector2 = default(Vector3);
			int num = 0;
			Prefab.PrefabTeleportVolume tpVolume;
			if (world.IsWorldEvent(World.WorldEvent.BloodMoon) && traderAreaAt.IsWithinProtectArea(vector))
			{
				vector2 = traderAreaAt.ProtectPosition + traderAreaAt.ProtectSize * 0.5f;
				num = Math.Max(traderAreaAt.ProtectSize.x, traderAreaAt.ProtectSize.z) / 2;
			}
			else if (traderAreaAt.IsWithinTeleportArea(vector, out tpVolume) && (traderAreaAt.IsClosed || EffectManager.GetValue(PassiveEffects.NoTrader, null, 0f, this) == 1f))
			{
				PrefabInstance pOIAtPosition = world.GetPOIAtPosition(vector3i);
				if (pOIAtPosition == null)
				{
					return;
				}
				vector2 = pOIAtPosition.boundingBoxPosition + traderAreaAt.PrefabSize * 0.5f;
				num = Math.Max(traderAreaAt.PrefabSize.x, traderAreaAt.PrefabSize.z) / 2;
			}
			if (num <= 0)
			{
				return;
			}
			num += traderTeleportStreak;
			traderTeleportStreak++;
			if (!world.GetRandomSpawnPositionMinMaxToPosition(vector2, num, num + 1, 1, _checkBedrolls: false, out var _position, entityId, _checkWater: true, 20, _checkLandClaim: true, EnumLandClaimOwner.Ally, _useSquareRadius: true))
			{
				Log.Warning("Trader teleport: Could not find a valid teleport position, returning original position");
				return;
			}
			if (isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(_position));
			}
			else if ((bool)entityPlayer)
			{
				entityPlayer.Teleport(_position);
			}
			else if (AttachedToEntity != null)
			{
				AttachedToEntity.SetPosition(_position);
			}
			else
			{
				SetPosition(_position);
			}
			if ((bool)entityPlayer)
			{
				GameEventManager.Current.HandleAction("game_on_trader_teleport", entityPlayer, entityPlayer, twitchActivated: false);
			}
		}
		else
		{
			traderTeleportStreak = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJump()
	{
		jumpState = JumpState.Leap;
		jumpStateTicks = 0;
		jumpDistance = 1f;
		jumpHeightDiff = 0f;
		disableFallBehaviorUntilOnGround = true;
		if (isSwimming)
		{
			jumpState = JumpState.SwimStart;
			if (emodel.avatarController != null)
			{
				emodel.avatarController.SetSwim(_enable: true);
			}
		}
		else if (emodel.avatarController != null)
		{
			emodel.avatarController.StartAnimationJump(AnimJumpMode.Start);
		}
	}

	public virtual void SetJumpDistance(float _distance, float _heightDiff)
	{
		jumpDistance = _distance;
		jumpHeightDiff = _heightDiff;
	}

	public virtual void SetSwimValues(float _durationTicks, Vector3 _motion)
	{
		jumpSwimDurationTicks = Mathf.Clamp(_durationTicks / swimSpeed - 6f, 3f, 20f);
		jumpSwimMotion = _motion;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJump()
	{
		if (IsFlyMode.Value)
		{
			Jumping = false;
			return;
		}
		jumpStateTicks++;
		switch (jumpState)
		{
		case JumpState.Leap:
			if (accumulatedRootMotion.y > 0.005f || (float)jumpStateTicks >= jumpDelay)
			{
				StartJumpMotion();
				jumpTicks = 200;
				jumpState = JumpState.Air;
				jumpStateTicks = 0;
				jumpIsMoving = true;
			}
			break;
		case JumpState.Air:
			if (onGround || (motionMultiplier < 0.45f && jumpStateTicks > 40))
			{
				jumpState = JumpState.Land;
				jumpStateTicks = 0;
				jumpIsMoving = false;
			}
			break;
		case JumpState.Land:
			if (jumpStateTicks > 5)
			{
				Jumping = false;
			}
			break;
		case JumpState.SwimStart:
			if ((float)jumpStateTicks > 6f)
			{
				jumpTicks = 100;
				jumpState = JumpState.Swim;
				jumpStateTicks = 0;
				jumpIsMoving = true;
				StartJumpSwimMotion();
			}
			break;
		case JumpState.Swim:
			if (!isSwimming || (float)jumpStateTicks >= jumpSwimDurationTicks)
			{
				Jumping = false;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJumpSwimMotion()
	{
		if (inWaterPercent > 0.65f)
		{
			float num = Mathf.Sqrt(jumpSwimMotion.x * jumpSwimMotion.x + jumpSwimMotion.z * jumpSwimMotion.z) + 0.001f;
			float min = Mathf.Lerp(-0.6f, -0.05f, num * 0.8f);
			jumpSwimMotion.y = Utils.FastClamp(jumpSwimMotion.y, min, 1f);
			float num2 = jumpSwimDurationTicks;
			float num3 = (num2 - 1f) * world.Gravity * 0.025f * 0.4999f;
			num3 /= Mathf.Pow(0.91f, (num2 - 3f) * 0.91f * 0.115f);
			float t = (num2 - 1f) / 15f;
			float num4 = Mathf.LerpUnclamped(0.46f, 0.41860002f, t);
			float num5 = Mathf.Pow(0.91f, (num2 - 1f) * num4);
			float num6 = 1f / num2 / num5;
			num3 += jumpSwimMotion.y * num6;
			num6 /= Utils.FastMax(1f, num);
			motion.x = jumpSwimMotion.x * num6;
			motion.z = jumpSwimMotion.z * num6;
			motion.y = num3;
		}
		else
		{
			motion.y = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FaceJumpTo()
	{
		Vector3 vector = moveHelper.JumpToPos - position;
		float yaw = Mathf.Round(Mathf.Atan2(vector.x, vector.z) * 57.29578f / 90f) * 90f;
		SeekYaw(yaw, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJumpMotion()
	{
		SetAirBorne(_b: true);
		float num = (int)(5f + Mathf.Pow(jumpDistance * 8f, 0.5f));
		motion = GetForwardVector() * (jumpDistance / num);
		float num2 = num * world.Gravity * 0.5f;
		motion.y = Utils.FastMax(num2 * 0.5f, num2 + jumpHeightDiff / num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void JumpMove()
	{
		accumulatedRootMotion = Vector3.zero;
		Vector3 vector = motion;
		entityCollision(motion);
		motion.x = vector.x;
		motion.z = vector.z;
		if (motion.y != 0f)
		{
			motion.y = vector.y;
		}
		if (jumpState == JumpState.Air)
		{
			motion.y -= world.Gravity;
			return;
		}
		motion.x *= 0.91f;
		motion.z *= 0.91f;
		motion.y -= world.Gravity * 0.025f;
		motion.y *= 0.91f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void EndJump()
	{
		jumpState = JumpState.Off;
		jumpIsMoving = false;
		if (!isEntityRemote && emodel.avatarController != null)
		{
			emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CalcIfSwimming()
	{
		float num = ((onGround || Jumping) ? 0.7f : 0.5f);
		return inWaterPercent >= num;
	}

	public override void SwimChanged()
	{
		if ((bool)emodel.avatarController)
		{
			emodel.avatarController.SetSwim(isSwimming);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		updateNetworkStats();
		if (!isEntityRemote && RootMotion && lerpForwardSpeed)
		{
			float num = 0.06935714f;
			if (speedForward > 0.01942f)
			{
				num = speedForwardTargetStep;
			}
			float num2 = Utils.FastMoveTowards(speedForward, speedForwardTarget, num * Time.deltaTime);
			if (speedForward > 0.01942f && num2 <= 0.01942f)
			{
				num2 = 0.01942f;
			}
			speedForward = num2;
		}
		if (isHeadUnderwater != (Buffs.GetCustomVar("_underwater") == 1f))
		{
			Buffs.SetCustomVar("_underwater", isHeadUnderwater ? 1 : 0);
		}
		MinEventContext.Area = boundingBox;
		MinEventContext.Biome = biomeStandingOn;
		MinEventContext.ItemValue = inventory.holdingItemItemValue;
		MinEventContext.BlockValue = blockValueStandingOn;
		MinEventContext.ItemInventoryData = inventory.holdingItemData;
		MinEventContext.Position = position;
		MinEventContext.Seed = entityId + Mathf.Abs(GameManager.Instance.World.Seed);
		MinEventContext.Transform = base.transform;
		FastTags<TagGroup.Global>.CombineTags(EntityClass.list[entityClass].Tags, inventory.holdingItem.ItemTags, CurrentStanceTag, CurrentMovementTag, ref MinEventContext.Tags);
		if (Progression != null)
		{
			Progression.Update();
		}
		if (renderFade != renderFadeTarget)
		{
			renderFade = Mathf.MoveTowards(renderFade, renderFadeTarget, Time.deltaTime);
			emodel.SetFade(renderFade);
			bool flag = renderFade > 0.01f;
			if (emodel.visible != flag)
			{
				emodel.SetVisible(flag);
			}
		}
	}

	public virtual void OnDeathUpdate()
	{
		if (deathUpdateTime < timeStayAfterDeath)
		{
			deathUpdateTime++;
		}
		int deadBodyHitPoints = EntityClass.list[entityClass].DeadBodyHitPoints;
		if (deadBodyHitPoints > 0 && DeathHealth <= -deadBodyHitPoints)
		{
			deathUpdateTime = timeStayAfterDeath;
		}
		if (deathUpdateTime >= timeStayAfterDeath && !isEntityRemote && !markedForUnload)
		{
			dropCorpseBlock();
			if (particleOnDestroy != null && particleOnDestroy.Length > 0)
			{
				float lightBrightness = world.GetLightBrightness(GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(particleOnDestroy, getHeadPosition(), lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3i dropCorpseBlock()
	{
		if (corpseBlockValue.isair)
		{
			return Vector3i.zero;
		}
		if (rand.RandomFloat > corpseBlockChance)
		{
			return Vector3i.zero;
		}
		Vector3i vector3i = World.worldToBlockPos(position);
		while (vector3i.y < 254 && (float)vector3i.y - position.y < 3f && !corpseBlockValue.Block.CanPlaceBlockAt(world, 0, vector3i, corpseBlockValue))
		{
			vector3i += Vector3i.up;
		}
		if (vector3i.y >= 254)
		{
			return Vector3i.zero;
		}
		if ((float)vector3i.y - position.y >= 2.1f)
		{
			return Vector3i.zero;
		}
		world.SetBlockRPC(vector3i, corpseBlockValue);
		return vector3i;
	}

	public void NotifyRootMotion(Animator animator)
	{
		accumulatedRootMotion += animator.deltaPosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DefaultMoveEntity(Vector3 _direction, bool _isDirAbsolute)
	{
		float num = 0.91f;
		if (AIDirector.debugFreezePos && aiManager != null)
		{
			motion = Vector3.zero;
		}
		if (onGround)
		{
			num = 0.546f;
			if (!IsDead() && this is EntityPlayer)
			{
				BlockValue block = world.GetBlock(Utils.Fastfloor(position.x), Utils.Fastfloor(boundingBox.min.y), Utils.Fastfloor(position.z));
				if (block.isair || block.Block.blockMaterial.IsGroundCover)
				{
					block = world.GetBlock(Utils.Fastfloor(position.x), Utils.Fastfloor(boundingBox.min.y - 1f), Utils.Fastfloor(position.z));
				}
				if (!block.isair)
				{
					num = Mathf.Clamp(1f - block.Block.blockMaterial.Friction, 0.01f, 1f);
				}
			}
		}
		if (!RootMotion || (!onGround && jumpTicks > 0))
		{
			float num2;
			if (onGround)
			{
				num2 = landMovementFactor;
				float num3 = 0.163f / (num * num * num);
				num2 *= num3;
			}
			else
			{
				num2 = jumpMovementFactor;
			}
			Move(_direction, _isDirAbsolute, num2, MaxVelocity);
		}
		if (Climbing)
		{
			fallDistance = 0f;
			entityCollision(motion);
			distanceClimbed += motion.magnitude;
			if (distanceClimbed > 0.5f)
			{
				internalPlayStepSound(1f);
				distanceClimbed = 0f;
			}
		}
		else
		{
			if (IsInElevator())
			{
				if (!RootMotion)
				{
					float num4 = 0.15f;
					if (motion.x < 0f - num4)
					{
						motion.x = 0f - num4;
					}
					if (motion.x > num4)
					{
						motion.x = num4;
					}
					if (motion.z < 0f - num4)
					{
						motion.z = 0f - num4;
					}
					if (motion.z > num4)
					{
						motion.z = num4;
					}
				}
				fallDistance = 0f;
			}
			if (IsSleeping)
			{
				motion.x = 0f;
				motion.z = 0f;
			}
			entityCollision(motion);
		}
		if (isSwimming)
		{
			motion.x *= 0.91f;
			motion.z *= 0.91f;
			motion.y -= world.Gravity * 0.025f;
			motion.y *= 0.91f;
			return;
		}
		motion.x *= num;
		motion.z *= num;
		if (!bInElevator)
		{
			motion.y -= world.Gravity;
		}
		motion.y *= 0.98f;
	}

	public virtual void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		if (AttachedToEntity != null)
		{
			return;
		}
		if (jumpIsMoving)
		{
			JumpMove();
			return;
		}
		if (RootMotion)
		{
			if (isEntityRemote && bodyDamage.CurrentStun == EnumEntityStunType.None && !IsDead() && (!(emodel != null) || !(emodel.avatarController != null) || !emodel.avatarController.IsAnimationHitRunning()))
			{
				accumulatedRootMotion = Vector3.zero;
				return;
			}
			bool flag = (bool)emodel && emodel.IsRagdollActive;
			if (isSwimming && !flag)
			{
				motion += accumulatedRootMotion * 0.001f;
			}
			else if (onGround || jumpTicks > 0)
			{
				if (flag)
				{
					motion.x = 0f;
					motion.z = 0f;
				}
				else
				{
					float y = motion.y;
					motion = accumulatedRootMotion;
					motion.y += y;
				}
			}
			accumulatedRootMotion = Vector3.zero;
		}
		if (IsFlyMode.Value)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			float num = ((primaryPlayer != null) ? primaryPlayer.GodModeSpeedModifier : 1f);
			float num2 = 2f * (MovementRunning ? 0.35f : 0.12f) * num;
			if (!RootMotion)
			{
				Move(_direction, _isDirAbsolute, GetPassiveEffectSpeedModifier() * num2, GetPassiveEffectSpeedModifier() * num2);
			}
			if (!IsNoCollisionMode.Value)
			{
				entityCollision(motion);
				motion *= ConditionalScalePhysicsMulConstant(0.546f);
			}
			else
			{
				SetPosition(position + motion);
				motion = Vector3.zero;
			}
		}
		else
		{
			DefaultMoveEntity(_direction, _isDirAbsolute);
		}
		if (isEntityRemote || !RootMotion)
		{
			return;
		}
		float num3 = landMovementFactor;
		num3 *= 2.5f;
		if (inWaterPercent > 0.3f)
		{
			if (num3 > 0.01f)
			{
				float t = (inWaterPercent - 0.3f) * 1.4285715f;
				num3 = Mathf.Lerp(num3, 0.01f + (num3 - 0.01f) * 0.1f, t);
			}
			if (isSwimming)
			{
				num3 = landMovementFactor * 5f;
			}
		}
		float magnitude = _direction.magnitude;
		if (magnitude > 1f)
		{
			num3 /= magnitude;
		}
		float num4 = _direction.z * num3;
		if (lerpForwardSpeed)
		{
			if (Utils.FastAbs(speedForwardTarget - num4) > 0.05f)
			{
				speedForwardTargetStep = Utils.FastAbs(num4 - speedForward) / 0.18f;
			}
			speedForwardTarget = num4;
		}
		else
		{
			speedForward = num4;
		}
		speedStrafe = _direction.x * num3;
		SetMovementState();
		ReplicateSpeeds();
	}

	public float GetPassiveEffectSpeedModifier()
	{
		if (IsCrouching)
		{
			if (MovementRunning)
			{
				return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, Constants.cPlayerSpeedModifierWalking, this);
			}
			return EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, Constants.cPlayerSpeedModifierCrouching, this);
		}
		if (MovementRunning)
		{
			return EffectManager.GetValue(PassiveEffects.RunSpeed, null, Constants.cPlayerSpeedModifierRunning, this);
		}
		return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, Constants.cPlayerSpeedModifierWalking, this);
	}

	public void SetMoveForward(float _moveForward)
	{
		moveDirection.x = 0f;
		moveDirection.z = _moveForward;
		isMoveDirAbsolute = false;
		Climbing = false;
		lerpForwardSpeed = true;
		motion.x = 0f;
		motion.z = 0f;
		accumulatedRootMotion.x = 0f;
		accumulatedRootMotion.z = 0f;
		if (bInElevator)
		{
			motion.y = 0f;
		}
	}

	public void SetMoveForwardWithModifiers(float _speedModifier, float _speedScale, bool _climb)
	{
		moveDirection.x = 0f;
		moveDirection.z = 1f;
		isMoveDirAbsolute = false;
		Climbing = _climb;
		lerpForwardSpeed = true;
		float num = speedModifier;
		speedModifier = _speedModifier * _speedScale;
		if (num > 0.2f)
		{
			num = speedModifier / num;
			accumulatedRootMotion.x *= num;
			accumulatedRootMotion.z *= num;
		}
	}

	public void AddMotion(float dir, float speed)
	{
		float f = dir * (MathF.PI / 180f);
		accumulatedRootMotion.x += Mathf.Sin(f) * speed;
		accumulatedRootMotion.z += Mathf.Cos(f) * speed;
	}

	public void MakeMotionMoveToward(float x, float z, float minMotion, float maxMotion)
	{
		if (RootMotion)
		{
			float num = Mathf.Sqrt(x * x + z * z);
			if (num > 0f)
			{
				num = Utils.FastClamp(Mathf.Sqrt(accumulatedRootMotion.x * accumulatedRootMotion.x + accumulatedRootMotion.z * accumulatedRootMotion.z), minMotion, maxMotion) / num;
				if (num < 1f)
				{
					x *= num;
					z *= num;
				}
			}
			accumulatedRootMotion.x = x;
			accumulatedRootMotion.z = z;
		}
		else
		{
			moveDirection.x = x;
			moveDirection.z = z;
			isMoveDirAbsolute = true;
		}
	}

	public bool IsInFrontOfMe(Vector3 _position)
	{
		Vector3 headPosition = getHeadPosition();
		Vector3 dir = _position - headPosition;
		Vector3 forwardVector = GetForwardVector();
		float angleBetween = Utils.GetAngleBetween(dir, forwardVector);
		float num = GetMaxViewAngle() * 0.5f;
		if (angleBetween < 0f - num || angleBetween > num)
		{
			return false;
		}
		return true;
	}

	public bool IsInViewCone(Vector3 _position)
	{
		Vector3 headPosition = getHeadPosition();
		Vector3 dir = _position - headPosition;
		Vector3 lookVector;
		float num;
		if (IsSleeping)
		{
			lookVector = sleeperLookDir;
			num = sleeperViewAngle;
		}
		else
		{
			lookVector = GetLookVector();
			num = GetMaxViewAngle();
		}
		num *= 0.5f;
		float angleBetween = Utils.GetAngleBetween(dir, lookVector);
		if (angleBetween < 0f - num || angleBetween > num)
		{
			return false;
		}
		return true;
	}

	public void DrawViewCone()
	{
		Vector3 lookVector;
		float num;
		if (IsSleeping)
		{
			lookVector = sleeperLookDir;
			num = sleeperViewAngle;
		}
		else
		{
			lookVector = GetLookVector();
			num = GetMaxViewAngle();
		}
		lookVector *= GetSeeDistance();
		num *= 0.5f;
		Vector3 start = getHeadPosition() - Origin.position;
		Debug.DrawRay(start, lookVector, new Color(0.9f, 0.9f, 0.5f), 0.1f);
		Vector3 dir = Quaternion.Euler(0f, 0f - num, 0f) * lookVector;
		Debug.DrawRay(start, dir, new Color(0.6f, 0.6f, 0.3f), 0.1f);
		Vector3 dir2 = Quaternion.Euler(0f, num, 0f) * lookVector;
		Debug.DrawRay(start, dir2, new Color(0.6f, 0.6f, 0.3f), 0.1f);
	}

	public bool CanSee(Vector3 _pos)
	{
		Vector3 headPosition = getHeadPosition();
		Vector3 direction = _pos - headPosition;
		float seeDistance = GetSeeDistance();
		if (direction.magnitude > seeDistance)
		{
			return false;
		}
		if (!IsInViewCone(_pos))
		{
			return false;
		}
		Ray ray = new Ray(headPosition, direction);
		ray.origin += direction.normalized * 0.2f;
		int modelLayer = GetModelLayer();
		SetModelLayer(2);
		bool result = true;
		if (Voxel.Raycast(world, ray, seeDistance, bHitTransparentBlocks: false, bHitNotCollidableBlocks: false))
		{
			result = false;
		}
		SetModelLayer(modelLayer);
		return result;
	}

	public bool CanEntityBeSeen(Entity _other)
	{
		Vector3 headPosition = getHeadPosition();
		Vector3 headPosition2 = _other.getHeadPosition();
		Vector3 direction = headPosition2 - headPosition;
		float magnitude = direction.magnitude;
		float num = GetSeeDistance();
		if (_other is EntityPlayer entityPlayer)
		{
			num *= entityPlayer.DetectUsScale(this);
		}
		if (magnitude > num)
		{
			return false;
		}
		if (!IsInViewCone(headPosition2))
		{
			return false;
		}
		bool result = false;
		Ray ray = new Ray(headPosition, direction);
		ray.origin += direction.normalized * -0.1f;
		int modelLayer = GetModelLayer();
		SetModelLayer(2);
		if (Voxel.Raycast(world, ray, num, -1612492821, 64, 0f))
		{
			if (Voxel.voxelRayHitInfo.tag == "E_Vehicle")
			{
				EntityVehicle entityVehicle = EntityVehicle.FindCollisionEntity(Voxel.voxelRayHitInfo.transform);
				if ((bool)entityVehicle && entityVehicle.IsAttached(_other))
				{
					result = true;
				}
			}
			else
			{
				if (Voxel.voxelRayHitInfo.tag.StartsWith("E_BP_"))
				{
					Voxel.voxelRayHitInfo.transform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				}
				if (_other.transform == Voxel.voxelRayHitInfo.transform)
				{
					result = true;
				}
			}
		}
		SetModelLayer(modelLayer);
		return result;
	}

	public virtual float GetSeeDistance()
	{
		senseScale = 1f;
		if (IsSleeping)
		{
			sightRange = sleeperSightRange;
			return sleeperSightRange;
		}
		sightRange = sightRangeBase;
		if (aiManager != null)
		{
			float num = EAIManager.CalcSenseScale();
			senseScale = 1f + num * aiManager.feralSense;
			sightRange = sightRangeBase * senseScale;
		}
		return sightRange;
	}

	public bool CanSeeStealth(float dist, float lightLevel)
	{
		float t = dist / sightRange;
		float num = Utils.FastLerp(sightLightThreshold.x, sightLightThreshold.y, t);
		if (lightLevel > num)
		{
			return true;
		}
		return false;
	}

	public float GetSeeStealthDebugScale(float dist)
	{
		float t = dist / sightRange;
		return Utils.FastLerp(sightLightThreshold.x, sightLightThreshold.y, t);
	}

	public override void SetAlive()
	{
		if (IsDead())
		{
			lastAliveTime = Time.time;
		}
		base.SetAlive();
		if (!isEntityRemote)
		{
			Stats.ResetStats();
		}
		Stats.Health.MaxModifier = 0f;
		Health = (int)Stats.Health.ModifiedMax;
		Stamina = Stats.Stamina.ModifiedMax;
		deathUpdateTime = 0;
		bDead = false;
		RecordedDamage.Fatal = false;
		emodel.SetAlive();
	}

	public float YawForTarget(Entity _otherEntity)
	{
		return YawForTarget(_otherEntity.GetPosition());
	}

	public float YawForTarget(Vector3 target)
	{
		float num = target.x - position.x;
		return 0f - (float)(Math.Atan2(target.z - position.z, num) * 180.0 / Math.PI) + 90f;
	}

	public void RotateTo(Entity _otherEntity, float _dYaw, float _dPitch)
	{
		float num = _otherEntity.position.x - position.x;
		float num2 = _otherEntity.position.z - position.z;
		float num3;
		if (_otherEntity is EntityAlive)
		{
			EntityAlive entityAlive = (EntityAlive)_otherEntity;
			num3 = position.y + GetEyeHeight() - (entityAlive.position.y + entityAlive.GetEyeHeight());
		}
		else
		{
			num3 = (_otherEntity.boundingBox.min.y + _otherEntity.boundingBox.max.y) / 2f - (position.y + GetEyeHeight());
		}
		float num4 = Mathf.Sqrt(num * num + num2 * num2);
		float intendedRotation = 0f - (float)(Math.Atan2(num2, num) * 180.0 / Math.PI) + 90f;
		float intendedRotation2 = (float)(0.0 - Math.Atan2(num3, num4) * 180.0 / Math.PI);
		rotation.x = UpdateRotation(rotation.x, intendedRotation2, _dPitch);
		rotation.y = UpdateRotation(rotation.y, intendedRotation, _dYaw);
	}

	public void RotateTo(float _x, float _y, float _z, float _dYaw, float _dPitch)
	{
		float num = _x - position.x;
		float num2 = _z - position.z;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float intendedRotation = 0f - (float)(Math.Atan2(num2, num) * 180.0 / Math.PI) + 90f;
		rotation.y = UpdateRotation(rotation.y, intendedRotation, _dYaw);
		if (_dPitch > 0f)
		{
			float intendedRotation2 = (float)(0.0 - Math.Atan2(_y - position.y, num3) * 180.0 / Math.PI);
			rotation.x = 0f - UpdateRotation(rotation.x, intendedRotation2, _dPitch);
		}
	}

	public static float UpdateRotation(float _curRotation, float _intendedRotation, float _maxIncr)
	{
		float num;
		for (num = _intendedRotation - _curRotation; num < -180f; num += 360f)
		{
		}
		while (num >= 180f)
		{
			num -= 360f;
		}
		if (num > _maxIncr)
		{
			num = _maxIncr;
		}
		if (num < 0f - _maxIncr)
		{
			num = 0f - _maxIncr;
		}
		return _curRotation + num;
	}

	public override float GetEyeHeight()
	{
		if (walkType == 21)
		{
			return 0.15f;
		}
		if (walkType == 22)
		{
			return 0.6f;
		}
		if (!IsCrouching)
		{
			return base.height * 0.8f;
		}
		return base.height * 0.5f;
	}

	public virtual float GetSpeedModifier()
	{
		return speedModifier;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _distance, Vector3 _fallMotion)
	{
		base.fallHitGround(_distance, _fallMotion);
		if (_distance > 2f)
		{
			int num = (int)((0f - _fallMotion.y - 0.85f) * 160f);
			if (num > 0)
			{
				DamageEntity(DamageSource.fall, num, _criticalHit: false);
			}
			PlayHitGroundSound();
		}
		if (!IsDead() && !emodel.IsRagdollActive && (disableFallBehaviorUntilOnGround || !ChooseFallBehavior(_distance, _fallMotion)) && (bool)emodel && (bool)emodel.avatarController)
		{
			emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
		}
		if (aiManager != null)
		{
			aiManager.FallHitGround(_distance);
		}
	}

	public bool NotifyDestroyedBlock(ItemActionAttack.AttackHitInfo attackHitInfo)
	{
		if (attackHitInfo != null && moveHelper != null && moveHelper.BlockedFlags > 0)
		{
			if (moveHelper.HitInfo.hit.blockPos == attackHitInfo.hitPosition)
			{
				moveHelper.ClearBlocked();
			}
			if (_destroyBlockBehaviors.Count == 0)
			{
				return false;
			}
			float num = 0f;
			weightBehaviorTemp.Clear();
			int num2 = GameStats.GetInt(EnumGameStats.GameDifficulty);
			WeightBehavior item = default(WeightBehavior);
			for (int i = 0; i < _destroyBlockBehaviors.Count; i++)
			{
				DestroyBlockBehavior destroyBlockBehavior = _destroyBlockBehaviors[i];
				if (num2 >= destroyBlockBehavior.Difficulty.min && num2 <= destroyBlockBehavior.Difficulty.max)
				{
					item.weight = destroyBlockBehavior.Weight + num;
					item.index = i;
					weightBehaviorTemp.Add(item);
					num += destroyBlockBehavior.Weight;
				}
			}
			bool result = false;
			if (num > 0f)
			{
				DestroyBlockBehavior destroyBlockBehavior2 = null;
				float num3 = rand.RandomFloat * num;
				for (int j = 0; j < weightBehaviorTemp.Count; j++)
				{
					if (num3 <= weightBehaviorTemp[j].weight)
					{
						destroyBlockBehavior2 = _destroyBlockBehaviors[weightBehaviorTemp[j].index];
						break;
					}
				}
				if (destroyBlockBehavior2 != null)
				{
					result = ExecuteDestroyBlockBehavior(destroyBlockBehavior2, attackHitInfo);
				}
			}
			return result;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExecuteDestroyBlockBehavior(DestroyBlockBehavior behavior, ItemActionAttack.AttackHitInfo attackHitInfo)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ChooseFallBehavior(float _distance, Vector3 _fallMotion)
	{
		if (fallBehaviors.Count == 0)
		{
			return false;
		}
		float num = 0f;
		weightBehaviorTemp.Clear();
		int num2 = GameStats.GetInt(EnumGameStats.GameDifficulty);
		WeightBehavior item = default(WeightBehavior);
		for (int i = 0; i < fallBehaviors.Count; i++)
		{
			FallBehavior fallBehavior = fallBehaviors[i];
			if (!(_distance < fallBehavior.Height.min) && !(_distance > fallBehavior.Height.max) && num2 >= fallBehavior.Difficulty.min && num2 <= fallBehavior.Difficulty.max)
			{
				item.weight = fallBehavior.Weight + num;
				item.index = i;
				weightBehaviorTemp.Add(item);
				num += fallBehavior.Weight;
			}
		}
		bool result = false;
		if (num > 0f)
		{
			FallBehavior fallBehavior2 = null;
			float num3 = rand.RandomFloat * num;
			for (int j = 0; j < weightBehaviorTemp.Count; j++)
			{
				if (num3 <= weightBehaviorTemp[j].weight)
				{
					fallBehavior2 = fallBehaviors[weightBehaviorTemp[j].index];
					break;
				}
			}
			if (fallBehavior2 != null)
			{
				result = ExecuteFallBehavior(fallBehavior2, _distance, _fallMotion);
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExecuteFallBehavior(FallBehavior behavior, float _distance, Vector3 _fallMotion)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayHitGroundSound()
	{
		if (soundLand == null || soundLand.Length == 0)
		{
			PlayOneShot("entityhitsground");
		}
		else
		{
			PlayOneShot(soundLand);
		}
	}

	public virtual bool FriendlyFireCheck(EntityAlive other)
	{
		return true;
	}

	public virtual bool HasImmunity(BuffClass _buffClass)
	{
		return false;
	}

	public int CalculateBlockDamage(BlockDamage block, int defaultBlockDamage, out bool bypassMaxDamage)
	{
		if (stompsSpikes && block.HasTag(BlockTags.Spike))
		{
			bypassMaxDamage = true;
			return 999;
		}
		bypassMaxDamage = false;
		return defaultBlockDamage;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
	{
		if (_damageSource.damageType == EnumDamageTypes.Suicide && (bool)emodel && emodel.avatarController is AvatarZombieController)
		{
			(emodel.avatarController as AvatarZombieController).CleanupDismemberedLimbs();
		}
		EnumDamageSource source = _damageSource.GetSource();
		if (_damageSource.IsIgnoreConsecutiveDamages() && source != EnumDamageSource.Internal)
		{
			if (damageSourceTimeouts.ContainsKey(source) && GameTimer.Instance.ticks - damageSourceTimeouts[source] < 30)
			{
				return -1;
			}
			damageSourceTimeouts[source] = GameTimer.Instance.ticks;
		}
		EntityAlive entityAlive = world.GetEntity(_damageSource.getEntityId()) as EntityAlive;
		if (!FriendlyFireCheck(entityAlive))
		{
			return -1;
		}
		bool flag = _damageSource.GetDamageType() == EnumDamageTypes.Heat;
		if (!flag && (bool)entityAlive && (entityFlags & entityAlive.entityFlags & EntityFlags.Zombie) != EntityFlags.None)
		{
			return -1;
		}
		if (IsGodMode.Value)
		{
			return -1;
		}
		if (!IsDead() && (bool)entityAlive)
		{
			float value = EffectManager.GetValue(PassiveEffects.DamageBonus, null, 0f, entityAlive);
			if (value > 0f)
			{
				_damageSource.DamageMultiplier = value;
				_damageSource.BonusDamageType = EnumDamageBonusType.Sneak;
			}
		}
		MinEventContext.Other = entityAlive;
		float num = Utils.FastMin(1f, EffectManager.GetValue(PassiveEffects.GeneralDamageResist, null, 0f, this));
		float num2 = (float)_strength * num + accumulatedDamageResisted;
		int num3 = Utils.FastMin(_strength, (int)num2);
		accumulatedDamageResisted = num2 - (float)num3;
		_strength -= num3;
		DamageResponse dmResponse = damageEntityLocal(_damageSource, _strength, _criticalHit, _impulseScale);
		NetPackage package = NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(entityId, dmResponse);
		if (world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
		else
		{
			int excludePlayer = -1;
			if (!flag && _damageSource.CreatorEntityId != -2)
			{
				excludePlayer = _damageSource.getEntityId();
				if (_damageSource.CreatorEntityId != -1)
				{
					Entity entity = world.GetEntity(_damageSource.CreatorEntityId);
					if ((bool)entity && !entity.isEntityRemote)
					{
						excludePlayer = -1;
					}
				}
			}
			world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityId, excludePlayer, package);
		}
		return dmResponse.ModStrength;
	}

	public virtual void SetDamagedTarget(EntityAlive _attackTarget)
	{
		damagedTarget = _attackTarget;
	}

	public virtual void ClearDamagedTarget()
	{
		damagedTarget = null;
	}

	public EntityAlive GetDamagedTarget()
	{
		return damagedTarget;
	}

	public override bool IsDead()
	{
		if (!base.IsDead())
		{
			return RecordedDamage.Fatal;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse _dmResponse = new DamageResponse
		{
			Source = _damageSource,
			Strength = _strength,
			Critical = _criticalHit,
			HitDirection = Utils.EnumHitDirection.None,
			MovementState = MovementState,
			Random = rand.RandomFloat,
			ImpulseScale = impulseScale,
			HitBodyPart = _damageSource.GetEntityDamageBodyPart(this),
			ArmorSlot = _damageSource.GetEntityDamageEquipmentSlot(this),
			ArmorSlotGroup = _damageSource.GetEntityDamageEquipmentSlotGroup(this)
		};
		if (_strength > 0)
		{
			_dmResponse.HitDirection = (_damageSource.Equals(DamageSource.fall) ? Utils.EnumHitDirection.Back : ((Utils.EnumHitDirection)Utils.Get4HitDirectionAsInt(_damageSource.getDirection(), GetLookVector())));
		}
		if (!GameManager.IsDedicatedServer && _damageSource.damageSource != EnumDamageSource.Internal && GameManager.Instance != null)
		{
			World world = GameManager.Instance.World;
			if (world != null && _damageSource.getEntityId() == world.GetPrimaryPlayerId())
			{
				Transform hitTransform = emodel.GetHitTransform(_damageSource);
				Vector3 vector = ((!hitTransform) ? emodel.transform.position : hitTransform.position);
				bool num = world.GetPrimaryPlayer().inventory.holdingItem.HasAnyTags(FastTags<TagGroup.Global>.Parse("ranged"));
				float magnitude = (world.GetPrimaryPlayer().GetPosition() - vector).magnitude;
				if (num && magnitude > HitSoundDistance)
				{
					Manager.PlayInsidePlayerHead("HitEntitySound");
				}
				if (ShowDebugDisplayHit)
				{
					Transform transform = (hitTransform ? hitTransform : emodel.transform);
					Vector3 vector2 = Camera.main.transform.position;
					DebugLines.CreateAttached("EntityDamage" + entityId, transform, vector2 + Origin.position, _damageSource.getHitTransformPosition(), new Color(0.3f, 0f, 0.3f), new Color(1f, 0f, 1f), DebugDisplayHitSize * 2f, DebugDisplayHitSize, DebugDisplayHitTime);
					DebugLines.CreateAttached("EntityDamage2" + entityId, transform, _damageSource.getHitTransformPosition(), transform.position + Origin.position, new Color(0f, 0f, 0.5f), new Color(0.3f, 0.3f, 1f), DebugDisplayHitSize * 2f, DebugDisplayHitSize, DebugDisplayHitTime);
				}
			}
		}
		if (_damageSource.AffectedByArmor())
		{
			equipment.CalcDamage(ref _dmResponse.Strength, ref _dmResponse.ArmorDamage, _dmResponse.Source.DamageTypeTag, MinEventContext.Other, _dmResponse.Source.AttackingItem);
		}
		float num2 = GetDamageFraction(_dmResponse.Strength);
		if (_dmResponse.Fatal || _dmResponse.Strength >= Health)
		{
			if ((_dmResponse.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
			{
				if (num2 >= 0.2f)
				{
					_dmResponse.Source.DismemberChance = Utils.FastMax(_dmResponse.Source.DismemberChance * 0.5f, 0.3f);
				}
			}
			else if (num2 >= 0.12f)
			{
				_dmResponse.Source.DismemberChance = Utils.FastMax(_dmResponse.Source.DismemberChance * 0.5f, 0.5f);
			}
			num2 = 1f;
			if (canDisintegrate)
			{
				Disintegrate();
			}
		}
		CheckDismember(ref _dmResponse, num2);
		int num3 = bodyDamage.StunKnee;
		int num4 = bodyDamage.StunProne;
		if ((_dmResponse.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None && _dmResponse.Dismember)
		{
			if (Health > 0)
			{
				_dmResponse.Strength = Health;
			}
		}
		else if (_damageSource.CanStun && GetWalkType() != 21 && bodyDamage.CurrentStun != EnumEntityStunType.Prone)
		{
			if ((_dmResponse.HitBodyPart & (EnumBodyPartHit.Arms | EnumBodyPartHit.Torso | EnumBodyPartHit.Head)) > EnumBodyPartHit.None)
			{
				num4 += _strength;
			}
			else if (_dmResponse.HitBodyPart.IsLeg())
			{
				num3 += _strength * ((!_criticalHit) ? 1 : 2);
			}
		}
		if ((!_dmResponse.HitBodyPart.IsLeg() || !_dmResponse.Dismember) && GetWalkType() != 21 && !sleepingOrWakingUp)
		{
			EntityClass entityClass = EntityClass.list[base.entityClass];
			if (GetDamageFraction(num4) >= entityClass.KnockdownProneDamageThreshold && entityClass.KnockdownProneDamageThreshold > 0f)
			{
				if (bodyDamage.CurrentStun != EnumEntityStunType.Prone)
				{
					_dmResponse.Stun = EnumEntityStunType.Prone;
					_dmResponse.StunDuration = rand.RandomRange(entityClass.KnockdownProneStunDuration.x, entityClass.KnockdownProneStunDuration.y);
				}
			}
			else if (GetDamageFraction(num3) >= entityClass.KnockdownKneelDamageThreshold && entityClass.KnockdownKneelDamageThreshold > 0f && bodyDamage.CurrentStun != EnumEntityStunType.Prone)
			{
				_dmResponse.Stun = EnumEntityStunType.Kneel;
				_dmResponse.StunDuration = rand.RandomRange(entityClass.KnockdownKneelStunDuration.x, entityClass.KnockdownKneelStunDuration.y);
			}
		}
		bool flag = false;
		int num5 = _dmResponse.Strength + _dmResponse.ArmorDamage / 2;
		if (num5 > 0 && !IsGodMode.Value && _dmResponse.Stun == EnumEntityStunType.None && !sleepingOrWakingUp)
		{
			flag = _dmResponse.Strength < Health;
			if (flag)
			{
				flag = GetWalkType() == 21 || !_dmResponse.Dismember || !_dmResponse.HitBodyPart.IsLeg();
			}
			if (flag && _dmResponse.Source.GetDamageType() != EnumDamageTypes.Bashing)
			{
				flag = num5 >= 6;
			}
			if (_dmResponse.Source.GetDamageType() == EnumDamageTypes.BarbedWire)
			{
				flag = true;
			}
		}
		_dmResponse.PainHit = flag;
		if (_dmResponse.Strength >= Health)
		{
			_dmResponse.Fatal = true;
		}
		if (_dmResponse.Fatal)
		{
			_dmResponse.Stun = EnumEntityStunType.None;
		}
		if (isEntityRemote)
		{
			_dmResponse.ModStrength = 0;
		}
		else
		{
			if (Health <= _dmResponse.Strength)
			{
				_strength -= Health;
			}
			_dmResponse.ModStrength = _strength;
		}
		if (_dmResponse.Dismember)
		{
			EntityAlive entityAlive = base.world.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.FireEvent(MinEventTypes.onDismember);
			}
		}
		if (MinEventContext.Other != null)
		{
			MinEventContext.Other.MinEventContext.DamageResponse = _dmResponse;
			float value = EffectManager.GetValue(PassiveEffects.HealthSteal, null, 0f, MinEventContext.Other);
			if (value != 0f)
			{
				int num6 = (int)((float)num5 * value);
				if (num6 + MinEventContext.Other.Health <= 0)
				{
					num6 = (MinEventContext.Other.Health - 1) * -1;
				}
				MinEventContext.Other.AddHealth(num6);
				if (num6 < 0 && MinEventContext.Other is EntityPlayerLocal)
				{
					((EntityPlayerLocal)MinEventContext.Other).ForceBloodSplatter();
				}
			}
		}
		FireAttackedEvents(_dmResponse);
		ProcessDamageResponseLocal(_dmResponse);
		return _dmResponse;
	}

	public override void FireAttackedEvents(DamageResponse _dmResponse)
	{
		base.FireAttackedEvents(_dmResponse);
		if (_dmResponse.Source.BuffClass == null || Progression != null)
		{
			MinEventContext.DamageResponse = _dmResponse;
			EntityAlive entityAlive = world.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
			if ((bool)entityAlive && !entityAlive.isEntityRemote)
			{
				MinEventContext.IsLocal = this is EntityPlayer && isEntityRemote;
			}
			if (_dmResponse.Source.BuffClass == null)
			{
				FireEvent(MinEventTypes.onOtherAttackedSelf);
			}
			else if (Progression != null)
			{
				Progression.FireEvent(MinEventTypes.onOtherAttackedSelf, MinEventContext);
			}
			MinEventContext.IsLocal = false;
		}
	}

	public override void ProcessDamageResponse(DamageResponse _dmResponse)
	{
		if (Time.time - lastAliveTime < 1f)
		{
			return;
		}
		base.ProcessDamageResponse(_dmResponse);
		ProcessDamageResponseLocal(_dmResponse);
		if (!world.IsRemote())
		{
			Entity entity = world.GetEntity(_dmResponse.Source.getEntityId());
			if ((bool)entity && !entity.isEntityRemote && isEntityRemote && this is EntityPlayer)
			{
				world.entityDistributer.SendPacketToTrackedPlayers(entityId, entityId, NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(entityId, _dmResponse));
			}
			else if (_dmResponse.Source.BuffClass != null)
			{
				world.entityDistributer.SendPacketToTrackedPlayers(entityId, entityId, NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(entityId, _dmResponse));
			}
			else
			{
				world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityId, _dmResponse.Source.getEntityId(), NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(entityId, _dmResponse));
			}
		}
	}

	public virtual void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (emodel == null)
		{
			return;
		}
		if (_dmResponse.Source.BonusDamageType != EnumDamageBonusType.None)
		{
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if ((bool)primaryPlayer && primaryPlayer.entityId == _dmResponse.Source.getEntityId())
			{
				switch (_dmResponse.Source.BonusDamageType)
				{
				case EnumDamageBonusType.Sneak:
					primaryPlayer.NotifySneakDamage(_dmResponse.Source.DamageMultiplier);
					break;
				case EnumDamageBonusType.Stun:
					primaryPlayer.NotifyDamageMultiplier(_dmResponse.Source.DamageMultiplier);
					break;
				}
			}
		}
		EntityAlive entityAlive = world.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.SetDamagedTarget(this);
		}
		if (IsSleeperPassive)
		{
			world.CheckSleeperVolumeNoise(position);
		}
		ConditionalTriggerSleeperWakeUp();
		SleeperSupressLivingSounds = false;
		bPlayHurtSound = false;
		if (equipment != null && _dmResponse.ArmorDamage > 0)
		{
			List<ItemValue> armor = equipment.GetArmor();
			if (armor.Count > 0)
			{
				float num = (float)_dmResponse.ArmorDamage / (float)armor.Count;
				if (num < 1f && num != 0f)
				{
					num = 1f;
				}
				for (int i = 0; i < armor.Count; i++)
				{
					armor[i].UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, armor[i], num, this, null, armor[i].ItemClass.ItemTags);
				}
			}
		}
		ApplyLocalBodyDamage(_dmResponse);
		lastHitRanged = false;
		lastDamageResponse = _dmResponse;
		bool flag = EffectManager.GetValue(PassiveEffects.NegateDamageSelf, null, 0f, this, null, FastTags<TagGroup.Global>.Parse(_dmResponse.HitBodyPart.ToString())) > 0f || EffectManager.GetValue(PassiveEffects.NegateDamageOther, (entityAlive != null) ? entityAlive.inventory.holdingItemItemValue : null, 0f, entityAlive) > 0f;
		if (_dmResponse.Dismember && !flag)
		{
			lastHitImpactDir = _dmResponse.Source.getDirection();
			if (entityAlive != null)
			{
				lastHitEntityFwd = entityAlive.GetForwardVector();
			}
			if (_dmResponse.Source.ItemClass != null && _dmResponse.Source.ItemClass.HasAnyTags(DismembermentManager.rangedTags))
			{
				lastHitRanged = true;
			}
			if (_dmResponse.Source.ItemClass != null)
			{
				float strength = (float)_dmResponse.ModStrength / (float)GetMaxHealth();
				lastHitForce = DismembermentManager.GetImpactForce(_dmResponse.Source.ItemClass, strength);
			}
			ExecuteDismember(restoreState: false);
		}
		bool flag2 = _dmResponse.Stun != EnumEntityStunType.None;
		bool flag3 = bodyDamage.CurrentStun != EnumEntityStunType.None;
		if (!flag && _dmResponse.Fatal && isEntityRemote)
		{
			ClientKill(_dmResponse);
		}
		else if (flag2 && (bool)emodel.avatarController)
		{
			if (_dmResponse.Stun == EnumEntityStunType.Prone)
			{
				if (bodyDamage.CurrentStun == EnumEntityStunType.None)
				{
					if ((_dmResponse.Critical && _dmResponse.Source.damageType == EnumDamageTypes.Bashing) || rand.RandomFloat < 0.6f)
					{
						DoRagdoll(_dmResponse);
					}
					else
					{
						emodel.avatarController.BeginStun(EnumEntityStunType.Prone, _dmResponse.HitBodyPart, _dmResponse.HitDirection, _dmResponse.Critical, _dmResponse.Random);
					}
					SetStun(EnumEntityStunType.Prone);
					bodyDamage.StunDuration = _dmResponse.StunDuration;
				}
				else if (bodyDamage.CurrentStun != EnumEntityStunType.Prone)
				{
					DoRagdoll(_dmResponse);
					SetStun(EnumEntityStunType.Prone);
					bodyDamage.StunDuration = _dmResponse.StunDuration * 0.5f;
				}
			}
			else if (_dmResponse.Stun == EnumEntityStunType.Kneel)
			{
				bool flag4 = false;
				if (bodyDamage.CurrentStun == EnumEntityStunType.None)
				{
					if (_dmResponse.Critical || rand.RandomFloat < 0.25f)
					{
						flag4 = true;
					}
					else
					{
						SetStun(EnumEntityStunType.Kneel);
						emodel.avatarController.BeginStun(EnumEntityStunType.Kneel, _dmResponse.HitBodyPart, _dmResponse.HitDirection, _dmResponse.Critical, _dmResponse.Random);
					}
				}
				else if (bodyDamage.CurrentStun == EnumEntityStunType.Kneel)
				{
					flag4 = true;
				}
				if (flag4)
				{
					DoRagdoll(_dmResponse);
					SetStun(EnumEntityStunType.Prone);
				}
				bodyDamage.StunDuration = _dmResponse.StunDuration;
			}
		}
		else if (_dmResponse.PainHit && !flag3 && (bool)emodel.avatarController)
		{
			EntityClass entityClass = EntityClass.list[base.entityClass];
			float num2 = entityClass.PainResistPerHit;
			if (num2 >= 0f)
			{
				float num3 = GetMaxHealth();
				if ((float)Health / num3 < entityClass.PainResistPerHitLowHealthPercent)
				{
					num2 = entityClass.PainResistPerHitLowHealth;
				}
				painResistPercent = Utils.FastMin(painResistPercent + num2, 3f);
				float duration = float.MaxValue;
				if (painResistPercent >= 3f && num2 >= 1f)
				{
					duration = 0f;
					painHitsFelt += 0.15f;
				}
				else if (painResistPercent >= 1f)
				{
					duration = Utils.FastLerp(0.5f, 0.15f, (painResistPercent - 1f) * 0.75f);
					painHitsFelt += 0.3f;
				}
				else
				{
					painHitsFelt += Utils.FastLerp(1f, 0.3f, painResistPercent);
				}
				emodel.avatarController.StartAnimationHit(_dmResponse.HitBodyPart, (int)_dmResponse.HitDirection, (int)((float)_dmResponse.Strength * 100f / num3), _dmResponse.Critical, _dmResponse.MovementState, _dmResponse.Random, duration);
			}
		}
		if (bodyDamage.CurrentStun == EnumEntityStunType.None)
		{
			if (_dmResponse.Source.CanStun)
			{
				if ((_dmResponse.HitBodyPart & (EnumBodyPartHit.Arms | EnumBodyPartHit.Torso | EnumBodyPartHit.Head)) > EnumBodyPartHit.None)
				{
					bodyDamage.StunProne += _dmResponse.Strength;
				}
				else if (_dmResponse.HitBodyPart.IsLeg())
				{
					bodyDamage.StunKnee += _dmResponse.Strength;
				}
			}
		}
		else
		{
			bodyDamage.StunProne = 0;
			bodyDamage.StunKnee = 0;
		}
		bool flag5 = Health <= 0;
		if (Health <= 0 && deathUpdateTime > 0)
		{
			DeathHealth -= _dmResponse.Strength;
		}
		int num4 = _dmResponse.Strength;
		if (EffectManager.GetValue(PassiveEffects.HeadShotOnly, null, 0f, GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive) > 0f && (_dmResponse.HitBodyPart & EnumBodyPartHit.Head) == 0)
		{
			num4 = 0;
			_dmResponse.Fatal = false;
		}
		if (flag)
		{
			num4 = 0;
			_dmResponse.Fatal = false;
		}
		if (isEntityRemote)
		{
			Health -= num4;
			RecordedDamage = _dmResponse;
		}
		else
		{
			if (!IsGodMode.Value)
			{
				Health -= num4;
				if (_dmResponse.Fatal && Health > 0)
				{
					Health = 0;
				}
				hasBeenAttackedTime = 0;
				if (_dmResponse.PainHit)
				{
					hasBeenAttackedTime = GetMaxAttackTime();
				}
			}
			bPlayHurtSound = (bBeenWounded = num4 > 0);
			if (bBeenWounded)
			{
				setBeenAttacked();
				MinEventContext.Other = GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
				FireEvent(MinEventTypes.onOtherDamagedSelf);
			}
			if (num4 > woundedStrength)
			{
				woundedStrength = _dmResponse.Strength;
				woundedDamageSource = _dmResponse.Source;
			}
			lastHitDirection = _dmResponse.HitDirection;
			if (Health <= 0)
			{
				_dmResponse.Source.getDirection();
				_dmResponse.Strength += Health;
				Entity entity = ((_dmResponse.Source.getEntityId() != -1) ? world.GetEntity(_dmResponse.Source.getEntityId()) : null);
				if (Spawned && !flag5)
				{
					if (entity is EntityAlive)
					{
						entityThatKilledMe = (EntityAlive)entity;
					}
					else
					{
						entityThatKilledMe = null;
					}
				}
				Kill(_dmResponse);
				if (!_dmResponse.Fatal && world.IsRemote())
				{
					DamageEntity(DamageSource.disease, 1, _criticalHit: false);
				}
			}
		}
		Entity entity2 = ((_dmResponse.Source.getEntityId() != -1) ? world.GetEntity(_dmResponse.Source.getEntityId()) : null);
		if (entity2 != null && entity2 != this)
		{
			if (entity2 is EntityAlive && !isEntityRemote && !entity2.IsIgnoredByAI())
			{
				SetRevengeTarget((EntityAlive)entity2);
				if (aiManager != null)
				{
					aiManager.DamagedByEntity();
				}
			}
			if (entity2 is EntityPlayer)
			{
				((EntityPlayer)entity2).FireEvent(MinEventTypes.onCombatEntered);
			}
			FireEvent(MinEventTypes.onCombatEntered);
		}
		if (_dmResponse.Strength > 0 && _dmResponse.Source.GetDamageType() == EnumDamageTypes.Electrical)
		{
			Electrocuted = true;
		}
		if (!GameManager.IsDedicatedServer && DamageText.Enabled && (world.GetPrimaryPlayer().cameraTransform.position + Origin.position - position).sqrMagnitude < 225f)
		{
			string text = $"{_dmResponse.Strength}";
			Color color = (((_dmResponse.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None) ? Color.red : Color.yellow);
			if (_dmResponse.Critical)
			{
				color.b = 0.8f;
			}
			DamageText.Create(text, color, getHeadPosition() + new Vector3(0f, 0.1f, 0f), new Vector3(rand.RandomRange(-0.7f, 0.7f), 0.8f, rand.RandomRange(-0.7f, 0.7f)), 0.22f);
		}
		RecordedDamage = _dmResponse;
	}

	public bool CanUseHeavyArmorSound()
	{
		foreach (ItemValue item in equipment.GetArmor())
		{
			if (item.ItemClass.MadeOfMaterial.id == "MarmorHeavy")
			{
				return true;
			}
		}
		return false;
	}

	public EntityAlive GetRevengeTarget()
	{
		return revengeEntity;
	}

	public void SetRevengeTarget(EntityAlive _other)
	{
		revengeEntity = _other;
		revengeTimer = ((!(revengeEntity == null)) ? 500 : 0);
	}

	public void SetRevengeTimer(int ticks)
	{
		revengeTimer = ticks;
	}

	public override bool CanBePushed()
	{
		return !IsDead();
	}

	public override bool CanCollideWith(Entity _other)
	{
		if (!IsDead() && !(_other is EntityItem))
		{
			return !(_other is EntitySupplyCrate);
		}
		return false;
	}

	public override bool CanCollideWithBlocks()
	{
		if (IsSleeping)
		{
			return false;
		}
		return true;
	}

	public void DoRagdoll(DamageResponse _dmResponse)
	{
		emodel.DoRagdoll(_dmResponse, _dmResponse.StunDuration);
	}

	public void AddScore(int _diedMySelfTimes, int _zombieKills, int _playerKills, int _otherTeamnumber, int _conditions)
	{
		KilledZombies += _zombieKills;
		KilledPlayers += _playerKills;
		Died += _diedMySelfTimes;
		Score += _zombieKills * GameStats.GetInt(EnumGameStats.ScoreZombieKillMultiplier) + _playerKills * GameStats.GetInt(EnumGameStats.ScorePlayerKillMultiplier) + _diedMySelfTimes * GameStats.GetInt(EnumGameStats.ScoreDiedMultiplier);
		if (Score < 0)
		{
			Score = 0;
		}
		if (this is EntityPlayerLocal)
		{
			if (_diedMySelfTimes > 0)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.Deaths, _diedMySelfTimes);
			}
			if (_zombieKills > 0)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.ZombiesKilled, _zombieKills);
			}
			if (_playerKills > 0)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.PlayersKilled, _playerKills);
			}
			if ((_conditions & 2) != 0)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.KilledWith44Magnum, 1);
			}
		}
	}

	public virtual void AwardKill(EntityAlive killer)
	{
		if (!(killer != null) || !(killer != this))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int conditions = 0;
		switch (entityType)
		{
		case EntityType.Player:
			num2++;
			break;
		case EntityType.Zombie:
			num++;
			break;
		}
		EntityPlayer entityPlayer = killer as EntityPlayer;
		if ((bool)entityPlayer)
		{
			GameManager.Instance.AwardKill(killer, this);
			if (entityPlayer.inventory.IsHoldingGun() && entityPlayer.inventory.holdingItem.Name.Equals("gunHandgunT2Magnum44"))
			{
				conditions = 2;
			}
			GameManager.Instance.AddScoreServer(killer.entityId, num, num2, TeamNumber, conditions);
		}
	}

	public virtual void OnEntityDeath()
	{
		if (deathUpdateTime != 0)
		{
			return;
		}
		AddScore(1, 0, 0, -1, 0);
		if (soundLiving != null && soundLivingID >= 0)
		{
			Manager.Stop(entityId, soundLiving);
			soundLivingID = -1;
		}
		if ((bool)AttachedToEntity)
		{
			Detach();
		}
		if (!isEntityRemote)
		{
			AwardKill(entityThatKilledMe);
			if (particleOnDeath != null && particleOnDeath.Length > 0)
			{
				float lightBrightness = world.GetLightBrightness(GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(particleOnDeath, getHeadPosition(), lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityId);
			}
			if (isGameMessageOnDeath())
			{
				GameManager.Instance.GameMessage(EnumGameMessages.EntityWasKilled, this, entityThatKilledMe);
			}
			if (entityThatKilledMe != null)
			{
				Log.Out("Entity {0} {1} killed by {2} {3}", GetDebugName(), entityId, entityThatKilledMe.GetDebugName(), entityThatKilledMe.entityId);
			}
			else
			{
				Log.Out("Entity {0} {1} killed", GetDebugName(), entityId);
			}
			ModEvents.SEntityKilledData _data = new ModEvents.SEntityKilledData(this, entityThatKilledMe);
			ModEvents.EntityKilled.Invoke(ref _data);
			dropItemOnDeath();
			entityThatKilledMe = null;
		}
	}

	public void Disintegrate()
	{
		timeStayAfterDeath = 0;
		isDisintegrated = true;
	}

	public virtual void PlayGiveUpSound()
	{
		if (soundGiveUp != null)
		{
			PlayOneShot(soundGiveUp);
		}
	}

	public virtual Vector3 GetCameraLook(float _t)
	{
		return GetLookVector();
	}

	public Vector3 GetForwardVector()
	{
		float num = Mathf.Cos(rotation.y * 0.0175f - MathF.PI);
		float num2 = Mathf.Sin(rotation.y * 0.0175f - MathF.PI);
		float num3 = 0f - Mathf.Cos(0f);
		float y = Mathf.Sin(0f);
		return new Vector3(num2 * num3, y, num * num3);
	}

	public Vector2 GetForwardVector2()
	{
		float f = rotation.y * (MathF.PI / 180f);
		return new Vector2(y: Mathf.Cos(f), x: Mathf.Sin(f));
	}

	public virtual Vector3 GetLookVector()
	{
		float num = Mathf.Cos(rotation.y * 0.0175f - MathF.PI);
		float num2 = Mathf.Sin(rotation.y * 0.0175f - MathF.PI);
		float num3 = 0f - Mathf.Cos(rotation.x * 0.0175f);
		float y = Mathf.Sin(rotation.x * 0.0175f);
		return new Vector3(num2 * num3, y, num * num3);
	}

	public virtual Vector3 GetLookVector(Vector3 _altLookVector)
	{
		return GetLookVector();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetSoundRandomTicks()
	{
		return rand.RandomRange(soundRandomTicks / 2, soundRandomTicks);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetSoundAlertTicks()
	{
		return rand.RandomRange(soundAlertTicks / 2, soundAlertTicks);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundRandom()
	{
		return soundRandom;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundJump()
	{
		return soundJump;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundHurt(DamageSource _damageSource, int _damageStrength)
	{
		return soundHurt;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundHurtSmall()
	{
		return soundHurtSmall;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundHurt()
	{
		return soundHurt;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundDrownPain()
	{
		return soundDrownPain;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundDeath(DamageSource _damageSource)
	{
		return soundDeath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundAttack()
	{
		return soundAttack;
	}

	public virtual string GetSoundAlert()
	{
		return soundAlert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundStamina()
	{
		return soundStamina;
	}

	public virtual Ray GetLookRay()
	{
		return new Ray(position + new Vector3(0f, GetEyeHeight(), 0f), GetLookVector());
	}

	public virtual Ray GetMeleeRay()
	{
		return GetLookRay();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void dropItemOnDeath()
	{
		for (int i = 0; i < inventory.GetItemCount(); i++)
		{
			ItemStack item = inventory.GetItem(i);
			ItemClass forId = ItemClass.GetForId(item.itemValue.type);
			if (forId != null && forId.CanDrop())
			{
				world.GetGameManager().ItemDropServer(item, position, new Vector3(0.5f, 0f, 0.5f), -1, Constants.cItemDroppedOnDeathLifetime);
				inventory.SetItem(i, ItemValue.None.Clone(), 0);
			}
		}
		inventory.SetFlashlight(on: false);
		equipment.DropItems();
		if (world.IsDark())
		{
			lootDropProb *= 1f;
		}
		if ((bool)entityThatKilledMe)
		{
			lootDropProb = EffectManager.GetValue(PassiveEffects.LootDropProb, entityThatKilledMe.inventory.holdingItemItemValue, lootDropProb, entityThatKilledMe);
		}
		if (lootDropProb > rand.RandomFloat)
		{
			GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, new Vector3i(position), entityId);
		}
	}

	public virtual Vector3 GetDropPosition()
	{
		if (ParachuteWearing || JetpackWearing)
		{
			return base.transform.position + base.transform.forward - Vector3.up * 0.3f + Origin.position;
		}
		return base.transform.position + base.transform.forward + Vector3.up + Origin.position;
	}

	public virtual void OnFired()
	{
		if (emodel.avatarController != null)
		{
			emodel.avatarController.StartAnimationFiring();
		}
	}

	public virtual void OnReloadStart()
	{
		if (emodel.avatarController != null)
		{
			emodel.avatarController.StartAnimationReloading();
		}
	}

	public virtual void OnReloadEnd()
	{
	}

	public virtual bool WillForceToFollow(EntityAlive _other)
	{
		return false;
	}

	public void AddHealth(int _v)
	{
		if (Health > 0)
		{
			Health += _v;
		}
	}

	public void AddStamina(float _v)
	{
		if (entityStats.Stamina != null && Health > 0)
		{
			entityStats.Stamina.Value += _v;
		}
	}

	public void AddWater(float _v)
	{
		Stats.Water.Value += _v;
	}

	public int GetTicksNoPlayerAdjacent()
	{
		return ticksNoPlayerAdjacent;
	}

	public bool CanSee(EntityAlive _other)
	{
		return seeCache.CanSee(_other);
	}

	public void SetCanSee(EntityAlive _other)
	{
		seeCache.SetCanSee(_other);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateTasks()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
		{
			SetMoveForwardWithModifiers(0f, 0f, _climb: false);
			if (aiManager != null)
			{
				aiManager.UpdateDebugName();
			}
			return;
		}
		CheckDespawn();
		seeCache.ClearIfExpired();
		bool useAIPackages = EntityClass.list[entityClass].UseAIPackages;
		aiActiveDelay -= aiActiveScale;
		if (aiActiveDelay <= 0f)
		{
			aiActiveDelay = 1f;
			if (!useAIPackages)
			{
				aiManager.Update();
			}
			else
			{
				UAIBase.Update(utilityAIContext);
			}
		}
		PathInfo path = PathFinderThread.Instance.GetPath(entityId);
		if (path.path != null)
		{
			bool flag = true;
			if (!useAIPackages)
			{
				flag = aiManager.CheckPath(path);
			}
			if (flag)
			{
				navigator.SetPath(path, path.speed);
			}
		}
		navigator.UpdateNavigation();
		moveHelper.UpdateMoveHelper();
		lookHelper.onUpdateLook();
		if (distraction != null && (distraction.IsDead() || distraction.IsMarkedForUnload()))
		{
			distraction = null;
		}
		if (pendingDistraction != null && (pendingDistraction.IsDead() || pendingDistraction.IsMarkedForUnload()))
		{
			pendingDistraction = null;
		}
	}

	public PathNavigate getNavigator()
	{
		return navigator;
	}

	public void FindPath(Vector3 targetPos, float moveSpeed, bool canBreak, EAIBase behavior)
	{
		Vector3 vector = targetPos - position;
		if (vector.x * vector.x + vector.z * vector.z > 1225f)
		{
			if (vector.y > 45f)
			{
				targetPos.y = position.y + 45f;
			}
			else if (vector.y < -45f)
			{
				targetPos.y = position.y - 45f;
			}
		}
		PathFinderThread.Instance.FindPath(this, targetPos, moveSpeed, canBreak, behavior);
	}

	public bool isWithinHomeDistanceCurrentPosition()
	{
		return isWithinHomeDistance(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y), Utils.Fastfloor(position.z));
	}

	public bool isWithinHomeDistance(int _x, int _y, int _z)
	{
		if (maximumHomeDistance < 0)
		{
			return true;
		}
		return homePosition.getDistanceSquared(_x, _y, _z) < (float)(maximumHomeDistance * maximumHomeDistance);
	}

	public void setHomeArea(Vector3i _pos, int _maxDistance)
	{
		homePosition.position = _pos;
		maximumHomeDistance = _maxDistance;
	}

	public ChunkCoordinates getHomePosition()
	{
		return homePosition;
	}

	public int getMaximumHomeDistance()
	{
		return maximumHomeDistance;
	}

	public void detachHome()
	{
		maximumHomeDistance = -1;
	}

	public bool hasHome()
	{
		return maximumHomeDistance >= 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool canDespawn()
	{
		if (!IsClientControlled() && GetSpawnerSource() != EnumSpawnerSource.StaticSpawner)
		{
			return !IsSleeping;
		}
		return false;
	}

	public void ResetDespawnTime()
	{
		ticksNoPlayerAdjacent = 0;
		seeCache.SetLastTimePlayerSeen();
	}

	public void CheckDespawn()
	{
		if (isEntityRemote)
		{
			return;
		}
		if (!CanUpdateEntity() && bIsChunkObserver && world.GetClosestPlayer(this, -1f, _isDead: false) == null)
		{
			MarkToUnload();
		}
		else
		{
			if (!canDespawn() || ++despawnDelayCounter < 20)
			{
				return;
			}
			despawnDelayCounter = 0;
			ticksNoPlayerAdjacent += 20;
			EnumSpawnerSource enumSpawnerSource = GetSpawnerSource();
			EntityPlayer closestPlayer = world.GetClosestPlayer(this, -1f, _isDead: false);
			switch (enumSpawnerSource)
			{
			case EnumSpawnerSource.Dynamic:
				if (!closestPlayer)
				{
					if (!world.GetClosestPlayer(this, -1f, _isDead: true))
					{
						Despawn();
					}
					return;
				}
				break;
			case EnumSpawnerSource.Biome:
				if (!world.GetClosestPlayer(this, 130f, _isDead: false))
				{
					if ((bool)world.GetClosestPlayer(this, 20f, _isDead: true))
					{
						isDespawnWhenPlayerFar = true;
					}
					else if (isDespawnWhenPlayerFar)
					{
						Despawn();
					}
				}
				break;
			}
			if (!closestPlayer)
			{
				return;
			}
			float sqrMagnitude = (closestPlayer.position - position).sqrMagnitude;
			if (sqrMagnitude < 6400f)
			{
				ticksNoPlayerAdjacent = 0;
			}
			int num = int.MaxValue;
			float lastTimePlayerSeen = seeCache.GetLastTimePlayerSeen();
			if (lastTimePlayerSeen > 0f)
			{
				num = (int)(Time.time - lastTimePlayerSeen);
			}
			switch (enumSpawnerSource)
			{
			case EnumSpawnerSource.Dynamic:
				if ((bool)attackTarget)
				{
					num = 0;
				}
				if (IsSleeper && !IsSleeping)
				{
					if (sqrMagnitude > 9216f && num > 80)
					{
						Despawn();
					}
				}
				else if (sqrMagnitude > 2304f && num > 60 && !HasInvestigatePosition)
				{
					Despawn();
				}
				else if (ticksNoPlayerAdjacent > 1800)
				{
					Despawn();
				}
				break;
			case EnumSpawnerSource.Biome:
				if (ticksNoPlayerAdjacent > 100 && sqrMagnitude > 16384f)
				{
					Despawn();
				}
				else if (ticksNoPlayerAdjacent > 1800)
				{
					Despawn();
				}
				break;
			case EnumSpawnerSource.StaticSpawner:
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Despawn()
	{
		IsDespawned = true;
		MarkToUnload();
	}

	public void ForceDespawn()
	{
		Despawn();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EntityAlive GetAttackTarget()
	{
		return attackTarget;
	}

	public virtual Vector3 GetAttackTargetHitPosition()
	{
		return attackTarget.getChestPosition();
	}

	public EntityAlive GetAttackTargetLocal()
	{
		if (isEntityRemote)
		{
			return attackTargetClient;
		}
		return attackTarget;
	}

	public void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime)
	{
		if (_attackTarget == attackTarget)
		{
			attackTargetTime = _attackTargetTime;
			return;
		}
		if ((bool)attackTarget)
		{
			attackTargetLast = attackTarget;
		}
		targetAlertChanged = false;
		if ((bool)_attackTarget)
		{
			if (_attackTarget != attackTargetLast)
			{
				targetAlertChanged = true;
				soundDelayTicks = rand.RandomRange(5, 20);
			}
			investigatePositionTicks = 0;
		}
		if (!isEntityRemote)
		{
			world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityId, -1, NetPackageManager.GetPackage<NetPackageSetAttackTarget>().Setup(entityId, _attackTarget ? _attackTarget.entityId : (-1)));
		}
		attackTarget = _attackTarget;
		attackTargetTime = _attackTargetTime;
	}

	public void SetAttackTargetClient(EntityAlive _attackTarget)
	{
		attackTargetClient = _attackTarget;
	}

	public int GetInvestigatePositionTicks()
	{
		return investigatePositionTicks;
	}

	public void ClearInvestigatePosition()
	{
		investigatePos = Vector3.zero;
		investigatePositionTicks = 0;
		ResetDespawnTime();
		int num = rand.RandomRange(20, 35) * 20;
		if (entityType == EntityType.Zombie)
		{
			num /= 2;
		}
		SetAlertTicks(num);
	}

	public int CalcInvestigateTicks(int _ticks, EntityAlive _investigateEntity)
	{
		float value = EffectManager.GetValue(PassiveEffects.EnemySearchDuration, null, 1f, _investigateEntity, null, EntityClass.list[entityClass].Tags);
		return (int)((float)_ticks / value);
	}

	public void SetInvestigatePosition(Vector3 pos, int ticks, bool isAlert = true)
	{
		investigatePos = pos;
		investigatePositionTicks = ticks;
		isInvestigateAlert = isAlert;
	}

	public int GetAlertTicks()
	{
		return alertTicks;
	}

	public void SetAlertTicks(int ticks)
	{
		alertTicks = ticks;
	}

	public EntitySeeCache GetEntitySenses()
	{
		return seeCache;
	}

	public virtual float GetMoveSpeed()
	{
		if (IsBloodMoon || world.IsDark())
		{
			return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, moveSpeedNight, this);
		}
		return EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, moveSpeed, this);
	}

	public virtual float GetMoveSpeedAggro()
	{
		if (IsBloodMoon || world.IsDark())
		{
			return EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedAggroMax, this);
		}
		return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, moveSpeedAggro, this);
	}

	public float GetMoveSpeedPanic()
	{
		return EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeedPanic, this);
	}

	public override float GetWeight()
	{
		return weight;
	}

	public override float GetPushFactor()
	{
		return pushFactor;
	}

	public virtual bool CanEntityJump()
	{
		return true;
	}

	public void SetMaxViewAngle(float _angle)
	{
		maxViewAngle = _angle;
	}

	public virtual float GetMaxViewAngle()
	{
		return maxViewAngle;
	}

	public void SetSightLightThreshold(Vector2 _threshold)
	{
		sightLightThreshold = _threshold;
	}

	public int GetModelLayer()
	{
		return emodel.GetModelTransform().gameObject.layer;
	}

	public virtual void SetModelLayer(int _layerId, bool force = false, string[] excludeTags = null)
	{
		Utils.SetLayerRecursively(emodel.GetModelTransform().gameObject, _layerId);
	}

	public virtual void SetColliderLayer(int _layerId, bool _force = false)
	{
		Utils.SetColliderLayerRecursively(emodel.GetModelTransform().gameObject, _layerId);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int GetMaxAttackTime()
	{
		return 10;
	}

	public int GetAttackTimeoutTicks()
	{
		if (!world.IsDark())
		{
			return attackTimeoutDay;
		}
		return attackTimeoutNight;
	}

	public override string GetLootList()
	{
		if (!string.IsNullOrEmpty(lootListOnDeath) && IsDead())
		{
			return lootListOnDeath;
		}
		return base.GetLootList();
	}

	public override void MarkToUnload()
	{
		base.MarkToUnload();
		deathUpdateTime = timeStayAfterDeath;
	}

	public override bool IsMarkedForUnload()
	{
		if (base.IsMarkedForUnload())
		{
			return deathUpdateTime >= timeStayAfterDeath;
		}
		return false;
	}

	public virtual bool IsAttackValid()
	{
		if (!(this is EntityPlayer))
		{
			if (Electrocuted)
			{
				return false;
			}
			if (bodyDamage.CurrentStun == EnumEntityStunType.Kneel || bodyDamage.CurrentStun == EnumEntityStunType.Prone)
			{
				return false;
			}
		}
		if (emodel != null && emodel.avatarController != null && emodel.avatarController.IsAttackPrevented())
		{
			return false;
		}
		if (IsDead())
		{
			return false;
		}
		if (painResistPercent >= 1f)
		{
			return true;
		}
		if (hasBeenAttackedTime <= 0)
		{
			if (!(emodel.avatarController == null))
			{
				return !emodel.avatarController.IsAnimationHitRunning();
			}
			return true;
		}
		return false;
	}

	public virtual bool IsAttackImpact()
	{
		if ((bool)emodel && (bool)emodel.avatarController)
		{
			return emodel.avatarController.IsAttackImpact();
		}
		return false;
	}

	public virtual void ShowHoldingItem(bool _show)
	{
		inventory.ShowRightHand(_show);
	}

	public virtual bool IsHoldingItemInUse(int _actionIndex)
	{
		return inventory.holdingItem.Actions[_actionIndex]?.IsActionRunning(inventory.holdingItemData.actionData[_actionIndex]) ?? false;
	}

	public virtual bool UseHoldingItem(int _actionIndex, bool _isReleased)
	{
		if (!_isReleased)
		{
			if (_actionIndex == 0 && (bool)emodel && (bool)emodel.avatarController && emodel.avatarController.IsAnimationAttackPlaying())
			{
				return false;
			}
			if (!IsAttackValid())
			{
				return false;
			}
		}
		if (_actionIndex == 0 && _isReleased && GetSoundAttack() != null)
		{
			PlayOneShot(GetSoundAttack());
		}
		attackingTime = 60;
		inventory.holdingItem.Actions[_actionIndex]?.ExecuteAction(inventory.holdingItemData.actionData[_actionIndex], _isReleased);
		return true;
	}

	public bool Attack(bool _isReleased)
	{
		return UseHoldingItem(0, _isReleased);
	}

	public Entity GetTargetIfAttackedNow()
	{
		if (!IsAttackValid())
		{
			return null;
		}
		ItemClass holdingItem = inventory.holdingItem;
		if (holdingItem != null)
		{
			int holdingItemIdx = inventory.holdingItemIdx;
			ItemAction itemAction = holdingItem.Actions[holdingItemIdx];
			if (itemAction != null)
			{
				WorldRayHitInfo executeActionTarget = itemAction.GetExecuteActionTarget(inventory.holdingItemData.actionData[holdingItemIdx]);
				if (executeActionTarget != null && executeActionTarget.bHitValid && (bool)executeActionTarget.transform)
				{
					float num = itemAction.Range;
					if (num == 0f)
					{
						ItemValue holdingItemItemValue = inventory.holdingItemItemValue;
						num = EffectManager.GetItemValue(PassiveEffects.MaxRange, holdingItemItemValue);
					}
					num += 0.3f;
					if (executeActionTarget.hit.distanceSq <= num * num)
					{
						Transform hitRootTransform = executeActionTarget.transform;
						if (executeActionTarget.tag.StartsWith("E_BP_"))
						{
							hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, executeActionTarget.transform);
						}
						if (hitRootTransform != null)
						{
							Entity component = hitRootTransform.GetComponent<Entity>();
							if ((bool)component)
							{
								return component;
							}
						}
						if (executeActionTarget.tag == "E_Vehicle")
						{
							return EntityVehicle.FindCollisionEntity(hitRootTransform);
						}
					}
				}
			}
		}
		return null;
	}

	public virtual float GetBlockDamageScale()
	{
		EnumGamePrefs eProperty = EnumGamePrefs.BlockDamageAI;
		if (IsBloodMoon)
		{
			eProperty = EnumGamePrefs.BlockDamageAIBM;
		}
		return (float)GamePrefs.GetInt(eProperty) * 0.01f;
	}

	public virtual void PlayStepSound(float _volume)
	{
		internalPlayStepSound(_volume);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void internalPlayStepSound(float _volume)
	{
		if (blockValueStandingOn.isair)
		{
			return;
		}
		if ((!onGround && !IsInElevator()) || isHeadUnderwater)
		{
			if (!(this is EntityPlayerLocal) && (isHeadUnderwater || world.IsWater(blockPosStandingOn)))
			{
				Manager.Play(this, "player_swim");
			}
			return;
		}
		BlockValue blockValue = blockValueStandingOn;
		Vector3i vector3i = blockPosStandingOn;
		vector3i.y++;
		BlockValue block = world.GetBlock(vector3i);
		if (block.Block.blockMaterial.stepSound != null)
		{
			blockValue = block;
		}
		else
		{
			BlockValue blockValue2 = (block = world.GetBlock(vector3i + Vector3i.right));
			if (!blockValue2.isair && block.Block.blockMaterial.stepSound != null)
			{
				blockValue = block;
			}
			else
			{
				blockValue2 = (block = world.GetBlock(vector3i - Vector3i.right));
				if (!blockValue2.isair && block.Block.blockMaterial.stepSound != null)
				{
					blockValue = block;
				}
				else
				{
					blockValue2 = (block = world.GetBlock(vector3i + Vector3i.forward));
					if (!blockValue2.isair && block.Block.blockMaterial.stepSound != null)
					{
						blockValue = block;
					}
					else
					{
						blockValue2 = (block = world.GetBlock(vector3i - Vector3i.forward));
						if (!blockValue2.isair && block.Block.blockMaterial.stepSound != null)
						{
							blockValue = block;
						}
					}
				}
			}
		}
		if (blockValue.isair)
		{
			return;
		}
		Block block2 = blockValue.Block;
		if (EffectManager.GetValue(PassiveEffects.SilenceBlockSteps, null, 0f, this, null, block2.Tags) > 0f)
		{
			return;
		}
		MaterialBlock materialForSide = block2.GetMaterialForSide(blockValue, BlockFace.Top);
		if (materialForSide != null && materialForSide.stepSound != null)
		{
			string text = materialForSide.stepSound.name;
			if (text.Length > 0)
			{
				string stepSound = soundStepType + text;
				PlayStepSound(stepSound, _volume);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
		if (blockValueStandingOn.isair)
		{
			return;
		}
		float num = Mathf.Sqrt(_distX * _distX + _distZ * _distZ);
		if (!onGround || isHeadUnderwater)
		{
			distanceSwam += num;
			if (distanceSwam > nextSwimDistance)
			{
				nextSwimDistance += 1f;
				if (nextSwimDistance < distanceSwam || nextSwimDistance > distanceSwam + 1f)
				{
					nextSwimDistance = distanceSwam + 1f;
				}
				internalPlayStepSound(1f);
			}
			return;
		}
		distanceWalked += num;
		if (num == 0f)
		{
			stepSoundDistanceRemaining = 0.25f;
		}
		else
		{
			stepSoundDistanceRemaining -= num;
			if (stepSoundDistanceRemaining <= 0f)
			{
				stepSoundDistanceRemaining = getNextStepSoundDistance();
				internalPlayStepSound(1f);
			}
		}
		stepSoundRotYRemaining -= Utils.FastAbs(_rotYDelta);
		if (stepSoundRotYRemaining <= 0f)
		{
			stepSoundRotYRemaining = 90f;
			internalPlayStepSound(1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updatePlayerLandSound(float _distXZ, float _diffY)
	{
		if (blockValueStandingOn.isair)
		{
			return;
		}
		if (_distXZ >= 0.025f || Utils.FastAbs(_diffY) >= 0.015f)
		{
			float num = inWaterPercent * 2f;
			float x = num - landWaterLevel;
			landWaterLevel = num;
			float num2 = Utils.FastAbs(x);
			if (num > 0f)
			{
				num2 = Utils.FastMax(num2, _distXZ);
			}
			if (num2 >= 0.02f)
			{
				float volumeScale = Utils.FastMin(num2 * 2.2f + 0.01f, 1f);
				Manager.Play(this, "player_swim", volumeScale);
			}
		}
		if (isHeadUnderwater)
		{
			wasOnGround = true;
		}
		else
		{
			wasOnGround = onGround;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateCurrentBlockPosAndValue()
	{
		Vector3i blockPosition = GetBlockPosition();
		BlockValue block = world.GetBlock(blockPosition);
		if (block.isair)
		{
			blockPosition.y--;
			block = world.GetBlock(blockPosition);
		}
		if (block.ischild)
		{
			blockPosition += block.parent;
			block = world.GetBlock(blockPosition);
		}
		if (blockPosStandingOn != blockPosition || !blockValueStandingOn.Equals(block) || (onGround && !wasOnGround))
		{
			blockPosStandingOn = blockPosition;
			blockValueStandingOn = block;
			blockStandingOnChanged = !world.IsRemote();
			BiomeDefinition biome = world.GetBiome(blockPosStandingOn.x, blockPosStandingOn.z);
			if (biome != null && biomeStandingOn != biome && (biomeStandingOn == null || biomeStandingOn.m_Id != biome.m_Id))
			{
				onNewBiomeEntered(biome);
			}
		}
		CalcIfInElevator();
		Block block2 = blockValueStandingOn.Block;
		if (block2.BuffsWhenWalkedOn != null && block2.UseBuffsWhenWalkedOn(world, blockPosStandingOn, blockValueStandingOn))
		{
			bool flag = true;
			if (world.GetTileEntity(0, blockPosStandingOn) is TileEntityWorkstation tileEntityWorkstation)
			{
				flag = tileEntityWorkstation.IsBurning;
			}
			if (flag)
			{
				for (int i = 0; i < block2.BuffsWhenWalkedOn.Length; i++)
				{
					BuffValue buff = Buffs.GetBuff(block2.BuffsWhenWalkedOn[i]);
					if (buff == null || buff.DurationInSeconds >= 1f)
					{
						Buffs.AddBuff(block2.BuffsWhenWalkedOn[i], blockPosition);
					}
				}
			}
		}
		if (onGround && !IsFlyMode.Value)
		{
			if (block2.MovementFactor != 1f && block2.HasCollidingAABB(blockValueStandingOn, blockPosStandingOn.x, blockPosStandingOn.y, blockPosStandingOn.z, 0f, boundingBox))
			{
				SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, block2.MovementFactor, this));
			}
			if (blockStandingOnChanged)
			{
				blockStandingOnChanged = false;
				if (!blockValueStandingOn.isair)
				{
					block2.OnEntityWalking(world, blockPosStandingOn.x, blockPosStandingOn.y, blockPosStandingOn.z, blockValueStandingOn, this);
					if (GameManager.bPhysicsActive && !blockValueStandingOn.ischild && !blockValueStandingOn.Block.isOversized && world.GetStability(blockPosStandingOn) == 0 && Block.CanFallBelow(world, blockPosStandingOn.x, blockPosStandingOn.y, blockPosStandingOn.z))
					{
						Log.Warning("EntityAlive {0} AddFallingBlock stab 0 happens?", EntityName);
						world.AddFallingBlock(blockPosStandingOn);
					}
				}
				BlockValue block3 = world.GetBlock(blockPosStandingOn + Vector3i.up);
				if (!block3.isair)
				{
					block3.Block.OnEntityWalking(world, blockPosStandingOn.x, blockPosStandingOn.y + 1, blockPosStandingOn.z, block3, this);
				}
			}
		}
		HandleLootStageMaxCheck();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleLootStageMaxCheck()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcIfInElevator()
	{
		ChunkCluster chunkCache = world.ChunkCache;
		Vector3i pos = new Vector3i(blockPosStandingOn.x, Utils.Fastfloor(boundingBox.min.y), blockPosStandingOn.z);
		BlockValue block = chunkCache.GetBlock(pos);
		Block block2 = block.Block;
		bInElevator = block2.IsElevator(block.rotation);
		pos.y++;
		block = chunkCache.GetBlock(pos);
		block2 = block.Block;
		bInElevator |= block2.IsElevator(block.rotation);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float getNextStepSoundDistance()
	{
		return 1.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onNewBiomeEntered(BiomeDefinition _biome)
	{
		biomeStandingOn = _biome;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
		if (isEntityRemote && _partialTicks > 1f)
		{
			_dist /= _partialTicks;
		}
		speedForward *= 0.5f;
		speedStrafe *= 0.5f;
		speedVertical *= 0.5f;
		if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
		{
			float num = Mathf.Sin((0f - rotation.y) * MathF.PI / 180f);
			float num2 = Mathf.Cos((0f - rotation.y) * MathF.PI / 180f);
			speedForward += num2 * _dist.z - num * _dist.x;
			speedStrafe += num2 * _dist.x + num * _dist.z;
		}
		if (Mathf.Abs(_dist.y) > 0.001f)
		{
			speedVertical += _dist.y;
		}
		SetMovementState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PlayStepSound(string stepSound, float _volume)
	{
		if (this is EntityPlayerLocal)
		{
			Manager.BroadcastPlay(this, stepSound);
		}
		else
		{
			Manager.Play(this, stepSound);
		}
	}

	public void SetLookPosition(Vector3 _lookPos)
	{
		if (!((lookAtPosition - _lookPos).sqrMagnitude < 0.0016f))
		{
			lookAtPosition = _lookPos;
			if (world.entityDistributer != null)
			{
				world.entityDistributer.SendPacketToTrackedPlayers(entityId, world.GetPrimaryPlayerId(), NetPackageManager.GetPackage<NetPackageEntityLookAt>().Setup(entityId, _lookPos));
			}
			if ((bool)emodel.avatarController)
			{
				emodel.avatarController.SetLookPosition(_lookPos);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isRadiationSensitive()
	{
		return true;
	}

	public virtual bool IsAimingGunPossible()
	{
		return true;
	}

	public int GetDeathTime()
	{
		return deathUpdateTime;
	}

	public void SetDeathTime(int _deathTime)
	{
		deathUpdateTime = _deathTime;
	}

	public int GetTimeStayAfterDeath()
	{
		return timeStayAfterDeath;
	}

	public bool IsCorpse()
	{
		if ((bool)emodel && emodel.IsRagdollDead && (float)deathUpdateTime > 70f)
		{
			return true;
		}
		return false;
	}

	public override void OnAddedToWorld()
	{
		if (!(this is EntityPlayerLocal))
		{
			OcclusionManager.AddEntity(this, 7f);
		}
		m_addedToWorld = true;
		if (!isEntityRemote)
		{
			bSpawned = true;
		}
		if (this as EntityPlayer == null)
		{
			FireEvent(MinEventTypes.onSelfFirstSpawn);
		}
		StartStopLivingSound();
	}

	public override void OnEntityUnload()
	{
		if (!(this is EntityPlayerLocal))
		{
			OcclusionManager.RemoveEntity(this);
		}
		if (navigator != null)
		{
			navigator.SetPath(null, 0f);
			navigator = null;
		}
		base.OnEntityUnload();
		lookHelper = null;
		moveHelper = null;
		seeCache = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageFraction(float _damage)
	{
		return _damage / (float)GetMaxHealth();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDismemberChance(ref DamageResponse _dmResponse, float damagePer)
	{
		EnumBodyPartHit hitBodyPart = _dmResponse.HitBodyPart;
		EntityClass entityClass = EntityClass.list[base.entityClass];
		float originalValue = 0f;
		switch (hitBodyPart.ToPrimary())
		{
		case BodyPrimaryHit.Head:
			originalValue = entityClass.DismemberMultiplierHead;
			break;
		case BodyPrimaryHit.LeftUpperArm:
		case BodyPrimaryHit.RightUpperArm:
		case BodyPrimaryHit.LeftLowerArm:
		case BodyPrimaryHit.RightLowerArm:
			originalValue = entityClass.DismemberMultiplierArms;
			break;
		case BodyPrimaryHit.LeftUpperLeg:
		case BodyPrimaryHit.RightUpperLeg:
		case BodyPrimaryHit.LeftLowerLeg:
		case BodyPrimaryHit.RightLowerLeg:
			originalValue = entityClass.DismemberMultiplierLegs;
			break;
		}
		originalValue = EffectManager.GetValue(PassiveEffects.DismemberSelfChance, null, originalValue, this);
		float dismemberChance = _dmResponse.Source.DismemberChance;
		float num = ((dismemberChance < 100f) ? (dismemberChance * damagePer * originalValue) : 100f);
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(_dmResponse.Source.getEntityId()) as EntityPlayerLocal;
		if ((bool)entityPlayerLocal && entityPlayerLocal.DebugDismembermentChance)
		{
			num = 1f;
		}
		if (DismembermentManager.DebugLogEnabled && num > 0f)
		{
			Log.Out("[EntityAlive.GetDismemberChance] - {0}, primary {1}, damage {2}, chance {3} * damage% {4} * multiplier {5} = {6}", hitBodyPart, hitBodyPart.ToPrimary(), _dmResponse.Strength, dismemberChance.ToCultureInvariantString(), damagePer.ToCultureInvariantString(), originalValue.ToCultureInvariantString(), num.ToCultureInvariantString());
		}
		return num;
	}

	public virtual void CheckDismember(ref DamageResponse _dmResponse, float damagePer)
	{
		bool flag = _dmResponse.HitBodyPart.IsLeg();
		if (flag && IsAlive() && (bodyDamage.CurrentStun != EnumEntityStunType.None || sleepingOrWakingUp))
		{
			flag = false;
			return;
		}
		float dismemberChance = GetDismemberChance(ref _dmResponse, damagePer);
		if (dismemberChance > 0f && rand.RandomFloat <= dismemberChance)
		{
			_dmResponse.Dismember = true;
			if (flag)
			{
				_dmResponse.TurnIntoCrawler = true;
			}
		}
		else
		{
			if (!flag)
			{
				return;
			}
			EntityClass entityClass = EntityClass.list[base.entityClass];
			if (entityClass.LegCrawlerThreshold > 0f && GetDamageFraction(_dmResponse.Strength) >= entityClass.LegCrawlerThreshold)
			{
				_dmResponse.TurnIntoCrawler = true;
			}
			if (bodyDamage.ShouldBeCrawler || _dmResponse.TurnIntoCrawler || !(entityClass.LegCrippleScale > 0f))
			{
				return;
			}
			float num = GetDamageFraction(_dmResponse.Strength) * entityClass.LegCrippleScale;
			if (num >= 0.05f)
			{
				if ((bodyDamage.Flags & 0x1000) == 0 && _dmResponse.HitBodyPart.IsLeftLeg() && rand.RandomFloat < num)
				{
					_dmResponse.CrippleLegs = true;
				}
				if ((bodyDamage.Flags & 0x2000) == 0 && _dmResponse.HitBodyPart.IsRightLeg() && rand.RandomFloat < num)
				{
					_dmResponse.CrippleLegs = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLocalBodyDamage(DamageResponse _dmResponse)
	{
		EnumBodyPartHit enumBodyPartHit = _dmResponse.HitBodyPart;
		bodyDamage.bodyPartHit = enumBodyPartHit;
		bodyDamage.damageType = _dmResponse.Source.damageType;
		if (_dmResponse.Dismember)
		{
			if (DismembermentManager.DebugBodyPartHit != EnumBodyPartHit.None)
			{
				enumBodyPartHit = DismembermentManager.DebugBodyPartHit;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 1u;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 2u;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 4u;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 8u;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 16u;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 32u;
				bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 64u;
				bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 128u;
				bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
			{
				bodyDamage.Flags |= 256u;
				bodyDamage.ShouldBeCrawler = true;
			}
		}
		if (_dmResponse.TurnIntoCrawler)
		{
			bodyDamage.ShouldBeCrawler = true;
		}
		if (_dmResponse.CrippleLegs)
		{
			if (_dmResponse.HitBodyPart.IsLeftLeg())
			{
				bodyDamage.Flags |= 4096u;
			}
			if (_dmResponse.HitBodyPart.IsRightLeg())
			{
				bodyDamage.Flags |= 8192u;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ExecuteDismember(bool restoreState)
	{
		if (!(emodel == null) && !(emodel.avatarController == null))
		{
			emodel.avatarController.DismemberLimb(bodyDamage, restoreState);
			if (bodyDamage.ShouldBeCrawler)
			{
				SetupCrawlerState(restoreState);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCrawlerState(bool restoreState)
	{
		if (!IsDead())
		{
			emodel.avatarController.TurnIntoCrawler(restoreState);
			SetMaxHeight(0.5f);
			ItemValue itemValue = null;
			if (EntityClass.list[entityClass].Properties.Values.ContainsKey(EntityClass.PropHandItemCrawler))
			{
				itemValue = ItemClass.GetItem(EntityClass.list[entityClass].Properties.Values[EntityClass.PropHandItemCrawler]);
				if (itemValue.IsEmpty())
				{
					itemValue = null;
				}
			}
			if (itemValue == null)
			{
				itemValue = ItemClass.GetItem("meleeHandZombie02");
			}
			inventory.SetBareHandItem(itemValue);
			TurnIntoCrawler();
		}
		walkType = 21;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TurnIntoCrawler()
	{
	}

	public void ClearStun()
	{
		bodyDamage.CurrentStun = EnumEntityStunType.None;
		bodyDamage.StunDuration = 0f;
		SetCVar("_stunned", 0f);
	}

	public void SetStun(EnumEntityStunType stun)
	{
		bodyDamage.CurrentStun = stun;
		SetCVar("_stunned", 1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onSpawnStateChanged()
	{
		if (m_addedToWorld)
		{
			StartStopLivingSound();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartStopLivingSound()
	{
		if (soundLiving != null)
		{
			if (Spawned)
			{
				if (!IsDead() && Health > 0)
				{
					Manager.Play(this, soundLiving);
					soundLivingID = 0;
				}
			}
			else if (soundLivingID >= 0)
			{
				Manager.Stop(entityId, soundLiving);
				soundLivingID = -1;
			}
		}
		if (Spawned && soundSpawn != null && !SleeperSupressLivingSounds)
		{
			PlayOneShot(soundSpawn);
		}
	}

	public void CrouchHeightFixedUpdate()
	{
		if (crouchType == 0 || physicsBaseHeight <= 1.3f)
		{
			return;
		}
		float num = physicsBaseHeight;
		if (IsInElevator())
		{
			num *= 1.06f;
		}
		if (emodel.IsRagdollMovement || bodyDamage.CurrentStun == EnumEntityStunType.Prone)
		{
			num = physicsBaseHeight * 0.08f;
		}
		float num2 = m_characterController.GetRadius() * 0.9f;
		float num3 = num2 + 0.3f;
		float maxDistance = num + 0.01f - num3 - num2;
		Vector3 origin = PhysicsTransform.position;
		origin.y += num3;
		if (moveHelper != null && (moveHelper.BlockedFlags & 3) == 2)
		{
			origin += ModelTransform.forward * 0.15f;
		}
		if (Physics.SphereCast(origin, num2, Vector3.up, out var hitInfo, maxDistance, 1083277312))
		{
			Transform transform = hitInfo.transform;
			if ((bool)transform && transform.CompareTag("Physics"))
			{
				Entity component = transform.GetComponent<Entity>();
				if ((bool)component)
				{
					component.PhysicsPush(transform.forward * (0.1f * Time.fixedDeltaTime), hitInfo.point, affectLocalPlayerController: true);
				}
				return;
			}
			if (world.GetBlock(new Vector3i(hitInfo.point + Origin.position)).Block.Damage <= 0f)
			{
				num = hitInfo.point.y - (origin.y - num3) - 0.21f;
			}
		}
		if (num < physicsHeight)
		{
			if (IsInElevator())
			{
				return;
			}
			num = Mathf.MoveTowards(physicsHeight, num, 0.099999994f);
		}
		else
		{
			num = Mathf.MoveTowards(physicsHeight, num, 0.016666666f);
		}
		SetHeight(num);
		float num4 = physicsBaseHeight * 0.7f;
		if (num <= num4)
		{
			crouchBendPerTarget = 0f;
			int num5 = 8;
			if (walkType != num5 && walkType != 21)
			{
				walkTypeBeforeCrouch = walkType;
				SetWalkType(num5);
			}
		}
		else
		{
			crouchBendPerTarget = 1f - (num - num4) / (physicsBaseHeight - num4);
			if (walkTypeBeforeCrouch != 0)
			{
				SetWalkType(walkTypeBeforeCrouch);
				walkTypeBeforeCrouch = 0;
			}
		}
		crouchBendPer = Mathf.MoveTowards(crouchBendPer, crouchBendPerTarget, 0.099999994f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetWalkType(int _walkType)
	{
		walkType = _walkType;
		emodel.avatarController.SetWalkType(_walkType, _trigger: true);
	}

	public int GetWalkType()
	{
		return walkType;
	}

	public bool IsWalkTypeACrawl()
	{
		return walkType >= 20;
	}

	public string GetRightHandTransformName()
	{
		return rightHandTransformName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isGameMessageOnDeath()
	{
		return true;
	}

	public override float GetLightBrightness()
	{
		Vector3i blockPosition = GetBlockPosition();
		Vector3i blockPos = blockPosition;
		blockPos.y += Mathf.RoundToInt(base.height + 0.5f);
		return Utils.FastMax(world.GetLightBrightness(blockPosition), world.GetLightBrightness(blockPos));
	}

	public virtual float GetLightLevel()
	{
		EntityAlive entityAlive = AttachedToEntity as EntityAlive;
		if ((bool)entityAlive)
		{
			return entityAlive.GetLightLevel();
		}
		return inventory.GetLightLevel();
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			CurrentMovementTag = MovementTagIdle;
			Crouching = false;
			if (!isEntityRemote)
			{
				saveInventory = null;
				if (_other is EntityAlive && _other.GetAttachedToInfo(slot).bReplaceLocalInventory)
				{
					saveInventory = inventory;
					saveHoldingItemIdxBeforeAttach = inventory.holdingItemIdx;
					inventory.SetHoldingItemIdxNoHolsterTime(inventory.DUMMY_SLOT_IDX);
					inventory = ((EntityAlive)_other).inventory;
				}
				bPlayerStatsChanged |= true;
			}
			else
			{
				ShowHoldingItem(_show: false);
			}
		}
		return slot;
	}

	public override void Detach()
	{
		if (saveInventory != null)
		{
			inventory = saveInventory;
			inventory.SetHoldingItemIdxNoHolsterTime(saveHoldingItemIdxBeforeAttach);
			saveInventory = null;
		}
		base.Detach();
		bPlayerStatsChanged |= !isEntityRemote;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(deathHealth);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version > 24)
		{
			deathHealth = _br.ReadInt32();
		}
	}

	public override string ToString()
	{
		return $"[type={GetType().Name}, name={GameUtils.SafeStringFormat(EntityName)}, id={entityId}]";
	}

	public virtual void FireEvent(MinEventTypes _eventType, bool useInventory = true)
	{
		EntityClass.list[entityClass].Effects?.FireEvent(_eventType, MinEventContext);
		if (Progression != null)
		{
			Progression.FireEvent(_eventType, MinEventContext);
		}
		if (challengeJournal != null)
		{
			challengeJournal.FireEvent(_eventType, MinEventContext);
		}
		if (inventory != null && useInventory)
		{
			inventory.FireEvent(_eventType, MinEventContext);
		}
		equipment.FireEvent(_eventType, MinEventContext);
		Buffs.FireEvent(_eventType, MinEventContext);
	}

	public float GetCVar(string _varName)
	{
		if (Buffs == null)
		{
			return 0f;
		}
		return Buffs.GetCustomVar(_varName);
	}

	public void SetCVar(string _varName, float _value)
	{
		if (Buffs != null)
		{
			Buffs.SetCustomVar(_varName, _value);
		}
	}

	public virtual void BuffAdded(BuffValue _buff)
	{
	}

	public override void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
		if (!emodel.IsRagdollActive || collision.relativeVelocity.sqrMagnitude < 0.0625f)
		{
			return;
		}
		float sqrMagnitude = collision.impulse.sqrMagnitude;
		if (sqrMagnitude < 400f)
		{
			return;
		}
		if (IsDead())
		{
			impacts.TryGetValue(t, out var value);
			value.count++;
			impacts[t] = value;
			if (value.count >= 10)
			{
				if (value.count == 10)
				{
					Rigidbody component = t.GetComponent<Rigidbody>();
					if ((bool)component)
					{
						component.velocity = Vector3.zero;
						component.angularVelocity = Vector3.zero;
						component.drag = 0.5f;
						component.angularDrag = 0.5f;
					}
					CharacterJoint component2 = t.GetComponent<CharacterJoint>();
					if ((bool)component2)
					{
						component2.enableProjection = false;
					}
				}
				if (value.count == 25 && !t.gameObject.CompareTag("E_BP_Body"))
				{
					t.GetComponent<Collider>().enabled = false;
				}
				return;
			}
		}
		if (Time.time - impactSoundTime < 0.25f)
		{
			return;
		}
		impactSoundTime = Time.time;
		if (t.lossyScale.x != 0f)
		{
			string soundGroupName = "impactbodylight";
			if (sqrMagnitude >= 3600f)
			{
				soundGroupName = "impactbodyheavy";
			}
			Vector3 zero = Vector3.zero;
			int contactCount = collision.contactCount;
			for (int i = 0; i < contactCount; i++)
			{
				zero += collision.GetContact(i).point;
			}
			zero *= 1f / (float)contactCount;
			Manager.BroadcastPlay(zero + Origin.position, soundGroupName);
		}
	}

	public void AddParticle(string _name, Transform _t)
	{
		if (particles.ContainsKey(_name))
		{
			particles[_name] = _t;
		}
		else
		{
			particles.Add(_name, _t);
		}
	}

	public bool RemoveParticle(string _name)
	{
		if (particles.Remove(_name, out var value))
		{
			if ((bool)value)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			return true;
		}
		return false;
	}

	public bool HasParticle(string _name)
	{
		if (particles.TryGetValue(_name, out var _))
		{
			return true;
		}
		return false;
	}

	public void AddPart(string _name, Transform _t)
	{
		if (parts.ContainsKey(_name))
		{
			parts[_name] = _t;
		}
		else
		{
			parts.Add(_name, _t);
		}
	}

	public void RemovePart(string _name)
	{
		if (parts.TryGetValue(_name, out var value))
		{
			parts.Remove(_name);
			if ((bool)value)
			{
				value.gameObject.name = ".";
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
	}

	public void SetPartActive(string _name, bool isActive)
	{
		if (!parts.TryGetValue(_name, out var value) || !value)
		{
			return;
		}
		bool flag = true;
		for (int num = value.childCount - 1; num >= 0; num--)
		{
			Transform child = value.GetChild(num);
			if (child.CompareTag("ModOn"))
			{
				child.gameObject.SetActive(isActive);
				flag = false;
			}
			else if (child.CompareTag("ModMesh"))
			{
				if (value.parent.name == "CameraNode")
				{
					child.gameObject.SetActive(value: false);
				}
				flag = false;
			}
		}
		if (flag)
		{
			value.gameObject.SetActive(isActive);
		}
	}

	public void AddOwnedEntity(OwnedEntityData _entityData)
	{
		if (_entityData != null)
		{
			ownedEntities.Add(_entityData);
		}
	}

	public void AddOwnedEntity(Entity _entity)
	{
		if (ownedEntities.Find([PublicizedFrom(EAccessModifier.Internal)] (OwnedEntityData e) => e.Id == _entity.entityId) == null)
		{
			AddOwnedEntity(new OwnedEntityData(_entity));
		}
	}

	public void RemoveOwnedEntity(OwnedEntityData _entityData)
	{
		if (_entityData != null)
		{
			ownedEntities.Remove(_entityData);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageOwnedEntitySync>().Setup(entityId, _entityData.Id, NetPackageOwnedEntitySync.SyncType.Remove));
			}
		}
		else
		{
			Log.Warning("{0} RemoveOwnedEntity null", this);
		}
	}

	public void RemoveOwnedEntity(int _entityId)
	{
		RemoveOwnedEntity(ownedEntities.Find([PublicizedFrom(EAccessModifier.Internal)] (OwnedEntityData e) => e.Id == _entityId));
	}

	public void RemoveOwnedEntity(Entity _entity)
	{
		RemoveOwnedEntity(_entity.entityId);
	}

	public OwnedEntityData GetOwnedEntity(int _entityId)
	{
		return ownedEntities.Find([PublicizedFrom(EAccessModifier.Internal)] (OwnedEntityData e) => e.Id == _entityId);
	}

	public OwnedEntityData[] GetOwnedEntityClass(string _className)
	{
		List<OwnedEntityData> list = new List<OwnedEntityData>();
		for (int i = 0; i < ownedEntities.Count; i++)
		{
			OwnedEntityData ownedEntityData = ownedEntities[i];
			if (EntityClass.list[ownedEntityData.ClassId].entityClassName.ContainsCaseInsensitive(_className))
			{
				list.Add(ownedEntityData);
			}
		}
		return list.ToArray();
	}

	public bool HasOwnedEntity(int _entityId)
	{
		return GetOwnedEntity(_entityId) != null;
	}

	public OwnedEntityData[] GetOwnedEntities()
	{
		return ownedEntities.ToArray();
	}

	public void ClearOwnedEntities()
	{
		ownedEntities.Clear();
	}

	public void HandleSetNavName()
	{
		if (NavObject != null)
		{
			NavObject.name = entityName;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDynamicRagdoll()
	{
		if (_dynamicRagdoll.HasFlag(DynamicRagdollFlags.Active))
		{
			if (accumulatedRootMotion != Vector3.zero)
			{
				_dynamicRagdollRootMotion = accumulatedRootMotion;
			}
			if (_dynamicRagdoll.HasFlag(DynamicRagdollFlags.UseBoneVelocities))
			{
				_ragdollPositionsPrev.Clear();
				_ragdollPositionsCur.CopyTo(_ragdollPositionsPrev);
				emodel.CaptureRagdollPositions(_ragdollPositionsCur);
			}
			if (_dynamicRagdoll.HasFlag(DynamicRagdollFlags.RagdollOnFall) && !onGround)
			{
				ActivateDynamicRagdoll();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AnalyticsSendDeath(DamageResponse _dmResponse)
	{
	}

	public virtual string MakeDebugNameInfo()
	{
		return string.Empty;
	}

	public static void SetupAllDebugNameHUDs(bool _isAdd)
	{
		List<Entity> list = GameManager.Instance.World.Entities.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityAlive entityAlive = list[i] as EntityAlive;
			if ((bool)entityAlive)
			{
				entityAlive.SetupDebugNameHUD(_isAdd);
			}
		}
	}

	public void SetupDebugNameHUD(bool _isAdd)
	{
		if (this is EntityPlayer)
		{
			return;
		}
		GUIHUDEntityName component = ModelTransform.GetComponent<GUIHUDEntityName>();
		if (_isAdd)
		{
			if (!component)
			{
				ModelTransform.gameObject.AddComponent<GUIHUDEntityName>();
			}
		}
		else if ((bool)component)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public EModelBase.HeadStates GetHeadState()
	{
		if (base.EntityClass.CanBigHead)
		{
			return emodel.HeadState;
		}
		return EModelBase.HeadStates.Standard;
	}

	public void SetBigHead()
	{
		if ((this is EntityAnimal || this is EntityEnemy || this is EntityTrader) && base.EntityClass.CanBigHead && emodel.HeadState == EModelBase.HeadStates.Standard)
		{
			emodel.HeadState = EModelBase.HeadStates.Growing;
			Manager.BroadcastPlayByLocalPlayer(position, "twitch_bighead_inflate");
		}
	}

	public void ResetHead()
	{
		if ((this is EntityAnimal || this is EntityEnemy || this is EntityTrader) && base.EntityClass.CanBigHead && (emodel.HeadState == EModelBase.HeadStates.BigHead || emodel.HeadState == EModelBase.HeadStates.Growing))
		{
			StartCoroutine(resetHeadLater(emodel));
		}
	}

	public void SetDancing(bool enabled)
	{
		if (base.EntityClass.DanceTypeID != 0)
		{
			IsDancing = enabled;
		}
		else
		{
			IsDancing = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator resetHeadLater(EModelBase model)
	{
		yield return new WaitForSeconds(0.25f);
		if (emodel != null && emodel.GetHeadTransform() != null && emodel.GetHeadTransform().localScale.x > 1f)
		{
			emodel.HeadState = EModelBase.HeadStates.Shrinking;
			Manager.BroadcastPlayByLocalPlayer(position, "twitch_bighead_deflate");
		}
	}

	public void SetSpawnByData(int newSpawnByID, string newSpawnByName)
	{
		spawnById = newSpawnByID;
		spawnByName = newSpawnByName;
		bPlayerStatsChanged |= !isEntityRemote;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetHeadSize(float overrideHeadSize)
	{
		OverrideHeadSize = overrideHeadSize;
		emodel.SetHeadScale(overrideHeadSize);
	}

	public void SetVehiclePoseMode(int _pose)
	{
		vehiclePoseMode = _pose;
		if (_pose != GetVehicleAnimation())
		{
			Crouching = false;
			SetVehicleAnimation(AvatarController.vehiclePoseHash, _pose);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateNetworkStats()
	{
		if (networkStatsUpdateQueue.Count <= 0)
		{
			return;
		}
		NetworkStatChange networkStatChange = networkStatsUpdateQueue[0];
		networkStatsUpdateQueue.RemoveAt(0);
		if (networkStatChange.m_NetworkStats != null)
		{
			networkStatChange.m_NetworkStats.ToEntity(this);
			return;
		}
		EntityNetworkHoldingData holdingData = networkStatChange.m_HoldingData;
		if (holdingData != null)
		{
			ItemStack holdingItemStack = holdingData.m_HoldingItemStack;
			byte holdingItemIndex = holdingData.m_HoldingItemIndex;
			if (!inventory.GetItem(holdingItemIndex).Equals(holdingItemStack))
			{
				inventory.SetItem(holdingItemIndex, holdingItemStack);
			}
			if (inventory.holdingItemIdx != holdingItemIndex)
			{
				inventory.SetHoldingItemIdxNoHolsterTime(holdingItemIndex);
			}
		}
	}

	public void EnqueueNetworkStats(EntityNetworkStats netStats)
	{
		NetworkStatChange networkStatChange = new NetworkStatChange();
		networkStatChange.m_NetworkStats = netStats;
		networkStatsUpdateQueue.Add(networkStatChange);
	}

	public void EnqueueNetworkHoldingData(ItemStack holdingItemStack, byte holdingItemIndex)
	{
		NetworkStatChange networkStatChange = new NetworkStatChange();
		EntityNetworkHoldingData entityNetworkHoldingData = new EntityNetworkHoldingData();
		entityNetworkHoldingData.m_HoldingItemStack = holdingItemStack;
		entityNetworkHoldingData.m_HoldingItemIndex = holdingItemIndex;
		networkStatChange.m_HoldingData = entityNetworkHoldingData;
		networkStatsUpdateQueue.Add(networkStatChange);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive()
	{
	}
}
