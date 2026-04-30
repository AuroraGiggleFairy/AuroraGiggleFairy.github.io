using System;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTurret : EntityAlive
{
	public class TurretInventory : Inventory
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int cSlotCount;

		public TurretInventory(IGameManager _gameManager, EntityAlive _entity)
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

	public const int SaveVersion = 1;

	public const string JunkTurretSledgeItem = "gunBotT1JunkSledge";

	public const string JunkTurretRangedItem = "gunBotT2JunkTurret";

	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public MiniTurretFireController FireController;

	public Transform Laser;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	public float CenteredYaw;

	public float CenteredPitch;

	public bool TargetOwner;

	public bool TargetAllies;

	public bool TargetStrangers = true;

	public bool TargetEnemies = true;

	public int maxOwnerDistance = 10;

	public ItemValue OriginalItemValue = ItemValue.None.Clone();

	public bool PickedUpWaitingToDelete;

	public PlatformUserIdentifierAbs OwnerID;

	public bool IsOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody thisRigidBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UpdateLightOnAllMaterials uloam;

	public EntityAlive Owner;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTargetEntityId = -2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue lastOriginalItemValue = ItemValue.None.Clone();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPOSITION_UPDATE_CHECK_TIME = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float posCheckTimer = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int fallDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 fallPos;

	public int TargetEntityId = -1;

	public bool ForceOn;

	public float DistanceToOwner = float.MaxValue;

	public Vector3 groundPosition;

	public Vector3 groundUpDirection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFalling;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 StaticPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLerpTimeScale = 8f;

	public int tmpBelongsPlayerID;

	public override bool IsValidAimAssistSnapTarget => false;

	public override bool IsValidAimAssistSlowdownTarget => false;

	public override string LocalizedEntityName => Localization.Get(EntityName);

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

	public bool IsTurning
	{
		get
		{
			if (IsOn)
			{
				if (!YawController.IsTurning)
				{
					return PitchController.IsTurning;
				}
				return true;
			}
			return false;
		}
	}

	public override int Health
	{
		get
		{
			return (int)Mathf.Max((float)OriginalItemValue.MaxUseTimes - OriginalItemValue.UseTimes, 1f);
		}
		set
		{
			OriginalItemValue.UseTimes = OriginalItemValue.MaxUseTimes - value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		bag = new Bag(this);
		base.Awake();
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		_ = EntityClass.list[entityClass];
		base.transform.tag = "E_Vehicle";
		bag.SetupSlots(ItemStack.CreateArray(0));
		Transform transform = base.transform;
		thisRigidBody = transform.GetComponent<Rigidbody>();
		if ((bool)thisRigidBody)
		{
			thisRigidBody.centerOfMass = new Vector3(0f, 0.1f, 0f);
			thisRigidBody.sleepThreshold = thisRigidBody.mass * 0.01f * 0.01f * 0.5f;
			transform.gameObject.AddComponent<CollisionCallForward>().Entity = this;
			transform.gameObject.layer = 21;
			Utils.SetTagsRecursively(transform, "E_Vehicle");
		}
		alertEnabled = false;
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		_dmResponse.Fatal = false;
	}

	public override void SetDead()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ClientKill(DamageResponse _dmResponse)
	{
		_dmResponse.Fatal = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse result = base.damageEntityLocal(_damageSource, _strength, _criticalHit, impulseScale);
		result.Fatal = false;
		return result;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		IsOn = false;
		if (GameManager.Instance != null && GameManager.Instance.World != null && belongsPlayerId != -1)
		{
			EntityAlive entityAlive = (EntityAlive)GameManager.Instance.World.GetEntity(belongsPlayerId);
			if (entityAlive != null)
			{
				entityAlive.RemoveOwnedEntity(entityId);
			}
		}
		FireController.Update();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
	}

	public override void PostInit()
	{
		Transform transform = base.transform;
		transform.rotation = qrotation;
		StaticPosition = position;
		fallPos = position;
		YawController = transform.GetComponentInChildren<AutoTurretYawLerp>();
		PitchController = transform.GetComponentInChildren<AutoTurretPitchLerp>();
		FireController = transform.GetComponentInChildren<MiniTurretFireController>();
		Laser = transform.FindInChilds("turret_laser");
		Cone = transform.FindInChilds("turret_cone");
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(OwnerID);
		if (playerData != null)
		{
			belongsPlayerId = playerData.EntityId;
		}
		HandleNavObject();
		InitTurret();
	}

	public override void InitInventory()
	{
		inventory = new TurretInventory(GameManager.Instance, this);
	}

	public void InitTurret()
	{
		FireController.Init(base.EntityClass.Properties, this);
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
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
		if (!Owner)
		{
			Owner = (EntityAlive)GameManager.Instance.World.GetEntity(belongsPlayerId);
			if (Owner != null)
			{
				Owner.AddOwnedEntity(this);
			}
		}
		if (uloam == null && OriginalItemValue.ItemClass != null)
		{
			uloam = base.gameObject.AddMissingComponent<UpdateLightOnAllMaterials>();
			uloam.AddRendererNameToIgnore("turret_laser");
			uloam.SetTintColorForItem(Vector3.one);
			if (OriginalItemValue.ItemClass.Properties.Values.ContainsKey(Block.PropTintColor))
			{
				uloam.SetTintColorForItem(Block.StringToVector3(OriginalItemValue.GetPropertyOverride(Block.PropTintColor, OriginalItemValue.ItemClass.Properties.Values[Block.PropTintColor])));
			}
			else
			{
				uloam.SetTintColorForItem(Block.StringToVector3(OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255")));
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			IsOn = OriginalItemValue.PercentUsesLeft > 0f;
			if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, OriginalItemValue) > 0)
			{
				IsOn &= OriginalItemValue.Meta > 0;
			}
			if (GameManager.Instance != null && GameManager.Instance.World != null && belongsPlayerId != -1)
			{
				IsOn &= Owner != null;
				if (Owner != null)
				{
					if (EffectManager.GetValue(PassiveEffects.DisableItem, OriginalItemValue, 0f, Owner, null, OriginalItemValue.ItemClass.ItemTags) > 0f)
					{
						IsOn = false;
					}
					else
					{
						maxOwnerDistance = (int)EffectManager.GetValue(PassiveEffects.JunkTurretActiveRange, OriginalItemValue, 10f, Owner);
						if (IsOn)
						{
							DistanceToOwner = GetDistanceSq(Owner);
							IsOn &= DistanceToOwner < (float)(maxOwnerDistance * maxOwnerDistance);
						}
						if (IsOn)
						{
							int num = (int)EffectManager.GetValue(PassiveEffects.JunkTurretActiveCount, OriginalItemValue, 1f, Owner);
							int num2 = 0;
							OwnedEntityData[] array = Owner.GetOwnedEntities();
							for (int i = 0; i < array.Length; i++)
							{
								EntityTurret entityTurret = GameManager.Instance.World.GetEntity(array[i].Id) as EntityTurret;
								if (!(entityTurret == null) && entityTurret.entityId != entityId)
								{
									if (entityTurret.IsOn)
									{
										num2++;
									}
									IsOn &= num2 <= num || DistanceToOwner < entityTurret.DistanceToOwner || ForceOn;
									if (!IsOn)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
			else if (IsOn)
			{
				IsOn &= belongsPlayerId == -1 && OwnerID == null;
			}
			ForceOn = false;
			if (TargetEntityId != lastTargetEntityId || IsOn != lastIsOn || OriginalItemValue.Equals(lastOriginalItemValue))
			{
				lastOriginalItemValue = OriginalItemValue.Clone();
				lastTargetEntityId = TargetEntityId;
				lastIsOn = IsOn;
				NetPackageTurretSync package = NetPackageManager.GetPackage<NetPackageTurretSync>().Setup(entityId, TargetEntityId, IsOn, OriginalItemValue);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: true);
			}
		}
		if (Laser != null && IsOn != Laser.gameObject.activeSelf)
		{
			Laser.gameObject.SetActive(IsOn);
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public void InitDynamicSpawn()
	{
		for (int i = 1; i < ItemClass.list.Length - 1; i++)
		{
			if (ItemClass.list[i] == null)
			{
				continue;
			}
			string text = ItemClass.list[i].Name;
			if (text == "gunBotT1JunkSledge" || text == "gunBotT2JunkTurret")
			{
				OwnerID = PlatformManager.InternalLocalUserIdentifier;
				OriginalItemValue = new ItemValue(ItemClass.list[i].Id);
				AmmoCount = ItemClass.GetForId(ItemClass.list[i].Id).GetInitialMetadata(OriginalItemValue);
				ForceOn = true;
				PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(OwnerID);
				if (playerData != null)
				{
					(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(this);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		Vector3 vector = (StaticPosition = (position = base.transform.position + Origin.position));
		if (GameManager.Instance.World.GetChunkFromWorldPos((int)vector.x, (int)vector.y, (int)vector.z) is Chunk chunk && chunk.IsCollisionMeshGenerated)
		{
			if (posCheckTimer <= 0f)
			{
				posCheckTimer = 0.5f;
				int modelLayer = GetModelLayer();
				SetModelLayer(2);
				float y = fallPos.y;
				fallPos = vector;
				if (Voxel.Raycast(ray: new Ray(vector + Vector3.up * 0.375f, Vector3.down), _world: GameManager.Instance.World, distance: 255f, _layerMask: 1082195968, _hitMask: 128, _sphereRadius: 0.25f))
				{
					groundUpDirection = Voxel.phyxRaycastHit.normal;
					fallPos.y = Voxel.voxelRayHitInfo.fmcHit.pos.y;
					if (Vector3.Dot(Vector3.up, groundUpDirection) < 0.7f)
					{
						fallPos.y -= 0.1f;
					}
					if (fallPos.y < y)
					{
						fallDelay = 5;
					}
				}
				SetModelLayer(modelLayer);
			}
			float deltaTime = Time.deltaTime;
			posCheckTimer -= deltaTime;
			isFalling = false;
			if (vector != fallPos)
			{
				posCheckTimer = 0f;
				if (--fallDelay < 0)
				{
					isFalling = true;
					base.transform.position = Vector3.MoveTowards(base.transform.position, fallPos - Origin.position, 5f * deltaTime);
				}
			}
		}
		else
		{
			posCheckTimer = 0.5f;
		}
	}

	public void Collect(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(OriginalItemValue, 1);
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId);
		}
		OriginalItemValue = ItemValue.None.Clone();
		PickedUpWaitingToDelete = true;
		bPlayerStatsChanged = true;
		base.transform.gameObject.SetActive(value: false);
	}

	public bool CanInteract(int _interactingEntityId)
	{
		if (!isFalling && !PickedUpWaitingToDelete && OriginalItemValue.type != 0)
		{
			if (belongsPlayerId != _interactingEntityId)
			{
				return Health <= 1;
			}
			return true;
		}
		return false;
	}

	public override bool IsDead()
	{
		return false;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		OwnerID.ToStream(_bw);
		OriginalItemValue.Write(_bw);
		StreamUtils.Write(_bw, StaticPosition);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		int num = _br.ReadInt32();
		OwnerID = PlatformUserIdentifierAbs.FromStream(_br);
		OriginalItemValue = ItemValue.None.Clone();
		OriginalItemValue.Read(_br);
		if (num > 0)
		{
			StaticPosition = StreamUtils.ReadVector3(_br);
		}
	}
}
