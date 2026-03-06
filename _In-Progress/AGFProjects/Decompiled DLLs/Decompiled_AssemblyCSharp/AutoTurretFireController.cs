using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class AutoTurretFireController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum TurretState
	{
		Asleep,
		Awake,
		Overheated
	}

	public class TurretEntitySorter : IComparer<Entity>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 self;

		public TurretEntitySorter(Vector3 _self)
		{
			self = _self;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int isNearer(Entity _e, Entity _other)
		{
			float num = DistanceSqr(self, _e.position);
			float num2 = DistanceSqr(self, _other.position);
			if (num < num2)
			{
				return -1;
			}
			if (!(num <= num2))
			{
				return 1;
			}
			return 0;
		}

		public int Compare(Entity _obj1, Entity _obj2)
		{
			return isNearer(_obj1, _obj2);
		}

		public float DistanceSqr(Vector3 pointA, Vector3 pointB)
		{
			Vector3 vector = pointA - pointB;
			return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		}
	}

	public bool IsOn;

	public Transform Cone;

	public Transform Laser;

	public Transform Muzzle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 blockPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConeYaw = 22.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConePitch = 22.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConeDistance = 5.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fireRateMax = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float findTargetDelayMax = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int blockDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int rayCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int raySpread;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float wakeUpTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float wakeUpTimeMax = 0.6522f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallAsleepTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallAsleepTimeMax = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int burstRoundCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int burstRoundCountMax = 20;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float burstFireRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float burstFireRateMax = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float coolOffTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float coolOffTimeMax = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float overshootTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float overshootTimeMax = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float retargetSoundTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float retargetSoundTimeMax = 0.874f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds targetingBounds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TurretState state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string fireSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string wakeUpSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string overheatSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string targetingSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string idleSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string muzzleFireParticle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string muzzleSmokeParticle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string ammoItemName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DamageMultiplier damageMultiplier;

	public List<string> buffActions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 yawRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 pitchRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 spread;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fireRate = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float findTargetDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive currentEntityTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AutoTurretController atc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TurretEntitySorter sorter;

	public TileEntityPoweredRangedTrap TileEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTimeBetweenSoundDispatch = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeSinceDispatchSounds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> soundCommandDictionary = new Dictionary<string, bool>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> soundsPlayOrder = new List<string>();

	public Vector3 BlockPosition
	{
		get
		{
			return blockPos - Origin.position;
		}
		set
		{
			blockPos = value;
		}
	}

	public float CenteredYaw
	{
		get
		{
			return TileEntity.CenteredYaw;
		}
		set
		{
			TileEntity.CenteredYaw = value;
		}
	}

	public float CenteredPitch
	{
		get
		{
			return TileEntity.CenteredPitch;
		}
		set
		{
			TileEntity.CenteredPitch = value;
		}
	}

	public float MaxDistance => maxDistance;

	public bool hasTarget
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentEntityTarget != null;
		}
	}

	public void Init(DynamicProperties _properties, AutoTurretController _atc)
	{
		atc = _atc;
		IsOn = false;
		if (_properties.Values.ContainsKey("FireSound"))
		{
			fireSound = _properties.Values["FireSound"];
		}
		else
		{
			fireSound = "Electricity/Turret/turret_fire";
		}
		if (_properties.Values.ContainsKey("WakeUpSound"))
		{
			wakeUpSound = _properties.Values["WakeUpSound"];
		}
		else
		{
			wakeUpSound = "Electricity/Turret/turret_windup";
		}
		if (_properties.Values.ContainsKey("OverheatSound"))
		{
			overheatSound = _properties.Values["OverheatSound"];
		}
		else
		{
			overheatSound = "Electricity/Turret/turret_overheat_lp";
		}
		if (_properties.Values.ContainsKey("TargetingSound"))
		{
			targetingSound = _properties.Values["TargetingSound"];
		}
		else
		{
			targetingSound = "Electricity/Turret/turret_retarget_lp";
		}
		if (_properties.Values.ContainsKey("IdleSound"))
		{
			idleSound = _properties.Values["IdleSound"];
		}
		else
		{
			idleSound = "Electricity/Turret/turret_idle_lp";
		}
		if (_properties.Values.ContainsKey("EntityDamage"))
		{
			entityDamage = int.Parse(_properties.Values["EntityDamage"]);
		}
		if (_properties.Values.ContainsKey("BlockDamage"))
		{
			blockDamage = int.Parse(_properties.Values["BlockDamage"]);
		}
		else
		{
			blockDamage = 0;
		}
		if (_properties.Values.ContainsKey("MaxDistance"))
		{
			maxDistance = StringParsers.ParseFloat(_properties.Values["MaxDistance"]);
		}
		else
		{
			maxDistance = 16f;
		}
		if (_properties.Values.ContainsKey("YawRange"))
		{
			float num = StringParsers.ParseFloat(_properties.Values["YawRange"]);
			num *= 0.5f;
			yawRange = new Vector2(0f - num, num);
		}
		else
		{
			yawRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("PitchRange"))
		{
			float num2 = StringParsers.ParseFloat(_properties.Values["PitchRange"]);
			num2 *= 0.5f;
			pitchRange = new Vector2(0f - num2, num2);
		}
		else
		{
			pitchRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("RaySpread"))
		{
			float num3 = StringParsers.ParseFloat(_properties.Values["RaySpread"]);
			num3 *= 0.5f;
			spread = new Vector2(0f - num3, num3);
		}
		else
		{
			spread = new Vector2(-1f, 1f);
		}
		if (_properties.Values.ContainsKey("RayCount"))
		{
			rayCount = int.Parse(_properties.Values["RayCount"]);
		}
		else
		{
			rayCount = 1;
		}
		if (_properties.Values.ContainsKey("WakeUpTime"))
		{
			wakeUpTimeMax = StringParsers.ParseFloat(_properties.Values["WakeUpTime"]);
		}
		if (_properties.Values.ContainsKey("FallAsleepTime"))
		{
			fallAsleepTimeMax = StringParsers.ParseFloat(_properties.Values["FallAsleepTime"]);
		}
		if (_properties.Values.ContainsKey("BurstRoundCount"))
		{
			burstRoundCountMax = int.Parse(_properties.Values["BurstRoundCount"]);
		}
		if (_properties.Values.ContainsKey("BurstFireRate"))
		{
			burstFireRateMax = StringParsers.ParseFloat(_properties.Values["BurstFireRate"]);
		}
		if (_properties.Values.ContainsKey("CooldownTime"))
		{
			coolOffTimeMax = StringParsers.ParseFloat(_properties.Values["CooldownTime"]);
		}
		if (_properties.Values.ContainsKey("OvershootTime"))
		{
			overshootTimeMax = StringParsers.ParseFloat(_properties.Values["OvershootTime"]);
		}
		_properties.ParseString("ParticlesMuzzleFire", ref muzzleFireParticle);
		_properties.ParseString("ParticlesMuzzleSmoke", ref muzzleSmokeParticle);
		if (_properties.Values.ContainsKey("AmmoItem"))
		{
			ammoItemName = _properties.Values["AmmoItem"];
		}
		else
		{
			ammoItemName = "9mmBullet";
		}
		buffActions = new List<string>();
		if (_properties.Values.ContainsKey("Buff"))
		{
			string[] collection = _properties.Values["Buff"].Replace(" ", "").Split(',');
			buffActions.AddRange(collection);
		}
		targetingBounds = Cone.GetComponent<MeshRenderer>().bounds;
		damageMultiplier = new DamageMultiplier(_properties, null);
		sorter = new TurretEntitySorter(BlockPosition);
		state = TurretState.Asleep;
		Cone.localScale = new Vector3(Cone.localScale.x * (yawRange.y / 22.5f) * (maxDistance / 5.25f), Cone.localScale.y * (pitchRange.y / 22.5f) * (maxDistance / 5.25f), Cone.localScale.z * (maxDistance / 5.25f));
		Cone.gameObject.SetActive(value: false);
		Laser.localScale = new Vector3(Laser.localScale.x, Laser.localScale.y, Laser.localScale.z * (maxDistance / 5.25f));
		Laser.gameObject.SetActive(value: false);
	}

	public void OnDestroy()
	{
		OnPoweredOff();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (atc == null || TileEntity == null)
		{
			return;
		}
		if (!IsOn || atc.IsUserAccessing || TileEntity.IsUserAccessing())
		{
			if (atc.IsUserAccessing)
			{
				atc.YawController.Yaw = CenteredYaw;
				atc.YawController.UpdateYaw();
				atc.PitchController.Pitch = CenteredPitch;
				atc.PitchController.UpdatePitch();
				switch (state)
				{
				case TurretState.Asleep:
					state = TurretState.Awake;
					break;
				case TurretState.Awake:
					if (burstRoundCount >= burstRoundCountMax)
					{
						state = TurretState.Overheated;
						burstRoundCount = 0;
					}
					break;
				case TurretState.Overheated:
					if (coolOffTime == 0f)
					{
						broadcastPlay(overheatSound);
					}
					if (coolOffTime < coolOffTimeMax)
					{
						coolOffTime += Time.deltaTime;
						break;
					}
					state = TurretState.Awake;
					coolOffTime = 0f;
					broadcastStop(overheatSound);
					break;
				}
			}
			else if (!IsOn)
			{
				if (atc.YawController.Yaw != CenteredYaw)
				{
					atc.YawController.Yaw = CenteredYaw;
					atc.YawController.UpdateYaw();
				}
				if (atc.PitchController.Pitch != CenteredPitch)
				{
					atc.PitchController.Pitch = CenteredPitch;
					atc.PitchController.UpdatePitch();
				}
			}
			else
			{
				if (atc.YawController.Yaw != CenteredYaw)
				{
					atc.YawController.Yaw = CenteredYaw;
					atc.YawController.UpdateYaw();
				}
				if (atc.PitchController.Pitch != CenteredPitch)
				{
					atc.PitchController.Pitch = CenteredPitch;
					atc.PitchController.UpdatePitch();
				}
			}
			return;
		}
		if (!hasTarget)
		{
			findTarget();
		}
		else if (shouldIgnoreTarget(currentEntityTarget))
		{
			currentEntityTarget = null;
			if (!state.Equals(TurretState.Overheated))
			{
				state = TurretState.Asleep;
				wakeUpTime = 0f;
			}
		}
		if (atc.IsTurning)
		{
			broadcastPlay(targetingSound);
			broadcastStop(idleSound);
		}
		else
		{
			broadcastStop(targetingSound);
			broadcastPlay(idleSound);
		}
		switch (state)
		{
		case TurretState.Asleep:
			if (hasTarget)
			{
				if (wakeUpTime == 0f)
				{
					broadcastPlay(wakeUpSound);
				}
				if (wakeUpTime < EffectManager.GetValue(PassiveEffects.TurretWakeUp, null, _entity: currentEntityTarget, _originalValue: wakeUpTimeMax))
				{
					wakeUpTime += Time.deltaTime;
					break;
				}
				state = TurretState.Awake;
				wakeUpTime = 0f;
			}
			else
			{
				atc.YawController.Yaw = CenteredYaw;
				atc.PitchController.Pitch = CenteredPitch;
			}
			break;
		case TurretState.Awake:
			if (hasTarget)
			{
				float _yaw = atc.YawController.Yaw;
				float _pitch = atc.PitchController.Pitch;
				Vector3 targetPos = Vector3.zero;
				if (!canHitEntity(ref _yaw, ref _pitch, out targetPos))
				{
					overshootTime += Time.deltaTime;
				}
				if (overshootTime >= overshootTimeMax)
				{
					currentEntityTarget = null;
					overshootTime = 0f;
					return;
				}
				fallAsleepTime = 0f;
				atc.YawController.Yaw = _yaw;
				atc.PitchController.Pitch = _pitch;
				if (burstRoundCount < burstRoundCountMax)
				{
					if (burstFireRate < burstFireRateMax)
					{
						burstFireRate += Time.deltaTime;
						break;
					}
					Fire();
					burstFireRate = 0f;
				}
				else
				{
					state = TurretState.Overheated;
					burstRoundCount = 0;
				}
			}
			else if (currentEntityTarget != null && fallAsleepTime < fallAsleepTimeMax)
			{
				fallAsleepTime += Time.deltaTime;
			}
			else
			{
				currentEntityTarget = null;
				state = TurretState.Asleep;
				fallAsleepTime = 0f;
			}
			break;
		case TurretState.Overheated:
			if (coolOffTime == 0f)
			{
				broadcastPlay(overheatSound);
			}
			if (coolOffTime < coolOffTimeMax)
			{
				coolOffTime += Time.deltaTime;
				break;
			}
			state = TurretState.Awake;
			coolOffTime = 0f;
			broadcastStop(overheatSound);
			break;
		}
		dispatchSoundCommandsThrottle(Time.deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findTarget()
	{
		Vector3 position = Cone.transform.position;
		currentEntityTarget = null;
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(blockPos, Vector3.one * (maxDistance * 2f)), new List<Entity>());
		entitiesInBounds.Sort(sorter);
		bool flag = false;
		Collider[] array = Physics.OverlapSphere(position + Origin.position, 0.05f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject != atc.gameObject)
			{
				flag = true;
				break;
			}
		}
		if (entitiesInBounds.Count <= 0 || flag)
		{
			return;
		}
		for (int j = 0; j < entitiesInBounds.Count; j++)
		{
			if (shouldIgnoreTarget(entitiesInBounds[j]))
			{
				continue;
			}
			Vector3 _targetPos = Vector3.zero;
			float _yaw = CenteredYaw;
			float _pitch = CenteredPitch;
			if (!trackTarget(entitiesInBounds[j], ref _yaw, ref _pitch, out _targetPos))
			{
				continue;
			}
			Vector3 normalized = (_targetPos - position).normalized;
			if (!Voxel.Raycast(ray: new Ray(position + Origin.position - normalized * 0.05f, normalized), _world: GameManager.Instance.World, distance: maxDistance + 0.05f, _layerMask: -538750981, _hitMask: 8, _sphereRadius: 0f) || !Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
				continue;
			}
			if (Voxel.voxelRayHitInfo.tag == "E_Vehicle")
			{
				EntityVehicle entityVehicle = EntityVehicle.FindCollisionEntity(Voxel.voxelRayHitInfo.transform);
				if (entityVehicle != null && entityVehicle.IsAttached(entitiesInBounds[j]))
				{
					currentEntityTarget = entitiesInBounds[j] as EntityAlive;
					break;
				}
				currentEntityTarget = null;
				continue;
			}
			Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			if (hitRootTransform == null)
			{
				continue;
			}
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component != null)
			{
				if (component == entitiesInBounds[j])
				{
					currentEntityTarget = component as EntityAlive;
					break;
				}
				currentEntityTarget = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target)
	{
		if (Vector3.Dot(_target.position - TileEntity.ToWorldPos().ToVector3(), Cone.transform.forward) > 0f)
		{
			if (_target == currentEntityTarget)
			{
				currentEntityTarget = null;
			}
			return true;
		}
		if (!_target.IsAlive())
		{
			return true;
		}
		if (_target is EntitySupplyCrate)
		{
			return true;
		}
		if (_target is EntityVehicle)
		{
			Entity attachedMainEntity = (_target as EntityVehicle).AttachedMainEntity;
			if (attachedMainEntity == null)
			{
				return true;
			}
			_target = attachedMainEntity;
		}
		if (_target is EntityPlayer)
		{
			bool flag = false;
			bool flag2 = false;
			EnumPlayerKillingMode enumPlayerKillingMode = (EnumPlayerKillingMode)GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (persistentPlayerList != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId) && TileEntity.IsOwner(persistentPlayerList.EntityToPlayerMap[_target.entityId].PrimaryId))
			{
				flag = true;
			}
			if (!flag)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(TileEntity.GetOwner());
				if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
				{
					PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
					if (playerData.ACL != null && persistentPlayerData != null && playerData.ACL.Contains(persistentPlayerData.PrimaryId))
					{
						flag2 = true;
					}
				}
			}
			if (enumPlayerKillingMode == EnumPlayerKillingMode.NoKilling)
			{
				return true;
			}
			if (flag && !TileEntity.TargetSelf)
			{
				return true;
			}
			if (flag2 && (!TileEntity.TargetAllies || (enumPlayerKillingMode != EnumPlayerKillingMode.KillEveryone && enumPlayerKillingMode != EnumPlayerKillingMode.KillAlliesOnly)))
			{
				return true;
			}
			if (!flag && !flag2 && (!TileEntity.TargetStrangers || (enumPlayerKillingMode != EnumPlayerKillingMode.KillStrangersOnly && enumPlayerKillingMode != EnumPlayerKillingMode.KillEveryone)))
			{
				return true;
			}
		}
		if (_target is EntityTurret)
		{
			return true;
		}
		if (_target is EntityDrone)
		{
			return true;
		}
		if (_target is EntityNPC && !TileEntity.TargetStrangers)
		{
			return true;
		}
		if (_target is EntityEnemy && !TileEntity.TargetZombies)
		{
			return true;
		}
		if (_target is EntityAnimal && !_target.EntityClass.bIsEnemyEntity)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHitEntity(ref float _yaw, ref float _pitch, out Vector3 targetPos)
	{
		Vector3 origin = Cone.transform.position - Origin.position;
		if (!trackTarget(currentEntityTarget, ref _yaw, ref _pitch, out targetPos))
		{
			return false;
		}
		Ray ray = new Ray(origin, (targetPos - Cone.transform.position).normalized);
		if (Voxel.Raycast(GameManager.Instance.World, ray, maxDistance, -538750981, 8, 0f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			if (hitRootTransform == null)
			{
				return false;
			}
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component != null && component.IsAlive() && currentEntityTarget == component)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool trackTarget(Entity _target, ref float _yaw, ref float _pitch, out Vector3 _targetPos)
	{
		if (GameManager.Instance.World.GetGameRandom().RandomFloat < 0.05f)
		{
			_targetPos = _target.getHeadPosition() - Origin.position;
		}
		else
		{
			_targetPos = _target.getChestPosition() - Origin.position;
		}
		Vector3 normalized = (_targetPos - atc.YawController.transform.position).normalized;
		Vector3 normalized2 = (_targetPos - atc.PitchController.transform.position).normalized;
		float num = Quaternion.LookRotation(normalized).eulerAngles.y - atc.transform.rotation.eulerAngles.y;
		float num2 = Quaternion.LookRotation(normalized2).eulerAngles.x - atc.transform.rotation.z;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		float num3 = CenteredYaw % 360f;
		float num4 = CenteredPitch % 360f;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		if (!(num >= num3 + yawRange.x) || !(num <= num3 + yawRange.y) || !(num2 >= num4 + pitchRange.x) || !(num2 <= num4 + pitchRange.y))
		{
			return false;
		}
		_yaw = num;
		_pitch = num2;
		return true;
	}

	public void PlayerFire(bool buttonPressed)
	{
		if (state != TurretState.Awake)
		{
			return;
		}
		if (burstFireRate < burstFireRateMax)
		{
			burstFireRate += Time.deltaTime;
		}
		else if (buttonPressed)
		{
			if (TileEntity.ClientData != null)
			{
				TileEntity.ClientData.SendSlots = true;
			}
			Fire();
			if (TileEntity.ClientData != null)
			{
				TileEntity.ClientData.SendSlots = false;
			}
			burstFireRate = 0f;
		}
	}

	public void Fire()
	{
		ItemClass _ammoItem = null;
		if (TileEntity != null)
		{
			if (!TileEntity.IsLocked)
			{
				return;
			}
			if ((SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !TileEntity.IsUserAccessing()) || (atc != null && atc.IsUserAccessing))
			{
				if (!TileEntity.DecrementAmmo(out _ammoItem))
				{
					TileEntity.IsLocked = false;
					TileEntity.SetModified();
					return;
				}
				burstRoundCount++;
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || (atc != null && atc.IsUserAccessing))
		{
			ItemValue itemValue = null;
			if (_ammoItem != null)
			{
				itemValue = new ItemValue(_ammoItem.Id);
			}
			Vector3 origin = Cone.position + Origin.position;
			Ray ray = new Ray(origin, Vector3.forward);
			Vector3 turretLookDirection = Cone.forward * -1f;
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			int num = rayCount;
			if (itemValue != null)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, num);
			}
			Vector2 localSpread = getSpread(itemValue);
			float range = GetRange(itemValue);
			int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue));
			num2++;
			int blockPenFactor = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, itemValue, 251f));
			for (int i = 0; i < num; i++)
			{
				fireSingleDirectionBullet(gameRandom, itemValue, ray, turretLookDirection, localSpread, range, num2, blockPenFactor);
			}
			if (!string.IsNullOrEmpty(muzzleFireParticle))
			{
				FireControllerUtils.SpawnParticleEffect(new ParticleEffect(muzzleFireParticle, Muzzle.position + Origin.position, Muzzle.rotation, 1f, Color.white, fireSound, null), -1);
			}
			if (!string.IsNullOrEmpty(muzzleSmokeParticle))
			{
				float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(BlockPosition)) / 2f;
				FireControllerUtils.SpawnParticleEffect(new ParticleEffect(muzzleSmokeParticle, Muzzle.position + Origin.position, Muzzle.rotation, lightValue, new Color(1f, 1f, 1f, 0.3f), null, null), -1);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		Vector3 fireSingleDirectionBullet(GameRandom _random, ItemValue _ammo, Ray ray2, Vector3 _turretLookDirection, Vector2 _localSpread, float num3, int penCount, int num4)
		{
			ray2.direction = Quaternion.Euler((float)_random.RandomRange(-1, 1) * _localSpread.x, (float)_random.RandomRange(-1, 1) * _localSpread.y, 0f) * _turretLookDirection;
			waterCollisionParticles.Init(TileEntity.OwnerEntityID, "bullet", "water", 16);
			waterCollisionParticles.CheckCollision(ray2.origin, ray2.direction, num3);
			int hitMask = 8;
			EntityAlive entityAlive = null;
			for (int j = 0; j < penCount; j++)
			{
				if (Voxel.Raycast(GameManager.Instance.World, ray2, num3, -538750997, hitMask, 0f))
				{
					WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
					if (worldRayHitInfo.hit.distanceSq > num3 * num3)
					{
						return ray2.direction;
					}
					ray2.origin = worldRayHitInfo.hit.pos;
					if (worldRayHitInfo.tag.StartsWith("E_"))
					{
						string bodyPartName;
						EntityAlive entityAlive2 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out bodyPartName) as EntityAlive;
						if (entityAlive == entityAlive2)
						{
							ray2.origin = worldRayHitInfo.hit.pos + ray2.direction * 0.1f;
							j--;
							continue;
						}
						entityAlive = entityAlive2;
					}
					else
					{
						j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num4);
					}
					float num5 = 1f;
					float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _ammo, num3);
					if (worldRayHitInfo.hit.distanceSq > value * value)
					{
						num5 = 1f - (worldRayHitInfo.hit.distanceSq - value * value) / (num3 * num3 - value * value);
					}
					float num6 = 1f;
					World world = GameManager.Instance.World;
					Vector3i pos = World.worldToBlockPos(worldRayHitInfo.hit.pos);
					WaterValue water = world.GetWater(pos);
					if (water.HasMass())
					{
						Vector3i pos2 = new Vector3i(pos.x, pos.y + 1, pos.z);
						if (world.GetWater(pos2).GetMassPercent() > 0f)
						{
							num6 = 0.25f;
						}
						else
						{
							float num7 = worldRayHitInfo.hit.pos.y - (float)pos.y;
							float num8 = water.GetMassPercent() * 0.6f - num7;
							if (num8 > 0f)
							{
								num6 = 1f - 0.75f * num8;
							}
						}
					}
					ItemActionAttack.Hit(worldRayHitInfo, TileEntity.OwnerEntityID, EnumDamageTypes.Piercing, GetDamageBlock(_ammo, ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo)) * num5 * num6, GetDamageEntity(_ammo) * num5 * num6, 1f, 1f, 0.5f, 0.05f, "bullet", damageMultiplier, buffActions, new ItemActionAttack.AttackHitInfo(), 3, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, (atc != null && atc.IsUserAccessing) ? TileEntity.EntityId : (-2), _ammo);
				}
			}
			return ray2.direction;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetRange(ItemValue _itemValue)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, _itemValue, maxDistance, null, null, FastTags<TagGroup.Global>.none);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getSpread(ItemValue _itemValue)
	{
		float value = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _itemValue, spread.y * 2f, null, null, FastTags<TagGroup.Global>.none);
		float value2 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _itemValue, spread.y * 2f, null, null, FastTags<TagGroup.Global>.none);
		return new Vector2(value, value2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageEntity(ItemValue _itemValue)
	{
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, entityDamage, null, null, FastTags<TagGroup.Global>.none);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue)
	{
		FastTags<TagGroup.Global> none = FastTags<TagGroup.Global>.none;
		none |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, blockDamage, null, null, none);
		return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	public void OnPoweredOff()
	{
		broadcastStop(targetingSound);
		broadcastStop(overheatSound);
		broadcastStop(idleSound);
		dispatchSoundCommands();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastPlay(string name)
	{
		broadcastSoundAction(name, play: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastStop(string name)
	{
		broadcastSoundAction(name, play: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastSoundAction(string name, bool play)
	{
		if (!string.IsNullOrEmpty(name))
		{
			soundCommandDictionary[name] = play;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchSoundCommands()
	{
		foreach (KeyValuePair<string, bool> item in soundCommandDictionary)
		{
			if (item.Value)
			{
				Manager.BroadcastPlay(blockPos, item.Key);
			}
			else
			{
				Manager.BroadcastStop(blockPos, item.Key);
			}
		}
		soundCommandDictionary.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchSoundCommandsThrottle(float deltaTime)
	{
		timeSinceDispatchSounds += Time.deltaTime;
		if (timeSinceDispatchSounds > 1f)
		{
			timeSinceDispatchSounds %= 1f;
			dispatchSoundCommands();
		}
	}
}
