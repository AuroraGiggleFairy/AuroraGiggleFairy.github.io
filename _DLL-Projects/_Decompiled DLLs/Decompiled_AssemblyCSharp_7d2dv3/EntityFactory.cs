using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFactory
{
	public class CreateEntityOperation
	{
		public Entity entity;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityCreationData ecd;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityClass ec;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPlayer;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isLocalPlayer;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityInstanceAssets entityInstanceAssets;

		[PublicizedFrom(EAccessModifier.Private)]
		public EModelInstanceAssets eModelInstanceAssets;

		public int EntityId => ecd.id;

		public string DebugEntityInfo => ecd.ToString();

		public bool IsLoadingComplete
		{
			get
			{
				if (ec == null)
				{
					return true;
				}
				EntityInstanceAssets obj = entityInstanceAssets;
				if (obj != null && obj.IsLoadComplete)
				{
					return eModelInstanceAssets?.IsLoadComplete ?? false;
				}
				return false;
			}
		}

		public static CreateEntityOperation Start(EntityCreationData _ecd, bool _isSync)
		{
			if (_ecd.id == -1)
			{
				_ecd.id = nextEntityID++;
			}
			else
			{
				nextEntityID = Math.Max(_ecd.id + 1, nextEntityID);
			}
			CreateEntityOperation createEntityOperation = new CreateEntityOperation(_ecd);
			createEntityOperation.LoadAssets(_isSync);
			return createEntityOperation;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public CreateEntityOperation(EntityCreationData _ecd)
		{
			ecd = _ecd;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadAssets(bool _isSync)
		{
			ec = EntityClass.GetEntityClass(ecd.entityClass);
			if (ec == null)
			{
				Log.Error("EntityFactory CreateEntity: unknown type ({0}) {1}", ecd.entityClass, ecd.entityName);
				return;
			}
			if (ec.EntityTier > MaxEntityTier)
			{
				EntityClass previousTierEntity;
				for (previousTierEntity = ec.GetPreviousTierEntity(); previousTierEntity != null; previousTierEntity = previousTierEntity.GetPreviousTierEntity())
				{
					if (previousTierEntity == null)
					{
						Log.Warning("EntityFactory CreateEntity: Previous Tier is invalid for " + ec.entityClassName);
					}
					if (previousTierEntity.EntityTier <= MaxEntityTier)
					{
						break;
					}
					ec = previousTierEntity;
				}
				if (previousTierEntity == null)
				{
					Log.Warning("EntityFactory CreateEntity: Previous Tier is invalid for " + ec.entityClassName);
				}
				else
				{
					ec = previousTierEntity;
				}
				ecd.entityClass = EntityClass.GetId(ec.entityClassName);
			}
			isPlayer = ecd.entityClass == EntityClass.playerMaleClass || ecd.entityClass == EntityClass.playerFemaleClass;
			isLocalPlayer = isPlayer && ecd.id == ecd.belongsPlayerId;
			entityInstanceAssets = new EntityInstanceAssets();
			entityInstanceAssets.Load(_isSync, ec, isLocalPlayer);
			eModelInstanceAssets = new EModelInstanceAssets();
			eModelInstanceAssets.Load(_isSync, ecd, ec);
		}

		public void WaitForLoadingComplete()
		{
			entityInstanceAssets?.WaitForComplete();
			eModelInstanceAssets?.WaitForComplete();
		}

		public void CompleteEntity()
		{
			if (!entityInstanceAssets.IsLoadComplete || !eModelInstanceAssets.IsLoadComplete)
			{
				Log.Error($"CreateEntityOperation cannot complete {ecd}, assets not loaded yet");
				return;
			}
			if (!entityInstanceAssets.IsLoadSuccessful)
			{
				Log.Error($"CreateEntityOperation cannot complete {ecd}, entity assets did not load successfully");
				return;
			}
			if (!eModelInstanceAssets.IsLoadSuccessful)
			{
				Log.Error($"CreateEntityOperation cannot complete {ecd}, emodel assets did not load successfully");
				return;
			}
			if (this.entity != null)
			{
				Log.Error($"Entity {this.entity} has already been created. CompleteEntity is expected to only be called once");
				return;
			}
			Transform prefabT = entityInstanceAssets.PrefabT;
			prefabT = UnityEngine.Object.Instantiate(prefabT, Vector3.zero, Quaternion.identity);
			Transform transform = prefabT;
			Transform transform2 = prefabT.Find("GameObject");
			if ((bool)transform2)
			{
				transform = transform2;
			}
			transform.position = ecd.pos - Origin.position;
			GameObject gameObject = transform.gameObject;
			Entity entity;
			if (isPlayer)
			{
				EntityPlayer entityPlayer;
				if (isLocalPlayer)
				{
					entity = addEntityComponent(gameObject, ec.classname.FullName + "Local");
					entityPlayer = (EntityPlayer)entity;
					entity.RootTransform = prefabT;
					entity.ModelTransform = prefabT.Find("Graphics");
					entity.PhysicsTransform = prefabT;
					entityPlayer.playerProfile = ecd.playerProfile;
					entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
					gameObject.AddComponent<LocalPlayer>();
				}
				else
				{
					entity = addEntityComponent(gameObject, ec.classname);
					entityPlayer = (EntityPlayer)entity;
					entity.RootTransform = prefabT;
					entity.ModelTransform = transform;
					entity.PhysicsTransform = prefabT.Find("Physics");
					entityPlayer.playerProfile = ecd.playerProfile;
					entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
					gameObject.AddComponent<GUIHUDEntityName>();
				}
				if (!ecd.holdingItem.IsEmpty())
				{
					entityPlayer.inventory.AddItem(new ItemStack(ecd.holdingItem, 1));
					entityPlayer.inventory.SetHoldingItemIdx(0);
				}
				entityPlayer.TeamNumber = ecd.teamNumber;
				entityPlayer.emodel.SetSkinTexture(ecd.skinTexture);
				prefabT.SetParent(ParentNameToTransform[ec.parentGameObjectName], worldPositionStays: false);
				prefabT.name = "Player_" + ecd.id;
				Log.Out("Created player with id=" + ecd.id);
			}
			else if (ecd.entityClass == EntityClass.itemClass)
			{
				Entity obj = (entity = gameObject.AddComponent<EntityItem>());
				entity.RootTransform = prefabT;
				entity.ModelTransform = transform;
				entity.clientEntityId = ecd.clientEntityId;
				((EntityItem)obj).OwnerId = ecd.belongsPlayerId;
				entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
				prefabT.SetParent(ParentNameToTransform["Items"], worldPositionStays: false);
				prefabT.name = "Item_" + ecd.id;
				((EntityItem)obj).SetItemStack(ecd.itemStack);
			}
			else if (ecd.entityClass == EntityClass.fallingBlockClass)
			{
				Entity obj2 = (entity = gameObject.AddComponent<EntityFallingBlock>());
				entity.RootTransform = prefabT;
				entity.ModelTransform = transform;
				entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
				prefabT.SetParent(ParentNameToTransform["FallingBlocks"], worldPositionStays: false);
				prefabT.name = "FallingBlock_" + ecd.id;
				((EntityFallingBlock)obj2).SetBlockValue(ecd.blockValues[0]);
				((EntityFallingBlock)obj2).SetTextureFull(ecd.textureFullArrays[0]);
			}
			else if (ecd.entityClass == EntityClass.fallingBlocksClass)
			{
				Entity obj3 = (entity = gameObject.AddComponent<EntityFallingBlocks>());
				entity.RootTransform = prefabT;
				entity.ModelTransform = transform;
				entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
				prefabT.SetParent(ParentNameToTransform["FallingBlocks"], worldPositionStays: false);
				prefabT.name = "FallingBlock_" + ecd.id;
				((EntityFallingBlocks)obj3).SetBlockGroupData(ecd.blockPositions, ecd.blockValues);
				((EntityFallingBlocks)obj3).SetTextureFullArrays(ecd.textureFullArrays);
			}
			else if (ecd.entityClass == EntityClass.fallingTreeClass)
			{
				Entity obj4 = (entity = gameObject.AddComponent<EntityFallingTree>());
				entity.RootTransform = prefabT;
				entity.ModelTransform = transform;
				entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
				prefabT.SetParent(ParentNameToTransform["FallingTrees"], worldPositionStays: false);
				prefabT.name = "FallingTree_" + ecd.id;
				((EntityFallingTree)obj4).SetBlockPos(ecd.blockPos, ecd.fallTreeDir);
			}
			else
			{
				if (ec.classname == null)
				{
					Log.Error("Unknown entity " + ecd.entityClass);
					return;
				}
				entity = addEntityComponent(gameObject, ec.classname);
				if (!entity)
				{
					return;
				}
				transform.eulerAngles = ecd.rot;
				entity.entityId = ecd.id;
				entity.RootTransform = prefabT;
				entity.ModelTransform = transform;
				entity.Init(ecd.entityClass, entityInstanceAssets, eModelInstanceAssets);
				if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks) && entity is EntityAlive)
				{
					gameObject.AddComponent<GUIHUDEntityName>();
				}
				if (ec.parentGameObjectName != null)
				{
					prefabT.SetParent(ParentNameToTransform[ec.parentGameObjectName], worldPositionStays: false);
				}
				prefabT.name = ec.entityClassName + "_" + ecd.id;
				entity.SetEntityName(ec.entityClassName);
				entity.emodel.SetSkinTexture(ec.skinTexture);
				CapsuleCollider[] componentsInChildren = prefabT.GetComponentsInChildren<CapsuleCollider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					GameObject gameObject2 = componentsInChildren[i].gameObject;
					if (!gameObject2.CompareTag("LargeEntityBlocker") && !gameObject2.CompareTag("Physics"))
					{
						gameObject2.layer = 14;
					}
				}
				BoxCollider[] componentsInChildren2 = prefabT.GetComponentsInChildren<BoxCollider>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].gameObject.layer = 14;
				}
			}
			ecd.ApplyToEntity(entity);
			if (entity.GetSpawnerSource() == EnumSpawnerSource.Delete)
			{
				UnityEngine.Object.Destroy(prefabT.gameObject);
				return;
			}
			entity.lifetime = ecd.lifetime;
			entity.entityId = ecd.id;
			entity.belongsPlayerId = ecd.belongsPlayerId;
			entity.InitLocation(ecd.pos, ecd.rot);
			entity.onGround = ecd.onGround;
			if (ec.SizeScale != 1f)
			{
				entity.SetScale(ec.SizeScale);
			}
			if (ecd.overrideSize != 1f)
			{
				entity.SetScale(ecd.overrideSize);
			}
			if (ecd.overrideHeadSize != 1f && entity is EntityAlive entityAlive)
			{
				entityAlive.SetHeadSize(ecd.overrideHeadSize);
			}
			entity.PostInit();
			this.entity = entity;
		}
	}

	public static int nextEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNumberOfCachedFallingBlocks = 150;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNumberOfCachedItems = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFirstEntityID = 1;

	public const int StartEntityID = 171;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int playerNewMaleClass = EntityClass.FromString("playerNewMale");

	public static Dictionary<string, Transform> ParentNameToTransform = new Dictionary<string, Transform>();

	public static EntityClass.EntityTierTypes MaxEntityTier = EntityClass.EntityTierTypes.Elite;

	public static bool EnemySpawnMode = true;

	public static void Init(Transform _entitiesTransform)
	{
		ParentNameToTransform.Clear();
		FindOrCreateTransform(null, "Players", 0);
		FindOrCreateTransform(_entitiesTransform, "Items", 0);
		FindOrCreateTransform(_entitiesTransform, "FallingBlocks", 0);
		FindOrCreateTransform(_entitiesTransform, "FallingTrees", 0);
		FindOrCreateTransform(_entitiesTransform, "Enemies", 1);
		FindOrCreateTransform(_entitiesTransform, "Animals", 1);
		foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
		{
			FindOrCreateTransform(_entitiesTransform, item.Value.parentGameObjectName, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FindOrCreateTransform(Transform _parent, string _name, int _originLevel)
	{
		if (_name != null && !ParentNameToTransform.ContainsKey(_name))
		{
			Transform transform;
			if (_parent != null)
			{
				transform = _parent.Find(_name);
			}
			else
			{
				GameObject gameObject = GameObject.Find("/" + _name);
				transform = ((gameObject != null) ? gameObject.transform : null);
			}
			if (transform == null)
			{
				transform = new GameObject(_name).transform;
				transform.name = _name;
				transform.parent = _parent;
				Origin.Add(transform, _originLevel);
			}
			ParentNameToTransform[_name] = transform;
		}
	}

	public static void Cleanup()
	{
	}

	public static void CleanupStatic()
	{
		EntityClass.list.Clear();
	}

	public static Type GetEntityType(string _className)
	{
		switch (_className)
		{
		case "EntityAnimal":
			return typeof(EntityAnimal);
		case "EntityAnimalRabbit":
			return typeof(EntityAnimalRabbit);
		case "EntityAnimalStag":
			return typeof(EntityAnimalStag);
		case "EntityBandit":
			return typeof(EntityBandit);
		case "EntityDrone":
			return typeof(EntityDrone);
		case "EntityEnemyAnimal":
			return typeof(EntityEnemyAnimal);
		case "EntityHuman":
			return typeof(EntityHuman);
		case "EntityNPC":
			return typeof(EntityNPC);
		case "EntityPlayer":
			return typeof(EntityPlayer);
		case "EntityZombie":
			return typeof(EntityZombie);
		default:
			Log.Warning("GetEntityType slow lookup for {0}", _className);
			return Type.GetType(_className);
		}
	}

	public static EntityCreationData SetupEntityCreationData(int _et, int _id, ItemValue _itemValue, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, int _spawnById = -1, string _spawnByName = "")
	{
		return new EntityCreationData
		{
			entityClass = _et,
			id = _id,
			itemStack = new ItemStack(_itemValue, _count),
			pos = _transformPos,
			rot = _transformRot,
			lifetime = _lifetime,
			belongsPlayerId = _playerId,
			spawnById = _spawnById,
			spawnByName = _spawnByName
		};
	}

	public static EntityCreationData SetupEntityCreationData(int _et, int _id, BlockValue[] _blockValues, TextureFullArray[] _textureFullArrays, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, int _spawnById = -1, string _spawnByName = "")
	{
		return new EntityCreationData
		{
			entityClass = _et,
			id = _id,
			blockValues = _blockValues,
			textureFullArrays = _textureFullArrays,
			itemStack = 
			{
				count = _count
			},
			pos = _transformPos,
			rot = _transformRot,
			lifetime = _lifetime,
			belongsPlayerId = _playerId,
			spawnById = _spawnById,
			spawnByName = _spawnByName
		};
	}

	public static EntityCreationData SetupEntityCreationData(int _et, Vector3 _transformPos)
	{
		return SetupEntityCreationData(_et, nextEntityID++, _transformPos, Vector3.zero);
	}

	public static EntityCreationData SetupEntityCreationData(int _et, Vector3 _transformPos, Vector3 _rotation)
	{
		return SetupEntityCreationData(_et, nextEntityID++, _transformPos, _rotation);
	}

	public static EntityCreationData SetupEntityCreationData(int _et, int _id, Vector3 _transformPos, Vector3 _rotation)
	{
		return SetupEntityCreationData(_et, _id, ItemValue.None, 1, _transformPos, _rotation, float.MaxValue, -1);
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos)
	{
		return CreateEntity(SetupEntityCreationData(_et, _transformPos));
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation)
	{
		return CreateEntity(SetupEntityCreationData(_et, _transformPos, _rotation));
	}

	public static Entity CreateEntity(int _et, int _id, Vector3 _transformPos, Vector3 _rotation)
	{
		return CreateEntity(SetupEntityCreationData(_et, _id, _transformPos, _rotation));
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation, int _spawnById, string _spawnByName)
	{
		return CreateEntity(SetupEntityCreationData(_et, nextEntityID++, ItemValue.None, 1, _transformPos, _rotation, float.MaxValue, -1, _spawnById, _spawnByName));
	}

	public static Entity CreateEntity(int _et, int _id, BlockValue[] _blockValues, TextureFullArray[] _textureFullArrays, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, int _spawnById = -1, string _spawnByName = "")
	{
		return CreateEntity(SetupEntityCreationData(_et, _id, _blockValues, _textureFullArrays, _count, _transformPos, _transformRot, _lifetime, _playerId, _spawnById, _spawnByName));
	}

	public static Entity CreateEntity(EntityCreationData _ecd)
	{
		CreateEntityOperation createEntityOperation = CreateEntityOperation.Start(_ecd, _isSync: true);
		createEntityOperation.CompleteEntity();
		return createEntityOperation.entity;
	}

	public static CreateEntityOperation CreateEntityAsync(EntityCreationData _ecd)
	{
		return CreateEntityOperation.Start(_ecd, _isSync: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Entity addEntityComponent(GameObject _gameObject, string _className)
	{
		return addEntityComponent(_gameObject, Type.GetType(_className));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Entity addEntityComponent(GameObject _gameObject, Type _classType)
	{
		if (_classType != null)
		{
			return (Entity)_gameObject.AddComponent(_classType);
		}
		return null;
	}
}
