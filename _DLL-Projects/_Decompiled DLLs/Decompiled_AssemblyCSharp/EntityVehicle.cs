using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class EntityVehicle : EntityAlive, ILockable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct RemoteData
	{
		public struct Part
		{
			public Vector3 pos;

			public Quaternion rot;
		}

		public const int cFHasData = 1;

		public const int cFAccel = 2;

		public const int cFBreak = 4;

		public int Flags;

		public float MotorTorquePercent;

		public float SteeringPercent;

		public Vector3 Velocity;

		public List<Part> parts;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Force
	{
		public enum Trigger
		{
			Off,
			On,
			InputForward,
			InputStrafe,
			InputUp,
			InputDown,
			Motor0,
			Motor1,
			Motor2,
			Motor3,
			Motor4,
			Motor5,
			Motor6,
			Motor7
		}

		public enum Type
		{
			Relative,
			RelativeTorque
		}

		public Vector2 ceiling;

		public Vector3 force;

		public Trigger trigger;

		public Type type;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Motor
	{
		public enum Trigger
		{
			Off,
			On,
			InputForward,
			InputStrafe,
			InputUp,
			InputDown,
			Vel
		}

		public enum Type
		{
			Spin,
			Relative,
			RelativeTorque
		}

		public VPEngine engine;

		public float engineOffPer;

		public float turbo;

		public float rpm;

		public float rpmAccelMin;

		public float rpmAccelMax;

		public float rpmDrag;

		public float rpmMax;

		public Trigger trigger;

		public Type type;

		public Transform transform;

		public int axis;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Wheel
	{
		public float motorTorqueScale;

		public float brakeTorqueScale;

		public string bounceSound;

		public string slideSound;

		public bool isSteerParentOfTire;

		public Transform steerT;

		public Quaternion steerBaseRot;

		public Transform tireT;

		public float tireSpinSpeed;

		public float tireSpin;

		public float tireSuspensionPercent;

		public WheelCollider wheelC;

		public WheelFrictionCurve forwardFriction;

		public float forwardStiffnessBase;

		public WheelFrictionCurve sideFriction;

		public float sideStiffnessBase;

		public float slideTime;

		public float ptlTime;

		public bool isGrounded;
	}

	public class VehicleInventory : Inventory
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int cSlotCount;

		public VehicleInventory(IGameManager _gameManager, EntityAlive _entity)
			: base(_gameManager, _entity)
		{
			cSlotCount = base.PUBLIC_SLOTS + 1;
			SetupSlots();
		}

		public override void Execute(int _actionIdx, bool _bReleased, PlayerActionsLocal _playerActions = null)
		{
		}

		public void SetupSlots()
		{
			slots = new ItemInventoryData[cSlotCount];
			models = new Transform[cSlotCount];
			m_HoldingItemIdx = 0;
			Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void updateHoldingItem()
		{
		}
	}

	public struct DelayedAttach
	{
		public int entityId;

		public int slot;
	}

	public static readonly FastTags<TagGroup.Global> StorageModifierTags = FastTags<TagGroup.Global>.Parse("storage");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBlockScale = 7f / 120f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBlockVelReduction = 1.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBlockMin = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBlockSelfPer = 2.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageTerrainSelfPer = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageEntityScale = 12f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageEntitySelfScale = 28f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cKillEntityXPPer = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cExitVelScale = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleepTime = 3f;

	public bool IsEngineRunning;

	public bool isLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInteractionLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int interactingPlayerId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int interactionRequestType;

	public Vehicle vehicle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTryToFall;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasDriver;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeInWater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReadingFromRemote;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MovementInput movementInput;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isTurnTowardsLook = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RemoteData incomingRemoteData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RemoteData currentRemoteData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RemoteData lastRemoteData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSyncHighRateDuration = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float syncHighRateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float syncPlayTime = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float syncLowRateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSyncLowRateDuration = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody vehicleRB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool RBActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float RBNoDriverGndTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float RBNoDriverSleepTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastRBPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion lastRBRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastRBVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastRBAngVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float velocityMax;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float damageAccumulator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int hitEffectCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float explodeHealth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHop;

	public const float cVehicleCameraOffset = 1.8f;

	public const float cVehicleCameraChaseSpeed = 7f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 cameraStartPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraStartBlend;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 cameraStartVec;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 cameraPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraDist;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float cameraDistScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraAngleTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraOutTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraVelY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Force[] forces = new Force[0];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Motor[] motors = new Motor[0];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float motorTorque;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float brakeTorque;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float wheelDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float wheelMotor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float wheelBrakes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Wheel[] wheels = new Wheel[0];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnHonkEvent = "HonkEvent";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string onHonkEvent = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int storageModCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<DelayedAttach> delayedAttachments = new List<DelayedAttach>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float collisionBlockDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 collisionVelNorm;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int collisionIgnoreCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ContactPoint> contactPoints = new List<ContactPoint>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<WorldRayHitInfo> collisionHits = new List<WorldRayHitInfo>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int collisionGrazeCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFuelItemScale = 25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncVersion = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncAttachment = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncInteractAndSecurity = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncItem = 4;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncStorage = 8;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncInteractRequest = 4096;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncLowRate = 16384;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncHighRate = 32768;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncAllNonRates = 15;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncLowRateAndNonRates = 16399;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncReplicate = 49159;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cSyncSave = 16398;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncInteractAndSecurityFInteracting = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWorldPad = 66;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasWorldValidPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldValidPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float worldValidDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int worldTerrainFailCount;

	public override EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return EnumPositionUpdateMovementType.Instant;
		}
	}

	public override bool IsValidAimAssistSnapTarget => false;

	public override bool IsValidAimAssistSlowdownTarget => false;

	public bool IsHeadlightOn
	{
		get
		{
			if (vehicle.FindPart("headlight") is VPHeadlight vPHeadlight)
			{
				return vPHeadlight.IsOn();
			}
			return false;
		}
		set
		{
			vehicle.FireEvent(VehiclePart.Event.LightsOn, null, value ? 1 : 0);
		}
	}

	public bool HasDriver => hasDriver;

	public override int Health
	{
		set
		{
			base.Stats.Health.Value = value;
			if (vehicle != null)
			{
				vehicle.FireEvent(Vehicle.Event.HealthChanged);
			}
		}
	}

	public int EntityId
	{
		get
		{
			return entityId;
		}
		set
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		bag = new Bag(this);
		base.Awake();
		isLocked = false;
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		EntityClass entityClass = EntityClass.list[base.entityClass];
		vehicle = new Vehicle(entityClass.entityClassName, this);
		base.transform.tag = "E_Vehicle";
		Vector2i size = LootContainer.GetLootContainer(GetLootList()).size;
		bag.SetupSlots(ItemStack.CreateArray(size.x * size.y));
		Transform physicsTransform = PhysicsTransform;
		vehicleRB = physicsTransform.GetComponent<Rigidbody>();
		if ((bool)vehicleRB)
		{
			if (vehicleRB.automaticCenterOfMass)
			{
				vehicleRB.centerOfMass = new Vector3(0f, 0.1f, 0f);
			}
			vehicleRB.sleepThreshold = vehicleRB.mass * 0.01f * 0.01f * 0.5f;
			physicsTransform.gameObject.AddComponent<CollisionCallForward>().Entity = this;
			physicsTransform.gameObject.layer = 21;
			Utils.SetTagsIfNoneRecursively(physicsTransform, "E_Vehicle");
			SetupDevices();
			SetVehicleDriven();
			if (!isEntityRemote)
			{
				isTryToFall = true;
			}
		}
		alertEnabled = false;
		GameManager.Instance.StartCoroutine(ApplyCollisionsCoroutine());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
	}

	public override void PostInit()
	{
		LogVehicle("PostInit {0}, {1} (chunk {2}), rbPos {3}", this, position, World.toChunkXZ(position), vehicleRB.position + Origin.position);
		base.transform.rotation = qrotation;
		if ((bool)vehicleRB)
		{
			PhysicsResetAndSleep();
			PhysicsTransform.rotation = qrotation;
			SetVehicleDriven();
		}
		HandleNavObject();
		UpdateContainerSize(forceUpdate: true);
	}

	public bool CanRemoveInventoryMod()
	{
		Vector2i size = LootContainer.GetLootContainer(GetLootList()).size;
		int num = 0;
		ItemStack[] slots = bag.GetSlots();
		ItemStack[] array = slots;
		foreach (ItemStack itemStack in array)
		{
			if (itemStack != null && !itemStack.IsEmpty())
			{
				num++;
			}
		}
		return num <= slots.Length - size.x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i getStorageSize()
	{
		Vector2i size = LootContainer.GetLootContainer(GetLootList()).size;
		size.y += storageModCount;
		return size;
	}

	public void UpdateStorageModCount(int _storageModCount)
	{
		storageModCount = _storageModCount;
	}

	public void UpdateContainerSize(bool forceUpdate = false)
	{
		Vector2i storageSize = getStorageSize();
		int num = storageSize.x * storageSize.y;
		int num2 = bag.GetSlots().Length;
		if (!forceUpdate && num == num2)
		{
			return;
		}
		SetLootContainerSize();
		if (num != num2)
		{
			ItemStack[] slots = bag.GetSlots();
			ItemStack[] array = new ItemStack[num];
			List<ItemStack> list = new List<ItemStack>();
			for (int i = 0; i < num2; i++)
			{
				ItemStack itemStack = slots[i];
				if (!itemStack.IsEmpty())
				{
					list.Add(itemStack.Clone());
				}
			}
			int num3 = list.Count;
			if (num >= num3)
			{
				int i;
				for (i = 0; i < num3; i++)
				{
					array[i] = list[i];
				}
				for (; i < num; i++)
				{
					array[i] = ItemStack.Empty.Clone();
				}
			}
			else
			{
				int i;
				for (i = 0; i < num; i++)
				{
					array[i] = list[i];
				}
				ItemStack[] array2 = new ItemStack[num3 - num];
				int num4 = 0;
				for (; i < num3; i++)
				{
					array2[num4++] = list[i];
				}
				dropLoot(array2, 1.5f);
			}
			bag.SetupSlots(array);
		}
		if (!isReadingFromRemote)
		{
			SetBagModified();
		}
	}

	public override void SetLootContainerSize()
	{
		if (lootContainer != null)
		{
			lootContainer.SetContainerSize(getStorageSize());
		}
	}

	public override void InitInventory()
	{
		inventory = new VehicleInventory(GameManager.Instance, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDevices()
	{
		SetupMotors();
		SetupForces();
		SetupWheels();
		vehicle.Properties.ParseString(PropOnHonkEvent, ref onHonkEvent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupForces()
	{
		DynamicProperties properties = vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < 99; i++)
		{
			if (!properties.Classes.TryGetValue("force" + i, out var _))
			{
				break;
			}
			num++;
		}
		forces = new Force[num];
		for (int j = 0; j < forces.Length; j++)
		{
			Force force = new Force();
			forces[j] = force;
			DynamicProperties dynamicProperties = properties.Classes["force" + j];
			force.ceiling.x = 9999f;
			force.ceiling.y = 9999f;
			dynamicProperties.ParseVec("ceiling", ref force.ceiling);
			force.ceiling.y = 1f / Utils.FastMax(0.5f, force.ceiling.y - force.ceiling.x);
			force.force = Vector3.forward;
			dynamicProperties.ParseVec("force", ref force.force);
			force.trigger = Force.Trigger.On;
			dynamicProperties.ParseEnum("trigger", ref force.trigger);
			force.type = Force.Type.Relative;
			dynamicProperties.ParseEnum("type", ref force.type);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMotors()
	{
		DynamicProperties properties = vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < 99; i++)
		{
			if (!properties.Classes.TryGetValue("motor" + i, out var _))
			{
				break;
			}
			num++;
		}
		motors = new Motor[num];
		Transform meshTransform = vehicle.GetMeshTransform();
		for (int j = 0; j < motors.Length; j++)
		{
			Motor motor = new Motor();
			motors[j] = motor;
			DynamicProperties dynamicProperties = properties.Classes["motor" + j];
			string text = dynamicProperties.GetString("engine");
			if (text.Length > 0)
			{
				motor.engine = vehicle.FindPart(text) as VPEngine;
			}
			motor.engineOffPer = 0f;
			dynamicProperties.ParseFloat("engineOffPer", ref motor.engineOffPer);
			motor.turbo = 1f;
			dynamicProperties.ParseFloat("turbo", ref motor.turbo);
			motor.rpmAccelMin = 1f;
			motor.rpmAccelMax = 1f;
			dynamicProperties.ParseVec("rpmAccel_min_max", ref motor.rpmAccelMin, ref motor.rpmAccelMax);
			motor.rpmDrag = 1f;
			dynamicProperties.ParseFloat("rpmDrag", ref motor.rpmDrag);
			motor.rpmMax = 1f;
			dynamicProperties.ParseFloat("rpmMax", ref motor.rpmMax);
			if (motor.rpmMax == 0f)
			{
				motor.rpmMax = 0.001f;
			}
			motor.trigger = Motor.Trigger.On;
			dynamicProperties.ParseEnum("trigger", ref motor.trigger);
			string text2 = dynamicProperties.GetString("transform");
			if (text2.Length > 0)
			{
				motor.transform = meshTransform.Find(text2);
			}
			float optionalValue = 0f;
			dynamicProperties.ParseFloat("axis", ref optionalValue);
			motor.axis = (int)optionalValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupWheels()
	{
		DynamicProperties properties = vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < 99; i++)
		{
			if (!properties.Classes.TryGetValue("wheel" + i, out var _))
			{
				break;
			}
			num++;
		}
		wheels = new Wheel[num];
		Transform physicsTransform = PhysicsTransform;
		Transform meshTransform = vehicle.GetMeshTransform();
		for (int j = 0; j < wheels.Length; j++)
		{
			Wheel wheel = new Wheel();
			wheels[j] = wheel;
			Transform transform = physicsTransform.Find("Wheel" + j);
			wheel.wheelC = transform.GetComponent<WheelCollider>();
			wheel.forwardFriction = wheel.wheelC.forwardFriction;
			wheel.forwardStiffnessBase = wheel.forwardFriction.stiffness;
			wheel.sideFriction = wheel.wheelC.sidewaysFriction;
			wheel.sideStiffnessBase = wheel.sideFriction.stiffness;
			DynamicProperties dynamicProperties = properties.Classes["wheel" + j];
			wheel.motorTorqueScale = 1f;
			wheel.brakeTorqueScale = 1f;
			dynamicProperties.ParseVec("torqueScale_motor_brake", ref wheel.motorTorqueScale, ref wheel.brakeTorqueScale);
			wheel.bounceSound = "vwheel_bounce";
			dynamicProperties.ParseString("bounceSound", ref wheel.bounceSound);
			wheel.slideSound = "vwheel_slide";
			dynamicProperties.ParseString("slideSound", ref wheel.slideSound);
			string text = dynamicProperties.GetString("steerTransform");
			if (text.Length > 0)
			{
				wheel.steerT = meshTransform.Find(text);
				if ((bool)wheel.steerT)
				{
					wheel.steerBaseRot = wheel.steerT.localRotation;
				}
			}
			string text2 = dynamicProperties.GetString("tireTransform");
			if (text2.Length > 0)
			{
				wheel.tireT = meshTransform.Find(text2);
			}
			wheel.isSteerParentOfTire = wheel.steerT != wheel.tireT;
			if (dynamicProperties.GetString("tireSuspensionPercent").Length > 0)
			{
				wheel.tireSuspensionPercent = 1f;
			}
		}
	}

	public override void OnXMLChanged()
	{
		vehicle.OnXMLChanged();
		SetupDevices();
	}

	public new void FixedUpdate()
	{
		PhysicsFixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsResetAndSleep()
	{
		Rigidbody rigidbody = vehicleRB;
		Transform physicsTransform = PhysicsTransform;
		Vector3 vector = (physicsTransform.position = position - Origin.position);
		rigidbody.position = vector;
		Quaternion quaternion = (physicsTransform.rotation = ModelTransform.rotation);
		rigidbody.rotation = quaternion;
		if (!vehicleRB.isKinematic)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.Sleep();
		}
		SetWheelsForces(0f, 1f, 0f, 1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsFixedUpdate()
	{
		float deltaTime = Time.deltaTime;
		Rigidbody rigidbody = vehicleRB;
		Transform physicsTransform = PhysicsTransform;
		wheelMotor = 0f;
		wheelBrakes = 0f;
		if (isEntityRemote)
		{
			vehicleRB.isKinematic = true;
			Vector3 vector = Vector3.Lerp(physicsTransform.position, position - Origin.position, 0.5f);
			physicsTransform.position = vector;
			physicsTransform.rotation = Quaternion.Slerp(physicsTransform.rotation, ModelTransform.rotation, 0.3f);
			if (incomingRemoteData.Flags > 0)
			{
				lastRemoteData = currentRemoteData;
				currentRemoteData = incomingRemoteData;
				incomingRemoteData.Flags = 0;
				syncPlayTime = 0f;
				vehicle.CurrentIsAccel = (currentRemoteData.Flags & 2) > 0;
				vehicle.CurrentIsBreak = (currentRemoteData.Flags & 4) > 0;
			}
			if (!(syncPlayTime >= 0f))
			{
				return;
			}
			float num = syncPlayTime / 0.5f;
			syncPlayTime += deltaTime;
			if (num >= 1f)
			{
				num = 1f;
				syncPlayTime = -1f;
			}
			float num2 = Mathf.Lerp(lastRemoteData.SteeringPercent, currentRemoteData.SteeringPercent, num);
			vehicle.CurrentSteeringPercent = num2;
			float currentMotorTorquePercent = Mathf.Lerp(lastRemoteData.MotorTorquePercent, currentRemoteData.MotorTorquePercent, num);
			vehicle.CurrentMotorTorquePercent = currentMotorTorquePercent;
			Vector3 vector2 = Vector3.Lerp(lastRemoteData.Velocity, currentRemoteData.Velocity, num);
			vehicle.CurrentVelocity = vector2;
			vehicle.CurrentForwardVelocity = Vector3.Dot(vector2, physicsTransform.forward);
			wheelDir = num2 * vehicle.SteerAngleMax;
			FixedUpdateMotors();
			vehicle.UpdateSimulation();
			int num3 = wheels.Length;
			if (num3 <= 0 || lastRemoteData.parts == null)
			{
				return;
			}
			int num4 = 0;
			for (int i = 0; i < num3; i++)
			{
				Wheel wheel = wheels[i];
				Transform steerT = wheel.steerT;
				if ((bool)steerT && wheel.isSteerParentOfTire)
				{
					Quaternion localRotation = Quaternion.Lerp(lastRemoteData.parts[num4].rot, currentRemoteData.parts[num4].rot, num);
					steerT.localRotation = localRotation;
					num4++;
				}
				Transform tireT = wheel.tireT;
				if ((bool)tireT)
				{
					Vector3 localPosition = Vector3.Lerp(lastRemoteData.parts[num4].pos, currentRemoteData.parts[num4].pos, num);
					tireT.localPosition = localPosition;
					Quaternion localRotation2 = Quaternion.Lerp(lastRemoteData.parts[num4].rot, currentRemoteData.parts[num4].rot, num);
					tireT.localRotation = localRotation2;
					num4++;
				}
			}
			return;
		}
		CheckForOutOfWorld();
		if (!RBActive)
		{
			PhysicsResetAndSleep();
			vehicleRB.isKinematic = true;
			return;
		}
		vehicleRB.isKinematic = false;
		if (!hasDriver)
		{
			Vector3 velocity = rigidbody.velocity;
			velocity.x *= 0.98f;
			velocity.z *= 0.98f;
			if (GetWheelsOnGround() > 0)
			{
				RBNoDriverGndTime += deltaTime;
				float f = RBNoDriverGndTime / 8f;
				float b = Utils.FastLerp(0.6f, 1f, (0.5f - physicsTransform.up.y) / 0.5f);
				b = Utils.FastLerp(1f, b, Mathf.Pow(f, 3f));
				velocity.x *= b;
				velocity.z *= b;
			}
			if (collisionGrazeCount >= 2)
			{
				float num5 = velocity.magnitude * 1.4f;
				if (num5 < 1f)
				{
					float num6 = Utils.FastLerpUnclamped((float)Utils.FastMin(3, collisionGrazeCount) * 0.29f, 0f, num5);
					velocity *= 1f - num6;
					rigidbody.angularVelocity *= 1f - num6 * 0.65f;
				}
			}
			velocity.y *= vehicle.AirDragVelScale;
			rigidbody.velocity = velocity;
			if (velocity.sqrMagnitude < 0.010000001f && rigidbody.angularVelocity.sqrMagnitude < 0.0049f)
			{
				RBNoDriverSleepTime += deltaTime;
				if (RBNoDriverSleepTime >= 3f)
				{
					RBActive = false;
					RBNoDriverSleepTime = 0f;
				}
			}
			else
			{
				RBNoDriverSleepTime = 0f;
			}
			collisionGrazeCount = 0;
		}
		Vector3 velocity2 = vehicleRB.velocity;
		float num7 = vehicle.MotorTorqueForward;
		float num8 = vehicle.VelocityMaxForward;
		vehicle.IsTurbo = false;
		if (movementInput != null)
		{
			if (movementInput.moveForward < 0f)
			{
				num7 = vehicle.MotorTorqueBackward;
				num8 = vehicle.VelocityMaxBackward;
			}
			if (movementInput.running && vehicle.CanTurbo && movementInput.moveForward != 0f)
			{
				vehicle.IsTurbo = true;
				num7 = vehicle.MotorTorqueTurboForward;
				num8 = vehicle.VelocityMaxTurboForward;
				if (movementInput.moveForward < 0f)
				{
					num7 = vehicle.MotorTorqueTurboBackward;
					num8 = vehicle.VelocityMaxTurboBackward;
				}
			}
		}
		num7 *= vehicle.EffectMotorTorquePer;
		num8 *= vehicle.EffectVelocityMaxPer;
		float num9 = ((num8 > velocityMax) ? 2.5f : 1.5f);
		num8 = (velocityMax = Mathf.MoveTowards(velocityMax, num8, num9 * deltaTime));
		if (CalcWaterDepth(vehicle.WaterDragY) > 0f)
		{
			timeInWater += deltaTime;
			if (vehicle.WaterDragVelScale != 1f)
			{
				velocity2 *= vehicle.WaterDragVelScale;
			}
			if (vehicle.WaterDragVelMaxScale != 1f)
			{
				num8 = Mathf.Lerp(num8, num8 * vehicle.WaterDragVelMaxScale, timeInWater * 0.5f);
			}
		}
		else
		{
			timeInWater = 0f;
		}
		float num10 = Mathf.Sqrt(velocity2.x * velocity2.x + velocity2.z * velocity2.z);
		if (num10 > num8)
		{
			float num11 = num8 / num10;
			velocity2.x *= num11;
			velocity2.z *= num11;
			vehicleRB.velocity = velocity2;
		}
		float magnitude = velocity2.magnitude;
		if (vehicle.WaterLiftForce > 0f)
		{
			float num12 = CalcWaterDepth(vehicle.WaterLiftY);
			if (num12 > 0f)
			{
				float y = Mathf.Lerp(vehicle.WaterLiftForce * 0.05f, vehicle.WaterLiftForce, num12 / (vehicle.WaterLiftDepth + 0.001f));
				vehicleRB.AddForce(new Vector3(0f, y, 0f), ForceMode.VelocityChange);
			}
		}
		float num13 = 0f - lastRBVel.y;
		if (num13 > 8f && (magnitude < num13 * 0.45f || Vector3.Dot(lastRBVel.normalized, velocity2.normalized) < 0.2f))
		{
			int num14 = (int)((num13 - 8f) * 4f + 0.999f);
			ApplyDamage(num14 * 10);
			ApplyCollisionDamageToAttached(num14);
		}
		lastRBPos = vehicleRB.position;
		lastRBRot = vehicleRB.rotation;
		lastRBVel = velocity2;
		lastRBAngVel = vehicleRB.angularVelocity;
		float num15 = Vector3.Dot(velocity2, physicsTransform.forward);
		vehicle.CurrentForwardVelocity = num15;
		float frictionPercent = 1f;
		if (hasDriver && wheels.Length < 4 && GetAttachedPlayerLocal().isPlayerInStorm)
		{
			frictionPercent = 0.75f;
			float num16 = 0.04f;
			float y2 = 0.01f;
			vehicleRB.AddForce(new Vector3(num16 * 0.707f, y2, num16 * 0.707f), ForceMode.VelocityChange);
		}
		motorTorque = 0f;
		brakeTorque = 0f;
		if (wheels.Length != 0)
		{
			if (movementInput != null)
			{
				float num17 = Mathf.Pow(magnitude * 0.1f, 2f);
				float num18 = Mathf.Clamp(1f - num17, 0.15f, 1f);
				wheelMotor = movementInput.moveForward;
				float steerAngleMax = vehicle.SteerAngleMax;
				float num19 = vehicle.SteerRate * num18 * deltaTime;
				if (isTurnTowardsLook)
				{
					float num20 = 0f;
					if (!Input.GetMouseButton(1))
					{
						vp_FPCamera obj = GetAttachedPlayerLocal().vp_FPCamera;
						Vector3 forward = base.transform.forward;
						forward.y = 0f;
						Vector3 forward2 = obj.Forward;
						forward2.y = 0f;
						num20 = Vector3.SignedAngle(forward, forward2, Vector3.up);
						if (num15 < -0.02f)
						{
							if (Mathf.Abs(num20) > 90f)
							{
								num20 += 180f;
								if (num20 > 180f)
								{
									num20 -= 360f;
								}
							}
							num20 = 0f - num20;
						}
					}
					float num21 = num19 * 1.2f;
					if ((wheelDir < 0f && wheelDir < num20) || (wheelDir > 0f && wheelDir > num20))
					{
						num21 *= 3f;
					}
					wheelDir = Mathf.MoveTowards(wheelDir, num20, num21);
					wheelDir = Mathf.Clamp(wheelDir, 0f - steerAngleMax, steerAngleMax);
				}
				else if (movementInput.lastInputController)
				{
					wheelDir = Mathf.MoveTowards(wheelDir, movementInput.moveStrafe * steerAngleMax, num19 * 1.5f);
				}
				else
				{
					float moveStrafe = movementInput.moveStrafe;
					float num22 = 0f;
					if (moveStrafe < 0f)
					{
						if (wheelDir > 0f)
						{
							num22 -= num19 * num17;
						}
						num22 -= num19;
					}
					if (moveStrafe > 0f)
					{
						if (wheelDir < 0f)
						{
							num22 += num19 * num17;
						}
						num22 += num19;
					}
					wheelDir += num22;
					wheelDir = Mathf.Clamp(wheelDir, 0f - steerAngleMax, steerAngleMax);
					if (moveStrafe == 0f)
					{
						wheelDir = Mathf.MoveTowards(wheelDir, 0f, vehicle.SteerCenteringRate * deltaTime);
					}
				}
				if (wheelMotor != 0f)
				{
					if (wheelMotor > 0f)
					{
						if (num15 < -0.5f)
						{
							wheelBrakes = 1f;
						}
					}
					else if (num15 > 0.5f)
					{
						wheelBrakes = 1f;
					}
					if (!movementInput.running)
					{
						wheelMotor *= 0.5f;
					}
				}
				if (movementInput.jump)
				{
					wheelBrakes = 2f;
				}
				if (canHop)
				{
					if (movementInput.down && GetWheelsOnGround() > 0)
					{
						canHop = false;
						Vector3 force = Vector3.Slerp(Vector3.up, physicsTransform.up, 0.5f) * vehicle.HopForce.x;
						vehicleRB.AddForceAtPosition(force, vehicleRB.position + physicsTransform.forward * vehicle.HopForce.y, ForceMode.VelocityChange);
					}
				}
				else if (!movementInput.down)
				{
					canHop = true;
				}
			}
			if (wheelMotor != 0f)
			{
				if (vehicle.HasEnginePart())
				{
					if (IsEngineRunning)
					{
						motorTorque = wheelMotor * num7;
					}
					else
					{
						motorTorque = wheelMotor * 50f;
					}
				}
				else if (vehicle.GetHealth() > 0)
				{
					motorTorque = wheelMotor * num7;
				}
				else
				{
					motorTorque = wheelMotor * 10f;
					if (rand.RandomFloat < 0.2f)
					{
						vehicleRB.AddRelativeForce(0.15f * rand.RandomOnUnitSphere, ForceMode.VelocityChange);
					}
					wheelDir = Mathf.Clamp(wheelDir + (rand.RandomFloat * 2f - 1f) * 5f, 0f - vehicle.SteerAngleMax, vehicle.SteerAngleMax);
				}
				if (magnitude < 0.15f && wheelBrakes == 0f && Utils.FastAbs(physicsTransform.up.y) > 0.34f)
				{
					Vector3 force2 = Quaternion.Euler(0f, wheelDir, 0f) * (vehicle.UnstickForce * Mathf.Sign(wheelMotor) * Vector3.forward);
					vehicleRB.AddRelativeForce(force2, ForceMode.VelocityChange);
				}
			}
			brakeTorque = wheelBrakes * vehicle.BrakeTorque;
			SetWheelsForces(motorTorque, num7, brakeTorque, frictionPercent);
			UpdateWheelsCollision();
			UpdateWheelsSteering();
		}
		vehicleRB.velocity *= vehicle.AirDragVelScale;
		vehicleRB.angularVelocity *= vehicle.AirDragAngVelScale;
		PhysicsInputMove();
		FixedUpdateMotors();
		FixedUpdateForces();
		if (hasDriver || (bool)GetFirstAttached())
		{
			if (vehicle.TiltUpForce > 0f)
			{
				Vector3 right = physicsTransform.right;
				Mathf.Abs(right.y);
				float num23 = Mathf.Asin(right.y) * 57.29578f;
				float num24 = wheelDir / vehicle.SteerAngleMax;
				num24 *= 2f;
				num24 = Mathf.LerpUnclamped(0f, num24, Mathf.Pow(magnitude * 0.1f, 2f));
				float tiltAngleMax = vehicle.TiltAngleMax;
				num24 = Mathf.Clamp(num24 * tiltAngleMax, 0f - tiltAngleMax, tiltAngleMax);
				float f2 = num23 + num24;
				float num25 = Mathf.Abs(f2);
				if (num25 > vehicle.TiltThreshold)
				{
					float value = (num25 - vehicle.TiltThreshold) * Mathf.Sign(f2) * 0.01f * (0f - vehicle.TiltUpForce);
					value = Mathf.Clamp(value, -4f, 4f);
					vehicleRB.AddRelativeTorque(0f, 0f, value, ForceMode.VelocityChange);
				}
				if (num25 < vehicle.TiltDampenThreshold)
				{
					Vector3 angularVelocity = vehicleRB.angularVelocity;
					float magnitude2 = angularVelocity.magnitude;
					if (magnitude2 > 0f)
					{
						Vector3 rhs = angularVelocity * (1f / magnitude2);
						float num26 = Mathf.Abs(Vector3.Dot(base.transform.forward, rhs));
						vehicleRB.angularVelocity -= angularVelocity * (0.02f + vehicle.TiltDampening * num26);
					}
				}
			}
			if (vehicle.UpForce > 0f)
			{
				Vector3 up = physicsTransform.up;
				float num27 = Mathf.Abs(Mathf.Acos(up.y) * 57.29578f) - vehicle.UpAngleMax;
				if (num27 > 0f)
				{
					float num28 = num27 / 90f;
					Vector3 torque = Vector3.Cross(up, Vector3.up) * (num28 * num28 * vehicle.UpForce);
					vehicleRB.AddRelativeTorque(torque, ForceMode.VelocityChange);
				}
			}
		}
		Vector3 vector3 = physicsTransform.position;
		SetPosition(vector3 + Origin.position, _bUpdatePhysics: false);
		qrotation = physicsTransform.rotation;
		rotation = qrotation.eulerAngles;
		ModelTransform.rotation = qrotation;
		vehicle.CurrentIsAccel = motorTorque != 0f && brakeTorque == 0f;
		vehicle.CurrentIsBreak = brakeTorque != 0f;
		vehicle.CurrentSteeringPercent = wheelDir / vehicle.SteerAngleMax;
		vehicle.CurrentVelocity = vehicleRB.velocity;
		vehicle.UpdateSimulation();
		if (!isEntityRemote)
		{
			syncHighRateTime += deltaTime;
			if (syncHighRateTime >= 0.5f)
			{
				SendSyncData(32768);
				syncHighRateTime = 0f;
			}
			syncLowRateTime += deltaTime;
			if (syncLowRateTime >= 2f)
			{
				SendSyncData(16384);
				syncLowRateTime = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PhysicsInputMove()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdateForces()
	{
		if (movementInput == null)
		{
			return;
		}
		float num = 1f;
		for (int i = 0; i < forces.Length; i++)
		{
			Force force = forces[i];
			float num2 = 1f;
			switch (force.trigger)
			{
			case Force.Trigger.Off:
				num2 = 0f;
				break;
			case Force.Trigger.InputForward:
				num2 = movementInput.moveForward;
				break;
			case Force.Trigger.InputStrafe:
				num2 = movementInput.moveStrafe;
				break;
			case Force.Trigger.InputUp:
				num2 = (movementInput.jump ? 1 : 0);
				break;
			case Force.Trigger.InputDown:
				num2 = (movementInput.down ? 1 : 0);
				break;
			case Force.Trigger.Motor0:
			case Force.Trigger.Motor1:
			case Force.Trigger.Motor2:
			case Force.Trigger.Motor3:
			case Force.Trigger.Motor4:
			case Force.Trigger.Motor5:
			case Force.Trigger.Motor6:
			case Force.Trigger.Motor7:
			{
				Motor motor = motors[(int)(force.trigger - 6)];
				num2 = motor.rpm / motor.rpmMax;
				break;
			}
			}
			if (num2 != 0f)
			{
				num2 *= num;
				float num3 = position.y - force.ceiling.x;
				if (num3 > 0f)
				{
					num2 *= Utils.FastMax(0f, 1f - num3 * force.ceiling.y);
				}
				switch (force.type)
				{
				case Force.Type.Relative:
					vehicleRB.AddRelativeForce(force.force * num2, ForceMode.VelocityChange);
					break;
				case Force.Type.RelativeTorque:
					vehicleRB.AddRelativeTorque(force.force * num2, ForceMode.VelocityChange);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdateMotors()
	{
		for (int i = 0; i < motors.Length; i++)
		{
			Motor motor = motors[i];
			motor.rpm *= motor.rpmDrag;
			float num = 0f;
			switch (motor.trigger)
			{
			case Motor.Trigger.On:
				num = 1f;
				break;
			case Motor.Trigger.InputForward:
				if (movementInput != null)
				{
					num = movementInput.moveForward;
				}
				break;
			case Motor.Trigger.InputStrafe:
				if (movementInput != null)
				{
					num = movementInput.moveStrafe;
				}
				break;
			case Motor.Trigger.InputUp:
				if (movementInput != null && movementInput.jump)
				{
					num = 1f;
				}
				break;
			case Motor.Trigger.InputDown:
				if (movementInput != null && movementInput.down)
				{
					num = 1f;
				}
				break;
			case Motor.Trigger.Vel:
				num = vehicle.CurrentForwardVelocity / (vehicle.VelocityMaxForward + 0.001f);
				if (num < 0.01f)
				{
					num = 0f;
				}
				break;
			}
			if (num == 0f)
			{
				continue;
			}
			float num2 = 1f;
			if (movementInput != null && movementInput.running)
			{
				num2 = motor.turbo;
			}
			if (motor.engine != null && !motor.engine.isRunning)
			{
				num *= motor.engineOffPer;
				num2 = 1f;
			}
			num *= num2;
			switch (motor.type)
			{
			case Motor.Type.Spin:
				if (hasDriver)
				{
					float num3 = Mathf.Lerp(motor.rpmAccelMin, motor.rpmAccelMax, num);
					motor.rpm += num3;
					motor.rpm = Mathf.Min(motor.rpm, motor.rpmMax * num2);
				}
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMotors()
	{
		for (int i = 0; i < motors.Length; i++)
		{
			Motor motor = motors[i];
			Transform transform = motor.transform;
			if ((bool)transform)
			{
				Vector3 localEulerAngles = transform.localEulerAngles;
				localEulerAngles[motor.axis] += motor.rpm * 360f * Time.deltaTime;
				transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetWheelsOnGround()
	{
		int num = 0;
		int num2 = wheels.Length;
		for (int i = 0; i < num2; i++)
		{
			if (wheels[i].isGrounded)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetWheelsForces(float motorTorque, float motorTorqueBase, float brakeTorque, float _frictionPercent)
	{
		vehicle.CurrentMotorTorquePercent = motorTorque / motorTorqueBase;
		float num = ((_frictionPercent == 1f) ? 1f : (_frictionPercent * 0.33f));
		int num2 = wheels.Length;
		for (int i = 0; i < num2; i++)
		{
			Wheel wheel = wheels[i];
			wheel.wheelC.motorTorque = motorTorque * wheel.motorTorqueScale;
			wheel.wheelC.brakeTorque = brakeTorque * wheel.brakeTorqueScale;
			wheel.forwardFriction.stiffness = wheel.forwardStiffnessBase * _frictionPercent;
			wheel.wheelC.forwardFriction = wheel.forwardFriction;
			wheel.sideFriction.stiffness = wheel.sideStiffnessBase * num;
			wheel.wheelC.sidewaysFriction = wheel.sideFriction;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWheelsCollision()
	{
		float wheelPtlScale = vehicle.WheelPtlScale;
		for (int i = 0; i < wheels.Length; i++)
		{
			Wheel wheel = wheels[i];
			wheel.isGrounded = false;
			if (!wheel.wheelC.GetGroundHit(out var hit))
			{
				continue;
			}
			float mass = wheel.wheelC.mass;
			if (hit.normal.y >= 0f)
			{
				wheel.isGrounded = true;
			}
			if (hit.force > 260f * mass)
			{
				PlayOneShot(wheel.bounceSound);
			}
			float forwardSlip = hit.forwardSlip;
			if (forwardSlip <= -0.9f || forwardSlip >= 0.995f)
			{
				wheel.slideTime += Time.deltaTime;
			}
			else if (Utils.FastAbs(hit.sidewaysSlip) >= 0.19f)
			{
				wheel.slideTime += Time.deltaTime;
			}
			else
			{
				wheel.slideTime = 0f;
			}
			if (wheel.slideTime > 0.2f)
			{
				wheel.slideTime = 0f;
				PlayOneShot(wheel.slideSound);
			}
			if (!(wheelPtlScale > 0f) || !(Utils.FastAbs(forwardSlip) >= 0.5f))
			{
				continue;
			}
			wheel.ptlTime += Time.deltaTime;
			if (wheel.ptlTime > 0.05f)
			{
				wheel.ptlTime = 0f;
				float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(hit.point)) * 0.5f;
				ParticleEffect pe = new ParticleEffect("tiresmoke", Vector3.zero, lightValue, new Color(1f, 1f, 1f, 1f), null, wheel.wheelC.transform, _OLDCreateColliders: false);
				Transform transform = GameManager.Instance.SpawnParticleEffectClientForceCreation(pe, -1, _worldSpawn: false);
				if ((bool)transform)
				{
					transform.position = hit.point;
					transform.localScale = new Vector3(wheelPtlScale, wheelPtlScale, wheelPtlScale);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateWheelsSteering()
	{
		wheels[0].wheelC.steerAngle = wheelDir;
	}

	public Vector3 GetRBVelocity()
	{
		return lastRBVel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (bool)GetFirstAttached())
		{
			world.entityDistributer.SendFullUpdateNextTick(this);
		}
		if ((bool)vehicleRB && RBActive)
		{
			Quaternion quaternion = Quaternion.Euler(0f, wheelDir, 0f);
			for (int i = 0; i < wheels.Length; i++)
			{
				Wheel wheel = wheels[i];
				wheel.tireSpinSpeed = Utils.FastLerpUnclamped(wheel.tireSpinSpeed, wheel.wheelC.rotationSpeed, 0.3f);
				wheel.tireSpin += Utils.FastClamp(wheel.tireSpinSpeed * Time.deltaTime, -13f, 13f);
				wheel.wheelC.GetWorldPose(out var pos, out var quat);
				if ((bool)wheel.steerT)
				{
					quat = Quaternion.Euler(wheel.tireSpin, 0f, 0f);
					Quaternion localRotation = wheel.steerBaseRot * quaternion;
					if (!wheel.isSteerParentOfTire)
					{
						localRotation *= quat;
					}
					wheel.steerT.localRotation = localRotation;
				}
				if (!wheel.tireT)
				{
					continue;
				}
				if (wheel.tireSuspensionPercent > 0f)
				{
					pos = wheel.tireT.parent.InverseTransformPoint(pos);
					Vector3 localPosition = wheel.tireT.localPosition;
					localPosition.y = pos.y;
					wheel.tireT.localPosition = localPosition;
				}
				if ((bool)wheel.steerT)
				{
					if (wheel.isSteerParentOfTire)
					{
						wheel.tireT.localRotation = quat;
					}
				}
				else
				{
					wheel.tireT.localRotation = Quaternion.Euler(wheel.tireSpin, 0f, 0f);
				}
			}
		}
		UpdateAttachment();
		if (RBActive || syncPlayTime >= 0f)
		{
			UpdateMotors();
		}
		vehicle.Update(Time.deltaTime);
		if ((Time.frameCount & 1) == 0)
		{
			hitEffectCount = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (isEntityRemote)
		{
			float t = Time.deltaTime * 10f;
			Transform modelTransform = ModelTransform;
			Vector3 vector = Vector3.Lerp(modelTransform.position, position - Origin.position, t);
			Quaternion quaternion = Quaternion.Slerp(modelTransform.rotation, qrotation, t);
			modelTransform.SetPositionAndRotation(vector, quaternion);
		}
	}

	public void CameraChangeRotation(float _newRotation)
	{
		if (isTurnTowardsLook)
		{
			EntityPlayerLocal attachedPlayerLocal = GetAttachedPlayerLocal();
			if ((bool)attachedPlayerLocal)
			{
				attachedPlayerLocal.vp_FPCamera.Yaw += _newRotation;
			}
		}
	}

	public override void OriginChanged(Vector3 _deltaPos)
	{
		base.OriginChanged(_deltaPos);
		Vector3 vector = position - Origin.position;
		ModelTransform.position = vector;
		PhysicsTransform.position = vector;
		if ((bool)vehicleRB)
		{
			vehicleRB.position = vector;
		}
		cameraPos += _deltaPos;
		cameraStartPos += _deltaPos;
		EntityPlayerLocal attachedPlayerLocal = GetAttachedPlayerLocal();
		if ((bool)attachedPlayerLocal)
		{
			attachedPlayerLocal.vp_FPCamera.DrivingPosition += _deltaPos;
		}
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
		if (!isEntityRemote)
		{
			ModelTransform.position = _pos - Origin.position;
		}
	}

	public override void SetRotation(Vector3 _rot)
	{
		base.SetRotation(_rot);
		if (!isEntityRemote)
		{
			ModelTransform.rotation = qrotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetCenterPosition()
	{
		return position + ModelTransform.up * 0.8f;
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override float GetHeight()
	{
		return 1f;
	}

	public void AddRelativeForce(Vector3 forceVec, ForceMode mode = ForceMode.VelocityChange)
	{
		if (!isEntityRemote)
		{
			if (!RBActive)
			{
				RBActive = true;
				vehicleRB.isKinematic = false;
			}
			vehicleRB.AddRelativeForce(forceVec, mode);
		}
	}

	public void AddForce(Vector3 forceVec, ForceMode mode = ForceMode.VelocityChange)
	{
		if (!isEntityRemote)
		{
			if (!RBActive)
			{
				RBActive = true;
				vehicleRB.isKinematic = false;
			}
			vehicleRB.AddForce(forceVec, mode);
		}
	}

	public override Vector3 GetVelocityPerSecond()
	{
		if (isEntityRemote)
		{
			return vehicle.CurrentVelocity;
		}
		return vehicleRB.velocity;
	}

	public void VelocityFlip()
	{
		if (isEntityRemote)
		{
			vehicle.CurrentVelocity = new Vector3(vehicle.CurrentVelocity.x * -1f, vehicle.CurrentVelocity.y, vehicle.CurrentVelocity.z * -1f);
		}
		else
		{
			vehicleRB.velocity = new Vector3(vehicleRB.velocity.x * -1f, vehicleRB.velocity.y, vehicleRB.velocity.z * -1f);
		}
	}

	public Vector3 GetCameraOffset(float deltaTime)
	{
		EntityPlayerLocal attachedPlayerLocal = GetAttachedPlayerLocal();
		Vector3 result = Vector3.zero;
		Vector3 pos;
		if (!isEntityRemote)
		{
			pos = PhysicsTransform.position + Origin.position;
			SetPosition(pos, _bUpdatePhysics: false);
			pos -= Origin.position;
			qrotation = PhysicsTransform.rotation;
			rotation = qrotation.eulerAngles;
			ModelTransform.rotation = qrotation;
		}
		else
		{
			pos = ModelTransform.position;
		}
		if ((bool)attachedPlayerLocal)
		{
			vp_FPCamera vp_FPCamera2 = attachedPlayerLocal.vp_FPCamera;
			if (!isTurnTowardsLook)
			{
				cameraAngleTarget = Vector2.SignedAngle(cameraStartVec, new Vector2(base.transform.forward.x, base.transform.forward.z));
				float num = cameraAngle;
				float num2 = Mathf.Abs(Mathf.DeltaAngle(cameraAngle, cameraAngleTarget));
				cameraAngle = Mathf.MoveTowardsAngle(cameraAngle, cameraAngleTarget, num2 * 0.3f);
				num -= cameraAngle;
				vp_FPCamera2.yaw3P += num;
			}
			float magnitude = vehicleRB.velocity.magnitude;
			float num3 = Mathf.Lerp(vehicle.CameraDistance.x, vehicle.CameraDistance.y, magnitude / vehicle.VelocityMaxForward) * cameraDistScale - cameraDist;
			if (num3 < 0f)
			{
				cameraOutTime += deltaTime;
				if (cameraOutTime > 1f)
				{
					num3 *= 0.03f;
					cameraDist += num3;
				}
			}
			else if (num3 > 0f)
			{
				cameraOutTime = 0f;
				num3 *= 0.22f;
				cameraDist += num3;
			}
			result = new Vector3(0f, 0f, Mathf.Abs(cameraDist));
			pos.y += 1.8f;
			cameraPos.x = pos.x;
			cameraPos.z = pos.z;
			cameraPos.y = pos.y;
			if (cameraStartBlend < 1f)
			{
				cameraStartBlend = Mathf.Min(cameraStartBlend + deltaTime, 1f);
			}
			vp_FPCamera2.DrivingPosition = Vector3.Lerp(attachedPlayerLocal.vp_FPController.SmoothPosition, cameraPos, cameraStartBlend);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterVehicle(EntityAlive _entity)
	{
		int slot = -1;
		_entity.StartAttachToEntity(this, slot);
		if (NavObject != null)
		{
			NavObject.IsActive = !(_entity is EntityPlayerLocal);
		}
		if (_entity is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.UpdateEntityVehicleWayPoint(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVehicleDriven()
	{
		if (base.AttachedMainEntity != null && !base.AttachedMainEntity.isEntityRemote)
		{
			Utils.SetLayerRecursively(vehicleRB.gameObject, 21);
			RBActive = true;
			vehicleRB.isKinematic = false;
			vehicleRB.WakeUp();
			if (world.IsRemote())
			{
				vehicleRB.velocity = vehicle.CurrentVelocity;
			}
			lastRBVel = Vector3.zero;
			if (base.AttachedMainEntity is EntityPlayerLocal entityPlayerLocal)
			{
				entityPlayerLocal.Waypoints.SetWaypointHiddenOnMap(entityId, _hidden: true);
			}
		}
		else
		{
			Utils.SetLayerRecursively(vehicleRB.gameObject, 21);
			if (isEntityRemote)
			{
				RBActive = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAttachment()
	{
		Entity attachedMainEntity = base.AttachedMainEntity;
		if (hasDriver && attachedMainEntity == null)
		{
			DriverRemoved();
		}
		if (attachedMainEntity != null && attachedMainEntity.IsDead())
		{
			((EntityAlive)attachedMainEntity).RemoveIKTargets();
			attachedMainEntity.Detach();
			DriverRemoved();
		}
		for (int num = delayedAttachments.Count - 1; num >= 0; num--)
		{
			DelayedAttach delayedAttach = delayedAttachments[num];
			Entity entity = GameManager.Instance.World.GetEntity(delayedAttach.entityId);
			if ((bool)entity)
			{
				if (!IsAttached(entity))
				{
					entity.AttachToEntity(this, delayedAttach.slot);
				}
				delayedAttachments.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DriverRemoved()
	{
		hasDriver = false;
		vehicle.SetColors();
		vehicle.FireEvent(Vehicle.Event.Stop);
		isInteractionLocked = false;
		RBNoDriverGndTime = 0f;
		RBNoDriverSleepTime = 0f;
		collisionGrazeCount = 0;
		if (GetWheelsOnGround() > 0 && !vehicleRB.isKinematic)
		{
			vehicleRB.velocity *= 0.5f;
		}
		if (NavObject != null)
		{
			NavObject.IsActive = true;
		}
	}

	public override int AttachEntityToSelf(Entity _entity, int slot = -1)
	{
		slot = base.AttachEntityToSelf(_entity, slot);
		if (slot >= 0)
		{
			EntityAlive obj = (EntityAlive)_entity;
			int seatPose = vehicle.GetSeatPose(slot);
			obj.SetVehiclePoseMode(seatPose);
			obj.transform.gameObject.layer = 24;
			obj.m_characterController.Enable(isEnabled: false);
			obj.SetIKTargets(vehicle.GetIKTargets(slot));
			isInteractionLocked = GetAttachFreeCount() == 0;
			if ((bool)nativeCollider)
			{
				nativeCollider.enabled = !isInteractionLocked;
			}
			if (slot == 0)
			{
				hasDriver = true;
				vehicle.SetColors();
				vehicle.FireEvent(Vehicle.Event.Start);
			}
			SetVehicleDriven();
			vehicle.TriggerUpdateEffects();
			if (!_entity.isEntityRemote && GameManager.Instance.World != null)
			{
				LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entity as EntityPlayerLocal);
				if (uIForPlayer != null && uIForPlayer.playerInput != null)
				{
					PlayerActionsVehicle vehicleActions = uIForPlayer.playerInput.VehicleActions;
					uIForPlayer.ActionSetManager.Insert(vehicleActions, 1);
					movementInput = new MovementInput();
					CameraInit();
				}
			}
		}
		return slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DetachEntity(Entity _entity)
	{
		for (int num = delayedAttachments.Count - 1; num >= 0; num--)
		{
			if (delayedAttachments[num].entityId == _entity.entityId)
			{
				delayedAttachments.RemoveAt(num);
			}
		}
		int num2 = FindAttachSlot(_entity);
		if (num2 < 0)
		{
			return;
		}
		EntityAlive obj = (EntityAlive)_entity;
		obj.SetVehiclePoseMode(-1);
		obj.RemoveIKTargets();
		int modelLayer = obj.GetModelLayer();
		obj.SetModelLayer(modelLayer, force: true);
		obj.transform.gameObject.layer = 20;
		obj.ModelTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		obj.m_characterController.Enable(isEnabled: true);
		if (!_entity.isEntityRemote && GameManager.Instance.World != null)
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entity as EntityPlayerLocal);
			if (uIForPlayer != null)
			{
				PlayerActionsVehicle vehicleActions = uIForPlayer.playerInput.VehicleActions;
				uIForPlayer.ActionSetManager.Remove(vehicleActions, 1);
			}
			movementInput = null;
		}
		if (num2 == 0)
		{
			DriverRemoved();
		}
		bool num3 = isEntityRemote;
		base.DetachEntity(_entity);
		isInteractionLocked = GetAttachFreeCount() == 0;
		if ((bool)nativeCollider)
		{
			nativeCollider.enabled = !isInteractionLocked;
		}
		SetVehicleDriven();
		vehicle.TriggerUpdateEffects();
		if (num3 && !isEntityRemote)
		{
			RBActive = true;
			RBNoDriverSleepTime = 0f;
			vehicleRB.isKinematic = false;
			vehicleRB.velocity = vehicle.CurrentVelocity;
		}
	}

	public override int AttachToEntity(Entity _entity, int slot = -1)
	{
		return -1;
	}

	public override AttachedToEntitySlotInfo GetAttachedToInfo(int _slotIdx)
	{
		AttachedToEntitySlotInfo attachedToEntitySlotInfo = new AttachedToEntitySlotInfo();
		attachedToEntitySlotInfo.bKeep3rdPersonModelVisible = true;
		attachedToEntitySlotInfo.bReplaceLocalInventory = true;
		attachedToEntitySlotInfo.pitchRestriction = new Vector2(-30f, 30f);
		attachedToEntitySlotInfo.yawRestriction = new Vector2(-90f, 90f);
		attachedToEntitySlotInfo.enterParentTransform = base.transform;
		attachedToEntitySlotInfo.enterPosition = new Vector3(0f, 0f, -0.201f);
		attachedToEntitySlotInfo.enterRotation = Vector3.zero;
		DynamicProperties propertiesForClass = vehicle.GetPropertiesForClass("seat" + _slotIdx);
		if (propertiesForClass != null)
		{
			propertiesForClass.ParseVec("position", ref attachedToEntitySlotInfo.enterPosition);
			propertiesForClass.ParseVec("rotation", ref attachedToEntitySlotInfo.enterRotation);
			string text = propertiesForClass.GetString("exit");
			if (text.Length > 0)
			{
				char[] separator = new char[1] { '~' };
				string[] array = text.Split(separator);
				AttachedToEntitySlotExit item = default(AttachedToEntitySlotExit);
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 direction = StringParsers.ParseVector3(array[i]);
					direction.y += 0.02f;
					item.position = GetPosition() + base.transform.TransformDirection(direction);
					float num = Mathf.Atan2(direction.x, direction.z) * 57.29578f;
					item.rotation = new Vector3(0f, num + 180f + rotation.y, 0f);
					attachedToEntitySlotInfo.exits.Add(item);
				}
			}
		}
		else
		{
			AttachedToEntitySlotExit item2 = new AttachedToEntitySlotExit
			{
				position = GetPosition() + -2f * base.transform.right,
				rotation = new Vector3(0f, rotation.y + 90f, 0f)
			};
			attachedToEntitySlotInfo.exits.Add(item2);
		}
		return attachedToEntitySlotInfo;
	}

	public Vector3 GetExitVelocity()
	{
		Vector3 velocityPerSecond = GetVelocityPerSecond();
		if (GetWheelsOnGround() > 0)
		{
			velocityPerSecond *= 0.5f;
		}
		return velocityPerSecond * 0.7f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraInit()
	{
		Transform transform = base.transform;
		Vector3 forward = transform.forward;
		cameraStartVec.x = forward.x;
		cameraStartVec.y = forward.z;
		cameraPos = transform.position;
		cameraPos.y += 1.8f;
		EntityPlayerLocal attachedPlayerLocal = GetAttachedPlayerLocal();
		if ((bool)attachedPlayerLocal)
		{
			vp_FPCamera vp_FPCamera2 = attachedPlayerLocal.vp_FPCamera;
			cameraStartPos = vp_FPCamera2.transform.position;
			cameraStartBlend = 0.5f;
			vp_FPCamera2.m_Current3rdPersonBlend = 1f;
			cameraDist = (cameraPos - cameraStartPos).magnitude;
			cameraPos.y = attachedPlayerLocal.vp_FPCamera.transform.position.y;
			vp_FPCamera2.Position3rdPersonOffset = new Vector3(0f, 1.8f, cameraDist);
			vp_FPCamera2.DrivingPosition = attachedPlayerLocal.vp_FPController.SmoothPosition;
		}
	}

	public override void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
		if (isEntityRemote)
		{
			return;
		}
		if (!RBActive)
		{
			if (vehicleRB.velocity.magnitude > 0.01f && vehicleRB.angularVelocity.magnitude > 0.05f)
			{
				RBActive = true;
			}
			if (vehicleRB.isKinematic && (!collision.rigidbody || collision.rigidbody.velocity.magnitude > 0.05f))
			{
				RBActive = true;
			}
		}
		Entity entity = null;
		int layer = collision.gameObject.layer;
		if (layer != 16)
		{
			ColliderHitCallForward component = collision.gameObject.GetComponent<ColliderHitCallForward>();
			if ((bool)component)
			{
				entity = component.Entity;
			}
			if (!entity)
			{
				entity = FindEntity(collision.transform.parent);
			}
			if (!entity)
			{
				Rigidbody rigidbody = collision.rigidbody;
				if ((bool)rigidbody)
				{
					entity = FindEntity(rigidbody.transform);
				}
			}
		}
		if ((bool)entity && entity.IsSpawned())
		{
			if (!(collision.impulse.sqrMagnitude > 4f))
			{
				return;
			}
			Vector3 vector = -collision.relativeVelocity;
			if (layer != 19)
			{
				vector *= 0.4f;
			}
			float num = vector.magnitude + 0.0001f;
			Vector3 vector2 = vector * (1f / num);
			EnumBodyPartHit enumBodyPartHit = EnumBodyPartHit.Torso;
			bool flag = false;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			int contactCount = collision.contactCount;
			for (int i = 0; i < contactCount; i++)
			{
				ContactPoint contact = collision.GetContact(i);
				zero += contact.point;
				zero2 += contact.normal;
				flag |= contact.thisCollider.CompareTag("E_VehicleStrong");
				string text = contact.otherCollider.tag;
				enumBodyPartHit |= DamageSource.TagToBodyPart(text);
			}
			zero *= 1f / (float)contactCount;
			zero += Origin.position;
			zero2 = Vector3.Normalize(zero2);
			float num2 = 0f - Vector3.Dot(vector2, zero2);
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			if (num > 1f)
			{
				float num3 = Vector3.Dot(entity.motion.normalized, vector2);
				if (num3 > 0.2f)
				{
					float num4 = entity.motion.magnitude * 20f;
					num -= num4 * num3;
				}
			}
			float num5 = num * num2;
			float num6 = vehicleRB.mass * 0.2f;
			num6 += 20f;
			float massKg = EntityClass.list[entity.entityClass].MassKg;
			float num7 = num6 / massKg;
			float num8 = Utils.FastClamp(num7, 0.25f, 1.6f);
			float num9 = num5 * num8;
			float num10 = Utils.FastClamp(num7, 1f, 1.5f);
			float num11 = num5 / num10;
			if (massKg < 2f)
			{
				num2 = 0f;
				num9 = 0f;
				num11 = 0f;
			}
			EntityPlayer entityPlayer = entity as EntityPlayer;
			if ((bool)entityPlayer && (float)entityPlayer.SpawnedTicks <= 80f)
			{
				num9 = 0f;
				num11 = 0f;
			}
			bool flag2 = world.IsWorldEvent(World.WorldEvent.BloodMoon);
			bool flag3 = num7 >= 2f && !flag2 && (lastRBVel.sqrMagnitude > 10.240001f || num9 > 2.1f);
			vector *= num6 * 0.008f;
			vector.y = Utils.FastMin(50f, vector.y + vector.magnitude * 3f);
			if (num9 > 2.1f)
			{
				int damageSourceEntityId = entityId;
				Entity firstAttached = GetFirstAttached();
				if ((bool)firstAttached)
				{
					damageSourceEntityId = firstAttached.entityId;
				}
				DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Crushing, damageSourceEntityId, vector);
				damageSourceEntity.bodyParts = enumBodyPartHit;
				damageSourceEntity.DismemberChance = 1.2f;
				float num12 = 1f + (num9 - 2.1f) * 12f;
				if ((bool)entityPlayer)
				{
					num12 = Utils.FastMin(num12, 10f);
				}
				if (flag)
				{
					num12 *= vehicle.EffectEntityDamagePer;
				}
				bool num13 = entity.IsAlive();
				entity.DamageEntity(damageSourceEntity, (int)num12, _criticalHit: false);
				if ((entity.entityFlags & (EntityFlags.AIHearing | EntityFlags.Player)) != EntityFlags.None && num12 > 70f)
				{
					SpawnParticle("blood_vehicle", entity.entityId, 0.22f);
					if (num12 > 200f)
					{
						SpawnParticle("blood_vehicle", entity.entityId, 0.35f);
					}
				}
				float num14 = 1f;
				if (flag2)
				{
					velocityMax *= 0.7f;
					num14 *= 15f;
				}
				EntityPlayer entityPlayer2 = firstAttached as EntityPlayer;
				if ((bool)entityPlayer2)
				{
					entityPlayer2.MinEventContext.Other = entity as EntityAlive;
					entityPlayer2.FireEvent(MinEventTypes.onSelfVehicleAttackedOther);
				}
				if (num13 && entity.IsDead())
				{
					flag3 = false;
					if ((bool)entityPlayer2)
					{
						EntityAlive entityAlive = entity as EntityAlive;
						if ((bool)entityAlive)
						{
							entityPlayer2.AddKillXP(entityAlive, 0.5f);
						}
					}
				}
				else if (num9 >= num14)
				{
					float num15 = num9 * 0.09f;
					if (num9 < 8f && num15 > 0.9f)
					{
						num15 = 0.9f;
					}
					if (rand.RandomFloat < num15)
					{
						flag3 = true;
					}
				}
			}
			if (entity.emodel.IsRagdollOn)
			{
				num11 *= 0.3f;
			}
			if (flag3)
			{
				entity.emodel.DoRagdoll(2.5f, enumBodyPartHit, vector, zero, isRemote: false);
			}
			if (num11 > 2.1f)
			{
				float num16 = 1f + (num11 - 2.1f) * 28f;
				num16 *= vehicle.EffectSelfDamagePer;
				if (flag)
				{
					num16 *= vehicle.EffectStrongSelfDamagePer;
				}
				float num17 = ((Health > 1) ? 1f : 0.1f);
				damageAccumulator += num16 * num17;
				ApplyAccumulatedDamage();
			}
			if (num > 0.1f && num2 > 0.2f)
			{
				velocityMax *= Mathf.LerpUnclamped(1f, 0.4f + num10 * 0.39666668f, num2);
			}
			return;
		}
		Vector3 vector3 = lastRBVel;
		float magnitude = vector3.magnitude;
		float num18 = Utils.FastMax(0f, magnitude - 1.5f) * vehicleRB.mass * (7f / 120f);
		if (isStay)
		{
			num18 *= 0.2f;
		}
		if (num18 < 2f)
		{
			collisionGrazeCount++;
			return;
		}
		collisionBlockDamage = num18;
		collisionVelNorm = vector3 * (1f / magnitude);
		collisionIgnoreCount = 0;
		int contactCount2 = collision.contactCount;
		for (int j = 0; j < contactCount2; j++)
		{
			ContactPoint contact2 = collision.GetContact(j);
			Ray ray = new Ray(contact2.point + Origin.position + contact2.normal * 0.004f, -contact2.normal);
			bool flag4 = Voxel.Raycast(world, ray, 0.03f, -555520021, 69, 0f);
			if (!flag4)
			{
				ray.origin += contact2.normal * (0f - contact2.separation);
				ray.direction = -contact2.normal + collisionVelNorm;
				flag4 = Voxel.Raycast(world, ray, 0.03f, -555520021, 69, 0f);
			}
			if (flag4 && GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
			{
				bool flag5 = false;
				for (int k = 0; k < collisionHits.Count; k++)
				{
					if (collisionHits[k].hit.blockPos == Voxel.voxelRayHitInfo.hit.blockPos)
					{
						flag5 = true;
						break;
					}
				}
				if (!flag5)
				{
					contactPoints.Add(contact2);
					collisionHits.Add(Voxel.voxelRayHitInfo.Clone());
				}
			}
			else
			{
				collisionIgnoreCount++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ApplyCollisionsCoroutine()
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while (true)
		{
			yield return wait;
			int num = contactPoints.Count;
			if (num <= 0)
			{
				continue;
			}
			float num2 = ((Health > 1) ? 1f : 0.1f);
			int attackerEntityId = entityId;
			ItemActionAttack.EnumAttackMode attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvesting;
			if (hitEffectCount <= 0)
			{
				attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
			}
			float num3 = 1f / ((float)num + 0.001f);
			float num4 = collisionBlockDamage;
			num4 *= num3;
			for (int i = 0; i < num; i++)
			{
				ContactPoint contactPoint = contactPoints[i];
				WorldRayHitInfo worldRayHitInfo = collisionHits[i];
				float num5 = 0f - Vector3.Dot(contactPoint.normal, collisionVelNorm);
				num5 = Mathf.Pow(num5 * 1.01f, 3f);
				num5 = Utils.FastClamp(num5, 0.01f, 1f);
				float num6 = 0f;
				float num7 = 2.5f;
				bool flag = contactPoint.thisCollider.CompareTag("E_VehicleStrong");
				bool flag2 = worldRayHitInfo.tag == "T_Mesh";
				if (flag2)
				{
					if (contactPoint.normal.y < 0.85f)
					{
						num6 = 0.7f + 4f * rand.RandomFloat * num5;
						num7 = 0.1f;
					}
				}
				else
				{
					num6 = num4 * num5;
					if (flag)
					{
						num6 *= vehicle.EffectBlockDamagePer;
					}
					float vehicleHitScale = worldRayHitInfo.hit.blockValue.Block.VehicleHitScale;
					num6 *= vehicleHitScale;
					num7 /= vehicleHitScale;
					if (num6 < 5f)
					{
						num6 = 0f;
					}
				}
				if (num6 >= 1f)
				{
					List<string> buffActions = null;
					ItemActionAttack.AttackHitInfo attackHitInfo = new ItemActionAttack.AttackHitInfo();
					attackHitInfo.hardnessScale = 1f;
					if (flag2 || !worldRayHitInfo.hit.blockValue.Block.shape.IsTerrain())
					{
						ItemActionAttack.Hit(worldRayHitInfo, attackerEntityId, EnumDamageTypes.Bashing, num6, num6, 1f, 1f, 0f, 0.05f, "metal", null, buffActions, attackHitInfo, 1, 0, 0f, null, null, attackMode);
						if (--hitEffectCount <= 0)
						{
							attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
						}
					}
					if (!attackHitInfo.bBlockHit)
					{
						ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[worldRayHitInfo.hit.clrIdx];
						if (chunkCluster != null)
						{
							Vector3i vector3i = Vector3i.FromVector3Rounded(contactPoint.point + Origin.position);
							for (int num8 = 0; num8 >= -1; num8--)
							{
								worldRayHitInfo.hit.blockPos.y = vector3i.y + num8;
								for (int num9 = 0; num9 >= -1; num9--)
								{
									worldRayHitInfo.hit.blockPos.z = vector3i.z + num9;
									for (int num10 = 0; num10 >= -1; num10--)
									{
										worldRayHitInfo.hit.blockPos.x = vector3i.x + num10;
										if (!chunkCluster.GetBlock(worldRayHitInfo.hit.blockPos).Block.shape.IsTerrain())
										{
											ItemActionAttack.Hit(worldRayHitInfo, attackerEntityId, EnumDamageTypes.Bashing, num6, num6, 1f, 1f, 0f, 0.05f, "metal", null, buffActions, attackHitInfo, 1, 0, 0f, null, null, attackMode);
											if (--hitEffectCount <= 0)
											{
												attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
											}
											if (attackHitInfo.bBlockHit)
											{
												num8 = -999;
												num9 = -999;
												break;
											}
										}
									}
								}
							}
						}
					}
					if (attackHitInfo.bKilled && attackHitInfo.bBlockHit && attackHitInfo.blockBeingDamaged.Block is BlockModelTree { isMultiBlock: not false } blockModelTree && blockModelTree.multiBlockPos.dim.y >= 12)
					{
						velocityMax *= 0.3f;
						vehicleRB.AddRelativeForce(Vector3.up * 2.5f, ForceMode.VelocityChange);
						vehicleRB.AddRelativeForce(collisionVelNorm * 2f, ForceMode.VelocityChange);
					}
					if ((attackHitInfo.bKilled || !attackHitInfo.bBlockHit) && attackHitInfo.hardnessScale > 0f)
					{
						collisionIgnoreCount++;
					}
					num6 = Utils.FastMin(num6, attackHitInfo.damageGiven);
				}
				float num11 = num6 * num7;
				num11 *= vehicle.EffectSelfDamagePer;
				if (flag)
				{
					num11 *= vehicle.EffectStrongSelfDamagePer;
				}
				damageAccumulator += num11 * num2;
				if (num11 > 50f)
				{
					SpawnParticle("blockdestroy_metal", worldRayHitInfo.hit.pos);
				}
			}
			ApplyAccumulatedDamage();
			int num12 = collisionIgnoreCount - num;
			if (num12 >= 0)
			{
				PhysicsRevertCollisionMotion(num12);
			}
			contactPoints.Clear();
			collisionHits.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyAccumulatedDamage()
	{
		if (damageAccumulator >= 1f)
		{
			int num = (int)damageAccumulator;
			damageAccumulator -= num;
			ApplyDamage(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnParticle(string _particleName, Vector3 _pos)
	{
		Vector3i blockPos = World.worldToBlockPos(_pos);
		float lightBrightness = world.GetLightBrightness(blockPos);
		world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(_particleName, _pos, lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnParticle(string _particleName, int _entityId, float _offsetY)
	{
		Vector3 pos = new Vector3(0f, _offsetY, 0f);
		world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(_particleName, pos, 1f, Color.white, null, _entityId, ParticleEffect.Attachment.Pelvis), entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsRevertCollisionMotion(int _ignoreExcess)
	{
		if (_ignoreExcess == 0)
		{
			float num = Time.fixedDeltaTime * 0.5f;
			float num2 = lastRBVel.x * num;
			float num3 = lastRBVel.z * num;
			if (num2 < -0.0001f || num2 > 0.0001f || num3 < -0.0001f || num3 > 0.0001f)
			{
				lastRBPos.x += num2;
				lastRBPos.z += num3;
				vehicleRB.position = lastRBPos;
			}
		}
		Vector3 velocity = vehicleRB.velocity;
		velocity.x = lastRBVel.x * 0.9f;
		velocity.z = lastRBVel.z * 0.9f;
		velocity.y = lastRBVel.y * 0.6f + velocity.y * 0.4f;
		vehicleRB.velocity = velocity;
		vehicleRB.angularVelocity = lastRBAngVel;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawRayHandle(Vector3 pos, Vector3 dir, Color color, float duration = 0f)
	{
		Vector3 normalized = Vector3.Cross(Vector3.up, dir).normalized;
		Debug.DrawRay(pos, normalized * 0.005f, Color.blue, duration);
		Debug.DrawRay(pos, dir, color, duration);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawBlocks(WorldRayHitInfo hitInfo)
	{
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					Vector3i blockPos = hitInfo.hit.blockPos;
					blockPos.x += j;
					blockPos.y += i;
					blockPos.z += k;
					Vector3 start = blockPos.ToVector3() - Origin.position;
					BlockValue block = chunkCluster.GetBlock(blockPos);
					Color color = Color.black;
					if (!block.isair)
					{
						color = ((!block.Block.shape.IsTerrain()) ? Color.white : Color.yellow);
					}
					Debug.DrawRay(start, Vector3.up, color);
					Debug.DrawRay(start, Vector3.right, color);
					Debug.DrawRay(start, Vector3.forward, color);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity FindEntity(Transform t)
	{
		Entity componentInChildren = t.GetComponentInChildren<Entity>();
		if ((bool)componentInChildren)
		{
			return componentInChildren;
		}
		return t.GetComponentInParent<Entity>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void entityCollision(Vector3 _motion)
	{
	}

	public static EntityVehicle FindCollisionEntity(Transform t)
	{
		EntityVehicle entityVehicle = t.GetComponent<EntityVehicle>();
		if (!entityVehicle)
		{
			CollisionCallForward componentInParent = t.GetComponentInParent<CollisionCallForward>();
			if ((bool)componentInParent)
			{
				entityVehicle = componentInParent.Entity as EntityVehicle;
			}
		}
		return entityVehicle;
	}

	public override float GetBlockDamageScale()
	{
		EntityAlive entityAlive = base.AttachedMainEntity as EntityAlive;
		if ((bool)entityAlive)
		{
			return entityAlive.GetBlockDamageScale();
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void switchModelView(EnumEntityModelView modelView)
	{
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public override void MoveByAttachedEntity(EntityPlayerLocal _player)
	{
		if (this.movementInput == null)
		{
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		if (uIForPlayer == null || uIForPlayer.playerInput == null)
		{
			return;
		}
		PlayerActionsVehicle vehicleActions = uIForPlayer.playerInput.VehicleActions;
		MovementInput movementInput = _player.movementInput;
		if (_player == base.AttachedMainEntity)
		{
			this.movementInput.moveForward = (_player.MoveController.isAutorun ? 1f : vehicleActions.Move.Y);
			this.movementInput.moveStrafe = vehicleActions.Move.X;
			this.movementInput.down = vehicleActions.Hop.IsPressed;
			this.movementInput.jump = vehicleActions.Brake.IsPressed;
			if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, _player) > 0f)
			{
				this.movementInput.moveForward *= -1f;
				this.movementInput.moveStrafe *= -1f;
			}
			this.movementInput.running = _player.movementInput.running;
			this.movementInput.lastInputController = movementInput.lastInputController;
			if (vehicleActions.ToggleTurnMode.WasPressed && !uIForPlayer.windowManager.IsModalWindowOpen())
			{
				isTurnTowardsLook = !isTurnTowardsLook;
			}
		}
		movementInput.rotation.x *= vehicle.CameraTurnRate.x;
		movementInput.rotation.y *= vehicle.CameraTurnRate.y;
		float num = vehicleActions.Scroll.Value;
		if (vehicleActions.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			num *= 0.25f;
		}
		if (num != 0f)
		{
			cameraDistScale += num * -0.5f;
			cameraDistScale = Utils.FastClamp(cameraDistScale, 0.3f, 1.2f);
			cameraOutTime = 999f;
		}
	}

	public bool HasHeadlight()
	{
		if (vehicle.FindPart("headlight") is VPHeadlight vPHeadlight && ((bool)vPHeadlight.GetTransform() || vPHeadlight.modInstalled))
		{
			return true;
		}
		return false;
	}

	public void ToggleHeadlight()
	{
		IsHeadlightOn = !IsHeadlightOn;
	}

	public override float GetLightLevel()
	{
		if (!(vehicle.FindPart("headlight") is VPHeadlight vPHeadlight))
		{
			return 0f;
		}
		return vPHeadlight.GetLightLevel();
	}

	public void UseHorn(EntityPlayerLocal player)
	{
		string hornSoundName = vehicle.GetHornSoundName();
		if (hornSoundName.Length > 0)
		{
			PlayOneShot(hornSoundName);
		}
		if (onHonkEvent != "")
		{
			GameEventManager.Current.HandleAction(onHonkEvent, null, player, twitchActivated: false, position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcWaterDepth(float offsetY)
	{
		Vector3 worldPos = position;
		worldPos.y += offsetY;
		Vector3i pos = World.worldToBlockPos(worldPos);
		if (world.IsWater(pos))
		{
			for (int i = 0; i < 5; i++)
			{
				pos.y++;
				if (!world.IsWater(pos))
				{
					break;
				}
			}
			return (float)pos.y - worldPos.y;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse damageResponse = new DamageResponse
		{
			Source = _damageSource,
			Strength = _strength,
			Critical = _criticalHit,
			HitDirection = Utils.EnumHitDirection.None,
			MovementState = MovementState,
			Random = rand.RandomFloat,
			ImpulseScale = impulseScale
		};
		ProcessDamageResponseLocal(damageResponse);
		return damageResponse;
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		DamageSource source = _dmResponse.Source;
		if (source.damageType == EnumDamageTypes.Disease || source.damageType == EnumDamageTypes.Suffocation)
		{
			return;
		}
		UpdateInteractionUI();
		int strength = _dmResponse.Strength;
		if ((bool)base.AttachedMainEntity && !isEntityRemote && world.IsWorldEvent(World.WorldEvent.BloodMoon))
		{
			velocityMax *= 0.6f;
			vehicleRB.AddRelativeForce(_dmResponse.Source.getDirection() * 6f, ForceMode.VelocityChange);
		}
		if (attachedEntities != null && _dmResponse.Source.GetSource() == EnumDamageSource.External)
		{
			int strength2 = Utils.FastRoundToInt((float)_dmResponse.Strength * vehicle.GetPlayerDamagePercent());
			DamageSource damageSource = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing);
			Entity[] array = attachedEntities;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DamageEntity(damageSource, strength2, _criticalHit: false);
			}
		}
		ApplyDamage(strength);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDamage(int damage)
	{
		int health = Health;
		if (health <= 0)
		{
			return;
		}
		bool flag = damage >= 99999;
		if (health == 1 || flag)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				explodeHealth -= damage;
				if (explodeHealth <= 0f && (flag || rand.RandomFloat < 0.2f))
				{
					DropItemsAsBackpack();
					Kill();
					GameManager.Instance.ExplosionServer(0, GetPosition(), World.worldToBlockPos(GetPosition()), base.transform.rotation, EntityClass.list[entityClass].explosionData, entityId, 0f, _bRemoveBlockAtExplPosition: false);
				}
			}
		}
		else
		{
			health -= damage;
			if (health <= 1)
			{
				health = 1;
				explodeHealth = (float)vehicle.GetMaxHealth() * 0.03f;
			}
			Health = health;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyCollisionDamageToAttached(int damage)
	{
		DamageSource damageSource = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.VehicleInside);
		int attachMaxCount = GetAttachMaxCount();
		for (int i = 0; i < attachMaxCount; i++)
		{
			Entity attached = GetAttached(i);
			if ((bool)attached)
			{
				attached.DamageEntity(damageSource, damage, _criticalHit: false);
			}
		}
	}

	public override bool HasImmunity(BuffClass _buffClass)
	{
		if (_buffClass.DamageType == EnumDamageTypes.Heat)
		{
			return false;
		}
		return true;
	}

	public bool IsLockedForLocalPlayer(EntityAlive _entityFocusing)
	{
		bool flag = LocalPlayerIsOwner();
		if (!(!isLocked || flag) && hasLock())
		{
			return !isAllowedUser(PlatformManager.InternalLocalUserIdentifier);
		}
		return false;
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (IsDead())
		{
			return new EntityActivationCommand[0];
		}
		bool flag = LocalPlayerIsOwner();
		bool flag2 = !isLocked || flag || !hasLock() || isAllowedUser(PlatformManager.InternalLocalUserIdentifier);
		bool flag3 = CanAttach(_entityFocusing) && isDriveable();
		bool flag4 = IsDriven();
		EntityActivationCommand entityActivationCommand = (flag4 ? new EntityActivationCommand("ride", "drive", flag3 && flag2) : new EntityActivationCommand("drive", "drive", flag3 && flag2));
		return new EntityActivationCommand[10]
		{
			entityActivationCommand,
			new EntityActivationCommand("service", "service", flag2),
			new EntityActivationCommand("repair", "wrench", vehicle.GetRepairAmountNeeded() > 0),
			new EntityActivationCommand("lock", "lock", hasLock() && !isLocked && !flag4),
			new EntityActivationCommand("unlock", "unlock", hasLock() && isLocked && flag),
			new EntityActivationCommand("keypad", "keypad", hasLock() && isLocked && (flag || vehicle.PasswordHash != 0)),
			new EntityActivationCommand("refuel", "gas", hasGasCan(_entityFocusing) && needsFuel()),
			new EntityActivationCommand("take", "hand", !hasDriver && flag2),
			new EntityActivationCommand("horn", "horn", vehicle.HasHorn()),
			new EntityActivationCommand("storage", "loot_sack", flag2)
		};
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (entityPlayerLocal.inventory.IsHoldingItemActionRunning() || uIForPlayer.xui.isUsingItemActionEntryUse)
		{
			return false;
		}
		int num = -1;
		switch (_indexInBlockActivationCommands)
		{
		case 0:
		{
			if ((uIForPlayer != null && uIForPlayer.windowManager.IsWindowOpen("windowpaging")) || !CanAttach(_entityFocusing) || !(_entityFocusing.AttachedToEntity == null) || !isDriveable() || (isLocked && hasLock() && !LocalPlayerIsOwner() && !isAllowedUser(PlatformManager.InternalLocalUserIdentifier)))
			{
				break;
			}
			if (EffectManager.GetValue(PassiveEffects.NoVehicle, null, 0f, entityPlayerLocal, null, base.EntityClass.Tags) > 0f)
			{
				Manager.PlayInsidePlayerHead("twitch_no_attack");
				break;
			}
			Vector3 vector = position - Origin.position;
			vector.y += 0.5f;
			Vector3 up = Vector3.up;
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector2 = Quaternion.AngleAxis(i * 45, up) * base.transform.forward;
				if (Physics.Raycast(vector + vector2 * 0.25f, up, 1.3f, 65536))
				{
					flag = true;
					Vector3 vector3 = _entityFocusing.position - Origin.position;
					vector3.y += 1.1f;
					vector3 = (vector3 - vector).normalized * vehicleRB.mass * 0.005f;
					AddForce(vector3);
					break;
				}
			}
			if (!flag)
			{
				EnterVehicle(_entityFocusing);
			}
			break;
		}
		case 1:
			num = _indexInBlockActivationCommands;
			break;
		case 2:
			num = _indexInBlockActivationCommands;
			break;
		case 3:
			vehicle.SetLocked(isLocked: true, entityPlayerLocal);
			PlayOneShot("misc/locking", sound_in_head: true);
			SendSyncData(2);
			break;
		case 4:
			vehicle.SetLocked(isLocked: false, entityPlayerLocal);
			PlayOneShot("misc/unlocking", sound_in_head: true);
			SendSyncData(2);
			break;
		case 5:
			PlayOneShot("misc/password_type", sound_in_head: true);
			XUiC_KeypadWindow.Open(uIForPlayer, this);
			break;
		case 6:
			num = _indexInBlockActivationCommands;
			break;
		case 7:
			num = _indexInBlockActivationCommands;
			break;
		case 8:
			UseHorn(entityPlayerLocal);
			break;
		case 9:
			num = _indexInBlockActivationCommands;
			break;
		}
		if (num >= 0)
		{
			interactionRequestType = num;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ValidateInteractingPlayer();
				int num2 = interactingPlayerId;
				if (num2 == -1)
				{
					num2 = entityPlayerLocal.entityId;
				}
				StartInteraction(entityPlayerLocal.entityId, num2);
			}
			else
			{
				interactingPlayerId = entityPlayerLocal.entityId;
				SendSyncData(4096);
				interactingPlayerId = -1;
			}
		}
		return false;
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
					num |= 0xE;
				}
				NetPackageVehicleDataSync package = NetPackageManager.GetPackage<NetPackageVehicleDataSync>().Setup(this, _playerId, num);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, _playerId);
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
	public void ValidateInteractingPlayer()
	{
		if (!GameManager.Instance.World.GetEntity(interactingPlayerId))
		{
			interactingPlayerId = -1;
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
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(localPlayerFromID);
		GUIWindowManager windowManager = uIForPlayer.windowManager;
		ushort num = 0;
		switch (interactionRequestType)
		{
		case 1:
			((XUiC_VehicleWindowGroup)((XUiWindowGroup)windowManager.GetWindow("vehicle")).Controller).CurrentVehicleEntity = this;
			windowManager.Open("vehicle", _bModal: true);
			Manager.BroadcastPlayByLocalPlayer(position, "UseActions/service_vehicle");
			break;
		case 2:
			if (XUiM_Vehicle.RepairVehicle(uIForPlayer.xui, vehicle))
			{
				num |= 4;
				PlayOneShot("crafting/craft_repair_item", sound_in_head: true);
			}
			StopInteraction(num);
			break;
		case 6:
			if (AddFuelFromInventory(localPlayerFromID))
			{
				num |= 4;
			}
			StopInteraction(num);
			break;
		case 7:
			if (!bag.IsEmpty())
			{
				GameManager.ShowTooltip(localPlayerFromID, Localization.Get("ttEmptyVehicleBeforePickup"), string.Empty, "ui_denied");
				StopInteraction(0);
				break;
			}
			if (!hasDriver)
			{
				ItemStack itemStack = new ItemStack(vehicle.GetUpdatedItemValue(), 1);
				if (localPlayerFromID.inventory.CanTakeItem(itemStack) || localPlayerFromID.bag.CanTakeItem(itemStack))
				{
					GameManager.Instance.CollectEntityServer(entityId, localPlayerFromID.entityId);
				}
				else
				{
					GameManager.ShowTooltip(localPlayerFromID, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
				}
			}
			StopInteraction(0);
			break;
		case 9:
			((XUiC_VehicleStorageWindowGroup)((XUiWindowGroup)windowManager.GetWindow("vehicleStorage")).Controller).CurrentVehicleEntity = this;
			windowManager.Open("vehicleStorage", _bModal: true);
			break;
		case 3:
		case 4:
		case 5:
		case 8:
			break;
		}
	}

	public bool CheckUIInteraction()
	{
		EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(interactingPlayerId);
		if (!localPlayerFromID)
		{
			return false;
		}
		float distanceSq = GetDistanceSq(localPlayerFromID);
		float num = Constants.cDigAndBuildDistance + 1f;
		if (distanceSq > num * num)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInteractionUI()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		for (int i = 0; i < LocalPlayerUI.PlayerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = LocalPlayerUI.PlayerUIs[i];
			if (localPlayerUI != null && localPlayerUI.xui != null && localPlayerUI.windowManager.IsWindowOpen("vehicle"))
			{
				XUiWindowGroup xUiWindowGroup = (XUiWindowGroup)localPlayerUI.windowManager.GetWindow("vehicle");
				if (xUiWindowGroup != null && xUiWindowGroup.Controller != null)
				{
					xUiWindowGroup.Controller.RefreshBindingsSelfAndChildren();
				}
			}
		}
	}

	public void StopUIInteraction()
	{
		StopInteraction(14);
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

	public void Collect(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(vehicle.GetUpdatedItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DropItemsAsBackpack()
	{
		List<ItemStack> list = new List<ItemStack>();
		ItemStack[] slots = bag.GetSlots();
		foreach (ItemStack itemStack in slots)
		{
			if (!itemStack.IsEmpty())
			{
				list.Add(itemStack);
			}
		}
		ItemValue updatedItemValue = vehicle.GetUpdatedItemValue();
		for (int j = 0; j < updatedItemValue.CosmeticMods.Length; j++)
		{
			ItemValue itemValue = updatedItemValue.CosmeticMods[j];
			if (itemValue != null && !itemValue.IsEmpty())
			{
				list.Add(new ItemStack(itemValue, 1));
			}
		}
		for (int k = 0; k < updatedItemValue.Modifications.Length; k++)
		{
			ItemValue itemValue2 = updatedItemValue.Modifications[k];
			if (itemValue2 != null && !itemValue2.IsEmpty())
			{
				list.Add(new ItemStack(itemValue2, 1));
			}
		}
		dropLoot(list.ToArray(), 0.9f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dropLoot(ItemStack[] items, float height)
	{
		Vector3 pos = position;
		pos.y += height;
		foreach (EntityLootContainer item in GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedVehicleContainer", pos, items, _skipIfEmpty: false, new Vector3(0f, 1f, 0f)))
		{
			Vector3 vel = rand.RandomOnUnitSphere * 16f;
			vel.y = Utils.FastAbs(vel.y);
			vel.y += 8f;
			item.AddVelocity(vel);
		}
	}

	public void AddMaxFuel()
	{
		vehicle.AddFuel(vehicle.GetMaxFuelLevel());
	}

	public bool AddFuelFromInventory(EntityAlive entity)
	{
		if (vehicle.GetFuelPercent() < 1f)
		{
			float maxFuelLevel = vehicle.GetMaxFuelLevel();
			float fuelLevel = vehicle.GetFuelLevel();
			float f = Mathf.Min(2500f, (maxFuelLevel - fuelLevel) * 25f);
			float num = takeFuel(entity, Mathf.CeilToInt(f));
			vehicle.AddFuel(num / 25f);
			PlayOneShot("useactions/gas_refill");
			return true;
		}
		return false;
	}

	public int GetFuelCount()
	{
		return Mathf.FloorToInt(vehicle.GetFuelLevel() * 25f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float takeFuel(EntityAlive _entityFocusing, int count)
	{
		EntityPlayer entityPlayer = _entityFocusing as EntityPlayer;
		if (!entityPlayer)
		{
			return 0f;
		}
		string fuelItem = GetVehicle().GetFuelItem();
		if (fuelItem == "")
		{
			return 0f;
		}
		ItemValue item = ItemClass.GetItem(fuelItem);
		int num = entityPlayer.inventory.DecItem(item, count);
		if (num == 0)
		{
			num = entityPlayer.bag.DecItem(item, count);
			if (num == 0)
			{
				return 0f;
			}
		}
		int num2 = num;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		if (null != uIForPlayer)
		{
			ItemStack itemStack = new ItemStack(item, num);
			uIForPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		}
		else
		{
			Log.Warning("EntityVehicle::takeFuel - Failed to remove item stack from player's collected item list.");
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEntityStatic()
	{
		return true;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public Vehicle GetVehicle()
	{
		return vehicle;
	}

	public void SetBagModified()
	{
		SendSyncData(8);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSyncData(ushort syncFlags)
	{
		NetPackageVehicleDataSync package = NetPackageManager.GetPackage<NetPackageVehicleDataSync>().Setup(this, GameManager.Instance.World.GetPrimaryPlayerId(), syncFlags);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}
	}

	public ushort GetSyncFlagsReplicated(ushort syncFlags)
	{
		return (ushort)(syncFlags & 0xC007);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version < 26)
		{
			Log.Warning("Vehicle: Ignoring old data v{0}", _version);
		}
		else
		{
			ushort syncFlags = _br.ReadUInt16();
			ReadSyncData(_br, syncFlags, 0);
		}
	}

	public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
	{
		isReadingFromRemote = senderId != GameManager.Instance.World.GetPrimaryPlayerId();
		byte b = _br.ReadByte();
		if ((syncFlags & 0x8000) > 0)
		{
			incomingRemoteData.Flags = _br.ReadInt32();
			incomingRemoteData.Flags |= 1;
			incomingRemoteData.MotorTorquePercent = (float)_br.ReadInt16() * 0.0001f;
			incomingRemoteData.SteeringPercent = (float)_br.ReadInt16() * 0.0001f;
			incomingRemoteData.Velocity = StreamUtils.ReadVector3(_br);
			List<RemoteData.Part> list = new List<RemoteData.Part>(4);
			incomingRemoteData.parts = list;
			RemoteData.Part item = default(RemoteData.Part);
			while (true)
			{
				switch (_br.ReadByte())
				{
				case 2:
					item.pos = StreamUtils.ReadVector3(_br);
					goto IL_00cd;
				default:
					item.pos = Vector3.zero;
					goto IL_00cd;
				case 0:
					break;
				}
				break;
				IL_00cd:
				item.rot = StreamUtils.ReadQuaterion(_br);
				list.Add(item);
			}
		}
		if ((syncFlags & 0x4000) > 0)
		{
			IsHeadlightOn = _br.ReadBoolean();
		}
		if ((syncFlags & 1) > 0)
		{
			delayedAttachments.Clear();
			int num = _br.ReadByte();
			DelayedAttach item2 = default(DelayedAttach);
			for (int i = 0; i < num; i++)
			{
				int num2 = _br.ReadInt32();
				if (num2 != -1)
				{
					item2.entityId = num2;
					item2.slot = i;
					delayedAttachments.Add(item2);
					continue;
				}
				Entity attached = GetAttached(i);
				if ((bool)attached)
				{
					attached.Detach();
				}
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b2 = _br.ReadByte();
			isInteractionLocked = (b2 & 1) > 0;
			isLocked = (b2 & 2) > 0;
			vehicle.OwnerId = PlatformUserIdentifierAbs.FromStream(_br);
			vehicle.PasswordHash = _br.ReadInt32();
			vehicle.AllowedUsers.Clear();
			int num3 = _br.ReadByte();
			for (int j = 0; j < num3; j++)
			{
				vehicle.AllowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br));
			}
		}
		if ((syncFlags & 4) > 0)
		{
			int num4 = _br.ReadByte();
			ItemStack[] array = new ItemStack[num4];
			for (int k = 0; k < num4; k++)
			{
				ItemStack itemStack = new ItemStack();
				itemStack.Read(_br);
				array[k] = itemStack;
			}
			vehicle.LoadItems(array);
		}
		if ((syncFlags & 8) > 0)
		{
			int num5 = _br.ReadByte();
			ItemStack[] array2 = new ItemStack[num5];
			for (int l = 0; l < num5; l++)
			{
				ItemStack itemStack2 = new ItemStack();
				array2[l] = itemStack2.Read(_br);
			}
			bag.SetSlots(array2);
			if (b >= 1)
			{
				if (_br.ReadBoolean())
				{
					bag.LockedSlots = new PackedBoolArray();
					bag.LockedSlots.Read(_br);
				}
				else
				{
					bag.LockedSlots = new PackedBoolArray(bag.SlotCount);
				}
			}
		}
		if ((syncFlags & 0x1000) > 0)
		{
			int requestId = _br.ReadInt32();
			CheckInteractionRequest(senderId, requestId);
		}
		isReadingFromRemote = false;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		ushort num = (ushort)(_bNetworkWrite ? 16399 : 16398);
		_bw.Write(num);
		WriteSyncData(_bw, num);
	}

	public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
	{
		_bw.Write((byte)1);
		if ((syncFlags & 0x8000) > 0)
		{
			int num = 0;
			if (vehicle.CurrentIsAccel)
			{
				num |= 2;
			}
			if (vehicle.CurrentIsBreak)
			{
				num |= 4;
			}
			_bw.Write(num);
			_bw.Write((short)(vehicle.CurrentMotorTorquePercent * 10000f));
			_bw.Write((short)(vehicle.CurrentSteeringPercent * 10000f));
			StreamUtils.Write(_bw, vehicle.CurrentVelocity);
			int num2 = wheels.Length;
			for (int i = 0; i < num2; i++)
			{
				Wheel wheel = wheels[i];
				if ((bool)wheel.steerT && wheel.isSteerParentOfTire)
				{
					_bw.Write((byte)1);
					StreamUtils.Write(_bw, wheel.steerT.localRotation);
				}
				if ((bool)wheel.tireT)
				{
					_bw.Write((byte)2);
					StreamUtils.Write(_bw, wheel.tireT.localPosition);
					StreamUtils.Write(_bw, wheel.tireT.localRotation);
				}
			}
			_bw.Write((byte)0);
		}
		if ((syncFlags & 0x4000) > 0)
		{
			_bw.Write(IsHeadlightOn);
		}
		if ((syncFlags & 1) > 0)
		{
			int attachMaxCount = GetAttachMaxCount();
			_bw.Write((byte)attachMaxCount);
			for (int j = 0; j < attachMaxCount; j++)
			{
				Entity attached = GetAttached(j);
				_bw.Write(attached ? attached.entityId : (-1));
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
			vehicle.OwnerId.ToStream(_bw);
			_bw.Write(vehicle.PasswordHash);
			_bw.Write((byte)vehicle.AllowedUsers.Count);
			for (int k = 0; k < vehicle.AllowedUsers.Count; k++)
			{
				vehicle.AllowedUsers[k].ToStream(_bw);
			}
		}
		if ((syncFlags & 4) > 0)
		{
			_bw.Write((byte)1);
			vehicle.GetItems()[0].Write(_bw);
		}
		if ((syncFlags & 8) > 0)
		{
			ItemStack[] slots = bag.GetSlots();
			_bw.Write((byte)slots.Length);
			for (int l = 0; l < slots.Length; l++)
			{
				slots[l].Write(_bw);
			}
			_bw.Write(bag.LockedSlots != null);
			bag.LockedSlots?.Write(_bw);
		}
		if ((syncFlags & 0x1000) > 0)
		{
			_bw.Write(interactingPlayerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDriveable()
	{
		return vehicle.IsDriveable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasStorage()
	{
		return vehicle.HasStorage();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasHandlebars()
	{
		return vehicle.HasSteering();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool HasChassis()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool needsFuel()
	{
		if (vehicle.HasEnginePart())
		{
			return vehicle.GetFuelPercent() < 1f;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasGasCan(EntityAlive _ea)
	{
		string fuelItem = GetVehicle().GetFuelItem();
		if (fuelItem == "")
		{
			return false;
		}
		ItemValue item = ItemClass.GetItem(fuelItem);
		int num = 0;
		ItemStack[] slots = _ea.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.type == item.type)
			{
				num++;
			}
		}
		for (int j = 0; j < _ea.inventory.PUBLIC_SLOTS; j++)
		{
			if (_ea.inventory.GetItem(j).itemValue.type == item.type)
			{
				num++;
			}
		}
		return num > 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasLock()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAllowedUser(PlatformUserIdentifierAbs _userIdentifier)
	{
		return vehicle.AllowedUsers.Contains(_userIdentifier);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PlayStepSound(string stepSound, float _volume)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _steamId, out bool changed)
	{
		changed = false;
		bool flag = Utils.HashString(_password) == vehicle.PasswordHash.ToString();
		if (LocalPlayerIsOwner())
		{
			if (!flag)
			{
				changed = true;
				vehicle.PasswordHash = _password.GetHashCode();
				vehicle.AllowedUsers.Clear();
				if (vehicle.OwnerId == null)
				{
					SetOwner(_steamId);
					isLocked = true;
				}
				SendSyncData(2);
			}
			return true;
		}
		if (flag)
		{
			vehicle.AllowedUsers.Add(_steamId);
			SendSyncData(2);
			return true;
		}
		return false;
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
		return vehicle.OwnerId;
	}

	public override void OnAddedToWorld()
	{
		bSpawned = true;
		HandleNavObject();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		vehicle.OwnerId = _userIdentifier;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return vehicle.AllowedUsers.Contains(_userIdentifier);
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
		if (vehicle.OwnerId == null)
		{
			return true;
		}
		return vehicle.OwnerId.Equals(_userIdentifier);
	}

	public bool HasPassword()
	{
		return vehicle.PasswordHash != 0;
	}

	public string GetPassword()
	{
		return vehicle.PasswordHash.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckForOutOfWorld()
	{
		if (bDead)
		{
			return;
		}
		Vector3 _pos = position;
		if (world.AdjustBoundsForPlayers(ref _pos, 0.2f))
		{
			if (!vehicleRB.isKinematic)
			{
				Vector3 velocity = vehicleRB.velocity;
				velocity.x *= -0.5f;
				velocity.z *= -0.5f;
				vehicleRB.velocity = velocity;
			}
			_pos.y = vehicleRB.position.y + Origin.position.y;
			SetPosition(_pos);
			EntityPlayerLocal attachedPlayerLocal = GetAttachedPlayerLocal();
			if ((bool)attachedPlayerLocal)
			{
				GameManager.ShowTooltip(attachedPlayerLocal, Localization.Get("ttWorldEnd"));
			}
			return;
		}
		Vector3 centerPosition = GetCenterPosition();
		Chunk chunk = (Chunk)world.GetChunkFromWorldPos((int)centerPosition.x, (int)centerPosition.y, (int)centerPosition.z);
		if (chunk == null || !chunk.IsCollisionMeshGenerated || !chunk.IsDisplayed)
		{
			if (!vehicleRB.isKinematic)
			{
				vehicleRB.velocity = Vector3.zero;
				vehicleRB.angularVelocity = Vector3.zero;
			}
			if (!hasDriver)
			{
				RBActive = false;
				isTryToFall = true;
			}
			return;
		}
		Entity firstAttached = GetFirstAttached();
		if ((bool)firstAttached && !firstAttached.IsSpawned())
		{
			return;
		}
		if (RBActive && !IsTerrainBelow(centerPosition))
		{
			if (++worldTerrainFailCount <= 6)
			{
				if (worldTerrainFailCount == 2)
				{
					chunk.NeedsRegeneration = true;
					LogVehicle("{0}, {1}, center {2}, rbPos {3}, in ground. Chunk regen {4}", base.transform.parent.name, _pos.ToCultureInvariantString(), centerPosition.ToCultureInvariantString(), (vehicleRB.position + Origin.position).ToCultureInvariantString(), chunk);
				}
			}
			else if (hasWorldValidPos)
			{
				Vector3 vector = worldValidPos - _pos;
				if (vector.y < 0f)
				{
					vector.y = 0f;
				}
				float sqrMagnitude = vector.sqrMagnitude;
				vector = vector.normalized;
				if (sqrMagnitude > 0.122499995f)
				{
					_pos = worldValidPos + vector * 0.1f;
					SetPosition(_pos);
				}
				if (!vehicleRB.isKinematic)
				{
					Vector3 velocity2 = vehicleRB.velocity;
					if (Vector3.Dot(velocity2, vector) < 0f)
					{
						velocity2 *= -0.5f;
					}
					velocity2.y = 1f + rand.RandomFloat * 2f;
					velocity2 += vector * 3f;
					vehicleRB.velocity = velocity2;
					vehicleRB.angularVelocity = Vector3.zero;
				}
				LogVehicle("{0}, {1}, center {2} in ground. back {3}", base.transform.parent.name, _pos.ToCultureInvariantString(), centerPosition.ToCultureInvariantString(), worldValidPos.ToCultureInvariantString());
				worldValidPos.x += (rand.RandomFloat - 0.5f) * 2f * 0.05f;
				worldValidPos.z += (rand.RandomFloat - 0.5f) * 2f * 0.05f;
				worldValidPos.y += 0.001f;
				worldValidDelay -= Time.deltaTime;
				if (worldValidDelay <= 0f)
				{
					worldValidDelay = 1f;
					worldValidPos.y += 1.2f;
				}
			}
			else
			{
				Vector3 pos = centerPosition;
				pos.y = 257f;
				bool flag = IsTerrainBelow(pos);
				if (flag)
				{
					_pos.y += 3f;
					SetPosition(_pos);
				}
				LogVehicle("{0}, {1}, center {2} (vel {3}, {4}) {5}", base.transform.parent.name, _pos.ToCultureInvariantString(), centerPosition.ToCultureInvariantString(), vehicleRB.velocity, vehicleRB.angularVelocity, flag ? " in ground. up" : " out of world");
				if (!vehicleRB.isKinematic)
				{
					vehicleRB.velocity *= 0.5f;
					vehicleRB.angularVelocity *= 0.5f;
				}
			}
		}
		else
		{
			worldTerrainFailCount = 0;
			if (hasWorldValidPos)
			{
				if ((worldValidPos - _pos).sqrMagnitude > 4f)
				{
					worldValidPos = _pos;
				}
			}
			else
			{
				hasWorldValidPos = true;
				worldValidPos = _pos;
			}
		}
		if (isTryToFall)
		{
			isTryToFall = false;
			RBActive = true;
			vehicleRB.WakeUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTerrainBelow(Vector3 pos)
	{
		Ray ray = new Ray(pos - Origin.position, Vector3.down);
		if (Physics.Raycast(ray, out var hitInfo, float.MaxValue, 1073807360))
		{
			return true;
		}
		Utils.DrawCircleLinesHorzontal(ray.origin, 0.25f, new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), 8, 5f);
		Utils.DrawLine(ray.origin, new Vector3(ray.origin.x, 0f - Origin.position.y, ray.origin.z), new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), 5, 5f);
		ray.origin += new Vector3(0.02f, 0.5f, 0.03f);
		if (Physics.SphereCast(ray, 0.1f, out hitInfo, float.MaxValue, 1073807360))
		{
			return true;
		}
		return false;
	}

	public override bool IsDeadIfOutOfWorld()
	{
		return false;
	}

	public override void CheckPosition()
	{
		base.CheckPosition();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || !Spawned || hasDriver)
		{
			return;
		}
		world.GetWorldExtent(out var _minSize, out var _maxSize);
		if (position.y < (float)_minSize.y)
		{
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(new Vector3i((int)position.x, (int)position.y, (int)position.z));
			if (chunk != null && chunk.IsCollisionMeshGenerated && chunk.IsDisplayed)
			{
				TeleportToWithinBounds(_minSize.ToVector3(), _maxSize.ToVector3());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TeleportToWithinBounds(Vector3 _min, Vector3 _max)
	{
		_min.x += 66f;
		_min.z += 66f;
		_max.x -= 66f;
		_max.z -= 66f;
		Vector3 vector = position;
		if (vector.x < _min.x)
		{
			vector.x = _min.x;
		}
		else if (vector.x > _max.x)
		{
			vector.x = _max.x;
		}
		if (vector.z < _min.z)
		{
			vector.z = _min.z;
		}
		else if (vector.z > _max.z)
		{
			vector.z = _max.z;
		}
		if (Physics.Raycast(new Ray(new Vector3(vector.x, 999f, vector.z) - Origin.position, Vector3.down), out var hitInfo, float.MaxValue, 1076428800))
		{
			vector.y = hitInfo.point.y + Origin.position.y + 1f;
			SetPosition(vector);
			Log.Out("Vehicle out of world. Teleporting to " + vector.ToCultureInvariantString());
		}
	}

	public void Kill()
	{
		int attachMaxCount = GetAttachMaxCount();
		for (int i = 0; i < attachMaxCount; i++)
		{
			Entity attached = GetAttached(i);
			if (attached != null)
			{
				attached.Detach();
			}
		}
		timeStayAfterDeath = 0;
		SetDead();
		MarkToUnload();
	}

	public override void OnEntityUnload()
	{
		if ((bool)vehicleRB)
		{
			position = vehicleRB.position + Origin.position;
			rotation = vehicleRB.rotation.eulerAngles;
		}
		base.OnEntityUnload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (!(EntityClass.list[entityClass].NavObject != ""))
		{
			return;
		}
		if (LocalPlayerIsOwner())
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, vehicle.GetMeshTransform());
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				primaryPlayer.Waypoints.UpdateEntityVehicleWayPoint(this);
			}
		}
		else if (NavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(NavObject);
			NavObject = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogVehicle(string format, params object[] args)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || (bool)GetAttachedPlayerLocal())
		{
			format = $"{GameManager.frameCount} Vehicle {format}";
			Log.Out(format, args);
		}
	}
}
