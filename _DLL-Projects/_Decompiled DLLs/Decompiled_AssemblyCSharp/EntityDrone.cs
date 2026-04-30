using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using GamePath;
using Platform;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityDrone : EntityNPC, ILockable
{
	public class SoundKeys
	{
		public const string cIdleHover = "drone_idle_hover";

		public const string cFly = "drone_fly";

		public const string cStorageOpen = "vehicle_storage_open";

		public const string cStorageClose = "vehicle_storage_close";

		public const string cHealEffect = "drone_healeffect";

		public const string cAttackEffect = "drone_attackeffect";

		public const string cCommand = "drone_command";

		public const string cEmpty = "drone_empty";

		public const string cEnemySense = "drone_enemy_sense";

		public const string cEnemyEngauge = "drone_enemy_engauge";

		public const string cDroneHeal = "drone_heal";

		public const string cDroneOther = "drone_other";

		public const string cShutDown = "drone_shutdown";

		public const string cTake = "drone_take";

		public const string cTakeFail = "drone_takefail";

		public const string cWakeUp = "drone_wakeup";

		public const string cGreeting = "drone_greeting";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ModKeys
	{
		public const string cStorageMod = "modRoboticDroneCargoMod";

		public const string cArmorMod = "modRoboticDroneArmorPlatingMod";

		public const string cHealMod = "modRoboticDroneMedicMod";

		public const string cStunMod = "modRoboticDroneStunWeaponMod";

		public const string cGunMod = "modRoboticDroneWeaponMod";

		public const string cMoraleMod = "modRoboticDroneMoraleBoosterMod";

		public const string cHeadlampMod = "modRoboticDroneHeadlampMod";

		public const string cHeadlampLightName = "junkDroneLamp";
	}

	public enum State
	{
		Idle,
		Sentry,
		Follow,
		Heal,
		Attack,
		Shutdown,
		NoClip,
		Teleport,
		None
	}

	public enum Orders
	{
		Follow,
		Stay
	}

	public enum Stance
	{
		Defensive,
		Passive,
		Aggressive
	}

	public enum AllyHealMode
	{
		DoNotHeal,
		HealAllies
	}

	[Preserve]
	public class NetPackageDroneDataSync : NetPackage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int senderId;

		[PublicizedFrom(EAccessModifier.Private)]
		public int vehicleId;

		[PublicizedFrom(EAccessModifier.Private)]
		public ushort syncFlags;

		[PublicizedFrom(EAccessModifier.Private)]
		public PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

		public NetPackageDroneDataSync Setup(EntityDrone _ev, int _senderId, ushort _syncFlags)
		{
			senderId = _senderId;
			vehicleId = _ev.entityId;
			syncFlags = _syncFlags;
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(entityData);
			_ev.WriteSyncData(pooledBinaryWriter, _syncFlags);
			return this;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~NetPackageDroneDataSync()
		{
			MemoryPools.poolMemoryStream.FreeSync(entityData);
		}

		public override void read(PooledBinaryReader _br)
		{
			senderId = _br.ReadInt32();
			vehicleId = _br.ReadInt32();
			syncFlags = _br.ReadUInt16();
			int length = _br.ReadUInt16();
			StreamUtils.StreamCopy(_br.BaseStream, entityData, length);
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			_bw.Write(senderId);
			_bw.Write(vehicleId);
			_bw.Write(syncFlags);
			_bw.Write((ushort)entityData.Length);
			entityData.WriteTo(_bw.BaseStream);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			EntityDrone entityDrone = GameManager.Instance.World.GetEntity(vehicleId) as EntityDrone;
			if (entityDrone == null)
			{
				return;
			}
			if (entityData.Length > 0)
			{
				lock (entityData)
				{
					entityData.Position = 0L;
					try
					{
						using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
						pooledBinaryReader.SetBaseStream(entityData);
						entityDrone.ReadSyncData(pooledBinaryReader, syncFlags, senderId);
					}
					catch (Exception e)
					{
						Log.Exception(e);
						Log.Error("Error syncing data for entity " + entityDrone?.ToString() + "; Sender id = " + senderId);
					}
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ushort syncFlagsReplicated = entityDrone.GetSyncFlagsReplicated(syncFlags);
				if (syncFlagsReplicated != 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDroneDataSync>().Setup(entityDrone, senderId, syncFlagsReplicated), _onlyClientsAttachedToAnEntity: false, -1, senderId);
				}
			}
		}

		public override int GetLength()
		{
			return (int)(12 + entityData.Length);
		}
	}

	public class DroneInventory : Inventory
	{
		public DroneInventory(IGameManager _gameManager, EntityAlive _entity)
			: base(_gameManager, _entity)
		{
			SetupSlots();
		}

		public void SetupSlots()
		{
			int num = base.PUBLIC_SLOTS + 1;
			slots = new ItemInventoryData[num];
			models = new Transform[num];
			m_HoldingItemIdx = 0;
			Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SteeringMan
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public const int kMaxDistance = 1000;

		public Vector3 Seek(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return doSeek(pos, target, slowingRadius);
		}

		public Vector3 Seek2D(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return doSeek2D(pos, target, slowingRadius);
		}

		public Vector3 Flee(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return doFlee(pos, target, avoidRadius);
		}

		public Vector3 Flee2D(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return doFlee2D(pos, target, avoidRadius);
		}

		public Vector3 GetDir(Vector3 from, Vector3 to)
		{
			return getDirVector(from, to);
		}

		public Vector3 GetDir2D(Vector3 from, Vector3 to)
		{
			Vector3 pos = new Vector3(from.x, 0f, from.z);
			Vector3 target = new Vector3(to.x, 0f, to.z);
			return getDirVector(pos, target);
		}

		public bool IsInRange(Vector3 from, Vector3 to, float dist)
		{
			return isInRange(from, to, dist);
		}

		public bool IsInRange2D(Vector3 from, Vector3 to, float dist)
		{
			return isInRange2D(from, to, dist);
		}

		public Vector3 GetPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return getPointAround(lhs, rhs, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getVec(Vector3 pos, Vector3 target)
		{
			return target - pos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getDirVector(Vector3 pos, Vector3 target)
		{
			return getVec(pos, target).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getDist(Vector3 pos, Vector3 target)
		{
			return getVec(pos, target).magnitude;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange(Vector3 from, Vector3 to, float dist)
		{
			return (from - to).sqrMagnitude < dist * dist;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange2D(Vector3 from, Vector3 to, float dist)
		{
			Vector3 vector = new Vector3(from.x, 0f, from.z);
			Vector3 to2 = new Vector3(to.x, 0f, to.z);
			return isInRange(vector, to2, dist);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return Vector3.Cross(lhs, rhs) * radius * 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek(Vector3 pos, Vector3 target, float radius)
		{
			float dist = getDist(pos, target);
			if (dist < radius)
			{
				return getDirVector(pos, target) * (dist / radius);
			}
			return getDirVector(pos, target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 result = doSeek(pos, target, radius);
			result.y = 0f;
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee(Vector3 pos, Vector3 target, float radius)
		{
			return -doSeek(pos, target, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 result = doFlee(pos, target, radius);
			result.y = 0f;
			return result;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class EntitySteering : SteeringMan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;

		public EntitySteering(EntityAlive _entity)
		{
			entity = _entity;
		}

		public Vector3 Hover(float height, float slowingRadius = 1f)
		{
			return doHover(entity.position, height, slowingRadius);
		}

		public Vector3 FollowPlayer(Vector3 playerPos, Vector3 playerLookDir, float followDist, float degrees = 90f, float maxDist = 15f)
		{
			return followTarget(entity.position, playerPos, playerLookDir, followDist, degrees, maxDist);
		}

		public Vector3 AvoidArc(Vector3 fromPos, Vector3 toPos, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist = 1000f)
		{
			return doAvoidArc(fromPos, toPos, dir, up, subtract, degrees, maxDist);
		}

		public Vector3 AvoidArc2D(Vector3 fromPos, Vector3 toPos, Vector3 dir, bool subtract, float degrees, float maxDist = 1000f)
		{
			return doAvoidArc2D(fromPos, toPos, dir, subtract, degrees, maxDist);
		}

		public Vector3 AvoidTargetView(EntityAlive target, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return avoidTargetView(entity.position, target.getHeadPosition(), target.GetLookVector(), followDist, subtract, degrees, maxDist);
		}

		public Vector3 FollowTarget(EntityAlive target, Vector3 viewDir, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return pursueAvoidOwnerView(entity.position, target.getHeadPosition(), viewDir, Vector3.zero, followDist, subtract, degrees, maxDist);
		}

		public bool IsInRange(Vector3 target, float dist)
		{
			return IsInRange(entity.position, target, dist);
		}

		public bool IsInRange2D(Vector3 target, float dist)
		{
			return IsInRange2D(entity.position, target, dist);
		}

		public bool IsInRange2D(EntityAlive target, float dist)
		{
			return IsInRange2D(entity.position, target.position, dist);
		}

		public float GetYPos(float height)
		{
			return getYPos(entity.position, height);
		}

		public float GetAltitude(Vector3 pos)
		{
			return getAltitude(pos);
		}

		public bool IsAboveGround(Vector3 pos)
		{
			return getAltitude(pos) > -1f;
		}

		public float GetCeiling(Vector3 pos)
		{
			return getCeiling(pos);
		}

		public bool IsBelowCeiling(Vector3 pos)
		{
			return getCeiling(pos) > -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getAltitude(Vector3 pos)
		{
			if (Physics.Raycast(pos - Origin.position, Vector3.down, out var hitInfo, 1000f, 65536))
			{
				return hitInfo.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getCeiling(Vector3 pos)
		{
			if (Physics.Raycast(pos - Origin.position, Vector3.up, out var hitInfo, 1000f, 65536))
			{
				return hitInfo.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getYPos(Vector3 pos, float height)
		{
			float altitude = getAltitude(pos);
			if (altitude >= 0f)
			{
				return pos.y - altitude + height;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doHover(Vector3 pos, float height, float radius)
		{
			float altitude = getAltitude(pos);
			if (altitude > 0f)
			{
				Vector3 vector = ((altitude < height) ? Vector3.up : Vector3.down);
				float num = Mathf.Abs(height - altitude);
				if (num < radius)
				{
					return vector * (num / radius);
				}
				return vector;
			}
			return Vector3.zero;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 followTarget(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, float degrees, float maxDist)
		{
			return Seek(pos, target, followDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 avoidTargetView(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, bool subtract, float degrees, float maxDist)
		{
			return AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 pursueAvoidOwnerView(Vector3 pos, Vector3 target, Vector3 lookDir, Vector3 offSet, float followDist, bool subtract, float degrees, float maxDist)
		{
			Vector3 vector = Seek(pos, target, followDist);
			Vector3 vector2 = AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist);
			return (vector + vector2).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc(Vector3 from, Vector3 to, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist)
		{
			Vector3 to2 = from - to;
			if (Vector3.Angle(dir, to2) < degrees * 0.5f)
			{
				Vector3 pointAround = GetPointAround((to - from).normalized, up, maxDist);
				pointAround = (subtract ? (to - pointAround) : (to + pointAround));
				if (IsInRange(from, pointAround, maxDist))
				{
					return Flee(from, pointAround + dir * to2.magnitude, 0f);
				}
			}
			return Vector3.zero;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc2D(Vector3 from, Vector3 to, Vector3 dir, bool subtract, float degrees, float maxDist)
		{
			Vector3 result = doAvoidArc(from, to, dir, Vector3.up, subtract, degrees, maxDist);
			result.y = 0f;
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class DroneSensors
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;

		public float EnemyDetectionRadius = 20f;

		public float EnemyDetectedBarkCooldown = 90f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enemyDetectedBarkTimer = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canBarkEnemyDetected;

		public DroneSensors(EntityAlive _entity)
		{
			entity = _entity;
		}

		public void Init()
		{
		}

		public void Update()
		{
			if (enemyDetectedBarkTimer > 0f)
			{
				enemyDetectedBarkTimer -= 0.05f;
				if (enemyDetectedBarkTimer <= 0f)
				{
					canBarkEnemyDetected = true;
				}
			}
		}

		public bool AreEnemiesInRange()
		{
			List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(entity, new Bounds(entity.position, Vector3.one * EnemyDetectionRadius));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
				if (entityAlive != null && entitiesInBounds[i].EntityClass != null && entitiesInBounds[i].EntityClass.bIsEnemyEntity && !(entityAlive is EntityNPC) && (!entityAlive.IsSleeper || !entityAlive.IsSleeping) && !RaycastPathUtils.IsPointBlocked(entity.position, entityAlive.getChestPosition(), 65536))
				{
					return true;
				}
			}
			return false;
		}

		public EntityAlive TargetInRange()
		{
			EntityAlive entityAlive = targetCheck();
			if ((bool)entityAlive && canBarkEnemyDetected)
			{
				barkEnemyDetected();
			}
			return entityAlive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive targetCheck()
		{
			if ((bool)entity.GetRevengeTarget() && !entity.GetRevengeTarget().Buffs.HasBuff("buffShocked"))
			{
				return entity.GetRevengeTarget();
			}
			List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(entity, new Bounds(entity.position, Vector3.one * EnemyDetectionRadius));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
				if (entityAlive != null && entitiesInBounds[i].EntityClass != null && entitiesInBounds[i].EntityClass.bIsEnemyEntity && !(entityAlive is EntityNPC) && (!entityAlive.IsSleeper || !entityAlive.IsSleeping) && !(entitiesInBounds[i] as EntityAlive).Buffs.HasBuff("buffShocked"))
				{
					return entitiesInBounds[i] as EntityAlive;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void barkEnemyDetected()
		{
			EntityDrone entityDrone = entity as EntityDrone;
			if ((bool)entityDrone)
			{
				if ((bool)entityDrone.Owner)
				{
					Manager.Stop(entityDrone.Owner.entityId, "drone_take");
				}
				if (entityDrone.state != State.Shutdown)
				{
					entityDrone.PlayVO("drone_enemy_sense", _hasPriority: true);
					enemyDetectedBarkTimer = EnemyDetectedBarkCooldown;
					canBarkEnemyDetected = false;
				}
			}
		}
	}

	public const string ClassName = "entityJunkDrone";

	public const string ItemName = "gunBotT3JunkDrone";

	public const int SaveVersion = 1;

	public const string cSupportModBuff = "buffJunkDroneSupportEffect";

	public static readonly FastTags<TagGroup.Global> StorageModifierTags = FastTags<TagGroup.Global>.Parse("droneStorage");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cIdleAnimName = "Base Layer.Idle";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cSpawnAnimName = "Base Layer.SpawnIn";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> repairKitTags = FastTags<TagGroup.Global>.Parse("junk");

	public static bool DebugModeEnabled;

	public ItemValue OriginalItemValue;

	public PlatformUserIdentifierAbs OwnerID;

	public float FollowDistance = 5f;

	public float MaxDistFromOwner = 32f;

	public float IdleHoverHeight = 2f;

	public float FollowHoverHeight = 1.5f;

	public float StayHoverHeight = 2f;

	public float SpeedPathing = 2f;

	public float SpeedFlying = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxSpeedFlying = 15f;

	public float RotationSpeed = 12f;

	public float AttackActionTime = 3f;

	public float HealActionTime = 7f;

	public EntityAlive Owner;

	public DroneWeapons.HealBeamWeapon healWeapon;

	public DroneWeapons.StunBeamWeapon stunWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float accelerationTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float decelerationTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float armorDamageReduction = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentSpeedFlying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isStunModAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHealModAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGunModAttached;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public State state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public State lastState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public State transitionState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stateMaxTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeSpentAtLocation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isVisible = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive currentTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> currentPath = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySteering steering;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DroneSensors sensors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DroneWeapons.MachineGunWeapon machineGunWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DroneWeapons.Weapon activeWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public FloodFillEntityPathGenerator pathMan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cInitSuppressVOTime = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float initSuppressVOTimer = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalGFXOffset = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform head;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float wakeupAnimTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color prefabColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DroneLightManager _lm;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Orders orderState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasNavObjectsEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwnerSyncPending;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAnimationStateSet;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemvalueToLoad;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBeingPickedUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider interactionCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] paintableParts = new string[3] { "BaseMesh", "junkDroneArmRight", "armor" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugFriendlyFire;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugShowCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject debugCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera reconCam;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuietMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFlashlightAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFlashlightOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSupportModAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Handle voHandle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Handle idleLoop;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool partyEventsSet;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] knownPartyMembers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float areaScanTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float areaScanTimer = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInConfinedSpace;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float debugInputRotX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float debugInputRotY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 debugInputFwd;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 debugInputRgt;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 debugInputUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float debugInputSpeed = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i debugOwnerPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float overItemLimitCooldown;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTeleporting;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float retryPathTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTryingToFindPath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue currentBlock;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i currentBlockPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lastBlockPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeInBlock;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool userRequestedHeal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physColHeight = 0.6f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTalkCommand = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cServiceCommand = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRepairCommand = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLockCommand = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cUnlockCommand = 4;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cKeypadCommand = 5;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTakeCommand = 6;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cStayCommand = 7;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFollowCommand = 8;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHealCommand = 9;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cStorageCommand = 10;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cQuiteCommand = 11;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cToggleLightCommand = 12;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCommandCount = 12;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDebugPickup = 13;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDebugFriendlyFire = 13;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDebugDroneCamera = 14;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredOwnerHandlers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> registeredPartyMembers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNode focusBoxNode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AllyHealMode allyHealMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cNotifyNeedsHealItemCooldown = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNotifyNeedsHealMaxNotifyCount = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float needsHealItemTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int needsHealNotifyCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sentryPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 followTargetPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasOpenGroupPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<RaycastNode> nodePath = new List<RaycastNode>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool transitionToIdle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float transitionToIdleTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPathLayer = 1073807360;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currentPathTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTargetPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDebugExtraPathTime = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float debugPathDelay = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugPathTiming;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ownerIsOnVehicle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTargetPosBlocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cOwnerFocusTime = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ownerFocusTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool triedFollowTeleport;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> projectedPath = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isShutdown;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGrounded;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 fallPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasFallPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i fallBlockPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 blockHeightOffset = new Vector3(0f, 0.5f, 0f);

	public bool DebugEnemiesInRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeSpentToNextTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetDestination;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] groupPositions = new Vector3[3];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] fallbackGroupPos = new Vector3[3];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncReplicate = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncVersion = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncOwnerKey = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncInteractAndSecurity = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncAction = 4;

	public const ushort cSyncStorage = 8;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncInteractRequest = 4096;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncOrderState = 16384;

	public const ushort cSyncState = 32768;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncInteractAndSecurityFInteracting = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncRepairAction = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncQuiteMode = 32;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncLightMod = 64;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncService = 128;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncHealAllies = 256;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isShutdownPending;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int passwordHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUsers = new List<PlatformUserIdentifierAbs>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInteractionLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int interactingPlayerId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int interactionRequestType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerSteamId;

	public override bool IsValidAimAssistSlowdownTarget => false;

	public override bool IsValidAimAssistSnapTarget => false;

	public DroneLightManager lightManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!_lm)
			{
				_lm = base.transform.GetComponentInChildren<DroneLightManager>();
			}
			return _lm;
		}
	}

	public Orders OrderState => orderState;

	public override int Health
	{
		get
		{
			return (int)base.Stats.Health.Value;
		}
		set
		{
			float num = Mathf.Max(value, 1);
			if (num == 1f && state != State.Shutdown)
			{
				isShutdownPending = true;
			}
			base.Stats.Health.Value = num;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool PlayWakeupAnim { get; set; }

	public int StorageCapacity => lootContainer.items.Length;

	public Vector3 HealArmPosition => healWeapon.WeaponJoint.position + Origin.position;

	public AllyHealMode HealAllyMode => allyHealMode;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsHealingAllies
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsHealModAttached => isHealModAttached;

	public bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			if (value != isVisible)
			{
				Renderer[] componentsInChildren = emodel.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = value;
				}
				isVisible = value;
			}
		}
	}

	public int AmmoCount
	{
		get
		{
			return OriginalItemValue.Meta;
		}
		set
		{
			OriginalItemValue.Meta = value;
		}
	}

	public bool IsFrendlyFireEnabled => debugFriendlyFire;

	public bool IsDebugCameraEnabled => debugShowCamera;

	public int EntityId
	{
		get
		{
			return entityId;
		}
		set
		{
			entityId = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setOrders(Orders orders)
	{
		orderState = orders;
		initWorldValues(orderState == Orders.Follow);
		if (GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
		{
			HandleNavObject();
		}
	}

	public static bool IsValidForLocalPlayer()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(PlatformManager.InternalLocalUserIdentifier);
		if (playerData != null)
		{
			return IsValidForPlayer(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityPlayerLocal);
		}
		return false;
	}

	public static bool IsValidForPlayer(EntityPlayerLocal localPlayer)
	{
		OwnedEntityData[] array = localPlayer.GetOwnedEntities();
		foreach (OwnedEntityData ownedEntityData in array)
		{
			if (ownedEntityData.ClassId != -1 && EntityClass.list[ownedEntityData.ClassId].entityClassName == "entityJunkDrone")
			{
				GameManager.ShowTooltip(localPlayer, Localization.Get("xuiMaxDeployedDronesReached"), string.Empty, "ui_denied");
				return false;
			}
		}
		return true;
	}

	public static void OnClientSpawnRemote(Entity _entity)
	{
		GameManager instance = GameManager.Instance;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityDrone entityDrone = _entity as EntityDrone;
			if ((bool)entityDrone)
			{
				for (int i = 1; i < ItemClass.list.Length - 1; i++)
				{
					ItemClass itemClass = ItemClass.list[i];
					if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
					{
						entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
						entityDrone.PlayWakeupAnim = true;
						PersistentPlayerData playerData = instance.GetPersistentPlayerList().GetPlayerData(entityDrone.OwnerID);
						if (playerData != null)
						{
							entityDrone.belongsPlayerId = playerData.EntityId;
							(instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(_entity);
						}
						break;
					}
				}
			}
		}
		instance.World.EntityLoadedDelegates -= OnClientSpawnRemote;
	}

	public void InitDynamicSpawn()
	{
		for (int i = 1; i < ItemClass.list.Length - 1; i++)
		{
			ItemClass itemClass = ItemClass.list[i];
			if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
			{
				OriginalItemValue = new ItemValue(itemClass.Id);
				OwnerID = PlatformManager.InternalLocalUserIdentifier;
				PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(OwnerID);
				if (playerData != null)
				{
					belongsPlayerId = playerData.EntityId;
					(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(this);
				}
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		steering = new EntitySteering(this);
		isLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (DroneManager.Debug_LocalControl)
		{
			debugInputRotX += Input.GetAxis("Mouse X") * 30f * 0.05f;
			debugInputRotY += Input.GetAxis("Mouse Y") * 30f * 0.05f;
			debugInputRotY = Mathf.Clamp(debugInputRotY, -90f, 90f);
			reconCam.transform.localRotation = Quaternion.AngleAxis(debugInputRotX, Vector3.up);
			reconCam.transform.localRotation *= Quaternion.AngleAxis(debugInputRotY, Vector3.left);
			if (Input.GetMouseButtonDown(0) && RaycastPathUtils.IsPositionBlocked(reconCam.ScreenPointToRay(Input.mousePosition), out var hit, 65536, debugDraw: true))
			{
				RaycastPathUtils.DrawBounds(World.worldToBlockPos(hit.point + Origin.position), Color.yellow, 1f);
				pathMan.CreatePath(Owner.position, hit.point + Origin.position, currentSpeedFlying, canBreakBlocks: false, FollowHoverHeight);
			}
		}
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
	}

	public override void InitInventory()
	{
		inventory = new DroneInventory(GameManager.Instance, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDrone(string format, params object[] args)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || (bool)GetAttachedPlayerLocal())
		{
			format = $"{GameManager.frameCount} Drone {format}";
			Log.Out(format, args);
		}
	}

	public override void PostInit()
	{
		LogDrone("PostInit {0}, {1} (chunk {2}), rbPos {3}", this, position, World.toChunkXZ(position), PhysicsTransform.position + Origin.position);
		float num = 1f / base.transform.localScale.x;
		interactionCollider = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)interactionCollider)
		{
			interactionCollider.center = new Vector3(0f, 0.05f * num, 0.05f * num);
			interactionCollider.size = new Vector3(2.5f, 2f, 2f);
		}
		sensors = new DroneSensors(this);
		sensors.Init();
		initWorldValues(orderState == Orders.Follow);
		IsFlyMode.Value = true;
		bCanClimbLadders = true;
		bCanClimbVertical = true;
		prefabColor = GetPaintColor();
	}

	public override void OnAddedToWorld()
	{
		if (itemvalueToLoad != null)
		{
			OriginalItemValue = itemvalueToLoad;
		}
		isOwnerSyncPending = true;
		InitWeapons();
		LoadMods();
		if ((bool)nativeCollider)
		{
			nativeCollider.enabled = true;
		}
		Health = Mathf.RoundToInt(base.Stats.Health.Max * (1f - OriginalItemValue.UseTimes / (float)OriginalItemValue.MaxUseTimes));
		Animator componentInChildren = GetComponentInChildren<Animator>();
		if (!componentInChildren.enabled)
		{
			componentInChildren.enabled = true;
		}
		componentInChildren.Play("Base Layer.Idle", 0, 0f);
		componentInChildren.Update(0f);
		componentInChildren.StopPlayback();
		pathMan = new FloodFillEntityPathGenerator(world, this);
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
		Vector3 vector = _origin;
		Log.Out("EntityDrone - OnOriginChanged: " + vector.ToString());
	}

	public override void OnEntityUnload()
	{
		Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		UnRegsiterMovingLights();
		registeredPartyMembers = null;
		EntityPlayer entityPlayer = Owner as EntityPlayer;
		if ((bool)entityPlayer)
		{
			Party party = entityPlayer.Party;
			if (party != null)
			{
				party.PartyMemberAdded -= OnPartyMemberAdded;
				party.PartyMemberRemoved -= OnPartyMemberRemoved;
			}
			entityPlayer.PlayerTeleportedDelegates -= TeleportIfFollowing;
		}
		base.OnEntityUnload();
	}

	public override bool CanUpdateEntity()
	{
		if (!Owner)
		{
			return base.CanUpdateEntity();
		}
		return true;
	}

	public override bool CanNavigatePath()
	{
		return true;
	}

	public override float GetEyeHeight()
	{
		if (head == null)
		{
			head = base.transform.FindInChilds("Head");
		}
		return head.position.y - base.transform.position.y;
	}

	public override Ray GetLookRay()
	{
		return new Ray(position + new Vector3(0f, GetEyeHeight(), 0f), (currentTarget == null) ? GetLookVector() : (currentTarget.getHeadPosition() - position).normalized);
	}

	public override bool CanBePushed()
	{
		return true;
	}

	public override float GetWeight()
	{
		return base.GetWeight();
	}

	public override bool IsDead()
	{
		return false;
	}

	public override bool IsAttackValid()
	{
		if (!stunWeapon.canFire())
		{
			return machineGunWeapon.canFire();
		}
		return true;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		int strength = 0;
		return base.DamageEntity(_damageSource, strength, _criticalHit, _impulseScale);
	}

	public override void PlayStepSound(float _volume)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		NavObjectManager instance = NavObjectManager.Instance;
		EntityClass eClass = EntityClass.list[entityClass];
		if (eClass.NavObject != "")
		{
			NavObject navObject = instance.NavObjectList.Find([PublicizedFrom(EAccessModifier.Internal)] (NavObject n) => n.NavObjectClass?.NavObjectClassName == eClass.NavObject);
			if (navObject != null)
			{
				instance.UnRegisterNavObject(navObject);
			}
			NavObject = instance.RegisterNavObject(eClass.NavObject, this);
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			primaryPlayer.Waypoints.UpdateEntityDroneWayPoint(this, OrderState == Orders.Follow);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
		base.AddCharacterController();
		if (!(PhysicsTransform == null) && m_characterController != null)
		{
			RootMotion = false;
			m_characterController.SetSize(Vector3.zero, physColHeight, physColHeight * 0.5f);
			setNoClip(value: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float GetPushBoundsVertical()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		OwnerID.ToStream(_bw);
		OriginalItemValue = GetUpdatedItemValue();
		OriginalItemValue.Write(_bw);
		ushort num = 49507;
		_bw.Write(num);
		WriteSyncData(_bw, num);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		_br.ReadInt32();
		OwnerID = PlatformUserIdentifierAbs.FromStream(_br);
		OriginalItemValue = ItemValue.None.Clone();
		OriginalItemValue.Read(_br);
		ushort syncFlags = _br.ReadUInt16();
		ReadSyncData(_br, syncFlags, 0);
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		bool flag = !IsDead();
		if (IsDead())
		{
			return new EntityActivationCommand[0];
		}
		bool flag2 = false;
		if (belongsToPlayerId(_entityFocusing.entityId))
		{
			flag2 = (_entityFocusing as EntityPlayerLocal).IsGodMode.Value && Debug.isDebugBuild;
			return new EntityActivationCommand[14]
			{
				new EntityActivationCommand("talk", "talk", flag && state != State.Shutdown),
				new EntityActivationCommand("service", "service", flag2),
				new EntityActivationCommand("repair", "wrench", (float)Health < base.Stats.Health.Max),
				new EntityActivationCommand("lock", "lock", !isLocked),
				new EntityActivationCommand("unlock", "unlock", isLocked),
				new EntityActivationCommand("keypad", "keypad", _enabled: true),
				new EntityActivationCommand("take", "hand", _enabled: true),
				new EntityActivationCommand("stay", "run_and_gun", flag && OrderState != Orders.Stay && state != State.Shutdown),
				new EntityActivationCommand("follow", "run", flag && OrderState != Orders.Follow && state != State.Shutdown),
				new EntityActivationCommand("heal", "cardio", flag && state != State.Shutdown && TargetCanBeHealed(_entityFocusing)),
				new EntityActivationCommand("storage", "loot_sack", _enabled: true),
				new EntityActivationCommand("drone_silent", isQuietMode ? "sight" : "stealth", _enabled: true),
				new EntityActivationCommand("drone_light", isFlashlightOn ? "electric_switch" : "lightbulb", isFlashlightAttached),
				new EntityActivationCommand("force_pickup", "store_all_up", flag2)
			};
		}
		bool flag3 = !isLocked || belongsToPlayerId(_entityFocusing.entityId) || IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		bool flag4 = isLocked && !IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		bool flag5 = (float)Health < base.Stats.Health.Max;
		if (flag3 || flag4 || flag5 || flag2)
		{
			return new EntityActivationCommand[4]
			{
				new EntityActivationCommand("storage", "loot_sack", flag3),
				new EntityActivationCommand("keypad", "keypad", flag4),
				new EntityActivationCommand("repair", "wrench", flag5),
				new EntityActivationCommand("force_pickup", "store_all_up", flag2)
			};
		}
		PlaySound("ui_denied");
		return new EntityActivationCommand[0];
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		EntityPlayerLocal entityPlayer = _entityFocusing as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayer);
		int requestType = -1;
		if (belongsToPlayerId(_entityFocusing.entityId))
		{
			switch (_indexInBlockActivationCommands)
			{
			case 0:
				startDialog(_entityFocusing);
				break;
			case 1:
				requestType = _indexInBlockActivationCommands;
				break;
			case 2:
				doRepairAction(entityPlayer, uIForPlayer);
				break;
			case 3:
				PlaySound("misc/locking");
				isLocked = !isLocked;
				SendSyncData(2);
				break;
			case 4:
				PlaySound("misc/unlocking");
				isLocked = !isLocked;
				SendSyncData(2);
				break;
			case 5:
				doKeypadAction(uIForPlayer);
				break;
			case 6:
				pickup(_entityFocusing);
				break;
			case 7:
				SentryMode();
				break;
			case 8:
				FollowMode();
				break;
			case 9:
				HealOwner();
				break;
			case 10:
				requestType = _indexInBlockActivationCommands;
				break;
			case 11:
				isQuietMode = !isQuietMode;
				idleLoop?.Stop(entityId);
				idleLoop = null;
				SendSyncData(32);
				break;
			case 12:
				doToggleLightAction();
				break;
			case 13:
				pickup(_entityFocusing);
				break;
			}
		}
		else
		{
			switch (_indexInBlockActivationCommands)
			{
			case 0:
				requestType = 10;
				break;
			case 1:
				doKeypadAction(uIForPlayer);
				break;
			case 2:
				doRepairAction(entityPlayer, uIForPlayer);
				break;
			case 3:
				pickup(_entityFocusing);
				break;
			}
		}
		processRequest(entityPlayer, requestType);
		return false;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (DroneManager.Debug_LocalControl)
		{
			return;
		}
		SyncOwnerData();
		UpdateTransitionState();
		UpdateAnimStates();
		UpdateShutdownState();
		if (!isQuietMode && idleLoop == null && state == State.Idle && !GameManager.IsDedicatedServer && GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
		{
			idleLoop = PlaySoundLoop("drone_idle_hover", 0.2f);
		}
		if ((state == State.Idle || state == State.Sentry || state == State.Follow) && areaScanTimer > 0f)
		{
			areaScanTimer -= Time.deltaTime;
			if (areaScanTimer <= 0f)
			{
				isInConfinedSpace = pathMan.IsConfinedSpace(position, 3f);
				areaScanTimer = areaScanTime;
			}
		}
		UpdatePartyBuffs();
		if ((bool)Owner)
		{
			if ((bool)currentTarget)
			{
				if (!steering.IsInRange(currentTarget.position, FollowDistance * 2f))
				{
					if (decelerationTime > 0f)
					{
						decelerationTime = 0f;
					}
					accelerationTime += 0.05f;
					currentSpeedFlying = Mathf.Lerp(currentSpeedFlying, Mathf.Max(15f, (currentTarget.position - position).magnitude), Mathf.Clamp01(accelerationTime / SpeedFlying));
				}
				else
				{
					if (accelerationTime > 0f)
					{
						accelerationTime = 0f;
					}
					decelerationTime += 0.05f;
					currentSpeedFlying = Mathf.Lerp(currentSpeedFlying, SpeedFlying, Mathf.Clamp01(decelerationTime / (SpeedFlying * 0.5f)));
				}
			}
			UpdateDroneSystems();
			EntityPlayerLocal entityPlayerLocal = Owner as EntityPlayerLocal;
			if ((bool)entityPlayerLocal && state == State.Idle)
			{
				if (focusBoxNode == null)
				{
					if (entityPlayerLocal.MoveController.FocusBoxPosition == World.worldToBlockPos(position))
					{
						RaycastNode raycastNode = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, position));
						if (raycastNode != null)
						{
							focusBoxNode = raycastNode;
						}
					}
				}
				else
				{
					Vector3 vector = focusBoxNode.Center - position;
					RaycastPathUtils.DrawLine(position, focusBoxNode.Center, Color.yellow);
					if (isOutOfRange(focusBoxNode.Center, 0.25f))
					{
						move(vector.normalized);
					}
					else
					{
						focusBoxNode = null;
					}
				}
			}
			else if (state != State.Idle && focusBoxNode != null)
			{
				focusBoxNode = null;
			}
		}
		UpdateDroneServiceMenu();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateTransitionState()
	{
		if (transitionState == State.None)
		{
			return;
		}
		if (state == transitionState)
		{
			transitionState = State.None;
			return;
		}
		switch (transitionState)
		{
		case State.Shutdown:
			isShutdownPending = true;
			break;
		case State.Heal:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ServerHealTarget(currentTarget, userRequestedHeal);
			}
			else
			{
				if (initSuppressVOTimer <= 0f)
				{
					PlayVO("drone_heal", _hasPriority: true);
					Manager.Play(this, "drone_healeffect");
				}
				setState(transitionState);
				initSuppressVOTimer = 0f;
			}
			userRequestedHeal = false;
			break;
		case State.Idle:
			if (state == State.Shutdown)
			{
				setShutdown(value: false);
			}
			else
			{
				setState(transitionState);
			}
			break;
		default:
			setState(transitionState);
			break;
		}
		transitionState = State.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPartyMemberAdded(EntityPlayer player)
	{
		registeredPartyMembers.Add(player.entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPartyMemberRemoved(EntityPlayer player)
	{
		registeredPartyMembers.Remove(player.entityId);
		removeSupportBuff(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAnimStates()
	{
		if (stateTime < 1f)
		{
			Animator componentInChildren = GetComponentInChildren<Animator>();
			if (!componentInChildren.enabled && !componentInChildren.GetAnimatorTransitionInfo(0).IsName("Base Layer.Idle"))
			{
				isAnimationStateSet = false;
			}
		}
		if (!isAnimationStateSet)
		{
			Animator componentInChildren2 = GetComponentInChildren<Animator>();
			if (!componentInChildren2.enabled)
			{
				componentInChildren2.enabled = true;
			}
			if (Health > 1 && (bool)Owner)
			{
				if (PlayWakeupAnim)
				{
					playWakeupAnim();
					PlayWakeupAnim = false;
				}
				else
				{
					playIdleAnim();
				}
			}
			else
			{
				componentInChildren2.Play("Base Layer.Idle", 0, 0f);
				componentInChildren2.Update(0f);
				componentInChildren2.StopPlayback();
				componentInChildren2.enabled = false;
			}
			if (wakeupAnimTime <= 0f && GameManager.Instance.GetPersistentLocalPlayer() != null)
			{
				if ((bool)Owner)
				{
					Manager.Stop(Owner.entityId, "drone_take");
				}
				PlayVO("drone_wakeup");
			}
			isAnimationStateSet = true;
		}
		if (!(wakeupAnimTime > 0f))
		{
			return;
		}
		wakeupAnimTime -= 0.05f;
		if (wakeupAnimTime <= 0f && !GameManager.IsDedicatedServer)
		{
			if ((bool)Owner)
			{
				Manager.Stop(Owner.entityId, "drone_take");
			}
			PlayVO("drone_wakeup");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShutdownState()
	{
		if (isShutdownPending)
		{
			performShutdown();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SendSyncData(32768);
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (!Owner && state != State.Shutdown)
		{
			performShutdown();
			SendSyncData(32768);
		}
		if ((bool)Owner && Owner.Health <= 0 && state != State.Shutdown && state != State.Sentry)
		{
			performShutdown();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SendSyncData(32768);
			}
		}
		if (Health > 1 && (bool)Owner && Owner.Health > 1 && Vector3.Distance(position, Owner.position) < 10f && state == State.Shutdown)
		{
			setShutdown(value: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SendSyncData(32768);
			}
		}
		if (state == State.Shutdown)
		{
			processShutdown();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePartyBuffs()
	{
		if ((bool)Owner && isSupportModAttached && !isEntityRemote)
		{
			BuffAllies();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (DroneManager.Debug_LocalControl)
		{
			float num = debugInputSpeed;
			if (InputUtils.ShiftKeyPressed)
			{
				num *= 10f;
			}
			debugInputFwd = reconCam.transform.forward;
			debugInputFwd.y = 0f;
			if (Input.GetKey(KeyCode.W))
			{
				move(debugInputFwd, num);
			}
			if (Input.GetKey(KeyCode.S))
			{
				move(-debugInputFwd, num);
			}
			debugInputRgt = reconCam.transform.right;
			debugInputRgt.y = 0f;
			if (Input.GetKey(KeyCode.A))
			{
				move(-debugInputRgt, num);
			}
			if (Input.GetKey(KeyCode.D))
			{
				move(debugInputRgt, num);
			}
			debugInputUp = reconCam.transform.up;
			debugInputUp.x = 0f;
			debugInputUp.z = 0f;
			if (Input.GetKey(KeyCode.Space))
			{
				move(debugInputUp, num * 0.5f);
			}
			if (Input.GetKey(KeyCode.C))
			{
				move(-debugInputUp, num * 0.5f);
			}
			RaycastPathUtils.DrawBounds(Owner.GetBlockPosition().ToVector3CenterXZ() - new Vector3(0.5f, 0f, 0.5f), Color.cyan, 1f);
		}
		else
		{
			GetEntitySenses().ClearIfExpired();
			if (Owner != null)
			{
				updateState();
				debugUpdate();
			}
		}
	}

	public void SetItemValueToLoad(ItemValue itemValue)
	{
		itemvalueToLoad = itemValue.Clone();
	}

	public void LoadMods()
	{
		LootContainer lootContainer = LootContainer.GetLootContainer("roboticDrone");
		Vector2i size = lootContainer.size;
		if (base.lootContainer == null)
		{
			base.lootContainer = new TileEntityLootContainer((Chunk)null);
			base.lootContainer.entityId = entityId;
			base.lootContainer.lootListName = lootContainer.Name;
			base.lootContainer.SetContainerSize(size);
			bag.SetupSlots(ItemStack.CreateArray(size.x * size.y));
		}
		base.lootContainer.bWasTouched = true;
		lightManager.DisableMaterials("junkDroneLamp");
		GameObject gameObject = base.transform.FindInChilds("freightBox").gameObject;
		GameObject gameObject2 = base.transform.FindInChilds("armor").gameObject;
		GameObject gameObject3 = base.transform.FindInChilds("machineGun").gameObject;
		GameObject gameObject4 = base.transform.FindInChilds("teddyBear").gameObject;
		gameObject?.SetActive(value: false);
		gameObject2?.SetActive(value: false);
		gameObject3?.SetActive(value: false);
		gameObject4?.SetActive(value: false);
		int num = size.x * size.y;
		if (OriginalItemValue.HasMods())
		{
			for (int i = 0; i < OriginalItemValue.Modifications.Length; i++)
			{
				ItemValue itemValue = OriginalItemValue.Modifications[i];
				if (itemValue.ItemClass == null)
				{
					continue;
				}
				switch (itemValue.ItemClass.Name)
				{
				case "modRoboticDroneCargoMod":
					num += 8;
					if ((bool)gameObject)
					{
						gameObject.SetActive(value: true);
					}
					break;
				case "modRoboticDroneArmorPlatingMod":
					armorDamageReduction = 0.5f;
					if ((bool)gameObject2)
					{
						gameObject2.SetActive(value: true);
					}
					break;
				case "modRoboticDroneStunWeaponMod":
					isStunModAttached = true;
					break;
				case "modRoboticDroneMedicMod":
					isHealModAttached = true;
					break;
				case "modRoboticDroneWeaponMod":
					isGunModAttached = true;
					if ((bool)gameObject3)
					{
						gameObject3.SetActive(value: true);
					}
					break;
				case "modRoboticDroneMoraleBoosterMod":
					isSupportModAttached = true;
					if ((bool)gameObject4)
					{
						gameObject4.SetActive(value: true);
					}
					break;
				case "modRoboticDroneHeadlampMod":
				{
					isFlashlightAttached = true;
					DroneLightManager.LightEffect[] lightEffects = lightManager.LightEffects;
					if (lightEffects.Length != 0)
					{
						LightManager.RegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
					}
					if (isFlashlightOn)
					{
						lightManager.InitMaterials("junkDroneLamp");
					}
					break;
				}
				}
			}
		}
		ItemStack[] array = ItemStack.CreateArray(num);
		Array.Copy(base.lootContainer.items, 0, array, 0, (base.lootContainer.items.Length < num) ? base.lootContainer.items.Length : num);
		bag.SetSlots(array);
		base.lootContainer.SetContainerSize(new Vector2i(8, Mathf.RoundToInt(num / 8)), clearItems: false);
		base.lootContainer.items = bag.GetSlots();
		Color color = prefabColor;
		ItemValue itemValue2 = OriginalItemValue.CosmeticMods[0];
		if (OriginalItemValue.CosmeticMods.Length != 0 && itemValue2 != null && !itemValue2.IsEmpty())
		{
			Vector3 vector = Block.StringToVector3(OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
			color.r = vector.x;
			color.g = vector.y;
			color.b = vector.z;
		}
		for (int j = 0; j < paintableParts.Length; j++)
		{
			SetPaint(paintableParts[j], color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initWorldValues(bool value)
	{
		bWillRespawn = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeapons()
	{
		stunWeapon = new DroneWeapons.StunBeamWeapon(this);
		stunWeapon.Init();
		machineGunWeapon = new DroneWeapons.MachineGunWeapon(this);
		machineGunWeapon.Init();
		healWeapon = new DroneWeapons.HealBeamWeapon(this);
		healWeapon.Init();
	}

	public Color GetPaintColor()
	{
		return base.transform.FindRecursive("BaseMesh").GetComponentInChildren<Renderer>().sharedMaterial.color;
	}

	public void SetPaint(string childName, Color color)
	{
		Transform transform = base.transform.FindRecursive(childName);
		if ((bool)transform && transform.gameObject.activeSelf)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.color = color;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playWakeupAnim()
	{
		GetComponentInChildren<Animator>().Play("Base Layer.SpawnIn");
		wakeupAnimTime = 2.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playIdleAnim()
	{
		Animator componentInChildren = GetComponentInChildren<Animator>();
		componentInChildren.Play("Base Layer.Idle", 0, 0f);
		componentInChildren.Update(0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnRegsiterMovingLights()
	{
		DroneLightManager.LightEffect[] lightEffects = lightManager.LightEffects;
		if (lightEffects.Length != 0)
		{
			LightManager.UnRegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doToggleLightAction()
	{
		isFlashlightOn = !isFlashlightOn;
		setFlashlightOn(isFlashlightOn);
		SendSyncData(64);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setFlashlightOn(bool value)
	{
		if (value)
		{
			lightManager.InitMaterials("junkDroneLamp");
		}
		else
		{
			lightManager.DisableMaterials("junkDroneLamp");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Handle PlaySoundLoop(string sound_path, float _vol = 1f)
	{
		return Manager.Play(this, sound_path, _vol, wantHandle: true);
	}

	public void PlaySound(string sound_path, float _vol = 1f)
	{
		PlaySound(this, sound_path, _isVO: false, _hasPriority: false, _vol);
	}

	public void PlayVO(string sound_path, bool _hasPriority = false, float _vol = 1f)
	{
		if (GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
		{
			PlaySound(this, sound_path, _isVO: true, _hasPriority, _vol);
		}
	}

	public void PlaySound(Entity entity, string sound_path, bool _isVO = false, bool _hasPriority = false, float _vol = 1f)
	{
		if (isQuietMode)
		{
			return;
		}
		if (_isVO)
		{
			if (_hasPriority)
			{
				voHandle?.Stop(entityId);
			}
			voHandle = Manager.Play(entity, sound_path, _vol, wantHandle: true);
		}
		else
		{
			Manager.Play(entity, sound_path, _vol);
		}
	}

	public void NotifySyncOwner()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(OwnerID);
		if (playerData != null)
		{
			belongsPlayerId = playerData.EntityId;
			Owner = GameManager.Instance.World.GetEntity(belongsPlayerId) as EntityAlive;
		}
		if ((bool)Owner)
		{
			currentTarget = Owner;
			rotation = Quaternion.LookRotation(Owner.position - position).eulerAngles;
			if (GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
			{
				HandleNavObject();
				hasNavObjectsEnabled = true;
				SetOwner(OwnerID);
				SendSyncData(3);
			}
		}
	}

	public void SyncOwnerData()
	{
		if (isOwnerSyncPending)
		{
			NotifySyncOwner();
			isOwnerSyncPending = false;
		}
		if (belongsPlayerId == -1)
		{
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (persistentPlayerList != null)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(OwnerID);
				if (playerData != null)
				{
					belongsPlayerId = playerData.EntityId;
				}
			}
		}
		if ((bool)Owner)
		{
			return;
		}
		Owner = (EntityAlive)GameManager.Instance.World.GetEntity(belongsPlayerId);
		if ((bool)Owner)
		{
			Owner.AddOwnedEntity(this);
			currentTarget = Owner;
			if (!hasNavObjectsEnabled && GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
			{
				HandleNavObject();
				hasNavObjectsEnabled = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool belongsToPlayerId(int id)
	{
		return belongsPlayerId == id;
	}

	public bool isAlly(EntityAlive _target)
	{
		if (debugFriendlyFire)
		{
			return false;
		}
		if ((bool)Owner && Owner == _target)
		{
			return true;
		}
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(OwnerID);
		if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
		{
			PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
			if (playerData.ACL != null && persistentPlayerData != null && playerData.ACL.Contains(persistentPlayerData.PrimaryId))
			{
				return true;
			}
			EntityPlayer entityPlayer = Owner as EntityPlayer;
			EntityPlayer entityPlayer2 = _target as EntityPlayer;
			if ((bool)entityPlayer && (bool)entityPlayer2 && entityPlayer.Party != null && entityPlayer.Party.ContainsMember(entityPlayer2))
			{
				return true;
			}
		}
		return false;
	}

	public void BuffAllies()
	{
		EntityPlayer entityPlayer = Owner as EntityPlayer;
		if (!entityPlayer)
		{
			return;
		}
		if (entityPlayer.Party != null)
		{
			knownPartyMembers = entityPlayer.Party.GetMemberIdArray();
			for (int i = 0; i < knownPartyMembers.Length; i++)
			{
				EntityAlive entity = world.GetEntity(knownPartyMembers[i]) as EntityAlive;
				ProcBuffRange(entity);
			}
			return;
		}
		if (knownPartyMembers != null && knownPartyMembers.Length != 0)
		{
			for (int j = 0; j < knownPartyMembers.Length; j++)
			{
				EntityAlive entity2 = world.GetEntity(knownPartyMembers[j]) as EntityAlive;
				removeSupportBuff(entity2);
			}
			knownPartyMembers = null;
		}
		ProcBuffRange(entityPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePartyBuffs(EntityPlayer owner)
	{
		if (owner.Party != null)
		{
			for (int i = 0; i < owner.Party.MemberList.Count; i++)
			{
				EntityAlive entity = owner.Party.MemberList[i];
				removeSupportBuff(entity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcBuffRange(EntityAlive entity)
	{
		if ((bool)entity)
		{
			if ((position - entity.position).magnitude < 32f)
			{
				addSupportBuff(entity);
			}
			else
			{
				removeSupportBuff(entity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addSupportBuff(EntityAlive entity)
	{
		if (state != State.Shutdown && !entity.Buffs.HasBuff("buffJunkDroneSupportEffect"))
		{
			entity.Buffs.AddBuff("buffJunkDroneSupportEffect");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeSupportBuff(EntityAlive entity)
	{
		if ((bool)entity && entity.Buffs.HasBuff("buffJunkDroneSupportEffect") && !doesEntityHaveSupport(entity))
		{
			entity.Buffs.RemoveBuff("buffJunkDroneSupportEffect");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doesEntityHaveSupport(EntityAlive entity)
	{
		OwnedEntityData[] array = entity.GetOwnedEntities();
		for (int i = 0; i < array.Length; i++)
		{
			EntityDrone entityDrone = world.GetEntity(array[i].Id) as EntityDrone;
			if ((bool)entityDrone && entityDrone.isSupportModAttached)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doKeypadAction(LocalPlayerUI playerUI)
	{
		PlaySound("misc/password_type");
		GUIWindow window = playerUI.windowManager.GetWindow(XUiC_KeypadWindow.ID);
		window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, new Action(StopUIInsteractionSecurity));
		XUiC_KeypadWindow.Open(playerUI, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processRequest(EntityPlayer entityPlayer, int requestType)
	{
		if (requestType < 0)
		{
			return;
		}
		interactionRequestType = requestType;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ValidateInteractingPlayer();
			int num = interactingPlayerId;
			if (num == -1)
			{
				num = entityPlayer.entityId;
			}
			StartInteraction(entityPlayer.entityId, num);
		}
		else
		{
			interactingPlayerId = entityPlayer.entityId;
			SendSyncData(4096);
			interactingPlayerId = -1;
		}
	}

	public void startDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uIForPlayer.xui.Dialog.Respondent = this;
		uIForPlayer.windowManager.CloseAllOpenWindows();
		uIForPlayer.windowManager.Open("dialog", _bModal: true);
		PlayVO("drone_greeting");
	}

	public bool HasStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName);
		ItemClass itemClass = item.ItemClass;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (itemClass != null)
		{
			num = entity.bag.GetItemCount(item);
			num2 = entity.inventory.GetItemCount(item);
			num3 = ((entity.lootContainer != null && entity.lootContainer.HasItem(item)) ? 1 : 0);
		}
		return num + num2 + num3 > 0;
	}

	public ItemStack TakeStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName);
		if (item.ItemClass != null)
		{
			entity.bag.GetItemCount(item);
			int itemCount = entity.inventory.GetItemCount(item);
			if (entity.lootContainer != null)
			{
				entity.lootContainer.HasItem(item);
			}
			if (itemCount > 0)
			{
				entity.inventory.DecItem(item, 1);
			}
			else if (entity.lootContainer != null)
			{
				takeFromEntityContainer(entity, itemGroupOrName, fastTags);
			}
			else
			{
				entity.bag.DecItem(item, 1);
			}
			return new ItemStack(item.Clone(), 1);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void takeFromEntityContainer(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemStack[] array = entity.bag.GetSlots();
		if (entity.lootContainer != null)
		{
			array = entity.lootContainer.GetItems();
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].itemValue != null && array[i].itemValue.ItemClass != null && array[i].itemValue.ItemClass.HasAnyTags(fastTags) && array[i].count > 0 && array[i].itemValue.ItemClass.Name.ContainsCaseInsensitive(itemGroupOrName))
			{
				array[i].count--;
				if (array[i].count == 0)
				{
					array[i] = ItemStack.Empty.Clone();
				}
				entity.bag.SetSlots(array);
				entity.bag.OnUpdate();
				if (entity.lootContainer != null)
				{
					entity.lootContainer.UpdateSlot(i, array[i]);
				}
			}
		}
	}

	public void OpenStorage(Entity _entityFocusing)
	{
		processRequest(_entityFocusing as EntityPlayerLocal, 10);
	}

	public ItemValue GetUpdatedItemValue()
	{
		OriginalItemValue.UseTimes = (float)OriginalItemValue.MaxUseTimes * (1f - (float)Health / base.Stats.Health.BaseMax);
		return OriginalItemValue;
	}

	public void Collect(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(GetUpdatedItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.Toolbelt.AddItem(itemStack) && !uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId);
		}
		OriginalItemValue = GetUpdatedItemValue();
		base.transform.gameObject.SetActive(value: false);
		if ((bool)Owner)
		{
			Owner.RemoveOwnedEntity(entityId);
			if (DroneManager.Instance != null)
			{
				DroneManager.Instance.RemoveTrackedDrone(this, EnumRemoveEntityReason.Despawned);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getItemCount()
	{
		int num = 0;
		ItemStack[] items = lootContainer.GetItems();
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].itemValue != null && items[i].itemValue.ItemClass != null)
			{
				num += items[i].count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void accessInventory(Entity _entityFocusing)
	{
		Log.Out("ItemCountOnOpen: " + getItemCount());
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uIForPlayer.xui.Dialog.Respondent = this;
		uIForPlayer.windowManager.CloseAllOpenWindows();
		string lootContainerName = Localization.Get(EntityClass.list[entityClass].entityClassName);
		GUIWindow window = uIForPlayer.windowManager.GetWindow("looting");
		((XUiC_LootWindowGroup)((XUiWindowGroup)window).Controller).SetTileEntityChest(lootContainerName, lootContainer);
		window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, new Action(StopUIInteraction));
		uIForPlayer.windowManager.Open("looting", _bModal: true);
		isInteractionLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pickup(Entity _entityFocusing)
	{
		if (!lootContainer.IsEmpty())
		{
			PlayVO("drone_takefail", _hasPriority: true);
			GameManager.ShowTooltip(Owner as EntityPlayerLocal, Localization.Get("ttEmptyVehicleBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		ItemStack itemStack = new ItemStack(GetUpdatedItemValue(), 1);
		EntityPlayer entityPlayer = _entityFocusing as EntityPlayer;
		if (entityPlayer.inventory.CanTakeItem(itemStack) || entityPlayer.bag.CanTakeItem(itemStack))
		{
			isBeingPickedUp = true;
			PlaySound(entityPlayer, "drone_take", _isVO: true, _hasPriority: true);
			initWorldValues(value: false);
			nativeCollider.enabled = false;
			GameManager.Instance.CollectEntityServer(entityId, entityPlayer.entityId);
			if (entityPlayer.Buffs.HasBuff("buffJunkDroneSupportEffect"))
			{
				entityPlayer.Buffs.RemoveBuff("buffJunkDroneSupportEffect");
			}
			RemovePartyBuffs(entityPlayer);
			UnRegsiterMovingLights();
		}
		else
		{
			GameManager.ShowTooltip(entityPlayer as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
		}
	}

	public int GetStoredItemCount()
	{
		int num = 0;
		for (int i = 0; i < lootContainer.items.Length; i++)
		{
			if (!lootContainer.items[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public bool CanRemoveExtraStorage()
	{
		if (GetStoredItemCount() < StorageCapacity - 8)
		{
			return true;
		}
		return false;
	}

	public void NotifyToManyStoredItems()
	{
		if (!(overItemLimitCooldown > 0f))
		{
			overItemLimitCooldown = 5f;
			if (!CanRemoveExtraStorage())
			{
				GameManager.ShowTooltip(Owner as EntityPlayerLocal, Localization.Get("ttJunkDroneEmptySomeStorage"), string.Empty, "ui_denied");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDroneServiceMenu()
	{
		if (overItemLimitCooldown > 0f)
		{
			overItemLimitCooldown -= 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, bool ignoreObsticles = false)
	{
		move(dir, currentSpeedFlying, ignoreObsticles);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, float speedFlying, bool ignoreObsticles = false)
	{
		Vector3 end = position + dir.normalized * physColHeight;
		if (ownerIsOnVehicle || !RaycastPathUtils.IsPositionBlocked(position, end, 1073807360) || ignoreObsticles)
		{
			motion += dir * speedFlying * 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void moveAlongPath(Vector3 dir)
	{
		_ = position + dir.normalized * physColHeight;
		motion += dir * SpeedFlying * 0.05f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canMove(Vector3 dir)
	{
		Vector3 end = position + dir.normalized * physColHeight;
		return !RaycastPathUtils.IsPositionBlocked(position, end, 1073807360);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToDir(Vector3 dir)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(dir), (1f - Vector3.Angle(base.transform.forward, dir) / 180f) * RotationSpeed * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToEuler(Vector3 rot)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(rot), (1f - Vector3.Angle(base.transform.forward, (rot - base.transform.eulerAngles).normalized) / 180f) * RotationSpeed * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateTo(Vector3 dir)
	{
		if (dir != Vector3.zero)
		{
			rotation = rotateToDir(dir);
		}
	}

	public int GetRepairAmountNeeded()
	{
		return GetMaxHealth() - Health;
	}

	public void RepairParts(int _amount)
	{
		Health += _amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doRepairAction(EntityPlayer entityPlayer, LocalPlayerUI playerUI)
	{
		string text = "resourceRepairKit";
		if (HasStoredItem(entityPlayer, text, repairKitTags))
		{
			playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(ItemClass.GetItem(text), 1));
			PlaySound("crafting/craft_repair_item");
			TakeStoredItem(entityPlayer, text, repairKitTags);
			performRepair();
			SendSyncData(16);
		}
		else
		{
			Manager.PlayInsidePlayerHead("misc/missingitemtorepair");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performRepair()
	{
		Health = (int)base.Stats.Health.Max;
		OriginalItemValue.UseTimes = 0f;
		setShutdown(value: false);
		PlayWakeupAnim = true;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SendSyncData(16);
		}
	}

	public bool TargetCanBeHealed(EntityAlive entity)
	{
		if (isHealModAttached && healWeapon.targetCanBeHealed(entity))
		{
			return HasHealingItem();
		}
		return false;
	}

	public bool HasHealingItem()
	{
		return healWeapon.hasHealingItem();
	}

	public void ToggleHealAllies()
	{
		SetHealAllies(!IsHealingAllies);
		SendSyncData(256);
	}

	public void SetHealAllies(bool value)
	{
		IsHealingAllies = value;
		allyHealMode = (value ? AllyHealMode.HealAllies : AllyHealMode.DoNotHeal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateNeedsHealItemCheck()
	{
		if (needsHealItemTimer > 0f)
		{
			needsHealItemTimer -= 0.05f;
		}
		if (needsHealItemTimer <= 0f && needsHealNotifyCount < 2 && checkNotifityNeedsHealItem())
		{
			needsHealItemTimer = 30f;
			needsHealNotifyCount++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearNeedsHealItemCheck()
	{
		needsHealItemTimer = 0f;
		needsHealNotifyCount = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkNotifityNeedsHealItem()
	{
		if (!healWeapon.hasHealingItem())
		{
			GameManager.ShowTooltip(Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored"), string.Empty, "ui_denied");
			PlaySound("drone_empty");
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> GetHealingTargetsInRange(List<int> playerIds, float range)
	{
		List<EntityAlive> list = new List<EntityAlive>();
		for (int i = 0; i < playerIds.Count; i++)
		{
			int num = playerIds[i];
			EntityAlive entityAlive = world.GetEntity(num) as EntityAlive;
			if ((bool)entityAlive)
			{
				float magnitude = (position - entityAlive.position).magnitude;
				bool flag = !RaycastPathUtils.IsPositionBlocked(position, entityAlive.getHeadPosition(), 65536);
				if (magnitude < range && healWeapon.targetNeedsHealing(entityAlive) && flag)
				{
					list.Add(entityAlive);
				}
			}
		}
		return list;
	}

	public void HealOwner()
	{
		if (!healWeapon.hasHealingItem())
		{
			GameManager.ShowTooltip(Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored"), string.Empty, "ui_denied");
			PlaySound("drone_empty");
		}
		else if (state != State.Heal && healWeapon.canFire())
		{
			userRequestedHeal = true;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ServerHealTarget(Owner, userRequestedHeal);
				return;
			}
			setState(State.Heal);
			SendSyncData(32768);
			setState(State.Idle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServerHealTarget(EntityAlive target, bool healReq = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && state != State.Heal && healWeapon.canFire() && (healReq || healWeapon.isTargetInNeedOfMedical(target)))
		{
			currentTarget = target;
			healTarget(target);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTarget(EntityAlive target)
	{
		SetAttackTarget(target, 1200);
		if ((bool)attackTarget)
		{
			setState(State.Heal);
			SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onHealDone()
	{
		SetRevengeTarget(null);
		SetAttackTarget(null, 0);
		if ((bool)Owner)
		{
			Owner.Buffs.RemoveBuff("buffJunkDroneHealCooldownEffect");
		}
		setState(State.Idle);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SendSyncData(32768);
		}
	}

	public State GetState()
	{
		return state;
	}

	public void FollowMode(bool playVO = true)
	{
		if (playVO)
		{
			PlayVO("drone_command", _hasPriority: true);
		}
		setOrders(Orders.Follow);
		setState(State.Follow);
		SendSyncData(49152);
	}

	public void SentryMode()
	{
		PlayVO("drone_command", _hasPriority: true);
		sentryPos = position;
		setOrders(Orders.Stay);
		setState(State.Sentry);
		SendSyncData(49152);
		if ((bool)Owner && Owner.HasOwnedEntity(entityId))
		{
			Owner.GetOwnedEntity(entityId).SetLastKnownPosition(position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(State next)
	{
		logDrone($"State: {state} > {next}");
		lastState = state;
		state = next;
		stateTime = 0f;
		if (lastState == State.Shutdown)
		{
			Animator componentInChildren = GetComponentInChildren<Animator>();
			if (!componentInChildren.enabled)
			{
				componentInChildren.enabled = true;
			}
		}
		switch (state)
		{
		case State.Follow:
			if (lastState == State.Sentry && (bool)Owner && Owner.HasOwnedEntity(entityId))
			{
				Owner.GetOwnedEntity(entityId).ClearLastKnownPostition();
			}
			break;
		case State.Heal:
			ClearNeedsHealItemCheck();
			break;
		case State.Idle:
		case State.Sentry:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void idleState()
	{
		if ((bool)currentTarget && !isEntityAboveOrBelow(currentTarget))
		{
			Vector3 headPosition = currentTarget.getHeadPosition();
			rotateTo(steering.GetDir2D(position, headPosition));
			float num = 0f;
			if (position.y - currentTarget.getHeadPosition().y > num || position.y - currentTarget.getHeadPosition().y < num)
			{
				Vector3 target = position;
				target.y = currentTarget.getHeadPosition().y;
				move(steering.Seek(position, target, SpeedFlying));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sentryState()
	{
		Vector3 target = sentryPos;
		if (world.IsChunkAreaLoaded(target) && !steering.IsInRange(target, 0.25f))
		{
			Vector3 dir = steering.Seek(position, target, 0.25f);
			move(dir);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findOpenFollowPoints(bool debugDraw = false)
	{
		Vector3 lookVector = currentTarget.GetLookVector();
		lookVector.y = 0f;
		Vector3[] array = getGroupPositions(currentTarget, FollowDistance + 1f);
		Array.Sort(array, [PublicizedFrom(EAccessModifier.Private)] (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
		hasOpenGroupPos = false;
		Vector3 vector = currentTarget.getHeadPosition();
		for (int num = 0; num < array.Length; num++)
		{
			Vector3i vector3i = World.worldToBlockPos(array[num]);
			if (!RaycastPathUtils.IsPointBlocked(array[num], currentTarget.getHeadPosition(), 1073807360, debugDraw: true) && !RaycastPathUtils.IsPointBlocked(currentTarget.getHeadPosition(), array[num], 1073807360, debugDraw: true))
			{
				RaycastNode raycastNode = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, vector3i.ToVector3Center(), useTarget: true));
				if (raycastNode != null)
				{
					if (!hasOpenGroupPos)
					{
						vector = raycastNode.Center;
						hasOpenGroupPos = true;
					}
					if (debugDraw)
					{
						RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center()), Color.yellow, 0f);
					}
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center()), Color.red, 0f);
			}
		}
		Vector3[] groupFallbackPositions = getGroupFallbackPositions(currentTarget, FollowDistance + 1f);
		Array.Sort(groupFallbackPositions, [PublicizedFrom(EAccessModifier.Private)] (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
		for (int num2 = 0; num2 < groupFallbackPositions.Length; num2++)
		{
			Vector3i vector3i2 = World.worldToBlockPos(groupFallbackPositions[num2]);
			if (!RaycastPathUtils.IsPointBlocked(groupFallbackPositions[num2], currentTarget.getHeadPosition(), 1073807360, debugDraw: true) && !RaycastPathUtils.IsPointBlocked(currentTarget.getHeadPosition(), groupFallbackPositions[num2], 1073807360, debugDraw: true))
			{
				RaycastNode raycastNode2 = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, vector3i2.ToVector3Center(), useTarget: true));
				if (raycastNode2 != null)
				{
					if (!hasOpenGroupPos)
					{
						vector = raycastNode2.Center;
						hasOpenGroupPos = true;
					}
					if (debugDraw)
					{
						RaycastPathUtils.DrawNode(new RaycastNode(vector3i2.ToVector3Center()), Color.yellow, 0f);
					}
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i2.ToVector3Center()), Color.red, 0f);
			}
		}
		if (!hasOpenGroupPos)
		{
			Vector3i vector3i3 = World.worldToBlockPos(currentTarget.getHeadPosition());
			RaycastNode raycastNode3 = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, vector3i3.ToVector3Center()));
			if (raycastNode3 != null)
			{
				vector = raycastNode3.Center;
			}
		}
		followTargetPos = vector;
		if (debugDraw)
		{
			RaycastPathUtils.DrawBounds(World.worldToBlockPos(followTargetPos), Color.green, 0f);
		}
		return followTargetPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void makePath()
	{
		if (pathMan.Path == null)
		{
			pathMan.CreatePath(position + Vector3.up, currentTarget.getHeadPosition() - currentTarget.GetForwardVector() * 2f + Vector3.up, SpeedFlying, canBreakBlocks: false);
		}
		if (pathMan.isBuildingPath)
		{
			return;
		}
		if (pathMan.Path != null && pathMan.Path.Nodes.Count > 0 && nodePath.Count == 0)
		{
			nodePath.AddRange(pathMan.Path.Nodes);
			nodePath.Reverse();
		}
		if (nodePath.Count > 0)
		{
			if ((pathMan.Path.Target - currentTarget.getHeadPosition()).magnitude > FollowDistance)
			{
				pathMan.Clear();
				nodePath.Clear();
				return;
			}
			Vector3 dir = steering.Seek(position, nodePath[0].Center, 0f);
			rotateTo(dir);
			move(dir, SpeedPathing);
			if (steering.IsInRange(nodePath[0].Center, 0.5f))
			{
				nodePath.RemoveAt(0);
				if (nodePath.Count == 0)
				{
					transitionToIdle = true;
				}
			}
		}
		if (transitionToIdle)
		{
			transitionToIdleTime -= 0.05f;
			if (transitionToIdleTime <= 0f)
			{
				transitionToIdleTime = 0.5f;
				pathMan.Clear();
				setState(State.Idle);
				transitionToIdle = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearCurrentPath()
	{
		currentPath.Clear();
		debugPathDelay = 2f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAttachedToVehicle(Entity target)
	{
		if ((bool)target)
		{
			return target.AttachedToEntity as EntityVehicle != null;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void followState()
	{
		if (!currentTarget)
		{
			currentTarget = Owner;
		}
		if (procEnemiesInRange())
		{
			return;
		}
		Vector3 pos = findOpenFollowPoints(debugDraw: true);
		if (IsAttachedToVehicle(currentTarget))
		{
			if (!ownerIsOnVehicle)
			{
				ownerIsOnVehicle = true;
				setNoClip(value: true);
			}
			Entity attachedToEntity = currentTarget.AttachedToEntity;
			rotateTo((attachedToEntity.position - position).normalized);
			Vector3 dir = steering.Seek(position, attachedToEntity.position - attachedToEntity.transform.forward * FollowDistance * 2f + Vector3.up * FollowDistance * 2f, SpeedFlying);
			move(dir);
			return;
		}
		if (ownerIsOnVehicle)
		{
			ownerIsOnVehicle = false;
			SetPosition(pos);
		}
		if (IsStuckInBlock())
		{
			TeleportUnstuck();
			return;
		}
		if (isTargetUnderWater(currentTarget.getHeadPosition()))
		{
			if (currentPath.Count > 0)
			{
				clearCurrentPath();
			}
			Vector3 dir2 = steering.Seek(position, findOpenBlockAbove(currentTarget.getHeadPosition()), 0.2f);
			rotateTo(dir2);
			move(dir2);
			return;
		}
		float magnitude = (currentTarget.getHeadPosition() - position).magnitude;
		bool flag = !RaycastPathUtils.IsPointBlocked(position, followTargetPos, 1073807360, debugDraw: true);
		if (debugPathTiming && debugPathDelay > 0f)
		{
			debugPathDelay -= Time.deltaTime;
			return;
		}
		if (!flag && currentPath.Count == 0)
		{
			getPath(currentTarget.position);
			return;
		}
		if (currentPath.Count > 0)
		{
			currentPathTarget = currentPath[0];
			Vector3 dir3 = steering.Seek(position, currentPathTarget, 1f);
			rotateTo((currentPathTarget - position).normalized);
			move(dir3);
			if (steering.IsInRange(currentPathTarget, 0.5f))
			{
				currentPath.RemoveAt(0);
				return;
			}
			if (currentPath.Count > 1)
			{
				RaycastPathUtils.DrawLine(currentPath[0], currentPath[1], Color.green);
			}
			if (IsStuckInBlock() || IsNotAbleToReachTarget(currentPath[0]))
			{
				if (currentPath.Count > 1)
				{
					teleportToPosition(currentPath[1]);
					currentPath.RemoveRange(0, 2);
				}
				else
				{
					teleportToPosition(currentPath[0]);
					currentPath.RemoveAt(0);
				}
			}
			else if (!RaycastPathUtils.IsPointBlocked(position, followTargetPos, 1073807360, debugDraw: true))
			{
				clearCurrentPath();
			}
			return;
		}
		Vector3 vector = steering.Seek(position, followTargetPos, SpeedFlying);
		if (!steering.IsInRange(followTargetPos, 0.1f))
		{
			if (magnitude > FollowDistance && magnitude < 24f && !RaycastPathUtils.IsPointBlocked(position, currentTarget.position, 1073807360) && Vector3.Angle(currentTarget.GetLookVector(), position - currentTarget.getHeadPosition()) < 45f)
			{
				float num = 0.5f;
				Vector3 vector2 = steering.Flee(position, currentTarget.getHeadPosition(), SpeedFlying);
				if (!RaycastPathUtils.IsPositionBlocked(position, position + (vector + vector2), 1073807360))
				{
					vector += vector2 * num;
				}
			}
			if (steering.GetAltitude(position) < magnitude * 0.33f && !RaycastPathUtils.IsPositionBlocked(position, position + Vector3.up, 1073807360))
			{
				float num2 = 0.75f;
				Vector3 vector3 = steering.Seek(position, position + Vector3.up, SpeedFlying);
				vector += vector3 * num2;
			}
			if (isInteractionFocusOwner())
			{
				ownerFocusTimer += 0.05f;
				if (ownerFocusTimer >= cOwnerFocusTime)
				{
					setState(State.Idle);
					ownerFocusTimer = 0f;
				}
			}
			rotateTo((currentTarget.getHeadPosition() - position).normalized);
			move(vector.normalized);
		}
		if (state == State.Follow && (steering.IsInRange(followTargetPos, 0.5f) || steering.IsInRange(currentTarget.getChestPosition(), FollowDistance)))
		{
			debugPathDelay = 2f;
			setState(State.Idle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInteractionFocusOwner()
	{
		if ((bool)Owner)
		{
			Ray lookRay = Owner.GetLookRay();
			if (Physics.Raycast(lookRay.origin - Origin.position, lookRay.direction, out var hitInfo, 1000f, 16384) && (bool)hitInfo.transform && hitInfo.transform.tag != "Physics")
			{
				Entity component = hitInfo.transform.GetComponent<Entity>();
				if ((bool)component && component.EntityClass.entityClassName == "entityJunkDrone")
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void getPath(Vector3 target)
	{
		if (!findPath(position, target))
		{
			return;
		}
		clearCurrentPath();
		currentPath.AddRange(projectedPath);
		currentPath.RemoveAt(0);
		for (int i = 0; i < currentPath.Count; i++)
		{
			Vector3 value = currentPath[i];
			value.y += 1.5f;
			currentPath[i] = value;
		}
		List<RaycastNode> list = RaycastPathWorldUtils.ScanPath(world, position, currentPath, useDiagnols: false);
		for (int j = 0; j < currentPath.Count; j++)
		{
			Vector3 vector = currentPath[j];
			RaycastNode raycastNode = list[j];
			if (raycastNode.FlowToWaypoint)
			{
				currentPath[j] = (vector + raycastNode.Waypoint.Center) * 0.5f;
			}
		}
		for (int k = 0; k < currentPath.Count - 1; k++)
		{
			Color endColor = Color.cyan;
			if (RaycastPathUtils.IsPointBlocked(currentPath[k], currentPath[k + 1], 1073807360))
			{
				endColor = Color.magenta;
			}
			Utils.DrawLine(currentPath[k] - Origin.position, currentPath[k + 1] - Origin.position, Color.white, endColor, 100, 10f);
		}
		projectedPath.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool findPath(Vector3 start, Vector3 end, bool debugDraw = false)
	{
		PathFinderThread instance = PathFinderThread.Instance;
		if (instance == null)
		{
			return false;
		}
		PathInfo path = instance.GetPath(entityId);
		if (path.path == null && !PathFinderThread.Instance.IsCalculatingPath(entityId))
		{
			PathFinderThread.Instance.FindPath(this, start, end, SpeedFlying, _canBreak: false, null);
			return false;
		}
		if (path.path == null)
		{
			return false;
		}
		for (int i = 0; i < path.path.points.Length; i++)
		{
			Vector3 projectedLocation = path.path.points[i].projectedLocation;
			projectedPath.Add(projectedLocation);
		}
		if (debugDraw)
		{
			for (int j = 0; j < projectedPath.Count - 1; j++)
			{
				Utils.DrawLine(projectedPath[j] - Origin.position, projectedPath[j + 1] - Origin.position, Color.white, Color.cyan, 100, 10f);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void healState()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (!currentTarget)
		{
			onHealDone();
			return;
		}
		rotateTo(currentTarget.position - position);
		float num = FollowDistance;
		EntityAlive entityAlive = GetAttackTarget();
		if ((bool)entityAlive && entityAlive.AttachedToEntity as EntityVehicle != null)
		{
			num *= 3f;
		}
		float magnitude = (position - currentTarget.getHeadPosition()).magnitude;
		bool flag = RaycastPathUtils.IsPointBlocked(position, currentTarget.getHeadPosition(), 1073807360, debugDraw: true);
		if (magnitude > FollowDistance || flag)
		{
			followState();
		}
		if (magnitude <= num && healWeapon.canFire() && !lootContainer.IsUserAccessing() && !isInteractionLocked)
		{
			healWeapon.RegisterOnFireComplete(onHealDone);
			healWeapon.Fire(currentTarget);
			if (GameManager.Instance.GetPersistentLocalPlayer() != null && initSuppressVOTimer <= 0f)
			{
				PlayVO("drone_heal", _hasPriority: true);
				Manager.Play(this, "drone_healeffect");
			}
		}
	}

	public void TeleportUnstuck()
	{
		StartCoroutine(DelayedTeleport());
	}

	public void TeleportIfFollowing()
	{
		if (orderState == Orders.Follow && !isTeleporting)
		{
			StartCoroutine(DelayedTeleport());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void teleportState()
	{
		if (!isTeleporting)
		{
			Log.Out("Drone.teleportState() - {0}", entityId);
			setState(State.Teleport);
			Vector3 vector = new Vector3(Owner.position.x, Owner.position.y + 0.5f, Owner.position.z) - new Vector3(Owner.GetLookVector().x, 0f, Owner.GetLookVector().z) * FollowDistance;
			isTeleporting = true;
			clearCurrentPath();
			motion = Vector3.zero;
			SetPosition(vector);
			FollowMode(playVO: false);
			StartCoroutine(validateTeleport(vector));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayedTeleport()
	{
		yield return new WaitForSeconds(1f);
		teleportState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator validateTeleport(Vector3 target)
	{
		yield return new WaitForSeconds(1f);
		if ((bool)Owner)
		{
			if (isOutOfRange(Owner.position, MaxDistFromOwner))
			{
				Log.Out("teleport failed");
				isTeleporting = false;
			}
			else if (!isOutOfRange(target, FollowDistance * 1.5f))
			{
				Log.Out("teleport success!");
			}
		}
		isTeleporting = false;
		setState(State.Idle);
		yield return null;
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportToPosition(Vector3 telePos)
	{
		motion = Vector3.zero;
		Utils.DrawLine(telePos, position, Color.yellow, Color.green, 100, 20f);
		SetPosition(telePos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performShutdown()
	{
		if ((bool)Owner)
		{
			Manager.Stop(Owner.entityId, "drone_take");
		}
		PlayVO("drone_shutdown", _hasPriority: true);
		if ((bool)Owner && Owner.HasOwnedEntity(entityId))
		{
			Owner.GetOwnedEntity(entityId).SetLastKnownPosition(position);
		}
		setShutdown(value: true);
		isShutdownPending = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdown(bool value)
	{
		GetComponentInChildren<Animator>().enabled = !value;
		PhysicsTransform.gameObject.SetActive(!value);
		IsNoCollisionMode.Value = value;
		if (value)
		{
			SetRevengeTarget(null);
			SetAttackTarget(null, 0);
			setState(State.Shutdown);
			idleLoop?.Stop(entityId);
			idleLoop = null;
			return;
		}
		isShutdown = value;
		isGrounded = value;
		if (orderState == Orders.Stay)
		{
			setState(State.Sentry);
		}
		else
		{
			setState(State.Idle);
		}
		if ((bool)Owner && Owner.HasOwnedEntity(entityId))
		{
			Owner.GetOwnedEntity(entityId).ClearLastKnownPostition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownDestruction(bool value)
	{
		base.transform.FindInChilds("p_smokeLeft").gameObject.SetActive(value);
		base.transform.FindInChilds("p_smokeRight").gameObject.SetActive(value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processShutdown()
	{
		fallBlockPos.RoundToInt(position - blockHeightOffset);
		if ((!hasFallPoint || world.GetBlock(fallBlockPos).isair) && Physics.Raycast(position - Origin.position + blockHeightOffset, Vector3.down, out var hitInfo, 999f, 268500992))
		{
			fallPoint = hitInfo.point;
			isShutdown = true;
			isGrounded = false;
			hasFallPoint = true;
		}
		if (!isGrounded && isShutdown)
		{
			Vector3 pos = position;
			float num = Mathf.Min(1f + Vector3.Distance(position, fallPoint + Origin.position), 5f);
			if (num < 1.2f)
			{
				isGrounded = true;
				return;
			}
			pos.y -= num * 0.05f;
			pos.y = Mathf.Max(pos.y, fallPoint.y);
			SetPosition(pos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDroneSystems()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (bool)Owner && !registeredOwnerHandlers)
		{
			EntityPlayer entityPlayer = Owner as EntityPlayer;
			if ((bool)entityPlayer)
			{
				entityPlayer.PlayerTeleportedDelegates += TeleportIfFollowing;
				registeredOwnerHandlers = true;
			}
		}
		if (registeredPartyMembers == null)
		{
			EntityPlayer entityPlayer2 = Owner as EntityPlayer;
			if ((bool)entityPlayer2)
			{
				Party party = entityPlayer2.Party;
				if (party != null)
				{
					registeredPartyMembers = new List<int>();
					int[] memberIdArray = party.GetMemberIdArray();
					for (int i = 0; i < memberIdArray.Length; i++)
					{
						registeredPartyMembers.Add(memberIdArray[i]);
					}
					party.PartyMemberAdded += OnPartyMemberAdded;
					party.PartyMemberRemoved += OnPartyMemberRemoved;
				}
			}
		}
		if (sensors != null)
		{
			sensors.TargetInRange();
			sensors.Update();
		}
		if (healWeapon == null || !isHealModAttached)
		{
			return;
		}
		bool flag = state != State.Shutdown && state != State.Sentry && state != State.Heal;
		if (flag && healWeapon.targetCanBeHealed(Owner) && initSuppressVOTimer <= 0f)
		{
			UpdateNeedsHealItemCheck();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && flag)
		{
			if (IsHealingAllies && registeredPartyMembers != null)
			{
				List<EntityAlive> healingTargetsInRange = GetHealingTargetsInRange(registeredPartyMembers, 15f);
				if (healingTargetsInRange.Count > 0)
				{
					if (healingTargetsInRange.Count > 1)
					{
						healingTargetsInRange.Sort([PublicizedFrom(EAccessModifier.Internal)] (EntityAlive x, EntityAlive y) => x.Health.CompareTo(y.Health));
					}
					currentTarget = healingTargetsInRange[0];
					ServerHealTarget(currentTarget);
				}
			}
			else
			{
				ServerHealTarget(Owner);
			}
		}
		healWeapon.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool procEnemiesInRange()
	{
		if (DebugEnemiesInRange && sensors.AreEnemiesInRange() && (bool)Owner)
		{
			EntityPlayerLocal entityPlayerLocal = Owner as EntityPlayerLocal;
			if ((bool)entityPlayerLocal)
			{
				rotateTo((Owner.position - position).normalized);
				Vector3 vector = entityPlayerLocal.vp_FPCamera.transform.position + Origin.position;
				if (entityPlayerLocal.bFirstPersonView)
				{
					vector = Owner.getHeadPosition() - Owner.transform.forward;
					vector.y = Mathf.Max(vector.y, Owner.getHeadPosition().y) + 0.5f;
				}
				else
				{
					vector.y = Mathf.Max(vector.y, Owner.getHeadPosition().y) + 1f;
				}
				float magnitude = (position - vector).magnitude;
				move(steering.Seek(position, vector, 1f), magnitude * 15f, ignoreObsticles: true);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState()
	{
		stateTime += 0.05f;
		if (initSuppressVOTimer > 0f)
		{
			initSuppressVOTimer -= 0.05f;
		}
		switch (state)
		{
		case State.Idle:
			if ((DebugEnemiesInRange && sensors.AreEnemiesInRange()) || isTargetUnderWater(Owner.getHeadPosition()))
			{
				currentTarget = Owner;
				setState(State.Follow);
			}
			else if (!steering.IsInRange(Owner.getHeadPosition(), FollowDistance + 1f))
			{
				currentTarget = Owner;
				setState(State.Follow);
			}
			else
			{
				idleState();
			}
			break;
		case State.Follow:
			followState();
			break;
		case State.Heal:
			healState();
			break;
		case State.Sentry:
			sentryState();
			break;
		case State.Attack:
		case State.Shutdown:
		case State.NoClip:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logDrone(string _log)
	{
		if (DroneManager.DebugLogEnabled)
		{
			Log.Out(GetType()?.ToString() + " " + _log);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugUpdate()
	{
		updateDebugName();
		if ((bool)debugCamera)
		{
			if ((bool)currentTarget && currentPath.Count == 0)
			{
				debugCamera.transform.LookAt(currentTarget.emodel.GetHeadTransform());
			}
			else
			{
				debugCamera.transform.forward = base.transform.forward;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugName()
	{
		aiManager.UpdateDebugName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setNoClip(bool value)
	{
		IsNoCollisionMode.Value = value;
		PhysicsTransform.gameObject.layer = (value ? 14 : 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> getAvoidEntities(float distance)
	{
		List<EntityAlive> list = new List<EntityAlive>();
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(position, Vector3.one * distance));
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
			if (entityAlive != null && entitiesInBounds[i].EntityClass != null && !(entityAlive is EntityNPC) && (!entityAlive.IsSleeper || !entityAlive.IsSleeping))
			{
				list.Add(entitiesInBounds[i] as EntityAlive);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOutOfRange(Vector3 _target, float _distance)
	{
		return (position - _target).sqrMagnitude > _distance * _distance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnyPlayerWithingDist(float dist)
	{
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		if (persistentPlayerList?.Players != null)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in persistentPlayerList.Players)
			{
				EntityPlayer entityPlayer = world.GetEntity(player.Value.EntityId) as EntityPlayer;
				if ((bool)entityPlayer && (entityPlayer.getChestPosition() - position).magnitude <= dist)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i getBlockPosition(Vector3 worldPos)
	{
		Vector3i vector3i = new Vector3i(worldPos);
		Vector3 v = worldPos - vector3i.ToVector3Center();
		return vector3i + Vector3i.FromVector3Rounded(v);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findOpenBlockAbove(Vector3 targetPosition, int maxHeight = 256)
	{
		Vector3i blockPosition = getBlockPosition(targetPosition);
		blockPosition += Vector3i.up;
		int num = 1;
		BlockValue block = world.GetBlock(blockPosition);
		while (!block.isair && num < maxHeight)
		{
			num++;
			blockPosition += Vector3i.up;
			block = world.GetBlock(blockPosition);
		}
		return blockPosition.ToVector3Center();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStuckInBlock()
	{
		currentBlockPosition.RoundToInt(position);
		timeInBlock += Time.deltaTime;
		if (currentBlockPosition != lastBlockPosition)
		{
			lastBlockPosition = currentBlockPosition;
			timeInBlock = 0f;
		}
		if (timeInBlock > 0.5f)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsNotAbleToReachTarget(Vector3 currentTarget)
	{
		timeSpentToNextTarget += Time.deltaTime;
		if (targetDestination != currentTarget)
		{
			targetDestination = currentTarget;
			timeSpentToNextTarget = 0f;
		}
		if (timeSpentToNextTarget > 1f)
		{
			timeSpentToNextTarget = 0f;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEntityAboveOrBelow(Entity entity)
	{
		bool result = false;
		_ = (position - entity.getHeadPosition()).normalized;
		float num = position.x - entity.position.x;
		float num2 = position.z - entity.position.z;
		float num3 = position.y - entity.getHeadPosition().y;
		if (num > -0.85f && num < 0.85f && num2 > -0.85f && num2 < 0.85f && (num3 < -1.2f || num3 > 1.2f))
		{
			result = true;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTargetUnderWater(Vector3 targetPosition)
	{
		Vector3i blockPosition = getBlockPosition(targetPosition);
		return world.GetBlock(blockPosition).type == 240;
	}

	public Vector3 getPositionOnGround()
	{
		return getPositionOnGround(position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 getPositionOnGround(Vector3 pos)
	{
		if (Physics.Raycast(pos - Origin.position, Vector3.down, out var hitInfo, 255f, 65536))
		{
			return hitInfo.point + Origin.position;
		}
		return position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] getGroupPositions(EntityAlive _entity, float followDist, bool debugDraw = false)
	{
		float num = 0.67f;
		Vector3 headPosition = _entity.getHeadPosition();
		Vector3 lookVector = _entity.GetLookVector();
		lookVector.y = 0f;
		groupPositions[0] = headPosition - lookVector * followDist;
		Vector3 normalized = (_entity.transform.right - lookVector).normalized;
		normalized.y = 0f;
		groupPositions[1] = headPosition + normalized * followDist * num;
		Vector3 normalized2 = (_entity.transform.right + lookVector).normalized;
		normalized2.y = 0f;
		groupPositions[2] = headPosition - normalized2 * followDist * num;
		return groupPositions;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] getGroupFallbackPositions(EntityAlive _entity, float followDist, bool debugDraw = false)
	{
		float num = 0.67f;
		Vector3 headPosition = _entity.getHeadPosition();
		Vector3 vector = -_entity.GetLookVector();
		vector.y = 0f;
		fallbackGroupPos[0] = headPosition - vector * followDist;
		Vector3 normalized = (_entity.transform.right - vector).normalized;
		normalized.y = 0f;
		fallbackGroupPos[1] = headPosition + normalized * followDist * num;
		Vector3 normalized2 = (_entity.transform.right + vector).normalized;
		normalized2.y = 0f;
		fallbackGroupPos[2] = headPosition - normalized2 * followDist * num;
		return fallbackGroupPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getTargetView(EntityAlive target, float degrees, float weight)
	{
		Vector3 lookVector = target.GetLookVector();
		Vector3 to = position - target.position;
		float num = Vector3.Angle(lookVector, to);
		if (num < degrees * 0.5f)
		{
			return (1f - num / degrees * 0.5f) * weight;
		}
		return 0f;
	}

	public void OnWakeUp()
	{
	}

	public void NotifyOffTheWorld()
	{
	}

	public override string MakeDebugNameInfo()
	{
		return $"\nState: {state.ToStringCached()}";
	}

	public void DebugToggleFriendlyFire()
	{
		debugFriendlyFire = !debugFriendlyFire;
	}

	public void DebugToggleDebugCamera()
	{
		_prepareDebugCamera();
		debugShowCamera = !debugShowCamera;
	}

	public void SetDebugCameraEnabled(bool value)
	{
		_prepareDebugCamera();
		debugShowCamera = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareDebugCamera()
	{
		if (debugShowCamera && (bool)debugCamera)
		{
			UnityEngine.Object.Destroy(debugCamera);
			return;
		}
		debugCamera = new GameObject("Camera");
		debugCamera.transform.SetParent(base.transform);
		debugCamera.transform.localPosition = Vector3.zero;
		debugCamera.transform.localRotation = Quaternion.identity;
		Camera camera = debugCamera.AddComponent<Camera>();
		Rect rect = camera.rect;
		float num = (rect.height = (rect.width = 0.35f));
		float y = (rect.x = 1f - num);
		rect.y = y;
		camera.rect = rect;
		camera.farClipPlane = 32f;
	}

	public void Debug_ToggleReconMode()
	{
		_prepareReconCam();
		DroneManager.Debug_LocalControl = !DroneManager.Debug_LocalControl;
		EntityPlayerLocal entityPlayerLocal = Owner as EntityPlayerLocal;
		entityPlayerLocal.PlayerUI.windowManager.SetHUDEnabled(DroneManager.Debug_LocalControl ? GUIWindowManager.HudEnabledStates.FullHide : GUIWindowManager.HudEnabledStates.Enabled);
		entityPlayerLocal.bEntityAliveFlagsChanged = true;
		entityPlayerLocal.IsGodMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsNoCollisionMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsFlyMode.Value = DroneManager.Debug_LocalControl;
		if (entityPlayerLocal.IsGodMode.Value)
		{
			entityPlayerLocal.Buffs.AddBuff("god");
		}
		else if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			entityPlayerLocal.Buffs.RemoveBuff("god");
		}
		entityPlayerLocal.IsSpectator = DroneManager.Debug_LocalControl;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareReconCam()
	{
		if (DroneManager.Debug_LocalControl && (bool)reconCam)
		{
			UnityEngine.Object.Destroy(reconCam.gameObject);
			return;
		}
		GameObject gameObject = new GameObject(Owner.EntityName + "-Drone|Recon");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		reconCam = gameObject.AddComponent<Camera>();
	}

	public ushort GetSyncFlagsReplicated(ushort syncFlags)
	{
		return (ushort)(syncFlags & 2);
	}

	public void SendSyncData(ushort syncFlags)
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		SendSyncData(syncFlags, primaryPlayerId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSyncData(ushort syncFlags, int playerId)
	{
		NetPackageDroneDataSync package = NetPackageManager.GetPackage<NetPackageDroneDataSync>().Setup(this, playerId, syncFlags);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}
	}

	public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
	{
		_bw.Write((byte)1);
		if ((syncFlags & 1) > 0)
		{
			OwnerID.ToStream(_bw);
			_bw.Write(Health);
		}
		if ((syncFlags & 0x4000) > 0)
		{
			_bw.Write((byte)OrderState);
			if (OrderState == Orders.Stay)
			{
				float[] array = new float[3] { sentryPos.x, sentryPos.y, sentryPos.z };
				for (int i = 0; i < array.Length; i++)
				{
					_bw.Write(array[i]);
				}
			}
		}
		if ((syncFlags & 0x8000) > 0)
		{
			_bw.Write((byte)state);
			if (state == State.Heal)
			{
				_bw.Write(userRequestedHeal);
				userRequestedHeal = false;
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b = 0;
			if (isInteractionLocked)
			{
				b |= 1;
			}
			if (isLocked)
			{
				b |= 2;
			}
			_bw.Write(b);
			ownerSteamId.ToStream(_bw);
			_bw.Write(passwordHash);
			_bw.Write((byte)allowedUsers.Count);
			for (int j = 0; j < allowedUsers.Count; j++)
			{
				allowedUsers[j].ToStream(_bw);
			}
		}
		if ((syncFlags & 8) > 0)
		{
			ItemStack[] slots = bag.GetSlots();
			_bw.Write((byte)slots.Length);
			for (int k = 0; k < slots.Length; k++)
			{
				slots[k].Write(_bw);
			}
		}
		if ((syncFlags & 0x1000) > 0)
		{
			_bw.Write(interactingPlayerId);
		}
		if ((syncFlags & 0x20) > 0)
		{
			_bw.Write(isQuietMode);
		}
		if ((syncFlags & 0x40) > 0)
		{
			_bw.Write(isFlashlightOn);
		}
		if ((syncFlags & 0x100) > 0)
		{
			_bw.Write(IsHealingAllies);
		}
		if ((syncFlags & 0x80) > 0)
		{
			OriginalItemValue.Write(_bw);
		}
	}

	public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
	{
		byte b = _br.ReadByte();
		if ((syncFlags & 1) > 0)
		{
			OwnerID = PlatformUserIdentifierAbs.FromStream(_br);
			Health = _br.ReadInt32();
		}
		if ((syncFlags & 0x4000) > 0)
		{
			Orders orders = (Orders)_br.ReadByte();
			if (orders == Orders.Stay)
			{
				sentryPos.x = _br.ReadSingle();
				sentryPos.y = _br.ReadSingle();
				sentryPos.z = _br.ReadSingle();
			}
			setOrders(orders);
			if (GameManager.IsDedicatedServer)
			{
				SendSyncData(16384, senderId);
			}
		}
		if ((syncFlags & 0x8000) > 0)
		{
			byte b2 = _br.ReadByte();
			transitionState = (State)b2;
			logDrone("Read Transition State: " + transitionState);
			if (b >= 1 && transitionState == State.Heal)
			{
				userRequestedHeal = _br.ReadBoolean();
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b3 = _br.ReadByte();
			isInteractionLocked = (b3 & 1) > 0;
			isLocked = (b3 & 2) > 0;
			ownerSteamId = PlatformUserIdentifierAbs.FromStream(_br);
			passwordHash = _br.ReadInt32();
			allowedUsers.Clear();
			int num = _br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				allowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: true));
			}
		}
		if ((syncFlags & 8) > 0)
		{
			int num2 = _br.ReadByte();
			ItemStack[] array = new ItemStack[num2];
			for (int j = 0; j < num2; j++)
			{
				ItemStack itemStack = new ItemStack();
				array[j] = itemStack.Read(_br);
				lootContainer.UpdateSlot(j, array[j]);
			}
			bag.SetSlots(array);
			bag.OnUpdate();
		}
		if ((syncFlags & 0x1000) > 0)
		{
			int requestId = _br.ReadInt32();
			CheckInteractionRequest(senderId, requestId);
		}
		if ((syncFlags & 0x10) > 0)
		{
			performRepair();
			Log.Warning("Read Repair Action: " + (ushort)16);
		}
		if ((syncFlags & 0x20) > 0)
		{
			isQuietMode = _br.ReadBoolean();
			if (isQuietMode)
			{
				idleLoop?.Stop(entityId);
				idleLoop = null;
			}
		}
		if ((syncFlags & 0x40) > 0)
		{
			isFlashlightOn = _br.ReadBoolean();
			setFlashlightOn(isFlashlightOn);
		}
		if ((syncFlags & 0x100) > 0)
		{
			SetHealAllies(_br.ReadBoolean());
		}
		if ((syncFlags & 0x80) > 0)
		{
			OriginalItemValue.Read(_br);
			LoadMods();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInteractionRequest(int _playerId, int _requestId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_requestId != -1)
			{
				ValidateInteractingPlayer();
				ushort num = 4096;
				if (interactingPlayerId == -1)
				{
					interactingPlayerId = _playerId;
					num |= 2;
				}
				SendSyncData(num, _playerId);
			}
			else if (interactingPlayerId == _playerId)
			{
				interactingPlayerId = -1;
			}
		}
		else
		{
			StartInteraction(_playerId, _requestId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartInteraction(int _playerId, int _requestId)
	{
		EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(_playerId);
		if (!localPlayerFromID)
		{
			return;
		}
		if (_requestId != _playerId)
		{
			GameManager.ShowTooltip(localPlayerFromID, Localization.Get("ttVehicleInUse"), string.Empty, "ui_denied");
			return;
		}
		interactingPlayerId = _playerId;
		switch (interactionRequestType)
		{
		case 1:
		{
			GUIWindowManager windowManager = LocalPlayerUI.GetUIForPlayer(localPlayerFromID).windowManager;
			((XUiC_DroneWindowGroup)((XUiWindowGroup)windowManager.GetWindow(XUiC_DroneWindowGroup.ID)).Controller).CurrentVehicleEntity = this;
			windowManager.Open(XUiC_DroneWindowGroup.ID, _bModal: true);
			Manager.BroadcastPlayByLocalPlayer(position, "UseActions/service_vehicle");
			break;
		}
		case 10:
			accessInventory(localPlayerFromID);
			break;
		}
	}

	public void StopUIInteraction()
	{
		Log.Out("ItemCountOnClose: " + getItemCount());
		isInteractionLocked = false;
		StopInteraction(234);
	}

	public void StopUIInsteractionSecurity()
	{
		StopInteraction(2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopInteraction(ushort syncFlags = 0)
	{
		interactingPlayerId = -1;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			syncFlags |= 0x1000;
		}
		if (syncFlags != 0)
		{
			SendSyncData(syncFlags);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateInteractingPlayer()
	{
		if (!GameManager.Instance.World.GetEntity(interactingPlayerId))
		{
			interactingPlayerId = -1;
		}
	}

	public bool IsLocked()
	{
		return isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		isLocked = _isLocked;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerSteamId;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerSteamId = _userIdentifier;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (_userIdentifier == null || !_userIdentifier.Equals(ownerSteamId))
		{
			return allowedUsers.Contains(_userIdentifier);
		}
		return true;
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return new List<PlatformUserIdentifierAbs>();
	}

	public bool LocalPlayerIsOwner()
	{
		return IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (ownerSteamId == null && OwnerID != null)
		{
			return OwnerID.Equals(_userIdentifier);
		}
		return ownerSteamId.Equals(_userIdentifier);
	}

	public bool HasPassword()
	{
		return passwordHash != 0;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		bool flag = Utils.HashString(_password) == passwordHash.ToString();
		if (LocalPlayerIsOwner())
		{
			if (!flag)
			{
				changed = true;
				passwordHash = _password.GetHashCode();
				allowedUsers.Clear();
				isLocked = true;
				if (ownerSteamId == null)
				{
					SetOwner(_userIdentifier);
				}
				SendSyncData(2);
			}
			return true;
		}
		if (flag)
		{
			allowedUsers.Add(_userIdentifier);
			SendSyncData(2);
			return true;
		}
		return false;
	}

	public string GetPassword()
	{
		return passwordHash.ToString();
	}
}
