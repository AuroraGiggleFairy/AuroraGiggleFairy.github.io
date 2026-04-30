using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Entity : MonoBehaviour
{
	public struct MoveHitSurface
	{
		public Vector3 hitPoint;

		public Vector3 lastHitPoint;

		public Vector3 normal;

		public Vector3 lastNormal;
	}

	public enum EnumPositionUpdateMovementType
	{
		Lerp,
		MoveTowards,
		Instant
	}

	public enum StopAnimatorAudioType
	{
		StopOnReloadCancel = 1,
		StopOnStopHolding
	}

	public const int EntityIdInvalid = -1;

	public const int cIdCreatorIsServer = -2;

	public const int cClientIdStart = -2;

	public const int cClientIdCreate = -1;

	public const int cClientIdNone = 0;

	public const int cKillAnythingDamage = 99999;

	public const int cIgnoreDamage = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> cachedTags;

	public bool RootMotion;

	public bool HasDeathAnim;

	public World world;

	public Transform PhysicsTransform;

	public Transform RootTransform;

	public Transform ModelTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 scaledExtent;

	public Bounds boundingBox;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider nativeCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int interpolateTargetRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int interpolateTargetQRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUpdatePosition;

	public int entityId;

	public int clientEntityId;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float yOffset;

	public bool onGround;

	public bool isCollided;

	public bool isCollidedHorizontally;

	public bool isCollidedVertically;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isMotionSlowedDown;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float motionMultiplier;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate = true;

	public Vector3 prevRotation;

	public Vector3 rotation;

	public Quaternion qrotation = Quaternion.identity;

	public Vector3 position;

	public Vector3 prevPos;

	public Vector3 targetPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 targetRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion targetQRot = Quaternion.identity;

	public Vector3i chunkPosAddedEntityTo;

	public Vector3i serverPos;

	public Vector3i serverRot;

	public Vector3[] lastTickPos = new Vector3[5];

	public Vector3 motion;

	public bool IsMovementReplicated = true;

	public bool IsStuck;

	public bool addedToChunk;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInWater;

	public bool isSwimming;

	public float inWaterLevel;

	public float inWaterPercent;

	public bool isHeadUnderwater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bInElevator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bAirBorne;

	public float stepHeight;

	public float ySize;

	public float distanceWalked;

	public float distanceSwam;

	public float distanceClimbed;

	public float fallDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallLastY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallVelY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 fallLastMotion;

	public float entityCollisionReduction = 0.9f;

	public bool isEntityRemote;

	public GameRandom rand;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int ticksExisted;

	public static float updatePositionLerpTimeScale = 8f;

	public static float updateRotationLerpTimeScale = 8f;

	public static float tickPositionMoveTowardsMaxDistance = 3f;

	public static float tickPositionLerpMultiplier = 0.5f;

	public int entityClass;

	public float lifetime;

	public int count;

	public int belongsPlayerId;

	public bool bWillRespawn;

	public ulong WorldTimeBorn;

	public DataItem<bool> IsFlyMode = new DataItem<bool>();

	public DataItem<bool> IsGodMode = new DataItem<bool>();

	public DataItem<bool> IsNoCollisionMode = new DataItem<bool>();

	public EntityFlags entityFlags;

	public EntityType entityType;

	public float lootDropProb;

	public string lootListOnDeath;

	public string lootListAlive;

	public TileEntityLootContainer lootContainer;

	public float speedForward;

	public float speedStrafe;

	public float speedVertical;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int speedSentTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedForwardSent = float.MaxValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedStrafeSent = float.MaxValue;

	public int MovementState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float yawSeekTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float yawSeekTimeMax;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float yawSeekAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float yawSeekAngleEnd;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IAIDirectorMarker m_marker;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string mapIcon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string compassIcon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string compassUpIcon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string compassDownIcon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string trackerIcon;

	public bool bDead;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWasDead;

	[Preserve]
	public EModelBase emodel;

	public CharacterControllerAbstract m_characterController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCCDelayed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canCCMove;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CollisionFlags collisionFlags;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MoveHitSurface groundSurface;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hitMove;

	public float projectedMove;

	public bool IsRotateToGroundFlat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRotateToGround;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotateToGroundPitch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rotateToGroundPitchVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EnumSpawnerSource spawnerSource;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnerSourceBiomeIdHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long spawnerSourceChunkKey;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityActivationCommand[] cmds = new EntityActivationCommand[1]
	{
		new EntityActivationCommand("Search", "search", _enabled: true)
	};

	public static int InstanceCount;

	public bool IsDespawned;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool markedForUnload;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MovableSharedChunkObserver movableChunkObserver;

	public bool bIsChunkObserver;

	public NavObject NavObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isIgnoredByAI;

	public Entity AttachedToEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity[] attachedEntities;

	public const int cPhysicsMasterTickRate = 2;

	public bool usePhysicsMaster;

	public bool isPhysicsMaster;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsMasterFromPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion physicsMasterFromRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physicsMasterTargetElapsed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physicsMasterTargetTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsMasterTargetPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion physicsMasterTargetRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsMasterSendPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion physicsMasterSendRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physicsHeightScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform physicsRBT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CapsuleCollider physicsCapsuleCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float physicsColliderRadius;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float physicsColliderLowerY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float physicsBaseHeight;

	public float physicsHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody physicsRB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsBasePos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsTargetPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float physicsPosMoveDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion physicsRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasFixedUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsAngVel;

	public bool spawnByAllowShare;

	public int spawnById = -1;

	public string spawnByName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityActivationCommand[] customCmds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaterHeightScale = 1.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] waterLevelDirOffsets = new float[6]
	{
		Mathf.Cos(0f),
		Mathf.Sin(0f),
		Mathf.Cos(MathF.PI * 2f / 3f),
		Mathf.Sin(MathF.PI * 2f / 3f),
		Mathf.Cos(4.1887903f),
		Mathf.Sin(4.1887903f)
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Bounds> collAABB = new List<Bounds>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float kAddFixedUpdateTimeScale = 1f;

	public EnumRemoveEntityReason unloadReason;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnloaded;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cAttachSlotNone = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[] adjacentPositions = new Vector3i[4]
	{
		Vector3i.forward,
		Vector3i.back,
		Vector3i.left,
		Vector3i.right
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<StopAnimatorAudioType, Handle> animatorAudioMonitoringDictionary = new Dictionary<StopAnimatorAudioType, Handle>();

	public FastTags<TagGroup.Global> EntityTags => cachedTags;

	public EntityClass EntityClass
	{
		get
		{
			EntityClass.list.TryGetValue(entityClass, out var _value);
			return _value;
		}
	}

	public EntityActivationCommand[] CustomCmds
	{
		get
		{
			if (customCmds == null)
			{
				EntityClass entityClass = EntityClass.list[this.entityClass];
				int num = 0;
				for (int i = 1; i <= 10 && entityClass.Properties.Values.ContainsKey($"{EntityClass.PropCustomCommandName}{i}"); i++)
				{
					num++;
				}
				customCmds = new EntityActivationCommand[num];
				if (num > 0)
				{
					for (int j = 1; j <= num; j++)
					{
						if (entityClass.Properties.Values.ContainsKey($"{EntityClass.PropCustomCommandName}{j}"))
						{
							EntityActivationCommand entityActivationCommand = new EntityActivationCommand
							{
								text = entityClass.Properties.Values[$"{EntityClass.PropCustomCommandName}{j}"],
								icon = entityClass.Properties.Values[$"{EntityClass.PropCustomCommandIcon}{j}"],
								eventName = entityClass.Properties.Values[$"{EntityClass.PropCustomCommandEvent}{j}"],
								enabled = true
							};
							string text = $"{EntityClass.PropCustomCommandIconColor}{j}";
							if (EntityClass.Properties.Values.ContainsKey(text))
							{
								entityActivationCommand.iconColor = StringParsers.ParseHexColor(EntityClass.Properties.Values[text]);
							}
							else
							{
								entityActivationCommand.iconColor = Color.white;
							}
							text = $"{EntityClass.PropCustomCommandActivateTime}{j}";
							if (entityClass.Properties.Values.ContainsKey(text))
							{
								entityActivationCommand.activateTime = StringParsers.ParseFloat(entityClass.Properties.Values[text]);
							}
							else
							{
								entityActivationCommand.activateTime = -1f;
							}
							customCmds[j - 1] = entityActivationCommand;
						}
					}
				}
			}
			return customCmds;
		}
	}

	public virtual string LocalizedEntityName => Localization.Get(EntityClass.list[entityClass].entityClassName);

	public virtual EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return EnumPositionUpdateMovementType.Lerp;
		}
	}

	public float width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return scaledExtent.x * 2f;
		}
	}

	public float depth
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return scaledExtent.z * 2f;
		}
	}

	public float height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return scaledExtent.y * 2f;
		}
	}

	public float radius
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return scaledExtent.x;
		}
	}

	public Entity AttachedMainEntity
	{
		get
		{
			if (attachedEntities == null)
			{
				return null;
			}
			return attachedEntities[0];
		}
	}

	public static bool CheckDistance(int entityID_A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		if (entity == null)
		{
			return false;
		}
		Entity entity2 = GameManager.Instance.World.GetEntity(entityID_B);
		if (entity2 == null)
		{
			return false;
		}
		return CheckDistance(entity, entity2);
	}

	public static bool CheckDistance(Entity entityB, int entityID_A)
	{
		return CheckDistance(entityID_A, entityB);
	}

	public static bool CheckDistance(int entityID_A, Entity entityB)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		if (entityB == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		if (entity == null)
		{
			return false;
		}
		return CheckDistance(entity, entityB);
	}

	public static bool CheckDistance(Entity A, Vector3 B)
	{
		if (A == null)
		{
			return false;
		}
		return CheckDistance(A.transform.position, B);
	}

	public static bool CheckDistance(Vector3 A, Entity B)
	{
		if (B == null)
		{
			return false;
		}
		return CheckDistance(A, B.transform.position);
	}

	public static bool CheckDistance(Vector3 A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_B);
		if (entity == null)
		{
			return false;
		}
		return CheckDistance(A - Origin.position, entity.transform.position);
	}

	public static bool CheckDistance(Vector3 A, Vector3 B)
	{
		return (A - B).magnitude < 256f;
	}

	public static bool CheckDistance(Entity listenerEntity, Entity sourceEntity)
	{
		return CheckDistance(sourceEntity.transform.position, listenerEntity.transform.position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		InstanceCount++;
		world = GameManager.Instance.World;
		isEntityRemote = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		WorldTimeBorn = world.worldTime;
		rand = world.GetGameRandom();
		SetupBounds();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~Entity()
	{
		InstanceCount--;
	}

	public virtual void OnXMLChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupBounds()
	{
		CharacterController component2;
		if (TryGetComponent<BoxCollider>(out var component))
		{
			nativeCollider = component;
			Vector3 localScale = base.transform.localScale;
			scaledExtent = Vector3.Scale(component.size, localScale) * 0.5f;
			Vector3 vector = Vector3.Scale(component.center, localScale);
			boundingBox = BoundsUtils.BoundsForMinMax(-scaledExtent, scaledExtent);
			boundingBox.center += vector;
			if (isDetailedHeadBodyColliders())
			{
				component.enabled = false;
			}
		}
		else if (TryGetComponent<CharacterController>(out component2))
		{
			Vector3 localScale2 = base.transform.localScale;
			float num = component2.radius;
			scaledExtent = new Vector3(num * localScale2.x, component2.height * localScale2.y * 0.5f, num * localScale2.z);
			boundingBox = BoundsUtils.BoundsForMinMax(-scaledExtent, scaledExtent);
		}
		else
		{
			boundingBox = BoundsUtils.BoundsForMinMax(Vector3.zero, Vector3.one);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		bWasDead = IsDead();
		animateYaw();
		if (physicsMasterTargetTime > 0f)
		{
			PhysicsMasterTargetFrameUpdate();
		}
		else
		{
			updateTransform();
		}
		if (bIsChunkObserver && !isEntityRemote)
		{
			if (movableChunkObserver == null)
			{
				movableChunkObserver = new MovableSharedChunkObserver(world.m_SharedChunkObserverCache);
			}
			movableChunkObserver.SetPosition(position);
		}
		else if (!bIsChunkObserver && movableChunkObserver != null)
		{
			movableChunkObserver.Dispose();
			movableChunkObserver = null;
		}
		if (animatorAudioMonitoringDictionary.Count <= 0)
		{
			return;
		}
		List<StopAnimatorAudioType> list = new List<StopAnimatorAudioType>();
		foreach (KeyValuePair<StopAnimatorAudioType, Handle> item in animatorAudioMonitoringDictionary)
		{
			if (!item.Value.IsPlaying())
			{
				item.Value.Stop(entityId);
				list.Add(item.Key);
			}
		}
		foreach (StopAnimatorAudioType item2 in list)
		{
			animatorAudioMonitoringDictionary.Remove(item2);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateTransform()
	{
		if (AttachedToEntity != null)
		{
			return;
		}
		ApplyFixedUpdate();
		if (!emodel || !emodel.IsRagdollOn)
		{
			float y;
			if ((bool)physicsRB)
			{
				Vector3 b = physicsRBT.position - physicsBasePos;
				Vector3 vector = Vector3.Lerp(base.transform.position, b, physicsPosMoveDistance * Time.deltaTime / Time.fixedDeltaTime);
				base.transform.position = vector;
				y = physicsRBT.eulerAngles.y;
			}
			else
			{
				Vector3 b2 = position - Origin.position;
				base.transform.position = Vector3.Lerp(base.transform.position, b2, Time.deltaTime * updatePositionLerpTimeScale);
				y = rotation.y;
			}
			if (isRotateToGround)
			{
				Vector3 vector2 = groundSurface.normal;
				float num = Vector3.Dot(vector2, Vector3.up);
				if (IsRotateToGroundFlat)
				{
					num = 1f;
				}
				if (num > 0.99f || num < 0.7f)
				{
					vector2 = Vector3.up;
				}
				Vector3 vector3 = Quaternion.AngleAxis(0f - y, Vector3.up) * vector2;
				float target = 90f - Mathf.Atan2(vector3.y, vector3.z) * 57.29578f;
				rotateToGroundPitchVel *= 0.86f;
				rotateToGroundPitchVel += Mathf.DeltaAngle(rotateToGroundPitch, target) * 0.8f * Time.deltaTime;
				rotateToGroundPitch += rotateToGroundPitchVel;
				base.transform.eulerAngles = new Vector3(rotateToGroundPitch, y, 0f);
			}
			else
			{
				base.transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(base.transform.eulerAngles.y, y, Time.deltaTime * updateRotationLerpTimeScale), 0f);
			}
		}
		if (isEntityRemote && PhysicsTransform != null)
		{
			PhysicsTransform.position = Vector3.Lerp(PhysicsTransform.position, position - Origin.position, Time.deltaTime * updateRotationLerpTimeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		ApplyFixedUpdate();
		wasFixedUpdate = true;
		if (!physicsRB)
		{
			return;
		}
		physicsRB.velocity *= 0.9f;
		physicsRB.angularVelocity *= 0.9f;
		Transform transform = physicsRBT;
		Vector3 b = physicsTargetPos + physicsBasePos;
		Vector3 vector = (physicsPos = Vector3.Lerp(transform.position, b, 0.4f));
		physicsRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, rotation.y, 0f), 0.3f);
		transform.SetPositionAndRotation(vector, physicsRot);
		if ((bool)physicsCapsuleCollider)
		{
			EntityAlive entityAlive = this as EntityAlive;
			if ((bool)entityAlive)
			{
				entityAlive.CrouchHeightFixedUpdate();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyFixedUpdate()
	{
		if (!wasFixedUpdate)
		{
			return;
		}
		wasFixedUpdate = false;
		if ((bool)physicsRB)
		{
			Transform transform = physicsRBT;
			Vector3 vector = transform.position;
			if ((vector - physicsPos).sqrMagnitude > 0.0001f)
			{
				_ = position;
				Vector3 vector2 = vector - physicsBasePos;
				physicsPos = vector;
				SetPosition(vector2 + Origin.position, _bUpdatePhysics: false);
				PhysicsTransform.position = vector2;
			}
			physicsPosMoveDistance = Vector3.Distance(physicsPos, base.transform.position);
			if (Mathf.Abs(Quaternion.Angle(transform.rotation, physicsRot)) > 0.1f)
			{
				Quaternion quaternion = (physicsRot = transform.rotation);
				rotation = quaternion.eulerAngles;
				qrotation = quaternion;
			}
		}
	}

	public virtual void OriginChanged(Vector3 _deltaPos)
	{
		physicsPos += _deltaPos;
		physicsTargetPos += _deltaPos;
		if ((bool)emodel)
		{
			emodel.OriginChanged(_deltaPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddCharacterController()
	{
		if (!PhysicsTransform)
		{
			return;
		}
		float num = 0.08f;
		Vector3 center = Vector3.zero;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		GameObject gameObject = PhysicsTransform.gameObject;
		if (this is EntityPlayer)
		{
			CharacterController component = gameObject.GetComponent<CharacterController>();
			if (!component)
			{
				Log.Error("Player !cc");
				return;
			}
			center = component.center;
			num2 = component.height;
			num3 = component.radius;
			m_characterController = new CharacterControllerUnity(component);
			if (!isEntityRemote)
			{
				gameObject.AddComponent<ColliderHitCallForward>().Entity = this;
			}
			BoxCollider boxCollider = nativeCollider as BoxCollider;
			if ((bool)boxCollider)
			{
				num2 = Utils.FastMax(boxCollider.size.y - num, stepHeight);
				center = boxCollider.center;
				center.y = num2 * 0.5f;
				if (boxCollider.size.x > boxCollider.size.y)
				{
					center.y += (boxCollider.size.x - boxCollider.size.y) * 0.5f;
				}
				num3 = boxCollider.size.x * 0.5f - num;
				flag = true;
			}
		}
		else
		{
			num = 0f;
			flag = true;
			CapsuleCollider component2 = gameObject.GetComponent<CapsuleCollider>();
			if ((bool)component2)
			{
				center = component2.center;
				num2 = component2.height;
				num3 = component2.radius;
			}
			else
			{
				gameObject.AddComponent<CapsuleCollider>();
				center.y = 0.9f;
				num2 = 1.8f;
				num3 = 0.3f;
			}
			if ((bool)physicsCapsuleCollider)
			{
				num2 = physicsBaseHeight;
				center.y = num2 * 0.5f;
			}
			if (gameObject.TryGetComponent<CharacterController>(out var component3))
			{
				center = component3.center;
				num2 = component3.height;
				num3 = component3.radius;
				UnityEngine.Object.Destroy(component3);
				Log.Warning("{0} has old CC", ToString());
			}
			m_characterController = new CharacterControllerKinematic(this);
		}
		if (num2 <= 0f)
		{
			return;
		}
		if (flag)
		{
			center.y /= physicsHeightScale;
			m_characterController.SetSize(center, num2 / physicsHeightScale, num3);
			physicsBaseHeight = num2;
			physicsHeight = num2;
			if ((bool)physicsCapsuleCollider)
			{
				PhysicsSetHeight(num2);
			}
		}
		m_characterController.SetStepOffset(stepHeight);
		Vector3 localScale = base.transform.localScale;
		scaledExtent = new Vector3(num3 * localScale.x, num2 * localScale.y * 0.5f, num3 * localScale.z);
		boundingBox = BoundsUtils.BoundsForMinMax(-scaledExtent, scaledExtent);
		if ((bool)nativeCollider)
		{
			nativeCollider.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCCScale(float scale)
	{
		CharacterControllerAbstract characterController = m_characterController;
		if (characterController != null)
		{
			PhysicsTransform.localScale = Vector3.one;
			Vector3 center = characterController.GetCenter() * scale;
			float num = characterController.GetHeight() * scale;
			if (num < 2.2f && num > 1.89f)
			{
				num = 1.89f;
				center.y = num * 0.5f;
			}
			float num2 = Utils.FastMax(scale, 1f);
			characterController.SetSize(center, num, characterController.GetRadius() * num2);
		}
	}

	public virtual void Init(int _entityClass)
	{
		entityClass = _entityClass;
		InitCommon();
		InitEModel();
		PhysicsInit();
	}

	public virtual void InitFromPrefab(int _entityClass)
	{
		entityClass = _entityClass;
		InitCommon();
		InitEModelFromPrefab();
		PhysicsInit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitCommon()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		cachedTags = entityClass.Tags;
		bIsChunkObserver = entityClass.bIsChunkObserver;
		CopyPropertiesFromEntityClass();
		if ((bool)PhysicsTransform)
		{
			PhysicsTransform.gameObject.tag = "Physics";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEModel()
	{
		Type modelType = EntityClass.list[entityClass].modelType;
		emodel = base.gameObject.AddComponent(modelType) as EModelBase;
		emodel.Init(world, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEModelFromPrefab()
	{
		Type modelType = EntityClass.list[entityClass].modelType;
		emodel = base.gameObject.GetComponent(modelType) as EModelBase;
		emodel.InitFromPrefab(world, this);
	}

	public virtual void PostInit()
	{
		if (emodel != null)
		{
			emodel.PostInit();
			HandleNavObject();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsInit()
	{
		Transform transform = GameUtils.FindTagInChilds(ModelTransform, "Physics");
		if ((bool)transform)
		{
			PhysicsTransform = RootTransform.Find("Physics");
			if ((bool)PhysicsTransform)
			{
				UnityEngine.Object.Destroy(PhysicsTransform.gameObject);
				Log.Warning("{0} has old Physics", ToString());
			}
			PhysicsTransform = transform;
			transform.SetParent(RootTransform, worldPositionStays: false);
		}
		else if (!PhysicsTransform)
		{
			PhysicsTransform = RootTransform.Find("Physics");
		}
		physicsRBT = GameUtils.FindTagInChilds(RootTransform, "LargeEntityBlocker");
		if (!physicsRBT)
		{
			return;
		}
		Transform transform2 = base.transform;
		Transform parent = physicsRBT.parent;
		physicsPos = physicsRBT.position;
		physicsRot = transform2.rotation;
		if (parent != transform2.parent)
		{
			Vector3 localPosition = physicsRBT.localPosition;
			float x = parent.lossyScale.x;
			localPosition += parent.localPosition * (1f / x);
			Collider[] componentsInChildren = physicsRBT.GetComponentsInChildren<Collider>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				Collider collider = componentsInChildren[num];
				CapsuleCollider capsuleCollider;
				BoxCollider boxCollider;
				SphereCollider sphereCollider;
				if ((bool)(capsuleCollider = collider as CapsuleCollider))
				{
					capsuleCollider.center = (capsuleCollider.center + localPosition) * x;
					capsuleCollider.height *= x;
					capsuleCollider.radius *= x;
				}
				else if ((bool)(boxCollider = collider as BoxCollider))
				{
					boxCollider.center = (boxCollider.center + localPosition) * x;
					boxCollider.size *= x;
				}
				else if ((bool)(sphereCollider = collider as SphereCollider))
				{
					sphereCollider.center = (sphereCollider.center + localPosition) * x;
					sphereCollider.radius *= x;
				}
			}
			physicsBasePos = Vector3.zero;
			physicsRBT.SetParent(transform2.parent, worldPositionStays: true);
			physicsRBT.localScale = Vector3.one;
		}
		else
		{
			physicsBasePos = Vector3.Scale(physicsRBT.localPosition, parent.lossyScale);
		}
		physicsRB = physicsRBT.gameObject.AddComponent<Rigidbody>();
		physicsRB.useGravity = false;
		float v = EntityClass.list[entityClass].MassKg * 0.6f;
		physicsRB.mass = Utils.FastMax(30f, v);
		physicsRB.constraints = (RigidbodyConstraints)80;
		physicsRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		physicsTargetPos = physicsPos;
		CapsuleCollider component = physicsRBT.GetComponent<CapsuleCollider>();
		if ((bool)component && component.direction == 1)
		{
			physicsCapsuleCollider = component;
			physicsColliderRadius = component.radius;
			physicsHeightScale = 1.09f;
			float num2 = component.height;
			float y = component.center.y;
			float num3 = y + num2 * 0.5f;
			physicsBaseHeight = num3 * physicsHeightScale;
			physicsColliderLowerY = y - num2 * 0.5f;
			if ((double)physicsBaseHeight > 1.95)
			{
				physicsBaseHeight = 1.95f;
			}
		}
	}

	public void PhysicsSetRB(Rigidbody rb)
	{
		physicsRB = rb;
	}

	public void PhysicsPause()
	{
		if ((bool)physicsRBT)
		{
			physicsRBT.gameObject.SetActive(value: false);
		}
	}

	public virtual void PhysicsResume(Vector3 pos, float rotY)
	{
		rotation = new Vector3(0f, rotY, 0f);
		if ((bool)physicsRBT)
		{
			physicsRBT.gameObject.SetActive(value: true);
			physicsRBT.eulerAngles = rotation;
			physicsPosMoveDistance = 0f;
		}
		SetPosition(pos);
		base.transform.SetPositionAndRotation(pos - Origin.position, Quaternion.Euler(rotation));
	}

	public virtual void PhysicsPush(Vector3 forceVec, Vector3 forceWorldPos, bool affectLocalPlayerController = false)
	{
		if (!(forceVec.sqrMagnitude > 0f))
		{
			return;
		}
		Rigidbody rigidbody = physicsRB;
		if ((bool)rigidbody)
		{
			if (!emodel.IsRagdollActive)
			{
				forceVec *= 5f;
			}
			if (forceWorldPos.sqrMagnitude > 0f)
			{
				rigidbody.AddForceAtPosition(forceVec, forceWorldPos - Origin.position, ForceMode.Impulse);
			}
			else
			{
				rigidbody.AddForce(forceVec, ForceMode.Impulse);
			}
		}
	}

	public void PhysicsSetHeight(float _height)
	{
		physicsHeight = _height;
		float num = physicsColliderLowerY;
		if (_height - num < physicsColliderRadius)
		{
			num = _height - physicsColliderRadius;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		physicsCapsuleCollider.height = _height - num;
		Vector3 center = physicsCapsuleCollider.center;
		center.y = (_height + num) * 0.5f;
		if (center.y < physicsColliderRadius)
		{
			center.y = physicsColliderRadius;
		}
		physicsCapsuleCollider.center = center;
	}

	public virtual void PhysicsMasterBecome()
	{
		isPhysicsMaster = true;
		physicsMasterTargetTime = 0f;
		SetPosition(physicsMasterTargetPos, _bUpdatePhysics: false);
		qrotation = physicsMasterTargetRot;
		if ((bool)physicsRB)
		{
			physicsRB.position = position - Origin.position;
			physicsRB.rotation = qrotation;
			physicsRB.velocity = physicsVel;
			physicsRB.angularVelocity = physicsAngVel;
		}
	}

	public NetPackageEntityPhysics PhysicsMasterSetupBroadcast()
	{
		if ((position - physicsMasterSendPos).sqrMagnitude < 0.0025000002f && Quaternion.Angle(qrotation, physicsMasterSendRot) < 1f)
		{
			return null;
		}
		physicsMasterSendPos = position;
		physicsMasterSendRot = qrotation;
		return NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
	}

	public void PhysicsMasterSendToServer(Transform t)
	{
		if (clientEntityId == 0)
		{
			position = t.position + Origin.position;
			qrotation = t.rotation;
			if (GetVelocityPerSecond().sqrMagnitude < 0.16000001f)
			{
				isPhysicsMaster = false;
			}
			NetPackageEntityPhysics package = NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public Vector3 PhysicsMasterGetFinalPosition()
	{
		if (physicsMasterTargetTime > 0f)
		{
			return physicsMasterTargetPos;
		}
		return position;
	}

	public void PhysicsMasterSetTargetOrientation(Vector3 pos, Quaternion rot)
	{
		physicsMasterFromPos = position;
		physicsMasterFromRot = qrotation;
		physicsMasterTargetElapsed = 0f;
		physicsMasterTargetTime = 0.1f;
		physicsMasterTargetPos = pos;
		physicsMasterTargetRot = rot;
	}

	public void PhysicsMasterTargetFrameUpdate()
	{
		physicsMasterTargetElapsed += Time.deltaTime;
		float t = physicsMasterTargetElapsed / physicsMasterTargetTime;
		Vector3 vector = Vector3.Lerp(physicsMasterFromPos, physicsMasterTargetPos, t);
		SetPosition(vector);
		Quaternion quaternion = (qrotation = Quaternion.Lerp(physicsMasterFromRot, physicsMasterTargetRot, t));
		physicsRB.position = vector - Origin.position;
		physicsRB.rotation = quaternion;
		if (physicsMasterTargetElapsed >= physicsMasterTargetTime)
		{
			physicsMasterTargetTime = 0f;
		}
	}

	public void SetHeight(float _height)
	{
		m_characterController.SetHeight(_height / physicsHeightScale);
		PhysicsSetHeight(_height);
	}

	public void SetMaxHeight(float _maxHeight)
	{
		physicsBaseHeight = _maxHeight;
		if (m_characterController != null)
		{
			m_characterController.SetHeight(_maxHeight / physicsHeightScale);
		}
		if ((bool)physicsCapsuleCollider)
		{
			PhysicsSetHeight(_maxHeight);
			float y = physicsCapsuleCollider.center.y;
			float num = physicsCapsuleCollider.height * 0.5f;
			physicsBaseHeight = y + num;
			physicsColliderLowerY = y - num;
		}
	}

	public void SetScale(float scale)
	{
		Vector3 localScale = new Vector3(scale, scale, scale);
		ModelTransform.localScale = localScale;
		CharacterJoint[] componentsInChildren = ModelTransform.GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint characterJoint in componentsInChildren)
		{
			if (characterJoint.autoConfigureConnectedAnchor)
			{
				characterJoint.autoConfigureConnectedAnchor = false;
				characterJoint.autoConfigureConnectedAnchor = true;
			}
		}
		if ((bool)physicsRBT)
		{
			physicsBaseHeight *= scale;
			physicsHeight *= scale;
			physicsColliderLowerY *= scale;
			Collider[] componentsInChildren2 = physicsRBT.GetComponentsInChildren<Collider>();
			for (int num = componentsInChildren2.Length - 1; num >= 0; num--)
			{
				Collider collider = componentsInChildren2[num];
				if (collider is CapsuleCollider capsuleCollider)
				{
					capsuleCollider.center *= scale;
					capsuleCollider.height *= scale;
					capsuleCollider.radius *= scale;
				}
				else if (collider is BoxCollider boxCollider)
				{
					boxCollider.center *= scale;
					boxCollider.size *= scale;
				}
				else if (collider is SphereCollider sphereCollider)
				{
					sphereCollider.center *= scale;
					sphereCollider.radius *= scale;
				}
			}
		}
		SetCCScale(scale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ReplicateSpeeds()
	{
		if (--speedSentTicks > 0)
		{
			return;
		}
		float num = speedForward - speedForwardSent;
		float num2 = speedStrafe - speedStrafeSent;
		if (num * num + num2 * num2 >= 4.0000004E-06f)
		{
			speedSentTicks = 3;
			speedForwardSent = speedForward;
			speedStrafeSent = speedStrafe;
			if (world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this));
			}
			else
			{
				world.entityDistributer.SendPacketToTrackedPlayers(entityId, entityId, NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetMovementState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void animateYaw()
	{
		if (yawSeekTimeMax > 0f)
		{
			yawSeekTime += Time.deltaTime;
			if (yawSeekTime < yawSeekTimeMax)
			{
				rotation.y = Mathf.Lerp(yawSeekAngle, yawSeekAngleEnd, yawSeekTime / yawSeekTimeMax);
				return;
			}
			yawSeekTimeMax = 0f;
			rotation.y = yawSeekAngleEnd;
		}
	}

	public void SeekYawToPos(Vector3 _pos, float _yawSlowAt)
	{
		float num = _pos.x - position.x;
		float num2 = _pos.z - position.z;
		if (num * num + num2 * num2 > 0.0001f)
		{
			float yaw = Mathf.Atan2(num, num2) * 57.29578f;
			SeekYaw(yaw, 0f, _yawSlowAt);
		}
	}

	public float SeekYaw(float yaw, float _, float yawSlowAt)
	{
		if (yaw < 0f)
		{
			yaw += 360f;
		}
		if (yaw > 360f)
		{
			yaw -= 360f;
		}
		if (rotation.y < 0f)
		{
			rotation.y += 360f;
		}
		if (rotation.y > 360f)
		{
			rotation.y -= 360f;
		}
		float num = EntityClass.list[entityClass].MaxTurnSpeed;
		if (inWaterPercent > 0.3f)
		{
			num *= 1f - inWaterPercent * 0.5f;
		}
		if (num > 0f)
		{
			float num2 = yaw - rotation.y;
			if (num2 != 0f)
			{
				if (num2 < -180f)
				{
					num2 += 360f;
				}
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float num3 = Utils.FastAbs(num2);
				if (num3 < yawSlowAt)
				{
					float num4 = num3 / yawSlowAt;
					num = num * num4 * num4;
					num = Utils.FastMax(num, 20f);
				}
				yawSeekTime = 0f;
				yawSeekTimeMax = num3 / num;
				yawSeekAngle = rotation.y;
				yawSeekAngleEnd = rotation.y + num2;
				return num2;
			}
		}
		rotation.y = yaw;
		yawSeekTimeMax = 0f;
		return 0f;
	}

	public virtual void KillLootContainer()
	{
		Kill(DamageResponse.New(_fatal: true));
	}

	public virtual void Kill(DamageResponse _dmResponse)
	{
		SetDead();
		if (attachedEntities == null)
		{
			return;
		}
		for (int i = 0; i < attachedEntities.Length; i++)
		{
			Entity entity = attachedEntities[i];
			if (entity != null)
			{
				entity.Kill(_dmResponse);
				entity.Detach();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickInWater()
	{
		inWaterLevel = CalcWaterLevel();
		inWaterPercent = inWaterLevel / (GetHeight() * 1.1f);
		isInWater = inWaterPercent >= 0.25f;
		bool flag = isSwimming;
		isSwimming = CalcIfSwimming();
		if (isSwimming != flag)
		{
			SwimChanged();
		}
		bool flag2 = isHeadUnderwater;
		isHeadUnderwater = IsHeadUnderwater();
		if (isHeadUnderwater != flag2)
		{
			OnHeadUnderwaterStateChanged(isHeadUnderwater);
		}
	}

	public float CalcWaterLevel()
	{
		float num = GetHeight() * 1.1f;
		int num2 = Utils.Fastfloor(position.y + num);
		int num3 = Utils.Fastfloor(position.y);
		int num4 = num2 - num3 + 1;
		int num5 = Utils.Fastfloor(position.x);
		int num6 = Utils.Fastfloor(position.z);
		Vector3i pos = default(Vector3i);
		for (int i = -2; i < 6; i += 2)
		{
			if (i < 0)
			{
				pos.x = num5;
				pos.z = num6;
			}
			else
			{
				pos.x = Utils.Fastfloor(position.x + waterLevelDirOffsets[i] * 0.28f);
				pos.z = Utils.Fastfloor(position.z + waterLevelDirOffsets[i + 1] * 0.28f);
				if (pos.x == num5 && pos.z == num6)
				{
					continue;
				}
			}
			pos.y = num2;
			int num7 = num4;
			do
			{
				float num8 = world.GetWaterPercent(pos);
				if (num8 > 0f)
				{
					if (num7 == num4)
					{
						pos.y++;
						if (world.GetWaterPercent(pos) == 0f)
						{
							num8 = 0.6f;
						}
						pos.y--;
					}
					else
					{
						num8 = 0.6f;
					}
					return Mathf.Clamp((float)pos.y + num8 - position.y, 0f, num);
				}
				pos.y--;
			}
			while (--num7 > 0);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CalcIfSwimming()
	{
		return inWaterPercent >= 0.5f;
	}

	public virtual void SwimChanged()
	{
	}

	public virtual bool IsHeadUnderwater()
	{
		return inWaterPercent >= 0.9f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		if (!_bUnderwater)
		{
			Manager.Play(this, "water_emerge");
		}
	}

	public virtual void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.normal.y > 0.707f && hit.normal.y > groundSurface.normal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - groundSurface.lastHitPoint).sqrMagnitude > 0.001 || groundSurface.lastNormal == Vector3.zero)
			{
				groundSurface.normal = hit.normal;
			}
			else
			{
				groundSurface.normal = groundSurface.lastNormal;
			}
			groundSurface.hitPoint = hit.point;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollision(Vector3 _vel)
	{
		canCCMove = true;
		ccEntityCollisionStart(_vel);
		if (!isCCDelayed)
		{
			ccEntityCollisionResults();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionStart(Vector3 _vel)
	{
		groundSurface.lastHitPoint = groundSurface.hitPoint;
		groundSurface.lastNormal = groundSurface.normal;
		groundSurface.normal = Vector3.up;
		ySize *= ConditionalScalePhysicsMulConstant(0.4f);
		if (isMotionSlowedDown)
		{
			isMotionSlowedDown = false;
			_vel.x *= motionMultiplier;
			if (!isCollidedVertically)
			{
				_vel.y *= motionMultiplier;
			}
			_vel.z *= motionMultiplier;
		}
		hitMove = _vel;
		collisionFlags = CollisionFlags.None;
		if (IsStuck)
		{
			PhysicsTransform.position += hitMove;
		}
		else
		{
			collisionFlags = m_characterController.Move(hitMove);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionResults()
	{
		Vector3 vector = (physicsTargetPos = PhysicsTransform.position);
		vector += Origin.position;
		Vector3 vector2 = vector - position;
		position = vector;
		boundingBox.center += vector2;
		Vector3 lhs = new Vector3(vector2.x, 0f, vector2.z);
		Vector3 vector3 = new Vector3(motion.x, 0f, motion.z);
		projectedMove = 0f;
		if (vector3 != Vector3.zero)
		{
			projectedMove = Utils.FastClamp01(Vector3.Dot(lhs, vector3) / vector3.sqrMagnitude);
			vector3 *= projectedMove;
		}
		if (motion.y > 0f)
		{
			if (vector2.y >= 0f && vector2.y < motion.y * 0.95f)
			{
				motion.y = 0f;
			}
			else
			{
				motion.y = Utils.FastClamp(vector2.y, 0f, motion.y);
			}
		}
		else
		{
			motion.y = Utils.FastClamp(vector2.y, motion.y, 0f);
		}
		motion.x = vector3.x;
		motion.z = vector3.z;
		isCollidedHorizontally = (collisionFlags & CollisionFlags.Sides) != 0;
		isCollidedVertically = (collisionFlags & (CollisionFlags)6) != 0;
		onGround = m_characterController.IsGrounded();
		if (onGround)
		{
			groundSurface.normal = m_characterController.GroundNormal;
		}
		world.CheckEntityCollisionWithBlocks(this);
		UpdateFall(hitMove.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aabbEntityCollision(Vector3 _vel)
	{
		ySize *= 0.4f;
		if (isMotionSlowedDown)
		{
			isMotionSlowedDown = false;
			_vel.x *= motionMultiplier;
			if (!isCollidedVertically)
			{
				_vel.y *= motionMultiplier;
			}
			_vel.z *= motionMultiplier;
			motion = Vector3.zero;
		}
		Vector3 vector = _vel;
		Bounds bounds = boundingBox;
		if (!(Math.Abs(_vel.x) > 0.0001f))
		{
			Math.Abs(_vel.z);
			_ = 0.0001f;
		}
		collAABB.Clear();
		Bounds aabb = BoundsUtils.ExpandDirectional(boundingBox, vector);
		world.GetCollidingBounds(this, aabb, collAABB);
		Vector3 vector2 = BoundsUtils.ClipBoundsMove(boundingBox, vector, collAABB, collAABB.Count);
		boundingBox.center += vector2;
		bool flag = onGround || (vector.y != vector2.y && vector.y < 0f);
		if (stepHeight > 0f && flag && ySize < 0.05f && (vector.x != vector2.x || vector.z != vector2.z))
		{
			Vector3 vector3 = vector2;
			vector2 = vector;
			vector2.y = stepHeight;
			Bounds bounds2 = boundingBox;
			boundingBox = bounds;
			collAABB.Clear();
			aabb = BoundsUtils.ExpandDirectional(boundingBox, new Vector3(vector2.x, 0f, vector2.z));
			world.GetCollidingBounds(this, aabb, collAABB);
			vector2 = BoundsUtils.ClipBoundsMove(boundingBox, vector2, collAABB, collAABB.Count);
			boundingBox.center += vector2;
			float y = BoundsUtils.ClipBoundsMoveY(boundingBox.min, boundingBox.max, 0f - stepHeight, collAABB, collAABB.Count);
			boundingBox.center += new Vector3(0f, y, 0f);
			vector2.y = y;
			if (vector3.x * vector3.x + vector3.z * vector3.z >= vector2.x * vector2.x + vector2.z * vector2.z)
			{
				vector2 = vector3;
				boundingBox = bounds2;
			}
			else if (boundingBox.min.y - (float)(int)boundingBox.min.y > 0f)
			{
				ySize += boundingBox.min.y - bounds2.min.y;
			}
		}
		Vector3 center = boundingBox.center;
		position.x = center.x;
		position.y = boundingBox.min.y + yOffset - ySize;
		position.z = center.z;
		if (PhysicsTransform != null && (PhysicsTransform.position - (position - Origin.position)).sqrMagnitude > 0.0001f)
		{
			PhysicsTransform.position = position - Origin.position;
		}
		isCollidedHorizontally = vector.x != vector2.x || vector.z != vector2.z;
		isCollidedVertically = vector.y != vector2.y;
		onGround = vector.y != vector2.y && vector.y < 0f;
		world.CheckEntityCollisionWithBlocks(this);
		UpdateFall(vector2.y);
		if (vector.x != vector2.x)
		{
			motion.x = 0f;
		}
		if (vector.y != vector2.y)
		{
			motion.y = 0f;
		}
		if (vector.z != vector2.z)
		{
			motion.z = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CalcFixedUpdateTimeScaleConstants()
	{
		kAddFixedUpdateTimeScale = Time.deltaTime / 0.05f;
	}

	public float ScalePhysicsMulConstant(float tickMulDelta)
	{
		return Mathf.Pow(tickMulDelta, kAddFixedUpdateTimeScale);
	}

	public float ScalePhysicsAddConstant(float tickAddDelta)
	{
		return kAddFixedUpdateTimeScale * tickAddDelta;
	}

	public float ConditionalScalePhysicsMulConstant(float tickMulDelta)
	{
		return tickMulDelta;
	}

	public float ConditionalScalePhysicsAddConstant(float tickAddDelta)
	{
		return tickAddDelta;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void entityCollision(Vector3 _motion)
	{
		if (emodel.IsRagdollMovement)
		{
			if (!emodel.pelvisRB)
			{
				return;
			}
			float num = emodel.bipedPelvisTransform.position.y + Origin.position.y;
			Vector3 velocity = emodel.pelvisRB.velocity;
			if (velocity.y < -1f)
			{
				fallVelY = Utils.FastMin(fallVelY, velocity.y);
				float num2 = fallLastY - num;
				if (num2 > 0f)
				{
					fallDistance += num2;
				}
			}
			else if (fallDistance > 0f)
			{
				fallLastMotion.y = fallVelY * 0.05f;
				onGround = true;
				UpdateFall(0f);
			}
			fallLastY = num;
		}
		else
		{
			ApplyFixedUpdate();
			if (m_characterController != null)
			{
				ccEntityCollision(_motion);
			}
			else
			{
				aabbEntityCollision(_motion);
			}
		}
	}

	public virtual void SetMotionMultiplier(float _motionMultiplier)
	{
		isMotionSlowedDown = true;
		motionMultiplier = _motionMultiplier;
		if (motionMultiplier < 0.5f)
		{
			fallDistance = 0f;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistance(Entity _other)
	{
		return (position - _other.position).magnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistanceSq(Entity _other)
	{
		return (position - _other.position).sqrMagnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistanceSq(Vector3 _pos)
	{
		return (position - _pos).sqrMagnitude;
	}

	public float GetSoundTravelTime(Vector3 _otherPos)
	{
		return (position - _otherPos).magnitude / 343f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInWater()
	{
		return isInWater;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsSwimming()
	{
		return isSwimming;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInElevator()
	{
		return bInElevator;
	}

	public void SetInElevator(bool _b)
	{
		bInElevator = _b;
	}

	public virtual bool IsAirBorne()
	{
		if (!bAirBorne)
		{
			return !onGround;
		}
		return true;
	}

	public void SetAirBorne(bool _b)
	{
		bAirBorne = _b;
	}

	public virtual float GetEyeHeight()
	{
		return 0f;
	}

	public virtual float GetHeight()
	{
		if (m_characterController != null)
		{
			return m_characterController.GetHeight();
		}
		return height;
	}

	public virtual void Move(Vector3 _direction, bool _isDirAbsolute, float _velocity, float _maxVelocity)
	{
		if (IsClientControlled() || (!GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving) && GameStats.GetInt(EnumGameStats.GameState) != 2))
		{
			float y = _direction.y;
			_direction.y = 0f;
			_direction.Normalize();
			if (_isDirAbsolute)
			{
				float num = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(motion, _direction)), 0f, _velocity);
				motion.x += ConditionalScalePhysicsAddConstant(_direction.x * num);
				motion.y += ConditionalScalePhysicsAddConstant(_direction.y * _velocity);
				motion.z += ConditionalScalePhysicsAddConstant(_direction.z * num);
			}
			else
			{
				Vector3 rhs = base.transform.forward * _direction.z + base.transform.right * _direction.x;
				rhs.Normalize();
				float num2 = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(motion, rhs)), 0f, _velocity);
				motion += base.transform.forward * ConditionalScalePhysicsAddConstant(_direction.z * num2) + base.transform.right * ConditionalScalePhysicsAddConstant(_direction.x * num2) + base.transform.up * ConditionalScalePhysicsAddConstant(y * _velocity);
			}
		}
	}

	public bool IsAlive()
	{
		return !IsDead();
	}

	public bool WasAlive()
	{
		return !WasDead();
	}

	public virtual bool IsDead()
	{
		return bDead;
	}

	public bool WasDead()
	{
		return bWasDead;
	}

	public virtual void SetDead()
	{
		bDead = true;
		Manager.DestroySoundsForEntity(entityId);
		if (m_marker != null)
		{
			m_marker.Release();
			m_marker = null;
		}
		if (PhysicsTransform != null)
		{
			if (emodel.HasRagdoll())
			{
				PhysicsTransform.gameObject.layer = 17;
			}
			else
			{
				PhysicsTransform.gameObject.layer = 14;
			}
		}
		if ((bool)physicsRBT)
		{
			physicsRBT.gameObject.SetActive(value: false);
		}
		if (emodel != null)
		{
			emodel.SetDead();
		}
	}

	public virtual void SetAlive()
	{
		bDead = false;
		if (PhysicsTransform != null)
		{
			if (this is EntityPlayerLocal)
			{
				PhysicsTransform.gameObject.layer = 20;
			}
			else
			{
				PhysicsTransform.gameObject.layer = 15;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFall(float mY)
	{
		if (onGround)
		{
			if (fallDistance > 0f)
			{
				fallHitGround(fallDistance, fallLastMotion);
				fallDistance = 0f;
			}
		}
		else if (mY < 0f)
		{
			fallLastMotion = motion;
			fallDistance -= mY;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void fallHitGround(float _v, Vector3 _fallMotion)
	{
	}

	public virtual void OnRagdoll(bool isActive)
	{
		if (isActive && (bool)emodel.bipedPelvisTransform)
		{
			fallLastY = emodel.bipedPelvisTransform.position.y + Origin.position.y;
			fallVelY = 0f;
		}
	}

	public virtual bool CanDamageEntity(int _sourceEntityId)
	{
		return true;
	}

	public virtual int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		setBeenAttacked();
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setBeenAttacked()
	{
	}

	public virtual void FireAttackedEvents(DamageResponse dmResponse)
	{
	}

	public virtual void ProcessDamageResponse(DamageResponse _dmResponse)
	{
	}

	public Bounds getBoundingBox()
	{
		return boundingBox;
	}

	public virtual void OnDamagedByExplosion()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnPushEntity(Entity _entity)
	{
		Vector3 vector = _entity.position - position;
		float num = Utils.FastMax(Mathf.Abs(vector.x), Mathf.Abs(vector.z));
		if (num >= 0.01f)
		{
			num = Mathf.Sqrt(num);
			float num2 = 1f / num;
			vector.x *= num2;
			vector.z *= num2;
			if (num2 < 1f)
			{
				vector.x *= num2;
				vector.z *= num2;
			}
			float num3 = 0.05f * (1f - entityCollisionReduction);
			num3 *= Utils.FastMin(_entity.GetWeight(), GetWeight()) / Utils.FastMax(_entity.GetWeight(), GetWeight());
			vector.x *= num3;
			vector.z *= num3;
			AddVelocity(new Vector3(0f - vector.x, 0f, 0f - vector.z));
			if (_entity.CanBePushed())
			{
				_entity.AddVelocity(new Vector3(vector.x, 0f, vector.z));
			}
		}
	}

	public virtual void AddVelocity(Vector3 _vel)
	{
		motion += _vel;
		SetAirBorne(_b: true);
	}

	public virtual Vector3 GetVelocityPerSecond()
	{
		if ((bool)AttachedToEntity)
		{
			return AttachedToEntity.GetVelocityPerSecond();
		}
		if ((bool)physicsRB)
		{
			return physicsRB.velocity;
		}
		return motion * 20f;
	}

	public virtual Vector3 GetAngularVelocityPerSecond()
	{
		if ((bool)AttachedToEntity)
		{
			return AttachedToEntity.GetAngularVelocityPerSecond();
		}
		if ((bool)physicsRB)
		{
			return physicsRB.angularVelocity;
		}
		return Vector3.zero;
	}

	public virtual void SetVelocityPerSecond(Vector3 vel, Vector3 angularVel)
	{
		if ((bool)AttachedToEntity)
		{
			AttachedToEntity.SetVelocityPerSecond(vel, angularVel);
			return;
		}
		physicsVel = vel;
		physicsAngVel = angularVel;
		if (isPhysicsMaster && (bool)physicsRB)
		{
			physicsRB.velocity = vel;
			physicsRB.angularVelocity = angularVel;
		}
		motion = vel * 0.05f;
	}

	public virtual bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetPushBoundsVertical()
	{
		return 0f;
	}

	public virtual bool CanCollideWith(Entity _other)
	{
		return true;
	}

	public virtual void OnUpdatePosition(float _partialTicks)
	{
		ticksExisted++;
		prevPos = position;
		prevRotation = rotation;
		if (isUpdatePosition)
		{
			if ((bool)AttachedToEntity || ((bool)emodel && emodel.IsRagdollOn))
			{
				isUpdatePosition = false;
			}
			else
			{
				switch (positionUpdateMovementType)
				{
				case EnumPositionUpdateMovementType.MoveTowards:
					SetPosition(Vector3.MoveTowards(position, targetPos, tickPositionMoveTowardsMaxDistance), _bUpdatePhysics: false);
					break;
				case EnumPositionUpdateMovementType.Lerp:
					SetPosition(Vector3.Lerp(position, targetPos, Time.deltaTime / Time.fixedDeltaTime * tickPositionLerpMultiplier), _bUpdatePhysics: false);
					break;
				default:
					SetPosition(targetPos, _bUpdatePhysics: false);
					break;
				}
				if (position == targetPos)
				{
					isUpdatePosition = false;
				}
				if (PhysicsTransform != null)
				{
					physicsTargetPos = position - Origin.position;
					PhysicsTransform.position = physicsTargetPos;
				}
			}
		}
		if (interpolateTargetQRot > 0)
		{
			qrotation = Quaternion.Lerp(qrotation, targetQRot, 1f / (float)interpolateTargetQRot);
			interpolateTargetQRot--;
		}
		if (interpolateTargetRot > 0)
		{
			float t = 1f / (float)interpolateTargetRot;
			SetRotation(new Vector3(Mathf.LerpAngle(rotation.x, targetRot.x, t), Mathf.LerpAngle(rotation.y, targetRot.y, t), Mathf.LerpAngle(rotation.z, targetRot.z, t)));
			interpolateTargetRot--;
		}
		if (!isEntityRemote && !IsDead() && !IsClientControlled() && position.y < 0f && IsDeadIfOutOfWorld())
		{
			EntityDrone entityDrone = this as EntityDrone;
			if ((bool)entityDrone)
			{
				entityDrone.NotifyOffTheWorld();
				return;
			}
			Log.Warning("Entity " + this?.ToString() + " fell off the world, id=" + entityId + " pos=" + position.ToCultureInvariantString());
			MarkToUnload();
		}
	}

	public virtual void CheckPosition()
	{
		if (float.IsNaN(position.x) || float.IsInfinity(position.x))
		{
			position.x = lastTickPos[0].x;
		}
		if (float.IsNaN(position.y) || float.IsInfinity(position.y))
		{
			position.y = lastTickPos[0].y;
		}
		if (float.IsNaN(position.z) || float.IsInfinity(position.z))
		{
			position.z = lastTickPos[0].z;
		}
		if (float.IsNaN(rotation.x) || float.IsInfinity(rotation.x))
		{
			rotation.x = prevRotation.x;
		}
		if (float.IsNaN(rotation.y) || float.IsInfinity(rotation.y))
		{
			rotation.y = prevRotation.y;
		}
		if (float.IsNaN(rotation.z) || float.IsInfinity(rotation.z))
		{
			rotation.z = prevRotation.z;
		}
	}

	public virtual void OnUpdateEntity()
	{
		bool flag = isInWater;
		if (!isEntityStatic())
		{
			TickInWater();
		}
		if (isEntityRemote)
		{
			return;
		}
		if (isInWater)
		{
			if (!flag && !firstUpdate && fallDistance > 1f)
			{
				PlayOneShot("waterfallinginto");
			}
			fallDistance = 0f;
		}
		if (!RootMotion && !IsDead() && CanBePushed())
		{
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(boundingBox, 0.2f, GetPushBoundsVertical(), 0.2f));
			if (entitiesInBounds != null && entitiesInBounds.Count > 0)
			{
				for (int i = 0; i < entitiesInBounds.Count; i++)
				{
					Entity entity = entitiesInBounds[i];
					OnPushEntity(entity);
				}
			}
		}
		firstUpdate = false;
	}

	public virtual void OnAddedToWorld()
	{
	}

	public virtual void OnEntityUnload()
	{
		if (isUnloaded)
		{
			Log.Warning("OnEntityUnload already unloaded {0} ", GetDebugName());
			return;
		}
		isUnloaded = true;
		Manager.DestroySoundsForEntity(entityId);
		if (movableChunkObserver != null)
		{
			movableChunkObserver.Dispose();
			movableChunkObserver = null;
		}
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				Entity entity = attachedEntities[i];
				if (entity != null)
				{
					entity.Detach();
				}
			}
		}
		if (AttachedToEntity != null)
		{
			Detach();
		}
		if (emodel != null)
		{
			emodel.OnUnload();
		}
		try
		{
			UnityEngine.Object.Destroy(RootTransform.gameObject);
		}
		catch (Exception e)
		{
			Log.Error("OnEntityUnload: {0}", GetDebugName());
			Log.Exception(e);
		}
	}

	public virtual float GetLightBrightness()
	{
		Vector3i blockPosition = GetBlockPosition();
		Vector3i blockPos = blockPosition;
		blockPos.y += Mathf.RoundToInt(height + 0.5f);
		return Utils.FastMax(world.GetLightBrightness(blockPosition), world.GetLightBrightness(blockPos));
	}

	public Vector3i GetBlockPosition()
	{
		return World.worldToBlockPos(position);
	}

	public virtual void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		serverPos = NetEntityDistributionEntry.EncodePos(_pos);
		SetPosition(_pos);
		SetRotation(_rot);
		SetPosAndRotFromNetwork(_pos, _rot, 0);
		ResetLastTickPos(_pos);
		base.transform.SetPositionAndRotation(position - Origin.position, Quaternion.Euler(rotation));
	}

	public Vector3 GetPosition()
	{
		return position;
	}

	public virtual void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		position = _pos;
		float num = width * 0.5f;
		float num2 = depth * 0.5f;
		float num3 = _pos.y - yOffset + ySize;
		boundingBox = BoundsUtils.BoundsForMinMax(_pos.x - num, num3, _pos.z - num2, _pos.x + num, num3 + height, _pos.z + num2);
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				Entity entity = attachedEntities[i];
				if (entity != null)
				{
					entity.SetPosition(_pos, _bUpdatePhysics: false);
				}
			}
		}
		if (_bUpdatePhysics && PhysicsTransform != null)
		{
			PhysicsTransform.position = _pos - Origin.position;
			if ((bool)physicsRBT)
			{
				physicsPos = _pos - Origin.position + physicsBasePos;
				physicsRBT.position = physicsPos;
				physicsTargetPos = PhysicsTransform.position;
			}
		}
	}

	public void SetRotationAndStopTurning(Vector3 _rot)
	{
		SetRotation(_rot);
		yawSeekTimeMax = 0f;
		interpolateTargetQRot = 0;
		interpolateTargetRot = 0;
	}

	public virtual void SetRotation(Vector3 _rot)
	{
		rotation = _rot;
		qrotation = Quaternion.Euler(_rot);
	}

	public void SetPosAndRotFromNetwork(Vector3 _pos, Vector3 _rot, int _steps)
	{
		targetPos = _pos;
		targetRot = _rot;
		isUpdatePosition = true;
		interpolateTargetRot = _steps;
	}

	public void SetPosAndQRotFromNetwork(Vector3 _pos, Quaternion _rot, int _steps)
	{
		targetPos = _pos;
		targetQRot = _rot;
		isUpdatePosition = true;
		interpolateTargetQRot = _steps;
	}

	public void SetRotFromNetwork(Vector3 _rot, int _steps)
	{
		targetRot = _rot;
		interpolateTargetRot = _steps;
	}

	public void SetQRotFromNetwork(Quaternion _qrot, int _steps)
	{
		targetQRot = _qrot;
		interpolateTargetQRot = _steps;
	}

	public float GetBrightness(float _t)
	{
		int num = Utils.Fastfloor(position.x);
		int num2 = Utils.Fastfloor(position.z);
		if (world.GetChunkSync(World.toChunkXZ(num), World.toChunkXZ(num2)) != null)
		{
			float num3 = (boundingBox.max.y - boundingBox.min.y) * 0.66f;
			int y = Utils.Fastfloor((double)position.y - (double)yOffset + (double)num3);
			return world.GetLightBrightness(new Vector3i(num, y, num2));
		}
		return 0f;
	}

	public virtual void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
	}

	public void SetIgnoredByAI(bool ignore)
	{
		isIgnoredByAI = ignore;
	}

	public virtual bool IsIgnoredByAI()
	{
		return isIgnoredByAI;
	}

	public virtual Vector3 getHeadPosition()
	{
		if (emodel == null)
		{
			return position + new Vector3(0f, GetEyeHeight(), 0f);
		}
		return emodel.GetHeadPosition();
	}

	public virtual Vector3 getNavObjectPosition()
	{
		if (emodel == null)
		{
			return position + new Vector3(0f, GetEyeHeight(), 0f);
		}
		return emodel.GetNavObjectPosition();
	}

	public virtual Vector3 getBellyPosition()
	{
		if (emodel == null)
		{
			return position + new Vector3(0f, GetEyeHeight() / 2f, 0f);
		}
		return emodel.GetBellyPosition();
	}

	public virtual Vector3 getHipPosition()
	{
		if (emodel == null)
		{
			return position + new Vector3(0f, GetEyeHeight() / 2f, 0f);
		}
		return emodel.GetHipPosition();
	}

	public virtual Vector3 getChestPosition()
	{
		if (emodel == null)
		{
			return position + new Vector3(0f, GetEyeHeight() / 2.4f, 0f);
		}
		return emodel.GetChestPosition();
	}

	public void SetVelocity(Vector3 _vel)
	{
		motion = _vel;
	}

	public virtual float GetWeight()
	{
		return 1f;
	}

	public virtual float GetPushFactor()
	{
		return 1f;
	}

	public virtual float GetSightDetectionScale()
	{
		return 1f;
	}

	public virtual void OnLoadedFromEntityCache(EntityCreationData _ed)
	{
		if (bIsChunkObserver && !isEntityRemote)
		{
			movableChunkObserver = new MovableSharedChunkObserver(world.m_SharedChunkObserverCache);
			movableChunkObserver.SetPosition(position);
		}
	}

	public virtual bool IsSavedToNetwork()
	{
		return true;
	}

	public virtual bool IsSavedToFile()
	{
		if (world.IsEditor() && GameManager.Instance.GetDynamicPrefabDecorator().IsEntityInPrefab(entityId))
		{
			return false;
		}
		return true;
	}

	public virtual void SetEntityName(string _name)
	{
	}

	public virtual void CopyPropertiesFromEntityClass()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		RootMotion = entityClass.RootMotion;
		HasDeathAnim = entityClass.HasDeathAnim;
		entityFlags = entityClass.entityFlags;
		entityType = EntityType.Unknown;
		entityClass.Properties.ParseEnum(EntityClass.PropEntityType, ref entityType);
		entityClass.Properties.ParseFloat(EntityClass.PropLootDropProb, ref lootDropProb);
		lootListOnDeath = entityClass.Properties.GetString(EntityClass.PropLootListOnDeath);
		entityClass.Properties.ParseString(EntityClass.PropLootListAlive, ref lootListAlive);
		entityClass.Properties.ParseString(EntityClass.PropMapIcon, ref mapIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassIcon, ref compassIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassUpIcon, ref compassUpIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassDownIcon, ref compassDownIcon);
		entityClass.Properties.ParseString(EntityClass.PropTrackerIcon, ref trackerIcon);
		entityClass.Properties.ParseBool(EntityClass.PropRotateToGround, ref isRotateToGround);
	}

	public virtual string GetLootList()
	{
		return lootListAlive;
	}

	public virtual void MarkToUnload()
	{
		markedForUnload = true;
	}

	public virtual bool IsMarkedForUnload()
	{
		if (!markedForUnload)
		{
			return IsDead();
		}
		return true;
	}

	public virtual bool IsSpawned()
	{
		return true;
	}

	public void ResetLastTickPos(Vector3 _pos)
	{
		for (int i = 0; i < lastTickPos.Length; i++)
		{
			lastTickPos[i] = _pos;
		}
	}

	public void SetLastTickPos(Vector3 _pos)
	{
		for (int num = lastTickPos.Length - 1; num > 0; num--)
		{
			lastTickPos[num] = lastTickPos[num - 1];
		}
		lastTickPos[0] = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isDetailedHeadBodyColliders()
	{
		return false;
	}

	public virtual Transform GetModelTransform()
	{
		return null;
	}

	public virtual Vector3 GetMapIconScale()
	{
		return new Vector3(1f, 1f, 1f);
	}

	public virtual string GetMapIcon()
	{
		return mapIcon;
	}

	public virtual string GetCompassIcon()
	{
		if (compassIcon == null)
		{
			return mapIcon;
		}
		return compassIcon;
	}

	public virtual string GetCompassUpIcon()
	{
		return compassUpIcon;
	}

	public virtual string GetCompassDownIcon()
	{
		return compassDownIcon;
	}

	public virtual string GetTrackerIcon()
	{
		return trackerIcon;
	}

	public virtual bool HasUIIcon()
	{
		if (mapIcon == null && trackerIcon == null)
		{
			return compassIcon != null;
		}
		return true;
	}

	public virtual EnumMapObjectType GetMapObjectType()
	{
		return EnumMapObjectType.Entity;
	}

	public virtual bool IsMapIconBlinking()
	{
		return false;
	}

	public virtual bool IsDrawMapIcon()
	{
		return IsSpawned();
	}

	public virtual Color GetMapIconColor()
	{
		return Color.white;
	}

	public virtual bool CanMapIconBeSelected()
	{
		return false;
	}

	public virtual int GetLayerForMapIcon()
	{
		return 2;
	}

	public virtual bool IsClientControlled()
	{
		if (attachedEntities != null && attachedEntities.Length != 0)
		{
			return attachedEntities[0] != null;
		}
		return false;
	}

	public virtual bool IsDeadIfOutOfWorld()
	{
		return true;
	}

	public virtual bool CanCollideWithBlocks()
	{
		return true;
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource)
	{
		SetSpawnerSource(_spawnerSource, 0L, 0);
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource, long _chunkKey, int _biomeIdHash)
	{
		spawnerSource = _spawnerSource;
		spawnerSourceChunkKey = _chunkKey;
		spawnerSourceBiomeIdHash = _biomeIdHash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EnumSpawnerSource GetSpawnerSource()
	{
		return spawnerSource;
	}

	public long GetSpawnerSourceChunkKey()
	{
		return spawnerSourceChunkKey;
	}

	public int GetSpawnerSourceBiomeIdHash()
	{
		return spawnerSourceBiomeIdHash;
	}

	public float CalculateAudioOcclusion()
	{
		return 0f;
	}

	public virtual void PlayOneShot(string clipName, bool sound_in_head = false, bool serverSignalOnly = false, bool isUnique = false, AnimationEvent _animEvent = null)
	{
		if (!sound_in_head)
		{
			if (serverSignalOnly)
			{
				Handle handle = Manager.Play(this, clipName, 1f, wantHandle: true);
				if (_animEvent != null)
				{
					int intParameter = _animEvent.intParameter;
					if (intParameter > 0)
					{
						addAnimatorAudioToMonitor((StopAnimatorAudioType)intParameter, handle);
					}
				}
			}
			else
			{
				Manager.BroadcastPlay(this, clipName, serverSignalOnly);
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead(clipName, -1, 0f, isLooping: false, isUnique);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addAnimatorAudioToMonitor(StopAnimatorAudioType _sat, Handle _handle)
	{
		if (animatorAudioMonitoringDictionary.TryGetValue(_sat, out var value))
		{
			value.Stop(entityId);
		}
		animatorAudioMonitoringDictionary[_sat] = _handle;
	}

	public void StopAnimatorAudio(StopAnimatorAudioType _sat)
	{
		if (animatorAudioMonitoringDictionary.TryGetValue(_sat, out var value))
		{
			value.Stop(entityId);
			animatorAudioMonitoringDictionary.Remove(_sat);
		}
	}

	public void StopOneShot(string clipName)
	{
		Manager.BroadcastStop(entityId, clipName);
	}

	public virtual EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (lootContainer == null)
		{
			cmds[0].enabled = false;
		}
		return cmds;
	}

	public virtual bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (_indexInBlockActivationCommands == 0)
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			if (entityClass.onActivateEvent != "")
			{
				GameEventManager.Current.HandleAction(entityClass.onActivateEvent, null, this, twitchActivated: false, _tePos);
			}
			GameManager.Instance.TELockServer(0, _tePos, entityId, _entityFocusing.entityId);
			return true;
		}
		return false;
	}

	public void SetAttachMaxCount(int maxCount)
	{
		if (attachedEntities != null)
		{
			if (attachedEntities.Length == maxCount)
			{
				return;
			}
			for (int i = maxCount; i < attachedEntities.Length; i++)
			{
				Entity entity = attachedEntities[i];
				if ((bool)entity)
				{
					entity.Detach();
				}
			}
		}
		Entity[] array = attachedEntities;
		attachedEntities = null;
		if (maxCount <= 0)
		{
			return;
		}
		attachedEntities = new Entity[maxCount];
		if (array != null)
		{
			int num = Utils.FastMin(array.Length, maxCount);
			for (int j = 0; j < num; j++)
			{
				attachedEntities[j] = array[j];
			}
		}
	}

	public int GetAttachMaxCount()
	{
		if (attachedEntities != null)
		{
			return (byte)attachedEntities.Length;
		}
		return 0;
	}

	public int GetAttachFreeCount()
	{
		int num = 0;
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				if (attachedEntities[i] == null)
				{
					num++;
				}
			}
		}
		return num;
	}

	public Entity GetAttached(int slot)
	{
		if (attachedEntities != null && slot < attachedEntities.Length)
		{
			return attachedEntities[slot];
		}
		return null;
	}

	public Entity GetFirstAttached()
	{
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				Entity entity = attachedEntities[i];
				if ((bool)entity)
				{
					return entity;
				}
			}
		}
		return null;
	}

	public EntityPlayerLocal GetAttachedPlayerLocal()
	{
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				EntityPlayerLocal entityPlayerLocal = attachedEntities[i] as EntityPlayerLocal;
				if ((bool)entityPlayerLocal)
				{
					return entityPlayerLocal;
				}
			}
		}
		return null;
	}

	public bool CanAttach(Entity _entity)
	{
		if (FindAttachSlot(_entity) >= 0)
		{
			return false;
		}
		return FindAttachSlot(null) >= 0;
	}

	public int FindAttachSlot(Entity _entity)
	{
		if (attachedEntities != null)
		{
			for (int i = 0; i < attachedEntities.Length; i++)
			{
				if (attachedEntities[i] == _entity)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool IsAttached(Entity _entity)
	{
		return FindAttachSlot(_entity) >= 0;
	}

	public bool IsDriven()
	{
		if (attachedEntities == null)
		{
			return false;
		}
		return attachedEntities[0];
	}

	public virtual int AttachEntityToSelf(Entity _other, int slot)
	{
		int num = FindAttachSlot(_other);
		if (num >= 0)
		{
			if (slot < 0 || slot == num)
			{
				return num;
			}
			DetachEntity(_other);
		}
		if (slot < 0)
		{
			slot = FindAttachSlot(null);
			if (slot < 0)
			{
				return -1;
			}
		}
		if (slot >= attachedEntities.Length)
		{
			return -1;
		}
		if (slot == 0)
		{
			serverPos = NetEntityDistributionEntry.EncodePos(position);
			isEntityRemote = _other.isEntityRemote;
		}
		attachedEntities[slot] = _other;
		return slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DetachEntity(Entity _other)
	{
		int num = FindAttachSlot(_other);
		if (num >= 0)
		{
			if (num == 0)
			{
				isEntityRemote = world.IsRemote();
			}
			attachedEntities[num] = null;
		}
	}

	public virtual void StartAttachToEntity(Entity _other, int slot = -1)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachServer, entityId, _other.entityId, slot));
			return;
		}
		slot = AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachClient, entityId, _other.entityId, slot));
		}
	}

	public virtual int AttachToEntity(Entity _other, int slot = -1)
	{
		if (_other.IsAttached(this))
		{
			return -1;
		}
		slot = _other.AttachEntityToSelf(this, slot);
		if (slot < 0)
		{
			return slot;
		}
		AttachedToEntitySlotInfo attachedToInfo = _other.GetAttachedToInfo(slot);
		RootTransform.SetParent(attachedToInfo.enterParentTransform, worldPositionStays: false);
		RootTransform.localPosition = Vector3.zero;
		RootTransform.localEulerAngles = Vector3.zero;
		ModelTransform.localPosition = attachedToInfo.enterPosition;
		ModelTransform.localEulerAngles = attachedToInfo.enterRotation;
		rotation = attachedToInfo.enterRotation;
		if (isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
		{
			emodel.SetVisible(_bVisible: false);
		}
		AttachedToEntity = _other;
		return slot;
	}

	public void SendDetach()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachServer, entityId, -1, -1));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachClient, entityId, -1, -1));
		}
		Detach();
	}

	public virtual void Detach()
	{
		RootTransform.parent = EntityFactory.ParentNameToTransform[EntityClass.list[entityClass].parentGameObjectName];
		if (!(AttachedToEntity == null))
		{
			int num = AttachedToEntity.FindAttachSlot(this);
			if (num < 0)
			{
				num = 0;
			}
			AttachedToEntitySlotInfo attachedToInfo = AttachedToEntity.GetAttachedToInfo(num);
			AttachedToEntitySlotExit attachedToEntitySlotExit = FindValidExitPosition(attachedToInfo.exits);
			Entity attachedToEntity = AttachedToEntity;
			AttachedToEntity = null;
			isUpdatePosition = false;
			if (attachedToEntitySlotExit.position != Vector3.zero)
			{
				SetPosition(attachedToEntitySlotExit.position);
				SetRotation(attachedToEntitySlotExit.rotation);
			}
			ResetLastTickPos(base.transform.position + Origin.position);
			attachedToEntity.DetachEntity(this);
			if (isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
			{
				emodel.SetVisible(_bVisible: true);
			}
		}
	}

	public virtual void MoveByAttachedEntity(EntityPlayerLocal _player)
	{
	}

	public virtual AttachedToEntitySlotExit FindValidExitPosition(List<AttachedToEntitySlotExit> candidatePositions)
	{
		AttachedToEntitySlotExit result = default(AttachedToEntitySlotExit);
		result.position = Vector3.zero;
		result.rotation = Vector3.zero;
		if (m_characterController == null)
		{
			return result;
		}
		AttachedToEntity.SetPhysicsCollidersLayer(14);
		float num = m_characterController.GetRadius();
		float num2 = m_characterController.GetHeight() - num * 2f;
		Vector3 vector = base.transform.position + m_characterController.GetCenter();
		vector.y -= num2 * 0.5f;
		for (int i = 0; i < candidatePositions.Count; i++)
		{
			for (float num3 = 0f; num3 < 0.75f; num3 += 0.24f)
			{
				Vector3 vector2 = vector;
				vector2.y += num3;
				result = candidatePositions[i];
				result.position.y += num3;
				Vector3 vector3 = result.position - Origin.position - vector2;
				vector3.y += num;
				Vector3 normalized = vector3.normalized;
				float num4 = vector3.magnitude;
				if (normalized.y < 0f)
				{
					float y = normalized.y;
					if (y < -0.707f)
					{
						break;
					}
					y *= -1.6f;
					num4 += y;
					result.position += normalized * y;
				}
				bool flag = false;
				Vector3 origin = vector2;
				for (float num5 = (0f - num) * 0.5f; num5 < num2; num5 += 0.2f)
				{
					origin.y = vector2.y + num5;
					flag = Physics.Raycast(origin, normalized, num4, 1084817408);
					if (flag)
					{
						break;
					}
				}
				Vector3 vector4 = vector2 - normalized * 0.1f;
				Vector3 point = vector4;
				point.y += num2;
				if (!flag && !Physics.CapsuleCast(vector4, point, num, normalized, num4, 1084817408))
				{
					AttachedToEntity.SetPhysicsCollidersLayer(21);
					return result;
				}
			}
		}
		AttachedToEntity.SetPhysicsCollidersLayer(21);
		result.position = Vector3.zero;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPhysicsCollidersLayer(int layer)
	{
		Collider[] componentsInChildren = PhysicsTransform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DebugCapsuleCast()
	{
	}

	public virtual AttachedToEntitySlotInfo GetAttachedToInfo(int _slotIdx)
	{
		return null;
	}

	public virtual bool CanUpdateEntity()
	{
		Vector3i vector3i = World.worldToBlockPos(position);
		IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
		if (chunkFromWorldPos == null || !chunkFromWorldPos.GetAvailable())
		{
			return false;
		}
		for (int i = 0; i < adjacentPositions.Length; i++)
		{
			int num = World.toChunkXZ(vector3i.x + adjacentPositions[i].x);
			int num2 = World.toChunkXZ(vector3i.z + adjacentPositions[i].z);
			if (num != chunkFromWorldPos.X || num2 != chunkFromWorldPos.Z)
			{
				IChunk chunkSync = world.GetChunkSync(num, num2);
				if (chunkSync == null || !chunkSync.GetAvailable())
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual Transform GetThirdPersonCameraTransform()
	{
		return emodel.GetThirdPersonCameraTransform();
	}

	public virtual void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write((byte)spawnerSource);
		if (spawnerSource == EnumSpawnerSource.Biome)
		{
			_bw.Write(spawnerSourceBiomeIdHash);
			_bw.Write(spawnerSourceChunkKey);
		}
		_bw.Write(WorldTimeBorn);
	}

	public virtual void Read(byte _version, BinaryReader _br)
	{
		if (_version >= 11)
		{
			spawnerSource = (EnumSpawnerSource)_br.ReadByte();
			if (spawnerSource == EnumSpawnerSource.Biome)
			{
				if (_version >= 28)
				{
					spawnerSourceBiomeIdHash = _br.ReadInt32();
				}
				else
				{
					_br.ReadString();
					spawnerSource = EnumSpawnerSource.Delete;
				}
				spawnerSourceChunkKey = _br.ReadInt64();
			}
		}
		if (_version >= 15)
		{
			WorldTimeBorn = _br.ReadUInt64();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isEntityStatic()
	{
		return false;
	}

	public virtual void AddUIHarvestingItem(ItemStack _is, bool _bAddOnlyIfNotExisting = false)
	{
	}

	public virtual bool IsQRotationUsed()
	{
		return false;
	}

	public bool HasAnyTags(FastTags<TagGroup.Global> tags)
	{
		return cachedTags.Test_AnySet(tags);
	}

	public bool HasAllTags(FastTags<TagGroup.Global> tags)
	{
		return cachedTags.Test_AllSet(tags);
	}

	public void SetTransformActive(string partName, bool active)
	{
		Transform transform = base.transform.FindInChilds(partName);
		if (transform != null)
		{
			transform.gameObject.SetActive(active);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleNavObject()
	{
		if (EntityClass.list[entityClass].NavObject != "")
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, this);
		}
	}

	public void AddNavObject(string navObjectName, string overrideSprite, string overrideText)
	{
		if (NavObject == null)
		{
			NavObjectManager.Instance.RegisterNavObject(navObjectName, this, overrideSprite).name = overrideText;
			return;
		}
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		NavObject.name = overrideText;
		NavObject.AddNavObjectClass(navObjectClass);
	}

	public void RemoveNavObject(string navObjectName)
	{
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		if (NavObject != null && NavObject.RemoveNavObjectClass(navObjectClass))
		{
			NavObject = null;
		}
	}

	public string GetDebugName()
	{
		if (this is EntityAlive entityAlive)
		{
			return entityAlive.EntityName;
		}
		return GetType().ToString();
	}

	public virtual void SetLootContainerSize()
	{
		if (lootContainer != null)
		{
			lootContainer.SetContainerSize(LootContainer.GetLootContainer(GetLootList()).size);
		}
	}
}
