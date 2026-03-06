using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Assets.DuckType.Jiggle;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public abstract class EModelBase : MonoBehaviour
{
	public enum HeadStates
	{
		Standard,
		Growing,
		BigHead,
		Shrinking
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ERagdollState
	{
		Off,
		On,
		BlendOutGround,
		BlendOutStand,
		Stand,
		StandCollide,
		SpawnWait,
		Dead
	}

	public struct RagdollPose
	{
		public Transform t;

		public Rigidbody rb;

		public Quaternion startRot;

		public Quaternion rot;
	}

	public const string ExtFirstPerson = "_FP";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cEmissiveColor = "_EmissiveColor";

	public AvatarController avatarController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform modelTransformParent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform modelTransform;

	public Transform meshTransform;

	public Transform bipedRootTransform;

	public Transform bipedPelvisTransform;

	public Rigidbody pelvisRB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform headTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform neckTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform neckParentTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterGazeController gazeController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public PhysicsBodyInstance physicsBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string modelName;

	public Material AltMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float ragdollChance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bHasRagdoll;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Cloth[] clothSim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClothSimOn = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Jiggle[] jiggles;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJiggleOn = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity entity;

	public HeadStates HeadState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float headScaleSpeed = 2f;

	public float HeadStandardSize = 1f;

	public float HeadBigSize = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform NavObjectTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float ragdollAlignmentForce = 25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float ragdollAlingmentDistance = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float ragdollBlendPositionSpeed = 10f;

	public static float serverPosSpringForce = 50f;

	public static float serverPosSpringDamping = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Renderer> rendererList = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SkinnedMeshRenderer> skinnedRendererList = new List<SkinnedMeshRenderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookAtSlerpPer = 0.16f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookAtAnimBlend = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lookAtEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAtMaxAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lookAtIsPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lookAtPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion lookAtRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAtBlendPer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAtBlendPerTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAtFullChangeTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAtFullBlendPer = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadgollBlendOutGroundTime = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadgollBlendOutTime = 0.7f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRagdollMinDisableVel = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRagdollDeadMaxDepentrationVel = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRagdollDeadMaxAngularVel = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRadgollPlayerAnimLayer = 5;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ERagdollState ragdollState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ragdollTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ragdollDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator ragdollAnimator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ragdollRotY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ragdollIsBlending;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ragdollAdjustPosDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ragdollIsPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ragdollIsAnimal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ragdollIsFacingUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<RagdollPose> ragdollPoses = new List<RagdollPose>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ragdollPosePelvisPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ragdollPosePelvisLocalPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Rigidbody> ragdollTempRBs = new List<Rigidbody>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ragdollZeroTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Transform> ragdollZeroBones;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] commonBips = new string[2] { "Bip001", "Bip01" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock matPropBlock;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int fadeId = Shader.PropertyToID("_Fade");

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsFPV { get; set; }

	public virtual Transform NeckTransform => neckTransform;

	public bool IsRagdollActive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ragdollState != ERagdollState.Off;
		}
	}

	public bool IsRagdollMovement
	{
		get
		{
			if (ragdollState != ERagdollState.Off)
			{
				return ragdollState != ERagdollState.StandCollide;
			}
			return false;
		}
	}

	public bool IsRagdollOn
	{
		get
		{
			if (ragdollState != ERagdollState.On)
			{
				return ragdollState == ERagdollState.Dead;
			}
			return true;
		}
	}

	public bool IsRagdollDead => ragdollState == ERagdollState.Dead;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool visible
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual void Init(World _world, Entity _entity)
	{
		EntityClass entityClass = EntityClass.list[_entity.entityClass];
		visible = false;
		entity = _entity;
		ragdollChance = entityClass.RagdollOnDeathChance;
		bHasRagdoll = entityClass.HasRagdoll;
		modelTransformParent = FindModel(base.transform);
		createModel(_world, entityClass);
		bool flag = entity.RootMotion || bHasRagdoll;
		if (GameManager.IsDedicatedServer && !flag)
		{
			if (entityClass.Properties.GetString(EntityClass.PropAvatarController).Length > 0)
			{
				avatarController = base.gameObject.AddComponent<AvatarControllerDummy>();
				Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}
		else
		{
			createAvatarController(entityClass);
			if (GameManager.IsDedicatedServer && (bool)avatarController && flag)
			{
				avatarController.SetVisible(_b: true);
			}
		}
		gazeController = base.transform.GetComponentInChildren<CharacterGazeController>();
		InitCommon();
	}

	public virtual void InitFromPrefab(World _world, Entity _entity)
	{
		EntityClass entityClass = EntityClass.list[_entity.entityClass];
		entity = _entity;
		ragdollChance = entityClass.RagdollOnDeathChance;
		bHasRagdoll = entityClass.HasRagdoll;
		modelTransformParent = FindModel(base.transform);
		modelName = entityClass.mesh.name;
		bool flag = entity.RootMotion || bHasRagdoll;
		if (GameManager.IsDedicatedServer && !flag)
		{
			avatarController = base.transform.gameObject.GetComponent<AvatarControllerDummy>();
			Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			if (entity is EntityPlayerLocal && entityClass.Properties.Values.ContainsKey(EntityClass.PropLocalAvatarController))
			{
				avatarController = base.transform.gameObject.GetComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropLocalAvatarController])) as AvatarController;
			}
			else if (entityClass.Properties.Values.ContainsKey(EntityClass.PropAvatarController))
			{
				avatarController = base.transform.gameObject.GetComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropAvatarController])) as AvatarController;
			}
			if (GameManager.IsDedicatedServer && avatarController != null && flag)
			{
				avatarController.SetVisible(_b: true);
			}
		}
		InitCommon();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCommon()
	{
		LookAtInit();
		if ((bool)modelTransformParent)
		{
			SwitchModelAndView(_bFPV: false, EntityClass.list[entity.entityClass].bIsMale);
		}
		else
		{
			modelTransformParent = base.transform;
			headTransform = base.transform;
		}
		InitRigidBodies();
		JiggleInit();
	}

	public void InitRigidBodies()
	{
		if (!bipedRootTransform)
		{
			return;
		}
		List<Rigidbody> list = new List<Rigidbody>();
		bipedRootTransform.GetComponentsInChildren(list);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].gameObject.CompareTag("AudioRigidBody"))
			{
				list.RemoveAt(num);
			}
		}
		float num2 = EntityClass.list[entity.entityClass].MassKg / (float)list.Count;
		if (list.Count == 11)
		{
			num2 *= 1.1224489f;
			for (int i = 0; i < list.Count; i++)
			{
				Rigidbody rigidbody = list[i];
				float num3 = 1f;
				bool flag = false;
				switch (rigidbody.name)
				{
				case "Head":
					num3 = 0.8f;
					break;
				case "LeftArm":
				case "RightArm":
					num3 = 0.5f;
					break;
				case "LeftForeArm":
				case "RightForeArm":
					num3 = 0.5f;
					flag = true;
					break;
				case "Spine":
				case "Spine1":
				case "Spine2":
					num3 = 2f;
					flag = true;
					break;
				case "Hips":
					num3 = 2f;
					break;
				case "LeftUpLeg":
				case "RightUpLeg":
					num3 = 1f;
					break;
				case "LeftLeg":
				case "RightLeg":
					num3 = 0.5f;
					flag = true;
					break;
				}
				rigidbody.mass = num2 * num3;
				if (flag && !entity.isEntityRemote)
				{
					rigidbody.gameObject.GetOrAddComponent<CollisionCallForward>().Entity = entity;
				}
				if (rigidbody.drag <= 0f)
				{
					rigidbody.drag = 0.25f;
				}
			}
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].mass = num2;
			}
		}
	}

	public virtual void PostInit()
	{
	}

	public static Transform FindModel(Transform _t)
	{
		Transform transform = _t.Find("Graphics/Model");
		if ((bool)transform)
		{
			return transform;
		}
		return _t;
	}

	public virtual void OnUnload()
	{
	}

	public void OriginChanged(Vector3 _deltaPos)
	{
		ragdollPosePelvisPos += _deltaPos;
	}

	public virtual Vector3 GetHeadPosition()
	{
		if (headTransform == null)
		{
			return entity.position + Vector3.up * entity.GetEyeHeight();
		}
		return headTransform.position + Origin.position;
	}

	public virtual Vector3 GetNavObjectPosition()
	{
		if (NavObjectTransform == null)
		{
			return GetHeadPosition();
		}
		return NavObjectTransform.position + Origin.position;
	}

	public virtual Vector3 GetHipPosition()
	{
		if (bipedPelvisTransform == null)
		{
			return entity.position + Vector3.up * (entity.height * 0.5f);
		}
		return bipedPelvisTransform.position + Origin.position;
	}

	public virtual Vector3 GetChestPosition()
	{
		if (bipedPelvisTransform == null || headTransform == null)
		{
			return Vector3.Lerp(GetHipPosition(), GetHeadPosition(), 0.4f);
		}
		return Vector3.Lerp(bipedPelvisTransform.position, headTransform.position, 0.6f) + Origin.position;
	}

	public virtual Vector3 GetBellyPosition()
	{
		if (bipedPelvisTransform == null || headTransform == null)
		{
			return Vector3.Lerp(GetHipPosition(), GetHeadPosition(), 0.2f);
		}
		return Vector3.Lerp(bipedPelvisTransform.position, headTransform.position, 0.2f) + Origin.position;
	}

	public IKController AddIKController()
	{
		IKController iKController = null;
		Transform transform = GetModelTransform();
		if ((bool)transform)
		{
			Animator componentInChildren = transform.GetComponentInChildren<Animator>();
			if ((bool)componentInChildren)
			{
				iKController = componentInChildren.GetComponent<IKController>();
				if (!iKController)
				{
					iKController = componentInChildren.gameObject.AddComponent<IKController>();
				}
			}
		}
		return iKController;
	}

	public void RemoveIKController()
	{
		Transform transform = GetModelTransform();
		if ((bool)transform)
		{
			IKController componentInChildren = transform.GetComponentInChildren<IKController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Cleanup();
				UnityEngine.Object.Destroy(componentInChildren);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CrouchUpdate(EntityAlive _ea)
	{
		Transform obj = neckParentTransform;
		Transform parent = obj.parent;
		float crouchBendPer = _ea.crouchBendPer;
		Quaternion quaternion = Quaternion.Euler(28f * crouchBendPer, 0f, 0f);
		obj.localRotation *= quaternion;
		parent.localRotation *= quaternion;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LookAtInit()
	{
		EntityClass entityClass = EntityClass.list[entity.entityClass];
		lookAtMaxAngle = entityClass.LookAtAngle;
		lookAtEnabled = lookAtMaxAngle > 0f;
	}

	public void ClearLookAt()
	{
		lookAtBlendPerTarget = 0f;
	}

	public void SetLookAt(Vector3 _pos)
	{
		lookAtPos = _pos;
		lookAtBlendPerTarget = lookAtFullBlendPer;
		lookAtIsPos = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetLookAt()
	{
		lookAtBlendPer = 0f;
		lookAtBlendPerTarget = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LookAtUpdate(EntityAlive e)
	{
		if (gazeController != null)
		{
			return;
		}
		EnumEntityStunType currentStun = e.bodyDamage.CurrentStun;
		float deltaTime = Time.deltaTime;
		if (e.IsDead() || (currentStun != EnumEntityStunType.None && currentStun != EnumEntityStunType.Getup))
		{
			lookAtBlendPerTarget = 0f;
		}
		else if (!lookAtIsPos)
		{
			lookAtBlendPerTarget -= deltaTime;
			EntityAlive attackTargetLocal = e.GetAttackTargetLocal();
			if ((bool)attackTargetLocal && e.CanSee(attackTargetLocal))
			{
				lookAtPos = attackTargetLocal.getHeadPosition();
				lookAtBlendPerTarget = lookAtFullBlendPer;
			}
		}
		if (lookAtBlendPer <= 0f && lookAtBlendPerTarget <= 0f)
		{
			return;
		}
		lookAtFullChangeTime -= deltaTime;
		if (lookAtFullChangeTime <= 0f)
		{
			lookAtFullChangeTime = 1.3f + 2.7f * e.rand.RandomFloat;
			lookAtFullBlendPer = 0.2f + 1.5f * e.rand.RandomFloat;
			if (lookAtFullBlendPer > 1f)
			{
				lookAtFullBlendPer = 1f;
			}
		}
		lookAtBlendPer = Mathf.MoveTowards(lookAtBlendPer, lookAtBlendPerTarget, deltaTime * 1.5f);
		Quaternion rotation = neckParentTransform.rotation;
		Transform transform = headTransform;
		Vector3 upwards = rotation * Vector3.up;
		Quaternion to;
		if (entity is EntityNPC && !(avatarController is AvatarSDCSController))
		{
			to = Quaternion.LookRotation(lookAtPos - Origin.position - transform.position);
			to *= Quaternion.AngleAxis(-90f, Vector3.forward);
		}
		else
		{
			to = Quaternion.LookRotation(lookAtPos - Origin.position - transform.position, upwards);
			to *= Quaternion.Slerp(Quaternion.identity, transform.localRotation, 0.5f);
		}
		Quaternion b = Quaternion.RotateTowards(rotation, to, lookAtMaxAngle);
		lookAtRot = Quaternion.Slerp(lookAtRot, b, 0.16f);
		float num = lookAtBlendPer;
		neckTransform.rotation = Quaternion.Slerp(neckTransform.rotation, lookAtRot, num * 0.4f);
		Quaternion rotation2 = transform.rotation;
		transform.rotation = Quaternion.Slerp(rotation2, lookAtRot, num);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		FrameUpdateRagdoll();
		if (modelTransformParent != headTransform)
		{
			UpdateHeadState();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (ragdollIsBlending)
		{
			BlendRagdoll();
		}
		if (entity is EntityAlive entityAlive && !IsRagdollActive)
		{
			if (entityAlive.crouchType > 0)
			{
				CrouchUpdate(entityAlive);
			}
			if (lookAtEnabled)
			{
				LookAtUpdate(entityAlive);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Transform GetModelTransform()
	{
		if (!modelTransform)
		{
			return modelTransformParent;
		}
		return modelTransform;
	}

	public Transform GetModelTransformParent()
	{
		return modelTransformParent;
	}

	public virtual Transform GetHitTransform(DamageSource _damageSource)
	{
		string hitTransformName = _damageSource.getHitTransformName();
		if (hitTransformName != null && (bool)bipedRootTransform)
		{
			return bipedRootTransform.FindInChilds(hitTransformName);
		}
		return null;
	}

	public virtual Transform GetHitTransform(BodyPrimaryHit _primary)
	{
		if (physicsBody != null)
		{
			string text;
			switch (_primary)
			{
			case BodyPrimaryHit.Head:
				text = "E_BP_Head";
				break;
			case BodyPrimaryHit.Torso:
				text = "E_BP_Body";
				break;
			case BodyPrimaryHit.LeftUpperArm:
				text = "E_BP_LArm";
				break;
			case BodyPrimaryHit.LeftUpperLeg:
				text = "E_BP_LLeg";
				break;
			case BodyPrimaryHit.LeftLowerArm:
				text = "E_BP_LLowerArm";
				break;
			case BodyPrimaryHit.LeftLowerLeg:
				text = "E_BP_LLowerLeg";
				break;
			case BodyPrimaryHit.RightUpperArm:
				text = "E_BP_RArm";
				break;
			case BodyPrimaryHit.RightUpperLeg:
				text = "E_BP_RLeg";
				break;
			case BodyPrimaryHit.RightLowerArm:
				text = "E_BP_RLowerArm";
				break;
			case BodyPrimaryHit.RightLowerLeg:
				text = "E_BP_RLowerLeg";
				break;
			default:
				return null;
			}
			return physicsBody.GetTransformForColliderTag(text);
		}
		return null;
	}

	public virtual Transform GetHeadTransform()
	{
		if (!headTransform)
		{
			return base.transform;
		}
		return headTransform;
	}

	public virtual Transform GetPelvisTransform()
	{
		return bipedPelvisTransform;
	}

	public virtual Transform GetThirdPersonCameraTransform()
	{
		return base.transform;
	}

	public virtual void OnDeath(DamageResponse _dmResponse, ChunkCluster _cc)
	{
		EntityAlive entityAlive = entity as EntityAlive;
		bool flag = (bool)entityAlive && entityAlive.bodyDamage.CurrentStun != EnumEntityStunType.None;
		bool flag2 = true;
		if (HasRagdoll() && (flag2 || !entity.HasDeathAnim || entityAlive.IsSleeper || entityAlive.GetWalkType() == 21 || flag || _dmResponse.Random < ragdollChance))
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				DoRagdoll(_dmResponse);
			}
			else if (!entityAlive.IsSpawned())
			{
				SpawnWithRagdoll();
			}
		}
		else
		{
			if (!(avatarController != null))
			{
				return;
			}
			avatarController.StartDeathAnimation(_dmResponse.HitBodyPart, _dmResponse.MovementState, _dmResponse.Random);
			if (!(entity is EntityPlayer) || !bipedRootTransform)
			{
				return;
			}
			Transform transform = bipedRootTransform.Find("Spine1");
			if ((bool)transform)
			{
				if (transform.TryGetComponent<RagdollWhenHit>(out var component))
				{
					component.enabled = true;
				}
				else
				{
					transform.gameObject.AddComponent<RagdollWhenHit>();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void restoreTPose(PhysicsBodyInstance physicsBody)
	{
	}

	public virtual bool HasRagdoll()
	{
		if (bHasRagdoll)
		{
			return physicsBody != null;
		}
		return false;
	}

	public void DoRagdoll(float stunTime, EnumBodyPartHit bodyPart, Vector3 forceVec, Vector3 forceWorldPos, bool isRemote)
	{
		if (entity.IsFlyMode.Value || (bool)entity.AttachedToEntity)
		{
			return;
		}
		if (!isRemote && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entity.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(entity, stunTime, bodyPart, forceVec, forceWorldPos));
			return;
		}
		bool flag = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entity.isEntityRemote;
		if (stunTime == 0f)
		{
			if (!flag)
			{
				entity.PhysicsPush(forceVec, forceWorldPos);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(entity, 0f, EnumBodyPartHit.Torso, forceVec, forceWorldPos));
			}
		}
		else
		{
			if (!StartRagdoll(stunTime))
			{
				return;
			}
			if (forceVec.sqrMagnitude > 0f && !entity.isEntityRemote)
			{
				Vector3 vector = forceVec;
				if (bodyPart == EnumBodyPartHit.None)
				{
					float num = -10f;
					if (vector.y < num)
					{
						vector.y = num;
					}
					SetRagdollVelocity(vector);
				}
				else
				{
					BodyPrimaryHit primary = bodyPart.ToPrimary();
					Transform hitTransform = GetHitTransform(primary);
					if ((bool)hitTransform)
					{
						Rigidbody component = hitTransform.GetComponent<Rigidbody>();
						if ((bool)component)
						{
							if (forceWorldPos.sqrMagnitude > 0f)
							{
								component.AddForceAtPosition(vector, forceWorldPos - Origin.position, ForceMode.Impulse);
							}
							else
							{
								component.AddForce(vector, ForceMode.Impulse);
							}
						}
					}
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(entity, stunTime, bodyPart, forceVec, forceWorldPos));
			}
		}
	}

	public void DoRagdoll(DamageResponse dr, float stunTime = 999999f)
	{
		EntityAlive entityAlive = entity as EntityAlive;
		if ((bool)entityAlive && entityAlive.isDisintegrated)
		{
			return;
		}
		float num = dr.Strength;
		DamageSource source = dr.Source;
		if (num > 0f && source != null)
		{
			Vector3 vector = source.getDirection();
			EnumDamageTypes damageType = source.GetDamageType();
			if (damageType != EnumDamageTypes.Falling && damageType != EnumDamageTypes.Crushing)
			{
				float num2;
				if (dr.HitBodyPart == EnumBodyPartHit.None)
				{
					num2 = entity.rand.RandomRange(5f, 25f);
				}
				else
				{
					float min = -10f;
					if (stunTime == 0f)
					{
						min = 5f;
					}
					num2 = entity.rand.RandomRange(min, 40f);
					num *= 0.5f;
					if (source.damageType == EnumDamageTypes.Bashing)
					{
						num *= 2.5f;
					}
					if (dr.Critical)
					{
						num2 += 25f;
						num *= 2f;
					}
					if ((dr.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
					{
						num *= 0.45f;
					}
					num = Utils.FastMin(20f + num, 500f);
					vector *= num;
				}
				Vector3 axis = Vector3.Cross(vector.normalized, Vector3.up);
				vector = Quaternion.AngleAxis(num2, axis) * vector;
			}
			DoRagdoll(stunTime, dr.HitBodyPart, vector, source.getHitTransformPosition(), isRemote: false);
		}
		else
		{
			DoRagdoll(stunTime, dr.HitBodyPart, Vector3.zero, Vector3.zero, isRemote: false);
		}
	}

	public void SetRagdollState(int newState)
	{
		if (ragdollState == ERagdollState.On && newState == 2)
		{
			ragdollTime = 9999f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnWithRagdoll()
	{
		if (ragdollState == ERagdollState.Off)
		{
			ragdollState = ERagdollState.SpawnWait;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StartRagdoll(float stunTime)
	{
		if (!HasRagdoll())
		{
			return false;
		}
		if (ragdollState == ERagdollState.Dead)
		{
			return true;
		}
		bool flag = entity.IsDead();
		if (!flag && ragdollState == ERagdollState.On)
		{
			ragdollDuration = Utils.FastMax(ragdollDuration, stunTime);
			return true;
		}
		if (entity.IsMarkedForUnload())
		{
			return false;
		}
		ragdollAnimator = avatarController.GetAnimator();
		if (!ragdollAnimator)
		{
			return false;
		}
		bool num = ragdollState != ERagdollState.Off;
		ragdollState = ERagdollState.On;
		ragdollTime = 0f;
		entity.OnRagdoll(isActive: true);
		ragdollIsPlayer = entity is EntityPlayer;
		ragdollAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		ragdollAnimator.keepAnimatorStateOnDisable = true;
		ragdollAnimator.enabled = false;
		Animation component = GetModelTransform().GetComponent<Animation>();
		if ((bool)component)
		{
			component.cullingType = AnimationCullingType.AlwaysAnimate;
			component.enabled = false;
		}
		CaptureRagdollBones();
		CaptureRagdollZeroBones();
		if (flag)
		{
			stunTime = 0.3f;
			SetRagdollDead();
		}
		if (physicsBody != null)
		{
			physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Ragdoll);
		}
		entity.PhysicsPause();
		EntityAlive obj = entity as EntityAlive;
		obj.SetStun(EnumEntityStunType.Prone);
		obj.bodyDamage.StunDuration = 1f;
		obj.SetCVar("ragdoll", 1f);
		if (!ragdollIsPlayer)
		{
			entity.PhysicsTransform.gameObject.SetActive(value: false);
		}
		ragdollTime = 0f;
		ragdollDuration = stunTime;
		ragdollRotY = entity.rotation.y;
		if (ragdollIsPlayer)
		{
			ragdollDuration *= 0.5f;
		}
		if (num)
		{
			ragdollIsBlending = false;
			ragdollAdjustPosDelay = 0f;
		}
		ResetLookAt();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollDead()
	{
		ragdollState = ERagdollState.Dead;
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			Rigidbody rb = ragdollPoses[i].rb;
			if ((bool)rb)
			{
				rb.maxDepenetrationVelocity = 2f;
				rb.maxAngularVelocity = 1f;
			}
		}
	}

	public void DisableRagdoll(bool isSetAlive)
	{
		if (ragdollState == ERagdollState.Off)
		{
			return;
		}
		if (physicsBody != null)
		{
			physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Collision);
		}
		if ((bool)bipedRootTransform)
		{
			RagdollWhenHit[] componentsInChildren = bipedRootTransform.GetComponentsInChildren<RagdollWhenHit>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
			}
		}
		EntityAlive entityAlive = entity as EntityAlive;
		if (!isSetAlive && !entity.IsDead())
		{
			Vector3 vector = headTransform.position - bipedPelvisTransform.position;
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			ragdollIsFacingUp = false;
			ragdollIsAnimal = EntityClass.list[entityAlive.entityClass].bIsAnimalEntity;
			string stateName;
			if (ragdollIsAnimal)
			{
				stateName = "Knockdown";
			}
			else
			{
				stateName = "Knockdown - Chest";
				if (bipedPelvisTransform.forward.y > 0f)
				{
					ragdollIsFacingUp = true;
					stateName = "Knockdown - Back";
					num += 180f;
				}
			}
			ragdollRotY = num;
			CopyRagdollRot();
			Animation component = GetModelTransform().GetComponent<Animation>();
			if (component != null)
			{
				component.cullingType = AnimationCullingType.AlwaysAnimate;
				component.enabled = true;
			}
			avatarController.ResetAnimations();
			ragdollAnimator.keepAnimatorStateOnDisable = true;
			int layer = (ragdollIsPlayer ? 5 : 0);
			ragdollAnimator.CrossFadeInFixedTime(stateName, 0.25f, layer, 2f, 0f);
			ragdollAdjustPosDelay = 0.05f;
			ragdollState = ERagdollState.BlendOutGround;
			ragdollTime = 0f;
			ragdollIsBlending = true;
			entityAlive.SetStun(EnumEntityStunType.Getup);
		}
		else
		{
			bipedPelvisTransform.localPosition = ragdollPosePelvisLocalPos;
			RestoreRagdollStartRot();
			SetRagdollOff();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CaptureRagdollBones()
	{
		if (ragdollPoses.Count > 0)
		{
			return;
		}
		Animator animator = avatarController.GetAnimator();
		if (!animator)
		{
			return;
		}
		ragdollPosePelvisLocalPos = bipedPelvisTransform.localPosition;
		animator.GetComponentsInChildren(ragdollTempRBs);
		RagdollPose item = default(RagdollPose);
		for (int i = 0; i < ragdollTempRBs.Count; i++)
		{
			Rigidbody rigidbody = ragdollTempRBs[i];
			GameObject gameObject = rigidbody.gameObject;
			if (!gameObject.CompareTag("Item") && !gameObject.CompareTag("AudioRigidBody"))
			{
				Transform transform = (item.t = rigidbody.transform);
				item.rb = rigidbody;
				item.rot = Quaternion.identity;
				item.startRot = transform.localRotation;
				ragdollPoses.Add(item);
			}
		}
		ragdollTempRBs.Clear();
	}

	public void CaptureRagdollPositions(List<Vector3> positionList)
	{
		CaptureRagdollBones();
		positionList.Clear();
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			positionList.Add(ragdollPoses[i].t.position);
		}
	}

	public void ApplyRagdollVelocities(List<Vector3> velocities)
	{
		if (velocities.Count != ragdollPoses.Count)
		{
			return;
		}
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			Rigidbody rb = ragdollPoses[i].rb;
			if ((bool)rb)
			{
				rb.velocity = velocities[i];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollVelocity(Vector3 _vel)
	{
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			ragdollPoses[i].rb.velocity = _vel;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyRagdollRot()
	{
		ragdollPosePelvisPos = bipedPelvisTransform.position;
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			RagdollPose value = ragdollPoses[i];
			if (value.t == bipedPelvisTransform)
			{
				value.rot = value.t.rotation;
			}
			else
			{
				value.rot = value.t.localRotation;
			}
			ragdollPoses[i] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RestoreRagdollStartRot()
	{
		for (int i = 0; i < ragdollPoses.Count; i++)
		{
			ragdollPoses[i].t.localRotation = ragdollPoses[i].startRot;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CaptureRagdollZeroBones()
	{
		if (ragdollZeroBones != null)
		{
			return;
		}
		Transform parent = headTransform;
		if (!parent)
		{
			return;
		}
		ragdollZeroTime = 0.33f;
		ragdollZeroBones = new List<Transform>();
		while ((bool)(parent = parent.parent) && !(parent == bipedPelvisTransform))
		{
			if (!parent.GetComponent<Rigidbody>())
			{
				GameObject gameObject = parent.gameObject;
				if (!gameObject.CompareTag("Item") && !gameObject.CompareTag("AudioRigidBody"))
				{
					ragdollZeroBones.Add(parent);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlendRagdollZeroBones()
	{
		if (ragdollZeroTime > 0f)
		{
			float deltaTime = Time.deltaTime;
			ragdollZeroTime -= deltaTime;
			for (int i = 0; i < ragdollZeroBones.Count; i++)
			{
				Transform obj = ragdollZeroBones[i];
				obj.localRotation = Quaternion.RotateTowards(obj.localRotation, Quaternion.identity, 120f * deltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlendRagdoll()
	{
		if (ragdollAdjustPosDelay > 0f)
		{
			ragdollAdjustPosDelay -= Time.deltaTime;
			if (ragdollAdjustPosDelay <= 0f)
			{
				Vector3 vector = bipedPelvisTransform.position - modelTransform.position;
				Vector3 vector2 = ragdollPosePelvisPos;
				vector2.x -= vector.x;
				vector2.z -= vector.z;
				if (entity.isEntityRemote)
				{
					vector2 = Vector3.Lerp(vector2, entity.targetPos - Origin.position, Time.fixedDeltaTime * 10f);
				}
				for (int i = 0; i < 5; i++)
				{
					if (!Physics.Raycast(vector2, Vector3.down, out var hitInfo, 3f, -538750981))
					{
						break;
					}
					RootTransformRefEntity component = hitInfo.transform.GetComponent<RootTransformRefEntity>();
					if (!component || component.RootTransform != entity.transform)
					{
						vector2.y = hitInfo.point.y + 0.02f;
						break;
					}
					vector2.y = hitInfo.point.y - 0.01f;
				}
				entity.PhysicsResume(vector2 + Origin.position, ragdollRotY);
			}
		}
		float num = ragdollTime / 0.7f;
		float num2 = num;
		if (!ragdollIsAnimal)
		{
			num2 = (num - 0.2f) / 0.8f;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		bipedPelvisTransform.position = Vector3.Lerp(ragdollPosePelvisPos, bipedPelvisTransform.position, num2);
		for (int j = 0; j < ragdollPoses.Count; j++)
		{
			Transform t = ragdollPoses[j].t;
			if (t == bipedPelvisTransform)
			{
				t.rotation = Quaternion.Slerp(ragdollPoses[j].rot, t.rotation, num2);
			}
			else
			{
				t.localRotation = Quaternion.Slerp(ragdollPoses[j].rot, t.localRotation, num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FrameUpdateRagdoll()
	{
		if (ragdollState == ERagdollState.Off)
		{
			return;
		}
		if (ragdollState == ERagdollState.SpawnWait)
		{
			Chunk chunk = (Chunk)entity.world.GetChunkFromWorldPos(entity.GetBlockPosition());
			if (chunk != null && chunk.IsCollisionMeshGenerated && chunk.IsDisplayed)
			{
				ragdollState = ERagdollState.Off;
				StartRagdoll(float.MaxValue);
			}
			return;
		}
		bool flag = entity.IsDead();
		if ((bool)pelvisRB && IsRagdollMovement)
		{
			if (!flag && entity.isEntityRemote)
			{
				Vector3 position = pelvisRB.position;
				Vector3 vector = entity.targetPos - Origin.position;
				pelvisRB.AddForce((0f - serverPosSpringForce) * (position - vector) - serverPosSpringDamping * pelvisRB.velocity, ForceMode.Acceleration);
			}
			if (!(entity is EntityPlayer))
			{
				entity.SetPosition(pelvisRB.position + Origin.position, _bUpdatePhysics: false);
			}
			else if (!entity.isEntityRemote)
			{
				entity.SetPosition(pelvisRB.position + Origin.position, _bUpdatePhysics: false);
			}
		}
		entity.SetRotationAndStopTurning(new Vector3(0f, ragdollRotY, 0f));
		ragdollTime += Time.deltaTime;
		switch (ragdollState)
		{
		case ERagdollState.On:
			BlendRagdollZeroBones();
			if (!(ragdollTime >= ragdollDuration) || (!(pelvisRB.velocity.sqrMagnitude <= 0.25f) && !(ragdollTime > 10f)))
			{
				break;
			}
			DisableRagdoll(isSetAlive: false);
			if (!entity.isEntityRemote)
			{
				NetPackageEntityRagdoll package = NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(entity, (sbyte)ragdollState);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entity.entityId, -1, package);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
				}
			}
			break;
		case ERagdollState.BlendOutGround:
			RestoreRagdollStartRot();
			if (ragdollTime >= 0.25f)
			{
				string stateName = "GetUpChest";
				if (ragdollIsFacingUp)
				{
					stateName = "GetUpBack";
				}
				if ((entity as EntityAlive).IsWalkTypeACrawl())
				{
					stateName = "CrawlerGetUpChest";
				}
				int layer = (ragdollIsPlayer ? 5 : 0);
				ragdollAnimator.CrossFade(stateName, 0.3f, layer);
				ragdollState = ERagdollState.BlendOutStand;
			}
			break;
		case ERagdollState.BlendOutStand:
			RestoreRagdollStartRot();
			if (ragdollTime >= 0.7f)
			{
				ragdollIsBlending = false;
				ragdollState = ERagdollState.Stand;
				ragdollTime = 0f;
			}
			break;
		case ERagdollState.Stand:
			if (ragdollTime >= 0.8f)
			{
				if (!ragdollIsPlayer)
				{
					entity.PhysicsTransform.gameObject.SetActive(value: true);
				}
				ragdollState = ERagdollState.StandCollide;
			}
			break;
		case ERagdollState.StandCollide:
			if (ragdollTime >= 1.7f)
			{
				SetRagdollOff();
				entity.OnRagdoll(isActive: false);
			}
			break;
		case ERagdollState.Dead:
			BlendRagdollZeroBones();
			if (ragdollTime >= ragdollDuration)
			{
				ragdollDuration = float.MaxValue;
				if (physicsBody != null)
				{
					physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.RagdollDead);
				}
			}
			break;
		case ERagdollState.SpawnWait:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollOff()
	{
		ragdollState = ERagdollState.Off;
		EntityAlive obj = entity as EntityAlive;
		obj.SetCVar("ragdoll", 0f);
		obj.ClearStun();
		if (!ragdollIsPlayer)
		{
			entity.PhysicsTransform.gameObject.SetActive(value: true);
		}
		ragdollPoses.Clear();
		ragdollZeroBones = null;
		CheckAnimFreeze();
	}

	public string GetRagdollDebugInfo()
	{
		return $"{ragdollTime.ToCultureInvariantString():0.#}/{ragdollDuration.ToCultureInvariantString():0.#} {ragdollState.ToStringCached()}";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ClothSimInit()
	{
		clothSim = GetComponentsInChildren<Cloth>();
		if (clothSim != null)
		{
			for (int num = clothSim.Length - 1; num >= 0; num--)
			{
				clothSim[num].gameObject.SetActive(value: false);
			}
			if (GameManager.IsDedicatedServer)
			{
				clothSim = null;
			}
			isClothSimOn = false;
		}
	}

	public void ClothSimOn(bool _on, bool _force = false)
	{
		if (clothSim == null || (isClothSimOn == _on && !_force))
		{
			return;
		}
		bool flag = entity is EntityPlayerLocal;
		isClothSimOn = _on;
		for (int num = clothSim.Length - 1; num >= 0; num--)
		{
			Cloth cloth = clothSim[num];
			cloth.gameObject.SetActive(_on);
			if (_on && flag)
			{
				cloth.worldAccelerationScale = 0.3f;
			}
		}
	}

	public void ClearClothMotion()
	{
		for (int num = clothSim.Length - 1; num >= 0; num--)
		{
			clothSim[num].ClearTransformMotion();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void JiggleInit()
	{
		jiggles = GetComponentsInChildren<Jiggle>();
		if (GameManager.IsDedicatedServer && jiggles != null)
		{
			for (int num = jiggles.Length - 1; num >= 0; num--)
			{
				jiggles[num].gameObject.SetActive(value: false);
			}
			jiggles = null;
		}
	}

	public void JiggleOn(bool _on)
	{
		if (jiggles != null && isJiggleOn != _on)
		{
			isJiggleOn = _on;
			for (int num = jiggles.Length - 1; num >= 0; num--)
			{
				jiggles[num].gameObject.SetActive(_on);
			}
		}
	}

	public virtual void SwitchModelAndView(bool _bFPV, bool _bMale)
	{
		if (GetModelTransform() != null)
		{
			Animator component = GetModelTransform().GetComponent<Animator>();
			if (component != null)
			{
				component.enabled = !_bFPV;
			}
		}
		IsFPV = _bFPV;
		if (modelName != null && modelTransformParent != null)
		{
			modelTransform = modelTransformParent.Find(modelName);
			meshTransform = GameUtils.FindTagInDirectChilds(modelTransform, "E_Mesh");
			if (!meshTransform)
			{
				meshTransform = modelTransform.Find("LOD0");
				if (!meshTransform)
				{
					Renderer componentInChildren = modelTransform.GetComponentInChildren<Renderer>();
					if ((bool)componentInChildren)
					{
						meshTransform = componentInChildren.transform;
					}
				}
			}
		}
		Transform transform = GetModelTransform();
		if ((bool)avatarController)
		{
			avatarController.SwitchModelAndView(modelName, _bFPV, _bMale);
		}
		else if ((bool)transform)
		{
			transform.gameObject.SetActive(value: true);
		}
		headTransform = GameUtils.FindTagInChilds(transform, "E_BP_Head");
		if ((bool)headTransform)
		{
			neckTransform = headTransform.parent;
			neckParentTransform = neckTransform.parent;
		}
		bipedRootTransform = GameUtils.FindTagInChilds(transform, "E_BP_BipedRoot");
		if (bipedRootTransform == null)
		{
			string[] array = commonBips;
			foreach (string n in array)
			{
				if ((bipedRootTransform = transform.Find(n)) != null || (bipedRootTransform = transform.FindInChilds(n)) != null)
				{
					break;
				}
			}
		}
		if (bipedRootTransform != null)
		{
			if (bipedRootTransform.name != "pelvis" && bipedRootTransform.name != "Hips")
			{
				bipedPelvisTransform = GameUtils.FindChildWithPartialName(bipedRootTransform, "pelvis");
				if (bipedPelvisTransform == null)
				{
					bipedPelvisTransform = GameUtils.FindChildWithPartialName(bipedRootTransform, "hips");
					if (bipedPelvisTransform == null)
					{
						bipedPelvisTransform = GameUtils.FindChildWithPartialName(bipedRootTransform, "hip");
					}
				}
			}
			else
			{
				bipedPelvisTransform = bipedRootTransform;
			}
		}
		if ((bool)bipedPelvisTransform)
		{
			pelvisRB = bipedPelvisTransform.GetComponent<Rigidbody>();
			if (!(entity is EntityPlayer))
			{
				SkinnedMeshRenderer[] componentsInChildren = modelTransformParent.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
				{
					if (skinnedMeshRenderer.quality == SkinQuality.Auto)
					{
						skinnedMeshRenderer.quality = SkinQuality.Bone2;
					}
					Bounds localBounds = skinnedMeshRenderer.localBounds;
					if (skinnedMeshRenderer.CompareTag("E_BP_Eye"))
					{
						if ((bool)headTransform)
						{
							skinnedMeshRenderer.rootBone = headTransform;
							localBounds.center = Vector3.zero;
							localBounds.extents = new Vector3(0.25f, 0.25f, 0.25f);
						}
						skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
					}
					else if (skinnedMeshRenderer.rootBone != bipedPelvisTransform)
					{
						skinnedMeshRenderer.rootBone = bipedPelvisTransform;
						localBounds.center += -bipedPelvisTransform.localPosition;
					}
					skinnedMeshRenderer.localBounds = localBounds;
				}
			}
		}
		EntityAlive entityAlive = entity as EntityAlive;
		if ((bool)entityAlive && entityAlive.inventory != null)
		{
			if (entity.isEntityRemote)
			{
				entityAlive.inventory.ForceHoldingItemUpdate();
			}
			else if (entity is EntityPlayerLocal && (entity as EntityPlayerLocal).PlayerUI != null && !(entity as EntityPlayerLocal).PlayerUI.windowManager.IsWindowOpen("character"))
			{
				entityAlive.inventory.ForceHoldingItemUpdate();
			}
		}
		if (GetRightHandTransform() != null)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = GetRightHandTransform().GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				componentsInChildren2[k].updateWhenOffscreen = true;
			}
		}
		NavObjectTransform = base.transform.FindInChilds("IconTag");
		PhysicsBodyLayout physicsBodyLayout = EntityClass.list[entity.entityClass].PhysicsBody;
		if (physicsBodyLayout != null && bipedRootTransform != null)
		{
			if (physicsBody != null)
			{
				physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Disabled);
			}
			physicsBody = new PhysicsBodyInstance(bipedRootTransform, physicsBodyLayout, EnumColliderMode.Collision);
			physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Collision);
			RagdollWhenHit[] componentsInChildren3 = bipedRootTransform.GetComponentsInChildren<RagdollWhenHit>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
		}
		else
		{
			physicsBody = null;
		}
		if (transform != null)
		{
			Animator component2 = transform.GetComponent<Animator>();
			if (component2 != null)
			{
				component2.enabled = !IsFPV;
			}
			SetColliderLayers(transform, 0);
		}
		CheckAnimFreeze();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetColliderLayers(Transform modelT, int layer)
	{
		CapsuleCollider[] componentsInChildren = modelT.GetComponentsInChildren<CapsuleCollider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			if (!gameObject.CompareTag("LargeEntityBlocker") && !gameObject.CompareTag("Physics"))
			{
				gameObject.layer = layer;
			}
		}
		BoxCollider[] componentsInChildren2 = modelT.GetComponentsInChildren<BoxCollider>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.layer = layer;
		}
	}

	public virtual void SetInRightHand(Transform _transform)
	{
		if (avatarController != null)
		{
			avatarController.SetInRightHand(_transform);
		}
	}

	public virtual void SetAlive()
	{
		DisableRagdoll(isSetAlive: true);
		Transform transform = GetModelTransform();
		if ((bool)transform)
		{
			Utils.MoveTaggedToLayer(transform.gameObject, "LargeEntityBlocker", 19);
			Animator component = transform.GetComponent<Animator>();
			if (component != null)
			{
				component.enabled = !IsFPV;
			}
		}
		if (avatarController != null)
		{
			avatarController.SetAlive();
		}
	}

	public virtual void SetDead()
	{
		Transform transform = GetModelTransform();
		if ((bool)transform)
		{
			Utils.MoveTaggedToLayer(transform.gameObject, "LargeEntityBlocker", 17);
		}
		if (ragdollState == ERagdollState.On)
		{
			SetRagdollDead();
			if (physicsBody != null && physicsBody.Mode != EnumColliderMode.RagdollDead)
			{
				physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.RagdollDead);
			}
		}
	}

	public virtual void SetVisible(bool _bVisible, bool _isKeepColliders = false)
	{
		visible = _bVisible;
		if (_isKeepColliders)
		{
			modelTransformParent.GetComponentsInChildren(rendererList);
			for (int i = 0; i < rendererList.Count; i++)
			{
				rendererList[i].enabled = _bVisible;
			}
			rendererList.Clear();
			if (!_bVisible)
			{
				return;
			}
		}
		if (avatarController != null)
		{
			avatarController.SetVisible(_bVisible);
		}
	}

	public void SetFade(float _fade)
	{
		if (matPropBlock == null)
		{
			matPropBlock = new MaterialPropertyBlock();
		}
		modelTransformParent.GetComponentsInChildren(rendererList);
		for (int i = 0; i < rendererList.Count; i++)
		{
			Renderer renderer = rendererList[i];
			bool flag = false;
			Material material = renderer.material;
			string text = material.shader.name;
			if (material.HasProperty("_Fade") && text.Contains("Game/Character"))
			{
				flag = true;
			}
			if (renderer.gameObject.CompareTag("LOD") || renderer.gameObject.CompareTag("E_Mesh") || flag)
			{
				matPropBlock.SetFloat(fadeId, _fade);
				renderer.SetPropertyBlock(matPropBlock);
			}
		}
		rendererList.Clear();
	}

	public virtual Transform GetRightHandTransform()
	{
		if (avatarController != null)
		{
			return avatarController.GetRightHandTransform();
		}
		return null;
	}

	public virtual void SetSkinTexture(string _textureName)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createModel(World _world, EntityClass _ec)
	{
		if (modelTransformParent == null)
		{
			return;
		}
		Transform transform = null;
		if (_ec.IsPrefabCombined && modelTransformParent.childCount > 0)
		{
			transform = modelTransformParent.GetChild(0);
		}
		if ((bool)_ec.mesh)
		{
			transform = UnityEngine.Object.Instantiate(_ec.mesh, modelTransformParent, worldPositionStays: false);
			transform.name = _ec.mesh.name;
			Vector3 localPosition = transform.localPosition;
			if ((double)localPosition.z < -0.5 || (double)localPosition.z > 0.5)
			{
				Log.Warning("createModel mesh moved {0} {1} {2}", transform.name, localPosition.ToString("f3"), transform.localRotation);
			}
			localPosition.x = 0f;
			localPosition.y = 0f;
			transform.localPosition = localPosition;
		}
		if (!transform)
		{
			return;
		}
		transform.gameObject.SetActive(value: true);
		modelName = transform.name;
		if (_ec.particleOnSpawn.fileName != null)
		{
			ParticleSystem particleSystem = DataLoader.LoadAsset<ParticleSystem>(_ec.particleOnSpawn.fileName);
			if (particleSystem != null)
			{
				ParticleSystem particleSystem2 = UnityEngine.Object.Instantiate(particleSystem);
				particleSystem2.transform.parent = modelTransformParent;
				if (_ec.particleOnSpawn.shapeMesh != null && _ec.particleOnSpawn.shapeMesh.Length > 0)
				{
					SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
					ParticleSystem.ShapeModule shape = particleSystem2.shape;
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					string text = _ec.particleOnSpawn.shapeMesh.ToLower();
					if (text.Contains("setshapetomesh"))
					{
						text = text.Replace("setshapetomesh", "");
						int num = int.Parse(text);
						if (num >= 0 && num < componentsInChildren.Length)
						{
							shape.skinnedMeshRenderer = componentsInChildren[num];
							ParticleSystem[] componentsInChildren2 = particleSystem2.transform.GetComponentsInChildren<ParticleSystem>();
							if (componentsInChildren2 != null)
							{
								for (int i = 0; i < componentsInChildren2.Length; i++)
								{
									shape = componentsInChildren2[i].shape;
									shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
									shape.skinnedMeshRenderer = componentsInChildren[num];
								}
							}
						}
					}
				}
			}
		}
		bool flag = false;
		if (_ec.AltMatNames != null)
		{
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(entity.entityId);
			int num2 = gameRandom.RandomRange(_ec.AltMatNames.Length + 1) - 1;
			if (num2 >= 0)
			{
				Material altMaterial = DataLoader.LoadAsset<Material>(_ec.AltMatNames[num2]);
				AltMaterial = altMaterial;
				flag = true;
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}
		Color value = Color.black;
		string text2 = _ec.Properties.GetString(EntityClass.PropMatColor);
		if (text2.Length > 0)
		{
			value = EntityClass.sColors[text2];
			flag = true;
		}
		if (flag)
		{
			GetComponentsInChildren(includeInactive: true, skinnedRendererList);
			Material material = AltMaterial;
			if (text2.Length > 0)
			{
				if (!material)
				{
					for (int j = 0; j < skinnedRendererList.Count; j++)
					{
						SkinnedMeshRenderer skinnedMeshRenderer = skinnedRendererList[j];
						if (skinnedMeshRenderer.CompareTag("LOD"))
						{
							material = skinnedMeshRenderer.sharedMaterials[0];
							break;
						}
					}
				}
				if (text2.Length > 0)
				{
					material = UnityEngine.Object.Instantiate(material);
					material.SetColor("_EmissiveColor", value);
				}
				AltMaterial = material;
			}
			for (int k = 0; k < skinnedRendererList.Count; k++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer2 = skinnedRendererList[k];
				if (skinnedMeshRenderer2.CompareTag("LOD"))
				{
					Material[] sharedMaterials = skinnedMeshRenderer2.sharedMaterials;
					sharedMaterials[0] = material;
					skinnedMeshRenderer2.materials = sharedMaterials;
				}
			}
			skinnedRendererList.Clear();
		}
		if (_ec.MatSwap == null)
		{
			return;
		}
		Renderer[] componentsInChildren3 = GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren3)
		{
			if (!renderer.CompareTag("LOD"))
			{
				continue;
			}
			Material[] sharedMaterials2 = renderer.sharedMaterials;
			int num3 = Utils.FastMin(sharedMaterials2.Length, _ec.MatSwap.Length);
			for (int m = 0; m < num3; m++)
			{
				string text3 = _ec.MatSwap[m];
				if (text3 != null && text3.Length > 0)
				{
					Material material2 = DataLoader.LoadAsset<Material>(text3);
					if ((bool)material2)
					{
						sharedMaterials2[m] = material2;
					}
				}
			}
			renderer.materials = sharedMaterials2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createAvatarController(EntityClass _ec)
	{
		string propName = ((entity is EntityPlayerLocal) ? EntityClass.PropLocalAvatarController : EntityClass.PropAvatarController);
		propName = _ec.Properties.GetString(propName);
		if (propName.Length > 0)
		{
			Type type = Type.GetType(propName);
			avatarController = base.gameObject.GetComponent(type) as AvatarController;
			if (!avatarController)
			{
				avatarController = base.gameObject.AddComponent(type) as AvatarController;
			}
		}
	}

	public virtual void Detach()
	{
		for (int i = 0; i < modelTransformParent.childCount; i++)
		{
			UnityEngine.Object.Destroy(modelTransformParent.GetChild(i).gameObject);
		}
		UnityEngine.Object.Destroy(avatarController);
		UnityEngine.Object.Destroy(this);
		avatarController = null;
	}

	[Conditional("DEBUG_RAGDOLL")]
	public void LogRagdoll(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Ragdoll {entity.GetDebugName()}, id{entity.entityId}, {ragdollState}, {_format}";
		Log.Warning(_format, _args);
	}

	[Conditional("DEBUG_RAGDOLLDO")]
	public void LogRagdollDo(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Ragdoll Do {entity.GetDebugName()}, id{entity.entityId}, {ragdollState}, time {ragdollTime}, {_format}";
		Log.Warning(_format, _args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckAnimFreeze()
	{
		if (!EAIManager.isAnimFreeze)
		{
			return;
		}
		EntityAlive entityAlive = entity as EntityAlive;
		if ((bool)entityAlive && entityAlive.aiManager != null && (bool)avatarController)
		{
			Animator animator = avatarController.GetAnimator();
			if ((bool)animator)
			{
				animator.enabled = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadState()
	{
		if (headTransform == null)
		{
			return;
		}
		switch (HeadState)
		{
		case HeadStates.Growing:
		{
			float num2 = headTransform.localScale.y + headScaleSpeed * Time.deltaTime;
			if (num2 >= HeadBigSize)
			{
				num2 = HeadBigSize;
				HeadState = HeadStates.BigHead;
				EntityAlive entityAlive2 = entity as EntityAlive;
				if (entityAlive2 != null)
				{
					entityAlive2.CurrentHeadState = HeadState;
				}
			}
			headTransform.localScale = new Vector3(num2, num2, num2);
			break;
		}
		case HeadStates.Shrinking:
		{
			float num = headTransform.localScale.y - headScaleSpeed * Time.deltaTime;
			if (num <= HeadStandardSize)
			{
				num = HeadStandardSize;
				HeadState = HeadStates.Standard;
				EntityAlive entityAlive = entity as EntityAlive;
				if (entityAlive != null)
				{
					entityAlive.CurrentHeadState = HeadState;
				}
			}
			headTransform.localScale = new Vector3(num, num, num);
			break;
		}
		case HeadStates.Standard:
			if (headTransform.localScale.x != HeadStandardSize)
			{
				headTransform.localScale = new Vector3(HeadStandardSize, HeadStandardSize, HeadStandardSize);
			}
			break;
		case HeadStates.BigHead:
			if (headTransform.localScale.x != HeadBigSize)
			{
				headTransform.localScale = new Vector3(HeadBigSize, HeadBigSize, HeadBigSize);
			}
			break;
		}
	}

	public void ForceHeadState(HeadStates headState)
	{
		if (!(headTransform == null))
		{
			HeadState = headState;
			switch (headState)
			{
			case HeadStates.BigHead:
			{
				float headBigSize = HeadBigSize;
				headTransform.localScale = new Vector3(headBigSize, headBigSize, headBigSize);
				break;
			}
			case HeadStates.Standard:
			{
				float headStandardSize = HeadStandardSize;
				headTransform.localScale = new Vector3(headStandardSize, headStandardSize, headStandardSize);
				break;
			}
			}
		}
	}

	public void SetHeadScale(float standard)
	{
		HeadStandardSize = standard;
		HeadBigSize = Mathf.Min(4.5f, standard * 3f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EModelBase()
	{
	}
}
