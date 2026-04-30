using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class MiniTurretFireController : MonoBehaviour
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

	public Transform Cone;

	public Transform Laser;

	public Transform Muzzle;

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
	public float wakeUpTimeMax = 0.6522f;

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
	[PublicizedFrom(EAccessModifier.Protected)]
	public float burstFireRateMax = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float coolOffTimeMax = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float overshootTimeMax = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TurretState state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
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
	public string muzzleFireParticle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string muzzleSmokeParticle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string ammoItemName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
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
	public Vector2 spreadHorizontal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 spreadVertical;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive currentEntityTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetChestHeadDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetChestHeadPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TurretEntitySorter sorter;

	public EntityTurret entityTurret;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Handle turretSpinAudioHandle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSeekRayRadius = 0.05f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;

	public static FastTags<TagGroup.Global> RangedTag = FastTags<TagGroup.Global>.Parse("ranged");

	public static FastTags<TagGroup.Global> MeleeTag = FastTags<TagGroup.Global>.Parse("melee");

	public static FastTags<TagGroup.Global> PrimaryTag = FastTags<TagGroup.Global>.Parse("primary");

	public static FastTags<TagGroup.Global> SecondaryTag = FastTags<TagGroup.Global>.Parse("secondary");

	public static FastTags<TagGroup.Global> TurretTag = FastTags<TagGroup.Global>.Parse("turret");

	public bool IsOn
	{
		get
		{
			if (entityTurret != null)
			{
				return entityTurret.IsOn;
			}
			return false;
		}
	}

	public Vector3 TurretPosition => entityTurret.transform.position + Origin.position;

	public float CenteredYaw
	{
		get
		{
			return entityTurret.CenteredYaw;
		}
		set
		{
			entityTurret.CenteredYaw = value;
		}
	}

	public float CenteredPitch
	{
		get
		{
			return entityTurret.CenteredPitch;
		}
		set
		{
			entityTurret.CenteredPitch = value;
		}
	}

	public float MaxDistance => maxDistance;

	public bool hasTarget => currentEntityTarget != null;

	public void Init(DynamicProperties _properties, EntityTurret _entity)
	{
		entityTurret = _entity;
		Cone = entityTurret.Cone;
		Laser = entityTurret.Laser;
		Muzzle = entityTurret.transform.FindInChilds("Muzzle");
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
			spreadHorizontal = new Vector2(0f - num3, num3);
		}
		else
		{
			spreadHorizontal = new Vector2(-1f, 1f);
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
		if (entityTurret.YawController != null)
		{
			entityTurret.YawController.Init(_properties);
		}
		if (entityTurret.PitchController != null)
		{
			entityTurret.PitchController.Init(_properties);
		}
		buffActions = new List<string>();
		if (_properties.Values.ContainsKey("Buff"))
		{
			string[] collection = _properties.Values["Buff"].Replace(" ", "").Split(',');
			buffActions.AddRange(collection);
		}
		damageMultiplier = new DamageMultiplier(_properties, null);
		sorter = new TurretEntitySorter(TurretPosition);
		state = TurretState.Asleep;
		if (Cone != null)
		{
			Cone.localScale = new Vector3(Cone.localScale.x * (yawRange.y / 22.5f) * (maxDistance / 5.25f), Cone.localScale.y * (pitchRange.y / 22.5f) * (maxDistance / 5.25f), Cone.localScale.z * (maxDistance / 5.25f));
			Cone.gameObject.SetActive(value: false);
		}
		if (Laser != null)
		{
			Laser.localScale = new Vector3(Laser.localScale.x, Laser.localScale.y, Laser.localScale.z * maxDistance);
			Laser.gameObject.SetActive(value: false);
		}
		entityTurret.transform.GetComponent<SphereCollider>().enabled = true;
		waterCollisionParticles.Init(entityTurret.belongsPlayerId, "bullet", "water", 16);
	}

	public virtual float GetRange(EntityAlive owner)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, entityTurret.OriginalItemValue, maxDistance, owner, null, default(FastTags<TagGroup.Global>), calcEquipment: true, calcHoldingItem: false);
	}

	public void Update()
	{
		if (entityTurret == null)
		{
			return;
		}
		if (!entityTurret.IsOn)
		{
			if (entityTurret.YawController.Yaw != CenteredYaw)
			{
				entityTurret.YawController.Yaw = CenteredYaw;
			}
			if (entityTurret.PitchController.Pitch != CenteredPitch + 35f)
			{
				entityTurret.PitchController.Pitch = CenteredPitch + 35f;
			}
			if (turretSpinAudioHandle != null)
			{
				turretSpinAudioHandle.Stop(entityTurret.entityId);
				turretSpinAudioHandle = null;
			}
			entityTurret.YawController.UpdateYaw();
			entityTurret.PitchController.UpdatePitch();
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!hasTarget)
			{
				findTarget();
			}
			else if (shouldIgnoreTarget(currentEntityTarget))
			{
				currentEntityTarget = null;
				entityTurret.TargetEntityId = -1;
			}
		}
		else if (entityTurret.TargetEntityId != -1)
		{
			currentEntityTarget = GameManager.Instance.World.GetEntity(entityTurret.TargetEntityId) as EntityAlive;
			if (currentEntityTarget == null || currentEntityTarget.IsDead())
			{
				currentEntityTarget = null;
				entityTurret.TargetEntityId = -1;
			}
		}
		else
		{
			currentEntityTarget = null;
			entityTurret.TargetEntityId = -1;
		}
		if (entityTurret.IsTurning)
		{
			if (turretSpinAudioHandle == null)
			{
				turretSpinAudioHandle = Manager.Play(entityTurret, targetingSound, 1f, wantHandle: true);
			}
		}
		else if (turretSpinAudioHandle != null)
		{
			turretSpinAudioHandle.Stop(entityTurret.entityId);
			turretSpinAudioHandle = null;
		}
		targetChestHeadDelay -= Time.deltaTime;
		if (targetChestHeadDelay <= 0f)
		{
			targetChestHeadDelay = 1f;
			targetChestHeadPercent = entityTurret.rand.RandomFloat;
		}
		burstFireRate += Time.deltaTime;
		if (hasTarget)
		{
			entityTurret.YawController.IdleScan = false;
			float _yaw = entityTurret.YawController.Yaw;
			float _pitch = entityTurret.PitchController.Pitch;
			if (trackTarget(currentEntityTarget, ref _yaw, ref _pitch, out var _))
			{
				entityTurret.YawController.Yaw = _yaw;
				entityTurret.PitchController.Pitch = _pitch;
				EntityAlive entity = GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityAlive;
				FastTags<TagGroup.Global> tags = entityTurret.OriginalItemValue.ItemClass.ItemTags | entityTurret.EntityClass.Tags;
				burstFireRateMax = 60f / (EffectManager.GetValue(PassiveEffects.RoundsPerMinute, entityTurret.OriginalItemValue, burstFireRateMax, entity, null, tags, calcEquipment: true, calcHoldingItem: false) + 1E-05f);
				if (burstFireRate >= burstFireRateMax)
				{
					Fire();
					burstFireRate = 0f;
				}
			}
		}
		else
		{
			if (!entityTurret.YawController.IdleScan || (entityTurret.YawController.Yaw != yawRange.y && entityTurret.YawController.Yaw != yawRange.x))
			{
				entityTurret.YawController.IdleScan = true;
				entityTurret.YawController.Yaw = yawRange.y;
			}
			float num = ((yawRange.y > 0f) ? yawRange.y : (360f + yawRange.y));
			float num2 = ((yawRange.x > 0f) ? yawRange.x : (360f + yawRange.x));
			if (Mathf.Abs(entityTurret.YawController.CurrentYaw - num) < 1f || Mathf.Abs(entityTurret.YawController.CurrentYaw - yawRange.y) < 1f)
			{
				entityTurret.YawController.Yaw = yawRange.x;
			}
			else if (Mathf.Abs(entityTurret.YawController.CurrentYaw - num2) < 1f || Mathf.Abs(entityTurret.YawController.CurrentYaw - yawRange.x) < 1f)
			{
				entityTurret.YawController.Yaw = yawRange.y;
			}
			entityTurret.PitchController.Pitch = CenteredPitch;
		}
		entityTurret.YawController.UpdateYaw();
		entityTurret.PitchController.UpdatePitch();
		if (Laser != null)
		{
			updateLaser();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findTarget()
	{
		Vector3 position = base.transform.position;
		if (Cone != null)
		{
			position = Cone.transform.position;
		}
		currentEntityTarget = null;
		entityTurret.TargetEntityId = -1;
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(TurretPosition, Vector3.one * (maxDistance * 2f + 1f)), new List<Entity>());
		entitiesInBounds.Sort(sorter);
		if (entitiesInBounds.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			Entity entity = entitiesInBounds[i];
			if (shouldIgnoreTarget(entity))
			{
				continue;
			}
			Vector3 _targetPos = Vector3.zero;
			float _yaw = CenteredYaw;
			float _pitch = CenteredPitch;
			if (!trackTarget(entity, ref _yaw, ref _pitch, out _targetPos))
			{
				continue;
			}
			Vector3 normalized = (_targetPos - position).normalized;
			if (!Voxel.Raycast(ray: new Ray(position + Origin.position - normalized * 0.05f, normalized), _world: GameManager.Instance.World, distance: maxDistance + 0.05f, _layerMask: -538750981, _hitMask: 8, _sphereRadius: 0.05f) || !Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
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
				if (component == entity)
				{
					currentEntityTarget = component as EntityAlive;
					entityTurret.TargetEntityId = currentEntityTarget.entityId;
					entityTurret.YawController.Yaw = _yaw;
					entityTurret.PitchController.Pitch = _pitch;
					break;
				}
				currentEntityTarget = null;
				entityTurret.TargetEntityId = -1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLaser()
	{
		float num = maxDistance;
		Ray ray = new Ray(Laser.transform.position + Origin.position, -Laser.transform.forward);
		if (Voxel.Raycast(GameManager.Instance.World, ray, num, 1082195968, 128, 0.25f))
		{
			num = Vector3.Distance(Voxel.voxelRayHitInfo.hit.pos - Origin.position, ray.origin - Origin.position);
		}
		Laser.transform.localScale = new Vector3(1f, 1f, num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target)
	{
		if (_target == null)
		{
			return true;
		}
		Vector3 forward = base.transform.forward;
		if (Cone != null)
		{
			forward = Cone.transform.forward;
		}
		if (Vector3.Dot(_target.position - entityTurret.position, forward) > 0f)
		{
			return true;
		}
		if (!_target.IsAlive())
		{
			return true;
		}
		if (_target.entityId == entityTurret.entityId)
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
			if (entityTurret.belongsPlayerId == _target.entityId)
			{
				flag = true;
			}
			if (!flag && persistentPlayerList.EntityToPlayerMap.ContainsKey(entityTurret.belongsPlayerId))
			{
				PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[entityTurret.belongsPlayerId];
				if (persistentPlayerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
				{
					PersistentPlayerData persistentPlayerData2 = persistentPlayerList.EntityToPlayerMap[_target.entityId];
					if (persistentPlayerData.ACL != null && persistentPlayerData2 != null && persistentPlayerData.ACL.Contains(persistentPlayerData2.PrimaryId))
					{
						flag2 = true;
					}
				}
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityPlayer;
				if (!flag2 && entityPlayer != null && entityPlayer.Party != null && entityPlayer.Party.ContainsMember(_target as EntityPlayer))
				{
					flag2 = true;
				}
			}
			if (enumPlayerKillingMode == EnumPlayerKillingMode.NoKilling)
			{
				return true;
			}
			if (flag && (!entityTurret.TargetOwner || enumPlayerKillingMode != EnumPlayerKillingMode.KillEveryone))
			{
				return true;
			}
			if (flag2 && (!entityTurret.TargetAllies || (enumPlayerKillingMode != EnumPlayerKillingMode.KillEveryone && enumPlayerKillingMode != EnumPlayerKillingMode.KillAlliesOnly)))
			{
				return true;
			}
			if (!flag && !flag2 && (!entityTurret.TargetStrangers || (enumPlayerKillingMode != EnumPlayerKillingMode.KillStrangersOnly && enumPlayerKillingMode != EnumPlayerKillingMode.KillEveryone)))
			{
				return true;
			}
		}
		if (_target is EntityNPC)
		{
			if (_target is EntityTrader)
			{
				return true;
			}
			if (!entityTurret.TargetStrangers)
			{
				return true;
			}
		}
		if (_target is EntityEnemy && !entityTurret.TargetEnemies)
		{
			return true;
		}
		if (_target is EntityTurret)
		{
			return true;
		}
		if (_target is EntityDrone)
		{
			return true;
		}
		if (_target is EntitySupplyCrate)
		{
			return true;
		}
		float _yaw = 0f;
		float _pitch = 0f;
		if (_target as EntityAlive != null && !canHitEntity(_target, ref _yaw, ref _pitch, out var _))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHitEntity(Entity _targetEntity, ref float _yaw, ref float _pitch, out Vector3 targetPos)
	{
		Vector3 position = base.transform.position;
		if (Cone != null)
		{
			position = Cone.transform.position;
		}
		if (!trackTarget(_targetEntity, ref _yaw, ref _pitch, out targetPos))
		{
			return false;
		}
		Ray ray = new Ray(position + Origin.position, (targetPos - position).normalized);
		if (Voxel.Raycast(GameManager.Instance.World, ray, maxDistance, -538750981, 8, 0f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			if (hitRootTransform == null)
			{
				return false;
			}
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component != null && component.IsAlive() && _targetEntity == component)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool trackTarget(Entity _target, ref float _yaw, ref float _pitch, out Vector3 _targetPos)
	{
		_targetPos = Vector3.Lerp(_target.getChestPosition(), _target.getHeadPosition(), targetChestHeadPercent) - Origin.position;
		Vector3 position = base.transform.position;
		if (Laser != null)
		{
			position = Laser.transform.position;
		}
		Vector3 eulerAngles = Quaternion.LookRotation((_targetPos - position).normalized).eulerAngles;
		float num = Mathf.DeltaAngle(entityTurret.transform.rotation.eulerAngles.y, eulerAngles.y);
		float num2 = eulerAngles.x;
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

	public virtual void Fire()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || entityTurret == null || !entityTurret.IsOn)
		{
			return;
		}
		Vector3 position = Laser.transform.position;
		EntityAlive entity = GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityAlive;
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		_ = entityTurret.OriginalItemValue.ItemClass.ItemTags;
		int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, entityTurret.OriginalItemValue, rayCount, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
		maxDistance = EffectManager.GetValue(PassiveEffects.MaxRange, entityTurret.OriginalItemValue, MaxDistance, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
		for (int i = 0; i < num; i++)
		{
			Vector3 forward = Muzzle.transform.forward;
			spreadHorizontal.x = 0f - EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, entityTurret.OriginalItemValue, 22f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false) * 0.5f;
			spreadHorizontal.y = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, entityTurret.OriginalItemValue, 22f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false) * 0.5f;
			spreadVertical.x = 0f - EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, entityTurret.OriginalItemValue, 22f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false) * 0.5f;
			spreadVertical.y = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, entityTurret.OriginalItemValue, 22f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false) * 0.5f;
			forward = Quaternion.Euler(gameRandom.RandomRange(spreadHorizontal.x, spreadHorizontal.y), gameRandom.RandomRange(spreadVertical.x, spreadVertical.y), 0f) * forward;
			Ray ray = new Ray(position + Origin.position, forward);
			waterCollisionParticles.Reset();
			waterCollisionParticles.CheckCollision(ray.origin, ray.direction, maxDistance);
			int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, entityTurret.OriginalItemValue, 0f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false));
			num2++;
			int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, entityTurret.OriginalItemValue, 1f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false));
			EntityAlive entityAlive = null;
			for (int j = 0; j < num2; j++)
			{
				if (!Voxel.Raycast(GameManager.Instance.World, ray, maxDistance, -538750981, 8, 0f))
				{
					continue;
				}
				WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
				if (worldRayHitInfo.tag.StartsWith("E_"))
				{
					string bodyPartName;
					EntityAlive entityAlive2 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out bodyPartName) as EntityAlive;
					if (entityAlive == entityAlive2)
					{
						ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
						j--;
						continue;
					}
					entityAlive = entityAlive2;
				}
				else
				{
					j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num3);
				}
				ItemActionAttack.Hit(worldRayHitInfo, entityTurret.belongsPlayerId, EnumDamageTypes.Piercing, GetDamageBlock(entityTurret.OriginalItemValue, BlockValue.Air, GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityAlive, 1), GetDamageEntity(entityTurret.OriginalItemValue, GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityAlive, 1), 1f, entityTurret.OriginalItemValue.PercentUsesLeft, 0f, 0f, "bullet", damageMultiplier, buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, entityTurret.entityId, entityTurret.OriginalItemValue);
			}
		}
		if (!string.IsNullOrEmpty(muzzleFireParticle))
		{
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect(muzzleFireParticle, Muzzle.position + Origin.position, Muzzle.rotation, 1f, Color.white, fireSound, null), -1);
		}
		if (!string.IsNullOrEmpty(muzzleSmokeParticle))
		{
			float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(TurretPosition)) / 2f;
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect(muzzleSmokeParticle, Muzzle.position + Origin.position, Muzzle.rotation, lightValue, new Color(1f, 1f, 1f, 0.3f), null, null), -1);
		}
		burstRoundCount++;
		if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, entityTurret.OriginalItemValue) > 0)
		{
			entityTurret.AmmoCount--;
		}
		entityTurret.OriginalItemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, entityTurret.OriginalItemValue, 1f, entity, null, entityTurret.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, entityDamage, _holdingEntity, null, _itemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = _itemValue.ItemClass.ItemTags;
		tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, blockDamage, _holdingEntity, null, tmpTag, calcEquipment: true, calcHoldingItem: false);
		return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}
}
