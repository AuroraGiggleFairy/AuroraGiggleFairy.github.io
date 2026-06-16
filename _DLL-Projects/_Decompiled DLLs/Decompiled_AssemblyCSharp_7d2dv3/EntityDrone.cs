using System;
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

		public const string cStunCharged = "stunbaton_hit4";

		public const string cCommand = "drone_command";

		public const string cEmpty = "drone_empty";

		public const string cEnemySense = "drone_enemy_sense";

		public const string cEnemyEngauge = "drone_enemy_engauge";

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

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LoggingTypes
	{
		Any,
		Init,
		Animation,
		Shutdown
	}

	public enum Orders
	{
		Follow,
		Stay
	}

	public enum AttackMode
	{
		Passive,
		Aggressive
	}

	public enum AllyHealMode
	{
		DoNotHeal,
		HealAllies
	}

	[Preserve]
	public class PathTracker
	{
		public Vector3i currentBlockPosition;

		public Vector3i lastBlockPosition;

		public float timeInBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public float timeSpentToNextTarget;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 targetDestination;

		public bool IsStuck(Vector3 pos, Vector3 target, float time = 0.5f, bool debugDraw = false)
		{
			if (IsStuckInBlock(pos, time, debugDraw))
			{
				return true;
			}
			if (IsNotAbleToReachTarget(target))
			{
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsStuckInBlock(Vector3 position, float time = 0.5f, bool debugDraw = false)
		{
			timeInBlock += Time.deltaTime;
			currentBlockPosition.FloorToInt(position);
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(currentBlockPosition, Color.green, 0.05f);
			}
			if (currentBlockPosition != lastBlockPosition)
			{
				lastBlockPosition = currentBlockPosition;
				timeInBlock = 0f;
			}
			if (timeInBlock > time)
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

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
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

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class DroneSensors
	{
		public float EnemyDetectionRadius = 22f;

		public float EnemyDetectedBarkCooldown = 90f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enemyDetectedBarkTimer = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canBarkEnemyDetected;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityDrone drone;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Entity> entitiesInRange = new List<Entity>();

		public DroneSensors(EntityDrone _drone)
		{
			drone = _drone;
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
			if (IsEnemyInRange() && canBarkEnemyDetected)
			{
				barkEnemyDetected();
			}
		}

		public bool IsEnemyInRange()
		{
			if ((bool)drone)
			{
				return GetNearestEnemyInRange(drone.position) != null;
			}
			return false;
		}

		public EntityAlive GetNearestEnemyInRange(Vector3 targetPos)
		{
			return GetNearestEnemyInRange(targetPos, EnemyDetectionRadius);
		}

		public EntityAlive GetNearestEnemyInRange(Vector3 targetPos, float weaponRange)
		{
			EntityAlive revengeTarget = drone.GetRevengeTarget();
			if ((bool)revengeTarget && !revengeTarget.Buffs.HasBuff("buffShocked"))
			{
				return revengeTarget;
			}
			EntityEnemy result = null;
			float num = float.MaxValue;
			entitiesInRange.Clear();
			GameManager.Instance.World.GetEntitiesAround(EntityFlags.AIHearing | EntityFlags.Player, targetPos, EnemyDetectionRadius, entitiesInRange);
			for (int i = 0; i < entitiesInRange.Count; i++)
			{
				EntityEnemy entityEnemy = entitiesInRange[i] as EntityEnemy;
				if ((bool)entityEnemy && entityEnemy.EntityClass != null && entityEnemy.EntityClass.bIsEnemyEntity && canAttackTarget(entityEnemy))
				{
					float sqrMagnitude = (targetPos - entityEnemy.position).sqrMagnitude;
					if (sqrMagnitude < num && sqrMagnitude < weaponRange * weaponRange)
					{
						num = sqrMagnitude;
						result = entityEnemy;
					}
				}
			}
			return result;
		}

		public bool IsOwnerAttackTarget()
		{
			if ((bool)drone && (bool)drone.Owner)
			{
				entitiesInRange.Clear();
				GameManager.Instance.World.GetEntitiesAround(EntityFlags.AIHearing | EntityFlags.Player, drone.position, EnemyDetectionRadius, entitiesInRange);
				for (int i = 0; i < entitiesInRange.Count; i++)
				{
					EntityEnemy entityEnemy = entitiesInRange[i] as EntityEnemy;
					if ((bool)entityEnemy && canAttackTarget(entityEnemy) && entityEnemy.GetAttackTarget() == drone.Owner)
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canAttackTarget(EntityAlive enemy)
		{
			if (enemy.IsDead())
			{
				return false;
			}
			if (enemy.IsSleeper && enemy.IsSleeping)
			{
				return false;
			}
			if (enemy.Buffs.HasBuff("buffShocked"))
			{
				return false;
			}
			if (IsPositionBlocked(drone.position, enemy.getChestPosition(), 1073807360))
			{
				return false;
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void barkEnemyDetected()
		{
			if ((bool)drone)
			{
				if ((bool)drone.Owner)
				{
					Manager.Stop(drone.Owner.entityId, "drone_take");
				}
				if (drone.state != State.Shutdown)
				{
					drone.PlayVO("drone_enemy_sense", _hasPriority: true);
					enemyDetectedBarkTimer = EnemyDetectedBarkCooldown;
					canBarkEnemyDetected = false;
				}
			}
		}
	}

	[Preserve]
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

	public EntityAlive Owner;

	public const float cBaseFollowDistance = 5f;

	public const float cCombatFollowRange = 10f;

	public float FollowDistance = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAvoidRange = 2.5f;

	public float FollowHoverHeight = 1f;

	public float StayHoverHeight = 2f;

	public float SpeedPathing = 2f;

	public float SpeedFlying = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxSpeedFlying = 15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVehicleInventorySize = 45;

	public float RotationSpeed = 30f;

	public float AttackActionTime = 3f;

	public float HealActionTime = 7f;

	public DroneWeapons.HealBeamWeapon healWeapon;

	public DroneWeapons.StunBeamWeapon stunWeapon;

	public DroneWeapons.Weapon activeWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<DroneWeapons.Weapon> installedWeapons = new List<DroneWeapons.Weapon>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool weaponDischarged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAttackEnterTime = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAttackExitTime = 1.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float attackEnterTimer = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float attackExitTimer = 1.5f;

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
	public float currentSpeedFlying = 3f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public State state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public State lastState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public State transitionState = State.None;

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
	public FloodFillEntityPathGenerator pathMan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cInitSuppressVOTime = 5f;

	public float initSuppressVOTimer = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform head;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color prefabColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DroneLightManager _lm;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float creationTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasNavObjectsEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwnerSyncPending;

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

	public bool IsFlashlightAttached;

	public bool IsFlashlightOn;

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
	public float retryPathTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTryingToFindPath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool userRequestedHeal;

	public bool isSystemSpawn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte debugDroneInitLogPriority = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte debugDroneAnimationLogPriority = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte debugDroneShutdownLogPriority = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physColHeight = 0.6f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNode focusBoxNode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationStay = "drone_command_stay";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationFollow = "drone_command_follow";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationDontHealAllies = "drone_dont_heal_allies";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationHealAllies = "drone_heal_allies";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationLightOn = "drone_light_on";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationLightOff = "drone_light_off";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationSilentOn = "drone_silent_on";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationSilentOff = "drone_silent_off";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationHealMe = "drone_command_heal";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationModPassive = "drone_attack_mode_passive";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActivationModAgressive = "drone_attack_mode_aggressive";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAnimationStateSet;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWakeUpTime = 2.5f;

	public float WakeupAnimTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredOwnerHandlers;

	public List<int> registeredPartyMembers;

	public Vector3 SentryPos;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Orders orderState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cOwnerFocusTime = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ownerFocusTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ownerIsOnVehicle;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AttackMode attackMode;

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
	public bool IsHealingAllies;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AllyHealMode allyHealMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<EntityAlive> healTargetsInRange = new List<EntityAlive>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTeleportAtkCooldownTime = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float teleportAtkCooldownTimer;

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

	public const int cPathLayer = 1073807360;

	public const float cFollowHoverHeight = 1f;

	public const float cAddPathDist = 1.414f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 blockHeightOffset = new Vector3(0f, 0.5f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PathTracker pathTracker = new PathTracker();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currentPathTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currentPathDest;

	public bool DebugEnemiesInRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncReplicate = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncVersion = 3;

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
	public const ushort cSyncOrderState = 16384;

	public const ushort cSyncState = 32768;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncRepairAction = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncQuietMode = 32;

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
	public string passwordHash = string.Empty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUsers = new List<PlatformUserIdentifierAbs>();

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

	public float TimeSinceCreation
	{
		get
		{
			if (creationTime == 0f)
			{
				return 0f;
			}
			return Time.time - creationTime;
		}
	}

	public override int Health
	{
		get
		{
			return (int)base.Stats.Health.Value;
		}
		set
		{
			float num = Mathf.Max(value, 1);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && num == 1f && state != State.Shutdown)
			{
				isShutdownPending = true;
			}
			base.Stats.Health.Value = num;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool PlayWakeupAnim { get; set; }

	public int StorageCapacity => bag.SlotCount;

	public float EnemyDetectionRadius => sensors.EnemyDetectionRadius;

	public Orders OrderState => orderState;

	public bool CanInterruptFollow => true;

	public bool IsWeaponAttached => stunWeapon != null;

	public AttackMode AttackState => attackMode;

	public bool CanAttack
	{
		get
		{
			if (state != State.Shutdown && state != State.Heal && state != State.Attack)
			{
				return WakeupAnimTime <= 0f;
			}
			return false;
		}
	}

	public bool IsHealModAttached => healWeapon != null;

	public AllyHealMode HealAllyMode => allyHealMode;

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

	public bool DebugFrendlyFireEnabled => debugFriendlyFire;

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

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetItemClassId()
	{
		for (int i = 1; i < ItemClass.list.Length - 1; i++)
		{
			ItemClass itemClass = ItemClass.list[i];
			if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
			{
				return itemClass.Id;
			}
		}
		return -1;
	}

	public void PrepareToSpawn()
	{
		PlayWakeupAnim = true;
	}

	public void OnApplyToEntity(int orderState)
	{
		if (orderState < 0)
		{
			return;
		}
		switch ((Orders)orderState)
		{
		case Orders.Follow:
			setOrders(Orders.Follow);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SendSyncData(16384);
			}
			break;
		case Orders.Stay:
			SentryMode();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugDroneLog(LoggingTypes logType, string format, params object[] args)
	{
		int num = 0;
		switch (logType)
		{
		case LoggingTypes.Init:
			num = debugDroneInitLogPriority;
			break;
		case LoggingTypes.Animation:
			num = debugDroneAnimationLogPriority;
			break;
		case LoggingTypes.Shutdown:
			num = debugDroneShutdownLogPriority;
			break;
		}
		switch (num)
		{
		case 1:
			Log.Warning(format, args);
			break;
		case 2:
			Log.Error(format, args);
			break;
		default:
			Log.Out(format, args);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugDroneLog(string format, params object[] args)
	{
		DebugDroneLog(LoggingTypes.Any, format, args);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		steering = new EntitySteering(this);
		isLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void LateUpdate()
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

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
	}

	public override void InitInventory()
	{
		inventory = new DroneInventory(GameManager.Instance, this);
	}

	public override void PostInit()
	{
		_ = 1f / base.transform.localScale.x;
		interactionCollider = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)interactionCollider)
		{
			interactionCollider.center = new Vector3(0f, 0.5f, 0.25f);
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
		base.OnAddedToWorld();
		if (itemvalueToLoad != null)
		{
			OriginalItemValue = itemvalueToLoad;
		}
		if (OriginalItemValue == null)
		{
			isSystemSpawn = true;
			int itemClassId = GetItemClassId();
			if (itemClassId == -1)
			{
				Log.Warning("Failed to load junk drone from system spawn");
				return;
			}
			OriginalItemValue = new ItemValue(itemClassId);
		}
		isOwnerSyncPending = true;
		LoadMods();
		if ((bool)nativeCollider)
		{
			nativeCollider.enabled = true;
		}
		float value = EffectManager.GetValue(PassiveEffects.DegradationMax, OriginalItemValue);
		base.Stats.Health.BaseMax = value;
		base.Stats.Health.OriginalMax = value;
		Health = Mathf.RoundToInt(value * (1f - OriginalItemValue.UseTimes / value));
		animator = GetComponentInChildren<Animator>();
		pathMan = new FloodFillEntityPathGenerator(world, this);
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		creationTime = Time.time;
	}

	public override void OnEntityUnload()
	{
		Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		unRegsiterMovingLights();
		registeredPartyMembers = null;
		EntityPlayer entityPlayer = Owner as EntityPlayer;
		if ((bool)entityPlayer)
		{
			Party party = entityPlayer.Party;
			if (party != null)
			{
				party.PartyMemberAdded -= onPartyMemberAdded;
				party.PartyMemberRemoved -= onPartyMemberRemoved;
			}
			entityPlayer.PlayerTeleportedDelegates -= TeleportIfFollowing;
		}
		base.OnEntityUnload();
	}

	public override bool CanUpdateEntity()
	{
		return base.CanUpdateEntity();
	}

	public override bool CanNavigatePath()
	{
		return true;
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
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
		return new Ray(position + new Vector3(0f, GetEyeHeight(), 0f), (GetAttackTarget() == null) ? GetLookVector() : (GetAttackTarget().getChestPosition() - position).normalized);
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
		if (activeWeapon != null)
		{
			return activeWeapon.canFire();
		}
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		int strength = Mathf.RoundToInt((float)_strength * armorDamageReduction);
		EntityAlive entityAlive = (EntityAlive)world.GetEntity(_damageSource.getEntityId());
		if ((bool)Owner && (bool)entityAlive && !debugFriendlyFire && (bool)entityAlive && isAlly(entityAlive))
		{
			strength = 0;
		}
		return base.DamageEntity(_damageSource, strength, _criticalHit, _impulseScale);
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

	public override void PlayStepSound(float _volume)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
	}

	public override bool IsIgnoredByAI()
	{
		return true;
	}

	public override string ToString()
	{
		return $"[type={GetType()}, id={entityId}, belongsPlayerId={belongsPlayerId}]";
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		OwnerID.ToStream(_bw);
		OriginalItemValue = GetUpdatedItemValue();
		OriginalItemValue.Write(_bw);
		ushort num = 49515;
		_bw.Write(num);
		WriteSyncData(_bw, num);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		_br.ReadInt32();
		OwnerID = PlatformUserIdentifierAbs.FromStream(_br);
		OriginalItemValue = ItemValue.None;
		OriginalItemValue.Read(_br);
		ushort syncFlags = _br.ReadUInt16();
		ReadSyncData(_br, syncFlags, 0);
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (DroneManager.Debug_LocalControl)
		{
			return;
		}
		SyncOwnerData();
		updateTransitionState();
		updateAnimStates();
		if (isShutdownPending || (!Owner && state != State.Shutdown))
		{
			performShutdown();
		}
		updateShutdownState();
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
		updatePartyBuffs();
		updateDroneSystems();
		EntityPlayerLocal entityPlayerLocal = Owner as EntityPlayerLocal;
		if ((bool)entityPlayerLocal && state == State.Idle && initSuppressVOTimer <= 0f)
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
		updateDroneServiceMenu();
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
			if (PathFinderThread.Instance != null)
			{
				base.updateTasks();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("talk", "talk"));
		_addCallback(new EntityActivationCommand("service", "service"));
		_addCallback(new EntityActivationCommand("repair", "wrench"));
		_addCallback(new EntityActivationCommand("lock", "lock"));
		_addCallback(new EntityActivationCommand("unlock", "unlock"));
		_addCallback(new EntityActivationCommand("storage", "loot_sack"));
		_addCallback(new EntityActivationCommand("keypad", "keypad"));
		_addCallback(new EntityActivationCommand("take", "hand"));
		_addCallback(new EntityActivationCommand("drone_command_stay", "run_and_gun"));
		_addCallback(new EntityActivationCommand("drone_command_follow", "run"));
		_addCallback(new EntityActivationCommand("drone_dont_heal_allies", "player"));
		_addCallback(new EntityActivationCommand("drone_heal_allies", "allies"));
		_addCallback(new EntityActivationCommand("drone_attack_mode_passive", "clock"));
		_addCallback(new EntityActivationCommand("drone_attack_mode_aggressive", "fire"));
		_addCallback(new EntityActivationCommand("drone_light_on", "lightbulb"));
		_addCallback(new EntityActivationCommand("drone_light_off", "electric_switch"));
		_addCallback(new EntityActivationCommand("drone_silent_on", "stealth", null, "drone_silent"));
		_addCallback(new EntityActivationCommand("drone_silent_off", "sight", null, "drone_silent"));
		_addCallback(new EntityActivationCommand("drone_command_heal", "cardio"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReorderActivationCommands(List<EntityActivationCommand> _commands)
	{
		if (IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			Entity.MoveActivationCommandAfter(_commands, "storage", "heal");
		}
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (IsDead())
		{
			return false;
		}
		if (belongsToPlayerId(_playerFocusing.entityId))
		{
			if (CommandIs(_commandName, "talk"))
			{
				return state != State.Shutdown;
			}
			if (CommandIs(_commandName, "service"))
			{
				return true;
			}
			if (CommandIs(_commandName, "repair"))
			{
				return (float)Health < base.Stats.Health.Max;
			}
			if (CommandIs(_commandName, "lock"))
			{
				return !isLocked;
			}
			if (CommandIs(_commandName, "unlock"))
			{
				return isLocked;
			}
			if (CommandIs(_commandName, "keypad"))
			{
				return true;
			}
			if (CommandIs(_commandName, "take"))
			{
				return true;
			}
			if (CommandIs(_commandName, "drone_command_stay"))
			{
				if (OrderState != Orders.Stay)
				{
					return state != State.Shutdown;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_command_follow"))
			{
				if (OrderState != Orders.Follow)
				{
					return state != State.Shutdown;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_command_heal"))
			{
				if (state != State.Shutdown)
				{
					return TargetCanBeHealed(_playerFocusing);
				}
				return false;
			}
			if (CommandIs(_commandName, "storage"))
			{
				return bag != null;
			}
			if (CommandIs(_commandName, "drone_silent_on"))
			{
				return !isQuietMode;
			}
			if (CommandIs(_commandName, "drone_silent_off"))
			{
				return isQuietMode;
			}
			if (CommandIs(_commandName, "drone_light_on"))
			{
				if (IsFlashlightAttached)
				{
					return !IsFlashlightOn;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_light_off"))
			{
				if (IsFlashlightAttached)
				{
					return IsFlashlightOn;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_dont_heal_allies"))
			{
				if (IsHealModAttached)
				{
					return allyHealMode == AllyHealMode.HealAllies;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_heal_allies"))
			{
				if (IsHealModAttached)
				{
					return allyHealMode == AllyHealMode.DoNotHeal;
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_attack_mode_passive"))
			{
				if (IsWeaponAttached && attackMode == AttackMode.Aggressive)
				{
					return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
				}
				return false;
			}
			if (CommandIs(_commandName, "drone_attack_mode_aggressive"))
			{
				if (IsWeaponAttached && attackMode == AttackMode.Passive)
				{
					return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
				}
				return false;
			}
			return base.AllowActivationCommand(_commandName, _playerFocusing);
		}
		bool result = isLocked && !IsUserAllowed(PlatformManager.InternalLocalUserIdentifier) && HasPassword();
		bool result2 = (float)Health < base.Stats.Health.Max;
		if (CommandIs(_commandName, "storage"))
		{
			return bag != null;
		}
		if (CommandIs(_commandName, "keypad"))
		{
			return result;
		}
		if (CommandIs(_commandName, "repair"))
		{
			return result2;
		}
		return false;
	}

	public override string GetActivationText()
	{
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance?.World?.GetPrimaryPlayer();
		if (entityPlayerLocal == null)
		{
			return string.Empty;
		}
		string arg = entityPlayerLocal.playerInput.Activate.GetBindingXuiMarkupString() + entityPlayerLocal.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string text = string.Format(Localization.Get("npcTooltipTalk"), arg, LocalizedEntityName);
		if (IsLocked() && !IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			text = Localization.Get("ttLocked") + "\n" + text;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (CommandIs(_command.commandId, "storage") && isLocked && !IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			Manager.Play(this, "locked");
			return;
		}
		EntityLockContext context = new EntityLockContext(_command.commandId.ToString(), bag);
		LockManager.Instance.LockRequestLocal(this, context, 0);
	}

	public void StopUIInteraction()
	{
		stopInteraction(234);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startInteraction(ReadOnlySpan<char> _commandName)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (CommandIs(_commandName, "talk"))
		{
			startDialog(primaryPlayer);
		}
		else if (CommandIs(_commandName, "service"))
		{
			((XUiC_DroneWindowGroup)((XUiWindowGroup)uIForPrimaryPlayer.windowManager.GetWindow(XUiC_DroneWindowGroup.ID)).Controller).CurrentVehicleEntity = this;
			uIForPrimaryPlayer.windowManager.Open(XUiC_DroneWindowGroup.ID, _bModal: true);
			Manager.BroadcastPlayByLocalPlayer(position, "UseActions/service_vehicle");
		}
		else if (CommandIs(_commandName, "repair"))
		{
			DoRepairAction(uIForPrimaryPlayer);
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "lock"))
		{
			PlaySound("misc/locking");
			isLocked = !isLocked;
			stopInteraction(2);
		}
		else if (CommandIs(_commandName, "unlock"))
		{
			PlaySound("misc/unlocking");
			isLocked = !isLocked;
			stopInteraction(2);
		}
		else if (CommandIs(_commandName, "keypad"))
		{
			doKeypadAction(uIForPrimaryPlayer);
		}
		else if (CommandIs(_commandName, "take") || CommandIs(_commandName, "force_pickup"))
		{
			pickup(primaryPlayer);
		}
		else if (CommandIs(_commandName, "drone_command_stay"))
		{
			SentryMode();
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "drone_command_follow"))
		{
			FollowMode();
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "drone_command_heal"))
		{
			HealRequest();
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "storage"))
		{
			openStorageWindow(uIForPrimaryPlayer);
		}
		else if (CommandIs(_commandName, "drone_silent_on") || CommandIs(_commandName, "drone_silent_off"))
		{
			isQuietMode = !isQuietMode;
			idleLoop?.Stop(entityId);
			idleLoop = null;
			stopInteraction(32);
		}
		else if (CommandIs(_commandName, "drone_light_on") || CommandIs(_commandName, "drone_light_off"))
		{
			ToggleLightAction();
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "drone_heal_allies") || CommandIs(_commandName, "drone_dont_heal_allies"))
		{
			ToggleHealAllies();
			stopInteraction(0);
		}
		else if (CommandIs(_commandName, "drone_attack_mode_passive") || CommandIs(_commandName, "drone_attack_mode_aggressive"))
		{
			ToggleAttackMode();
			stopInteraction(0);
		}
		else
		{
			stopInteraction(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopInteraction(ushort syncFlags = 0)
	{
		if (syncFlags != 0)
		{
			SendSyncData(syncFlags);
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	public void OpenStorageFromDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		openStorageWindow(uIForPlayer);
		EntityLockContext context = new EntityLockContext("storage", bag);
		LockManager.Instance.LockRequestLocal(this, context, 0);
	}

	public void ToggleOrderState()
	{
		switch (orderState)
		{
		case Orders.Follow:
			SentryMode();
			break;
		case Orders.Stay:
			FollowMode();
			break;
		}
	}

	public void ToggleHealAllies()
	{
		setHealAllies(!IsHealingAllies);
		SendSyncData(256);
	}

	public void ToggleAttackMode()
	{
		switch (attackMode)
		{
		case AttackMode.Passive:
			SetAttacKMode(AttackMode.Aggressive);
			break;
		case AttackMode.Aggressive:
			SetAttacKMode(AttackMode.Passive);
			break;
		}
	}

	public void ToggleLightAction()
	{
		IsFlashlightOn = !IsFlashlightOn;
		setFlashlightOn(IsFlashlightOn);
		SendSyncData(64);
	}

	public void DoRepairAction(LocalPlayerUI playerUI)
	{
		string text = "resourceRepairKit";
		if (HasStoredItem(playerUI.entityPlayer, text, repairKitTags))
		{
			if (GetRepairAmountNeeded() > 0)
			{
				playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(ItemClass.GetItem(text), 1));
				PlaySound("crafting/craft_repair_item");
				TakeStoredItem(playerUI.entityPlayer, text, repairKitTags);
				performRepair();
				SendSyncData(16);
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead("misc/missingitemtorepair");
		}
	}

	public void HealRequest()
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
				healTargetServer(Owner, userRequestedHeal);
			}
			else
			{
				healRequestClient();
			}
			userRequestedHeal = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uIForPlayer.xui.Dialog.Respondent = this;
		uIForPlayer.windowManager.CloseAllOpenModalWindows();
		XUiC_DialogWindowGroup.Open(uIForPlayer.xui, StopUIInteraction);
		PlayVO("drone_greeting");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openStorageWindow(LocalPlayerUI playerUI)
	{
		XUiC_BagStorageWindowGroup.Open(playerUI.xui, this, bag, LootContainer.GetLootContainer("roboticDrone"), Localization.Get("xuiStorage"), [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SendSyncData(8);
		}, StopUIInteraction, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			float distanceSq = GetDistanceSq(playerUI.entityPlayer);
			float num = Constants.cDigAndBuildDistance + 1f;
			return distanceSq <= num * num;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doKeypadAction(LocalPlayerUI playerUI)
	{
		XUiC_KeypadWindow.Open(playerUI, this, StopUIInteraction, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			float distanceSq = GetDistanceSq(playerUI.entityPlayer);
			float num = Constants.cDigAndBuildDistance + 1f;
			return distanceSq <= num * num;
		});
	}

	public void OnWakeUp()
	{
	}

	public void SetItemValueToLoad(ItemValue itemValue)
	{
		itemvalueToLoad = itemValue.Clone();
	}

	public void LoadMods()
	{
		Vector2i size = LootContainer.GetLootContainer("roboticDrone").size;
		int num = size.x * size.y;
		lightManager.DisableMaterials("junkDroneLamp");
		GameObject gameObject = base.transform.FindInChilds("freightBox").gameObject;
		GameObject gameObject2 = base.transform.FindInChilds("armor").gameObject;
		GameObject gameObject3 = base.transform.FindInChilds("machineGun").gameObject;
		GameObject gameObject4 = base.transform.FindInChilds("teddyBear").gameObject;
		GameObject gameObject5 = base.transform.FindInChilds("junkDroneArmRight").gameObject;
		gameObject?.SetActive(value: false);
		gameObject2?.SetActive(value: false);
		gameObject3?.SetActive(value: false);
		gameObject4?.SetActive(value: false);
		gameObject5?.SetActive(value: true);
		for (int i = 0; i < installedWeapons.Count; i++)
		{
			DroneWeapons.Weapon weapon = installedWeapons[i];
			installedWeapons.Remove(weapon);
			weapon.Unequip();
		}
		stunWeapon = null;
		healWeapon = null;
		if (state == State.Attack)
		{
			SetState(State.Idle, sync: true);
		}
		IsFlashlightAttached = false;
		setFlashlightOn(value: false);
		if (OriginalItemValue.HasMods())
		{
			for (int j = 0; j < OriginalItemValue.Modifications.Length; j++)
			{
				ItemValue itemValue = OriginalItemValue.Modifications[j];
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
					stunWeapon = new DroneWeapons.StunBeamWeapon(this);
					stunWeapon.Init();
					stunWeapon.Equip(itemValue);
					installedWeapons.Add(stunWeapon);
					if ((bool)gameObject3)
					{
						gameObject3.SetActive(value: true);
					}
					if ((bool)gameObject5)
					{
						gameObject5.SetActive(value: false);
					}
					break;
				case "modRoboticDroneMedicMod":
					healWeapon = new DroneWeapons.HealBeamWeapon(this);
					healWeapon.Init();
					healWeapon.Equip(itemValue);
					installedWeapons.Add(healWeapon);
					break;
				case "modRoboticDroneWeaponMod":
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
					IsFlashlightAttached = true;
					DroneLightManager.LightEffect[] lightEffects = lightManager.LightEffects;
					if (lightEffects.Length != 0)
					{
						LightManager.RegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
					}
					if (IsFlashlightOn)
					{
						setFlashlightOn(value: true);
					}
					break;
				}
				}
			}
		}
		ItemStack[] slots = bag.GetSlots();
		if (slots == null || slots.Length != num)
		{
			ItemStack[] array = ItemStack.CreateArray(num);
			if (slots != null)
			{
				int num2 = Mathf.Min(slots.Length, num);
				for (int k = 0; k < num2; k++)
				{
					array[k] = slots[k];
				}
			}
			bag.SetSlots(array);
		}
		Color color = prefabColor;
		ItemValue itemValue2 = OriginalItemValue.CosmeticMods[0];
		if (OriginalItemValue.CosmeticMods.Length != 0 && itemValue2 != null && !itemValue2.IsEmpty())
		{
			Vector3 vector = Block.StringToVector3(OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
			color.r = vector.x;
			color.g = vector.y;
			color.b = vector.z;
		}
		for (int l = 0; l < paintableParts.Length; l++)
		{
			SetPaint(paintableParts[l], color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initWorldValues(bool value)
	{
		bWillRespawn = value;
	}

	public void SyncOwnerData()
	{
		if (isOwnerSyncPending)
		{
			notifySyncOwner();
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
			if (!hasNavObjectsEnabled && GameManager.Instance.World.IsLocalPlayer(belongsPlayerId))
			{
				HandleNavObject();
				hasNavObjectsEnabled = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifySyncOwner()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(OwnerID);
		if (playerData != null)
		{
			belongsPlayerId = playerData.EntityId;
			Owner = GameManager.Instance.World.GetEntity(belongsPlayerId) as EntityAlive;
		}
		if ((bool)Owner)
		{
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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool belongsToPlayerId(int id)
	{
		return belongsPlayerId == id;
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
		animator = GetComponentInChildren<Animator>();
		if ((bool)animator)
		{
			animator.enabled = true;
			animator.Play("Base Layer.SpawnIn");
			WakeupAnimTime = 2.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playIdleAnim()
	{
		animator = GetComponentInChildren<Animator>();
		if ((bool)animator)
		{
			animator.enabled = true;
			animator.Play("Base Layer.Idle", 0, 0f);
			animator.Update(0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownAnim()
	{
		animator = GetComponentInChildren<Animator>();
		if ((bool)animator)
		{
			animator.Play("Base Layer.SpawnIn", 0, 0f);
			animator.Update(0f);
			animator.StopPlayback();
			animator.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimStates()
	{
		if (!isAnimationStateSet)
		{
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
					WakeupAnimTime = 2.5f;
				}
			}
			else
			{
				setShutdownAnim();
			}
			isAnimationStateSet = true;
		}
		if (!(WakeupAnimTime > 0f))
		{
			return;
		}
		WakeupAnimTime -= 0.05f;
		if (WakeupAnimTime <= 0f && !GameManager.IsDedicatedServer)
		{
			if ((bool)Owner)
			{
				Manager.Stop(Owner.entityId, "drone_take");
			}
			PlayVO("drone_wakeup");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unRegsiterMovingLights()
	{
		DroneLightManager.LightEffect[] lightEffects = lightManager.LightEffects;
		if (lightEffects.Length != 0)
		{
			LightManager.UnRegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
		}
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
			if (persistentPlayerData != null && playerData.IsAlly(persistentPlayerData))
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPartyMemberAdded(EntityPlayer player)
	{
		registeredPartyMembers.Add(player.entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPartyMemberRemoved(EntityPlayer player)
	{
		registeredPartyMembers.Remove(player.entityId);
		removeSupportBuff(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removePartyBuffs(EntityPlayer owner)
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
	public void procBuffRange(EntityAlive entity)
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
	public void buffAllies()
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
				procBuffRange(entity);
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
		procBuffRange(entityPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePartyBuffs()
	{
		if ((bool)Owner && isSupportModAttached && !isEntityRemote)
		{
			buffAllies();
		}
	}

	public bool HasStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName);
		ItemClass itemClass = item.ItemClass;
		int num = 0;
		int num2 = 0;
		if (itemClass != null)
		{
			num = entity.bag.GetItemCount(item);
			num2 = entity.inventory.GetItemCount(item);
		}
		return num + num2 > 0;
	}

	public ItemStack TakeStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName);
		if (item.ItemClass != null)
		{
			entity.bag.GetItemCount(item);
			if (entity.inventory.GetItemCount(item) > 0)
			{
				entity.inventory.DecItem(item, 1);
			}
			else
			{
				entity.bag.DecItem(item, 1);
			}
			return new ItemStack(item.Clone(), 1);
		}
		return null;
	}

	public ItemValue GetUpdatedItemValue()
	{
		OriginalItemValue.UseTimes = (float)OriginalItemValue.MaxUseTimes * (1f - (float)Health / base.Stats.Health.BaseMax);
		return OriginalItemValue;
	}

	public override void OnCollectServer(int _playerId)
	{
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
		world.RemoveEntity(entityId, EnumRemoveEntityReason.Killed);
	}

	public override void OnCollectLocal(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(GetUpdatedItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.Toolbelt.AddItem(itemStack) && !uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pickup(Entity _entityFocusing)
	{
		if (!bag.IsEmpty())
		{
			PlayVO("drone_takefail", _hasPriority: true);
			GameManager.ShowTooltip(Owner as EntityPlayerLocal, Localization.Get("ttEmptyDroneBeforePickup"), string.Empty, "ui_denied");
			stopInteraction(0);
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
			Collect(entityPlayer.entityId);
			if (entityPlayer.Buffs.HasBuff("buffJunkDroneSupportEffect"))
			{
				entityPlayer.Buffs.RemoveBuff("buffJunkDroneSupportEffect");
			}
			removePartyBuffs(entityPlayer);
			unRegsiterMovingLights();
		}
		else
		{
			GameManager.ShowTooltip(entityPlayer as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
		}
		stopInteraction(0);
	}

	public int GetStoredItemCount()
	{
		return bag.GetUsedSlotCount();
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
	public void updateDroneServiceMenu()
	{
		if (overItemLimitCooldown > 0f)
		{
			overItemLimitCooldown -= 0.05f;
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
	public void performRepair()
	{
		Health = (int)base.Stats.Health.Max;
		OriginalItemValue.UseTimes = 0f;
		setShutdown(value: false);
		playWakeupAnim();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SendSyncData(16);
		}
	}

	public EntityAlive GetNearestEnemyInRange(Vector3 targetPos)
	{
		return sensors.GetNearestEnemyInRange(targetPos);
	}

	public bool IsOwnerSneaking()
	{
		if ((bool)Owner && Owner.IsCrouching && !sensors.IsOwnerAttackTarget())
		{
			return true;
		}
		return false;
	}

	public State GetState()
	{
		return state;
	}

	public void SetState(State next, bool sync = false)
	{
		setState(next);
		if (sync)
		{
			SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(State next)
	{
		lastState = state;
		state = next;
		stateTime = 0f;
		switch (state)
		{
		case State.Follow:
			if (lastState == State.Sentry && (bool)Owner && Owner.HasOwnedEntity(entityId))
			{
				Owner.GetOwnedEntity(entityId).ClearLastKnownPostition();
			}
			break;
		case State.Heal:
			clearNeedsHealItemCheck();
			break;
		case State.Idle:
		case State.Sentry:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState()
	{
		stateTime += 0.05f;
		switch (state)
		{
		case State.Idle:
			idleState();
			break;
		case State.Follow:
			followState();
			break;
		case State.Attack:
			attackState();
			break;
		case State.Heal:
			healState();
			break;
		case State.Sentry:
			sentryState();
			break;
		case State.Shutdown:
		case State.NoClip:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTransitionState()
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
		if ((state == State.Attack || state == State.Heal) && transitionState == State.Idle)
		{
			for (int i = 0; i < installedWeapons.Count; i++)
			{
				installedWeapons[i].RefreshCooldown();
			}
		}
		switch (transitionState)
		{
		case State.Shutdown:
			isShutdownPending = true;
			break;
		case State.Heal:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				healTargetServer(GetAttackTarget(), userRequestedHeal);
			}
			else
			{
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
	public void idleState()
	{
		EntityAlive owner = Owner;
		if (!owner)
		{
			return;
		}
		Vector3 chestPosition = owner.getChestPosition();
		if (onUnderWaterState(chestPosition))
		{
			return;
		}
		bool flag = sensors.IsEnemyInRange();
		if (flag && !steering.IsInRange(chestPosition, sensors.EnemyDetectionRadius))
		{
			setState(State.Follow);
		}
		else if (!steering.IsInRange(chestPosition, FollowDistance + 2f) && !flag)
		{
			setState(State.Follow);
		}
		else if (!isEntityAboveOrBelow(owner))
		{
			rotateTo(steering.GetDir2D(position, chestPosition));
			float num = 0f;
			if (position.y - chestPosition.y > num || position.y - chestPosition.y < num)
			{
				Vector3 target = position;
				target.y = chestPosition.y;
				move(steering.Seek(position, target, SpeedFlying * 0.5f), SpeedFlying);
			}
		}
	}

	public void SentryMode()
	{
		PlayVO("drone_command", _hasPriority: true);
		SentryPos = position;
		setOrders(Orders.Stay);
		setState(State.Sentry);
		SendSyncData(49152);
		if ((bool)Owner && Owner.HasOwnedEntity(entityId))
		{
			Owner.GetOwnedEntity(entityId).SetLastKnownPosition(position);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void sentryState()
	{
		Vector3 sentryPos = SentryPos;
		if (!world.IsChunkAreaLoaded(sentryPos))
		{
			return;
		}
		if ((sentryPos - position).magnitude > 5f)
		{
			if (!DoMoveIntoFollowPos(sentryPos, 1.414f, base.transform.forward, 0.1f, debugDraw: true, 10f))
			{
				return;
			}
			clearCurrentPath();
		}
		if (!steering.IsInRange(sentryPos, 0.25f))
		{
			Vector3 dir = steering.Seek(position, sentryPos, 0.25f);
			rotateTo((sentryPos - position).normalized);
			move(dir);
		}
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

	public bool IsAttachedToVehicle(Entity entity)
	{
		if ((bool)entity)
		{
			return entity.AttachedToEntity as EntityVehicle != null;
		}
		return false;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onInterruptState()
	{
		if (isInteractionFocusOwner())
		{
			ownerFocusTimer += 0.05f;
			if (ownerFocusTimer >= 0.2f)
			{
				ownerFocusTimer = 0f;
				setState(State.Idle);
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onVehicleState(EntityAlive entity, Vector3 followPoint)
	{
		if (IsAttachedToVehicle(entity))
		{
			if (!ownerIsOnVehicle)
			{
				ownerIsOnVehicle = true;
				setNoClip(value: true);
				clearCurrentPath();
			}
			Entity attachedToEntity = entity.AttachedToEntity;
			Vector3 followPoint2 = attachedToEntity.position - attachedToEntity.transform.forward * 5f * 2f + Vector3.up * 5f * 2f;
			steerFollow(entity, followPoint2);
			return true;
		}
		if (ownerIsOnVehicle)
		{
			ownerIsOnVehicle = false;
			SetPosition(followPoint);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onUnderWaterState(Vector3 chestPos)
	{
		if (isTargetUnderWater(chestPos))
		{
			if (currentPath.Count > 0)
			{
				clearCurrentPath();
			}
			Vector3 dir = steering.Seek(position, findOpenBlockAbove(chestPos), 0.2f);
			rotateTo(dir);
			move(dir);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoMoveIntoFollowPos(Vector3 targetPos, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Utils.DrawCircleLinesHorzontal(position - Origin.position, seekDist, Color.white, Color.red, 24, 0.05f);
		Vector3 normalized = (targetPos - position).normalized;
		float magnitude = (targetPos - position).magnitude;
		if (currentPath.Count == 0)
		{
			GetPath(currentPath, this, position, targetPos, SpeedFlying, null, seekDist, pointRadius, debugDraw, duration);
			if (currentPath.Count > 0)
			{
				currentPathDest = currentPath[currentPath.Count - 1];
			}
			else
			{
				currentPathDest = targetPos;
			}
		}
		else if (IsPositionBlocked(position, targetPos, 1073807360) || (float)currentPath.Count > seekDist + 1f || (currentPath.Count > 0 && magnitude > seekDist + 1.414f))
		{
			followPlannedPath(SpeedFlying, pointRadius, debugDraw, duration);
		}
		else if (magnitude >= seekDist)
		{
			RotateTo(normalized);
			Move(targetPos, pointRadius);
		}
		if (!IsPositionBlocked(position, targetPos, 1073807360) && magnitude <= seekDist)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoMoveIntoFollowPos(EntityAlive avaliableTarget, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 chestPosition = avaliableTarget.getChestPosition();
		return DoMoveIntoFollowPos(chestPosition, seekDist, seekForward, pointRadius, debugDraw, duration);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void steerFollow(EntityAlive entity, Vector3 followPoint)
	{
		if ((bool)entity)
		{
			if (!steering.IsInRange(entity.position, 10f))
			{
				if (decelerationTime > 0f)
				{
					decelerationTime = 0f;
				}
				accelerationTime += 0.05f;
				currentSpeedFlying = Mathf.Lerp(currentSpeedFlying, Mathf.Max(15f, (entity.position - position).magnitude), Mathf.Clamp01(accelerationTime / SpeedFlying));
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
		Vector3 vector = steering.Seek(position, followPoint, SpeedFlying);
		if (steering.IsInRange(followPoint, 0.1f))
		{
			return;
		}
		Vector3 chestPosition = entity.getChestPosition();
		float magnitude = (chestPosition - position).magnitude;
		if (magnitude > 5f && magnitude < 24f && !RaycastPathUtils.IsPointBlocked(position, chestPosition, 1073807360) && Vector3.Angle(entity.GetLookVector(), position - chestPosition) < 45f)
		{
			float num = 0.5f;
			Vector3 vector2 = steering.Flee(position, chestPosition, SpeedFlying);
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
		rotateTo((chestPosition - position).normalized);
		move(vector.normalized, followPoint, currentSpeedFlying);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void followState()
	{
		EntityAlive owner = Owner;
		if (!owner || onInterruptState())
		{
			return;
		}
		Vector3 chestPosition = owner.getChestPosition();
		if (onUnderWaterState(chestPosition))
		{
			return;
		}
		Vector3[] groupPositions = GetGroupPositions(owner, 5f);
		Array.Sort(groupPositions, [PublicizedFrom(EAccessModifier.Private)] (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
		Vector3 vector = groupPositions[0];
		if (onVehicleState(owner, vector))
		{
			return;
		}
		if (PathFinderThread.Instance != null)
		{
			if (!DoMoveIntoFollowPos(owner, 5f, base.transform.forward, 0.1f, debugDraw: true, 10f))
			{
				return;
			}
			clearCurrentPath();
		}
		steerFollow(owner, vector);
		if (steering.IsInRange(vector, 0.5f) || steering.IsInRange(chestPosition, FollowDistance))
		{
			setState(State.Idle);
		}
	}

	public void SetAttacKMode(AttackMode mode)
	{
		attackMode = mode;
	}

	public List<DroneWeapons.Weapon> GetInstalledWeapons()
	{
		return installedWeapons;
	}

	public DroneWeapons.Weapon GetInstalledWeapon(string itemKey)
	{
		for (int i = 0; i < installedWeapons.Count; i++)
		{
			DroneWeapons.Weapon weapon = installedWeapons[i];
			if (weapon.ItemName.Equals(itemKey))
			{
				return weapon;
			}
		}
		return null;
	}

	public void SetActiveWeapon(DroneWeapons.Weapon _weapon)
	{
		activeWeapon = _weapon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void attackState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void exitAttackState()
	{
	}

	public Vector3 GetHealArmPosition()
	{
		if (healWeapon != null)
		{
			return healWeapon.WeaponJoint.position + Origin.position;
		}
		return position + Origin.position;
	}

	public bool TargetCanBeHealed(EntityAlive entity)
	{
		if (healWeapon != null && healWeapon.targetCanBeHealed(entity))
		{
			return healWeapon.hasHealingItem();
		}
		return false;
	}

	public bool IsTargetInNeedOfMedical(EntityAlive target)
	{
		if (healWeapon != null)
		{
			return healWeapon.isTargetInNeedOfMedical(target);
		}
		return false;
	}

	public EntityAlive GetNearestHealTargetInRange(float range)
	{
		if (IsHealingAllies && registeredPartyMembers != null)
		{
			List<EntityAlive> healingTargetsInRange = getHealingTargetsInRange(registeredPartyMembers, range);
			if (healingTargetsInRange.Count > 0)
			{
				if (healingTargetsInRange.Count > 1)
				{
					healingTargetsInRange.Sort([PublicizedFrom(EAccessModifier.Internal)] (EntityAlive x, EntityAlive y) => x.Health.CompareTo(y.Health));
				}
				return healingTargetsInRange[0];
			}
			return null;
		}
		return Owner;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHealAllies(bool value)
	{
		IsHealingAllies = value;
		allyHealMode = (value ? AllyHealMode.HealAllies : AllyHealMode.DoNotHeal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> getHealingTargetsInRange(List<int> playerIds, float range)
	{
		healTargetsInRange.Clear();
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
					healTargetsInRange.Add(entityAlive);
				}
			}
		}
		return healTargetsInRange;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTargetServer(EntityAlive target, bool healReq = false)
	{
		if (state != State.Heal && healWeapon.canFire() && (healReq || healWeapon.isTargetInNeedOfMedical(target)))
		{
			healTarget(target);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healRequestClient()
	{
		SetState(State.Heal, sync: true);
		SetState(State.Idle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTarget(EntityAlive target)
	{
		SetAttackTarget(target, 1200);
		if ((bool)GetAttackTarget())
		{
			SetActiveWeapon(healWeapon);
			SetState(State.Heal, sync: true);
		}
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
	public void updateNeedsHealItemCheck()
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
	public void clearNeedsHealItemCheck()
	{
		needsHealItemTimer = 0f;
		needsHealNotifyCount = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onHealDone()
	{
	}

	public bool IsOnTeleportCooldown()
	{
		return teleportAtkCooldownTimer > 0f;
	}

	public void TeleportIfFollowing()
	{
		if (orderState == Orders.Follow && !isShutdown)
		{
			teleportState();
		}
	}

	public void TeleportOutOfRange()
	{
		if (state == State.Attack)
		{
			exitAttackState();
		}
		if (state == State.Heal)
		{
			onHealDone();
		}
		teleportState();
	}

	public void TeleportToPosition(Vector3 telePos)
	{
		teleportToPosition(telePos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportToPosition(Vector3 telePos)
	{
		motion = Vector3.zero;
		SetPosition(telePos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkTeleportPos(Vector3 target)
	{
		if ((bool)Owner)
		{
			if (isOutOfRange(Owner.position, 32f))
			{
				Log.Out("teleport failed");
			}
			else
			{
				Log.Out("teleport success!");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportState()
	{
		teleportAtkCooldownTimer = 5f;
		setState(State.Teleport);
		clearCurrentPath();
		motion = Vector3.zero;
		Vector3 chestPosition = Owner.getChestPosition();
		Vector3 lookVector = Owner.GetLookVector();
		Vector3 vector = chestPosition - new Vector3(lookVector.x, 0f, lookVector.z) * 5f;
		Vector3[] groupPositions = GetGroupPositions(Owner, 5f);
		Array.Sort(groupPositions, [PublicizedFrom(EAccessModifier.Private)] (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
		foreach (Vector3 vector2 in groupPositions)
		{
			RaycastHit hit;
			bool flag = IsPositionBlocked(chestPosition, vector2, out hit, 1073807360);
			bool flag2 = IsPositionBlocked(vector2, chestPosition, 1073807360);
			if (!flag && !flag2)
			{
				vector = vector2;
				break;
			}
			if (flag)
			{
				vector = World.worldToBlockPos(hit.point + Origin.position).ToVector3Center();
				break;
			}
		}
		SetPosition(vector);
		ModelTransform.position = vector - Origin.position;
		setState(State.Idle);
		checkTeleportPos(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performShutdown()
	{
		DebugDroneLog(LoggingTypes.Shutdown, "performShutdown() {0}", this);
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
		DebugDroneLog(LoggingTypes.Shutdown, "setShutdown({0}) {1}", value, this);
		animator = GetComponentInChildren<Animator>();
		if ((bool)animator)
		{
			animator.enabled = !value;
		}
		PhysicsTransform.gameObject.SetActive(!value);
		IsNoCollisionMode.Value = value;
		setShutdownDestruction(value);
		isShutdown = value;
		if (value)
		{
			SetRevengeTarget(null);
			SetAttackTarget(null, 0);
			setShutdownAnim();
			setState(State.Shutdown);
			idleLoop?.Stop(entityId);
			idleLoop = null;
		}
		else
		{
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
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownDestruction(bool value)
	{
		if (!value || Health <= 1)
		{
			Transform transform = base.transform.FindInChilds("p_smokeLeft");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value);
			}
			Transform transform2 = base.transform.FindInChilds("p_smokeRight");
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processShutdown()
	{
		if (isGrounded)
		{
			return;
		}
		fallBlockPos.RoundToInt(position - blockHeightOffset);
		if ((!hasFallPoint || world.GetBlock(fallBlockPos).isair) && Physics.Raycast(position - Origin.position + blockHeightOffset, Vector3.down, out var hitInfo, 999f, 268500992))
		{
			fallPoint = hitInfo.point;
			isGrounded = false;
			hasFallPoint = true;
		}
		if (isShutdown)
		{
			Vector3 pos = position;
			float num = Vector3.Distance(position, fallPoint + Origin.position);
			if (num < 0.01f)
			{
				isGrounded = true;
				return;
			}
			pos.y -= num * SpeedFlying * 0.05f;
			pos.y = Mathf.Max(pos.y, fallPoint.y);
			SetPosition(pos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateShutdownState()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if ((bool)Owner)
		{
			if (Owner.Health <= 0 && state != State.Shutdown && state != State.Sentry)
			{
				performShutdown();
			}
			if (Health > 1 && Owner.Health > 1 && Vector3.Distance(position, Owner.position) < 10f && state == State.Shutdown)
			{
				setShutdown(value: false);
			}
		}
		if (state == State.Shutdown)
		{
			processShutdown();
		}
	}

	public void SetRenderersEnabled(bool value)
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setNoClip(bool value)
	{
		IsNoCollisionMode.Value = value;
		PhysicsTransform.gameObject.layer = (value ? 14 : 15);
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

	public void NotifyOffTheWorld()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDroneSystems()
	{
		if (initSuppressVOTimer > 0f)
		{
			initSuppressVOTimer -= 0.05f;
		}
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
					party.PartyMemberAdded += onPartyMemberAdded;
					party.PartyMemberRemoved += onPartyMemberRemoved;
				}
			}
		}
		if (sensors != null)
		{
			sensors.Update();
			if (sensors.IsEnemyInRange())
			{
				FollowDistance = 10f;
			}
			else
			{
				FollowDistance = 5f;
			}
		}
		bool flag = state != State.Shutdown && state != State.Sentry && state != State.Attack && state != State.Heal;
		if (healWeapon != null && flag && healWeapon.targetCanBeHealed(Owner) && initSuppressVOTimer <= 0f)
		{
			updateNeedsHealItemCheck();
		}
		if (teleportAtkCooldownTimer >= 0f)
		{
			teleportAtkCooldownTimer -= 0.05f;
		}
		for (int j = 0; j < installedWeapons.Count; j++)
		{
			installedWeapons[j].Update();
		}
	}

	public bool IsInRange(Vector3 target, float range)
	{
		return steering.IsInRange(target, range);
	}

	public void Move(Vector3 targetPos, float pointRadius)
	{
		Vector3 dir = steering.Seek(position, targetPos, pointRadius);
		move(dir, SpeedFlying);
	}

	public void RotateTo(Vector3 dir)
	{
		rotateTo(dir);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canMove(Vector3 dir)
	{
		Vector3 end = position + dir.normalized * physColHeight;
		return !RaycastPathUtils.IsPositionBlocked(position, end, 1073807360);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, float speedFlying, bool ignoreObsticles = false)
	{
		Vector3 end = position + dir.normalized * physColHeight;
		if (ownerIsOnVehicle || !IsPositionBlocked(position, end, 1073807360, debugDraw: true) || ignoreObsticles)
		{
			motion += dir * speedFlying * 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, Vector3 target, float speedFlying, bool ignoreObsticles = false)
	{
		Vector3 end = position + dir.normalized * physColHeight;
		if (ownerIsOnVehicle || !IsPositionBlocked(position, end, 1073807360, debugDraw: true) || ignoreObsticles)
		{
			motion += Vector3.ClampMagnitude(dir * speedFlying * 0.05f, (target - position).magnitude);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, bool ignoreObsticles = false)
	{
		move(dir, currentSpeedFlying, ignoreObsticles);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateTo(Vector3 dir)
	{
		if (dir != Vector3.zero)
		{
			rotation = rotateToDir(dir);
		}
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

	public static bool GetPath(List<Vector3> currentPath, EntityAlive entity, Vector3 start, Vector3 end, float speed, EAIBase aiTask = null, float seekDist = 0f, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		RaycastPathUtils.DrawBounds(start, Color.yellow, duration);
		RaycastPathUtils.DrawBounds(end, Color.green, duration);
		Vector3 projectedGroundPoint = GetProjectedGroundPoint(start, debugDraw, duration);
		Vector3 projectedGroundPoint2 = GetProjectedGroundPoint(end, debugDraw, duration);
		if (GetProjectedPath(currentPath, entity, projectedGroundPoint, projectedGroundPoint2, speed, aiTask, debugDraw, duration))
		{
			Utils.DrawCircleLinesHorzontal(end - Origin.position, seekDist, Color.white, Color.green, 12, duration);
			for (int i = 1; i < currentPath.Count; i++)
			{
				Vector3 value = currentPath[i];
				value += blockHeightOffset;
				value.y += 1f;
				currentPath[i] = value;
			}
			for (int j = 0; j < currentPath.Count - 1; j++)
			{
				if (IsPositionBlocked(currentPath[j], currentPath[j + 1], 1073807360))
				{
					if (debugDraw)
					{
						Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 1] - Origin.position, Color.white, Color.magenta, 2, duration);
						Utils.DrawCircleLinesHorzontal(currentPath[j] - Origin.position, pointRadius, Color.white, Color.green, 12, duration);
					}
					Vector3 vector = currentPath[j];
					vector.y -= 1f;
					Vector3 vector2 = currentPath[j + 1];
					vector2.y -= 1f;
					if (!IsPositionBlocked(currentPath[j], vector, 1073807360) && !IsPositionBlocked(vector, vector2, 1073807360))
					{
						Utils.DrawLine(vector - Origin.position, vector2 - Origin.position, Color.white, Color.yellow, 2, duration);
						currentPath[j] = vector;
						currentPath[j + 1] = vector2;
						if (j + 2 < currentPath.Count - 1 && !IsPositionBlocked(vector2, currentPath[j + 2] - blockHeightOffset, 1073807360))
						{
							currentPath[j + 2] -= blockHeightOffset;
						}
						j++;
					}
					if (j + 2 >= currentPath.Count - 1 || IsPositionBlocked(currentPath[j], currentPath[j + 2]))
					{
						currentPath.RemoveRange(j + 1, currentPath.Count - (j + 1));
						break;
					}
					Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 2] - Origin.position, Color.white, Color.blue, 2, duration);
					currentPath.RemoveAt(j + 1);
				}
				else if (debugDraw)
				{
					Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 1] - Origin.position, Color.white, Color.cyan, 2, duration);
					Utils.DrawCircleLinesHorzontal(currentPath[j] - Origin.position, pointRadius, Color.white, Color.cyan, 12, duration);
				}
			}
			currentPath.RemoveAt(0);
			return true;
		}
		return false;
	}

	public static bool GetProjectedPath(List<Vector3> projectedPath, EntityAlive entity, Vector3 start, Vector3 end, float speed, EAIBase aiTask = null, bool debugDraw = false, float duration = 0f)
	{
		projectedPath.Clear();
		int num = entity.entityId;
		PathFinderThread instance = PathFinderThread.Instance;
		PathEntity pathEntity = instance.GetPath(num)?.path;
		if (pathEntity == null && !instance.IsCalculatingPath(num))
		{
			instance.FindPath(entity, start, end, speed, _canBreak: false, aiTask);
			return false;
		}
		if (pathEntity == null)
		{
			return false;
		}
		for (int i = 0; i < pathEntity.points.Length; i++)
		{
			Vector3 projectedLocation = pathEntity.points[i].projectedLocation;
			projectedPath.Add(projectedLocation);
		}
		if (debugDraw)
		{
			for (int j = 0; j < projectedPath.Count - 1; j++)
			{
				Utils.DrawLine(projectedPath[j] - Origin.position, projectedPath[j + 1] - Origin.position, Color.white, Color.cyan, 2, duration);
			}
		}
		return true;
	}

	public static Vector3 GetProjectedGroundPoint(Vector3 currentPos, bool debugDraw = false, float duration = 0f)
	{
		Vector3 vector = currentPos;
		if (Physics.Raycast(vector - Origin.position + blockHeightOffset, Vector3.down, out var hitInfo, 100f, 1073807360))
		{
			vector = hitInfo.point + blockHeightOffset + Origin.position;
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(vector, Color.white, duration);
			}
		}
		else
		{
			vector -= blockHeightOffset;
			vector.y -= 1f;
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(vector, Color.white, duration);
			}
		}
		return vector;
	}

	public static Vector3[] GetGroupPositions(EntityAlive _entity, float followDist, bool debugDraw = false, float duration = 0f)
	{
		Vector3[] array = new Vector3[5];
		float num = 1f;
		Vector3 chestPosition = _entity.getChestPosition();
		Vector3 lookVector = _entity.GetLookVector();
		lookVector.y = 0f;
		array[0] = chestPosition - lookVector * followDist;
		Vector3 normalized = (_entity.transform.right - lookVector).normalized;
		normalized.y = 0f;
		array[1] = chestPosition + normalized * followDist * num;
		Vector3 normalized2 = (_entity.transform.right + lookVector).normalized;
		normalized2.y = 0f;
		array[2] = chestPosition - normalized2 * followDist * num;
		Vector3 normalized3 = (normalized - lookVector).normalized;
		normalized3.y = 0f;
		array[3] = chestPosition + normalized3 * followDist * num;
		Vector3 normalized4 = (normalized2 + lookVector).normalized;
		normalized4.y = 0f;
		array[4] = chestPosition - normalized4 * followDist * num;
		World world = GameManager.Instance.World;
		foreach (Vector3 vector in array)
		{
			bool num2 = IsPositionBlocked(chestPosition, vector, 1073807360);
			bool flag = IsPositionBlocked(vector, chestPosition, 1073807360);
			Vector3i vector3i = World.worldToBlockPos(vector);
			if (!num2 && !flag)
			{
				if (RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, vector3i.ToVector3Center())) != null && debugDraw)
				{
					RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center()), Color.yellow, duration);
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center()), Color.red, duration);
			}
		}
		return array;
	}

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		RaycastHit hit;
		return IsPositionBlocked(start, end, out hit, layerMask, debugDraw, duration);
	}

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		Vector3 direction = end - start;
		return IsPositionBlocked(new Ray(start - Origin.position, direction), out hit, layerMask, direction.magnitude, debugDraw, duration);
	}

	public static bool IsPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, float maxDist = 100f, bool debugDraw = false, float duration = 0f)
	{
		bool flag = Physics.Raycast(ray, out hit, maxDist, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, hit.point, Color.magenta, Color.magenta, 2, duration);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.cyan, 2, duration);
			}
		}
		return flag;
	}

	public static EntityDrone FindCollisionEntity(Transform t)
	{
		if ((bool)t)
		{
			EntityDrone component = t.GetComponent<EntityDrone>();
			if ((bool)component)
			{
				return component;
			}
		}
		return null;
	}

	public bool IgnoreCollisionEntity(Ray ray, float seeDist)
	{
		bool result = false;
		int layer = base.gameObject.layer;
		GameObject obj = PhysicsTransform.gameObject;
		int layer2 = obj.layer;
		Utils.SetLayerRecursively(base.gameObject, 2);
		Utils.SetLayerRecursively(obj, 2);
		if (Voxel.Raycast(world, ray, seeDist, -1612492829, 64, 0f))
		{
			result = true;
		}
		Utils.SetLayerRecursively(base.gameObject, layer);
		Utils.SetLayerRecursively(obj, layer2);
		return result;
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
	public bool isEntityAboveOrBelow(Entity entity)
	{
		bool result = false;
		Vector3 chestPosition = entity.getChestPosition();
		float num = position.x - chestPosition.x;
		float num2 = position.z - chestPosition.z;
		float num3 = position.y - chestPosition.y;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void followPlannedPath(float speed, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		if (currentPath.Count <= 0)
		{
			return;
		}
		currentPathTarget = currentPath[0];
		RotateTo((currentPathTarget - position).normalized);
		Move(currentPathTarget, pointRadius);
		if (IsInRange(currentPathTarget, pointRadius))
		{
			currentPath.RemoveAt(0);
			return;
		}
		if (debugDraw && currentPath.Count > 1)
		{
			RaycastPathUtils.DrawLine(currentPath[0], currentPath[1], Color.green);
		}
		if (pathTracker.IsStuck(position, currentPath[0]))
		{
			if (currentPath.Count > 1)
			{
				TeleportToPosition(currentPath[1]);
				currentPath.RemoveRange(0, 2);
			}
			else
			{
				TeleportToPosition(currentPath[0]);
				currentPath.RemoveAt(0);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearCurrentPath()
	{
		currentPath.Clear();
		OnPathInterupted();
	}

	public void OnPathInterupted()
	{
		moveHelper.Stop();
		navigator.clearPath();
		if (PathFinderThread.Instance != null)
		{
			PathFinderThread.Instance.RemovePathsFor(entityId);
		}
	}

	public override string MakeDebugNameInfo()
	{
		return $"\nState: {state.ToStringCached()}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugName()
	{
		aiManager.UpdateDebugName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugUpdate()
	{
		updateDebugName();
		if ((bool)debugCamera)
		{
			if ((bool)Owner && currentPath.Count == 0)
			{
				debugCamera.transform.LookAt(Owner.getHeadPosition() - Origin.position);
			}
			else
			{
				debugCamera.transform.forward = base.transform.forward;
			}
		}
	}

	public void DebugTeleportUnstuck()
	{
		teleportState();
	}

	public void DebugTeleportTo(Vector3 pos)
	{
		clearCurrentPath();
		motion = Vector3.zero;
		SetPosition(pos);
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
		camera.farClipPlane = 64f;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool procEnemiesInRange()
	{
		if (DebugEnemiesInRange && sensors.IsEnemyInRange() && (bool)Owner)
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
		_bw.Write((byte)3);
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
				float[] array = new float[3] { SentryPos.x, SentryPos.y, SentryPos.z };
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
			bag.Write(_bw);
		}
		if ((syncFlags & 0x20) > 0)
		{
			_bw.Write(isQuietMode);
		}
		if ((syncFlags & 0x40) > 0)
		{
			_bw.Write(IsFlashlightOn);
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
				SentryPos.x = _br.ReadSingle();
				SentryPos.y = _br.ReadSingle();
				SentryPos.z = _br.ReadSingle();
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
			if (b >= 1 && transitionState == State.Heal)
			{
				userRequestedHeal = _br.ReadBoolean();
			}
			DebugDroneLog("{0} read transition {1} > {2}", this, state, transitionState);
		}
		if ((syncFlags & 2) > 0)
		{
			byte b3 = _br.ReadByte();
			isLocked = (b3 & 2) > 0;
			ownerSteamId = PlatformUserIdentifierAbs.FromStream(_br);
			if (b > 1)
			{
				passwordHash = _br.ReadString();
			}
			else
			{
				passwordHash = _br.ReadInt32().ToString();
			}
			allowedUsers.Clear();
			int num = _br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				allowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: true));
			}
		}
		if ((syncFlags & 8) > 0)
		{
			if (b >= 3)
			{
				bag = Bag.Read(_br);
			}
			else
			{
				int num2 = _br.ReadByte();
				ItemStack[] array = new ItemStack[num2];
				for (int j = 0; j < num2; j++)
				{
					ItemStack itemStack = new ItemStack();
					array[j] = itemStack.Read(_br);
				}
				bag.SetSlots(array);
			}
		}
		if ((syncFlags & 0x10) > 0)
		{
			performRepair();
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
			IsFlashlightOn = _br.ReadBoolean();
			setFlashlightOn(IsFlashlightOn);
		}
		if ((syncFlags & 0x100) > 0)
		{
			setHealAllies(_br.ReadBoolean());
		}
		if ((syncFlags & 0x80) > 0)
		{
			OriginalItemValue.Read(_br);
			LoadMods();
		}
	}

	public override bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		base.OnLockedServer(_success, _lockingPlayerID, _context, _channel);
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		base.OnUnlockedServer(_unlockingPlayerId, _channel);
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		if (!_success)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), Localization.Get("ttVehicleInUse"), string.Empty, "ui_denied");
		}
		else if (!(_context is EntityLockContext entityLockContext))
		{
			Log.Warning("[EntityDrone] Missing or invalid lock context.");
			LockManager.Instance.UnlockRequestLocal();
		}
		else
		{
			bag = entityLockContext.Bag.Clone();
			startInteraction(entityLockContext.Command);
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
		if ((_userIdentifier == null || !_userIdentifier.Equals(ownerSteamId)) && !allowedUsers.Contains(_userIdentifier))
		{
			return IsOwner(_userIdentifier);
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
		if (ownerSteamId != null)
		{
			return ownerSteamId.Equals(_userIdentifier);
		}
		return false;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(passwordHash);
	}

	public string GetHashForPassword(string _password)
	{
		return Utils.HashString(_password);
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (LocalPlayerIsOwner() && _passwordHash != null)
		{
			if (_passwordHash != passwordHash)
			{
				passwordHash = _passwordHash;
				allowedUsers.Clear();
				if (ownerSteamId == null)
				{
					SetOwner(_userIdentifier);
				}
			}
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (LocalPlayerIsOwner() || !HasPassword())
		{
			SendSyncData(2);
			return true;
		}
		if (_passwordHash == passwordHash)
		{
			allowedUsers.Add(_userIdentifier);
			SendSyncData(2);
			return true;
		}
		return false;
	}

	public string GetPasswordHash()
	{
		return passwordHash;
	}
}
