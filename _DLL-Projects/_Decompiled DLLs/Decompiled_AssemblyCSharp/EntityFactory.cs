using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFactory
{
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

	public static Entity CreateEntity(int _et, Vector3 _transformPos)
	{
		return CreateEntity(_et, nextEntityID++, _transformPos, Vector3.zero);
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation)
	{
		return CreateEntity(_et, nextEntityID++, _transformPos, _rotation);
	}

	public static Entity CreateEntity(int _et, int _id, Vector3 _transformPos, Vector3 _rotation)
	{
		return CreateEntity(_et, _id, ItemValue.None.Clone(), 1, _transformPos, _rotation, float.MaxValue, -1, null);
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation, int _spawnById, string _spawnByName)
	{
		return CreateEntity(_et, nextEntityID++, ItemValue.None.Clone(), 1, _transformPos, _rotation, float.MaxValue, -1, null, _spawnById, _spawnByName);
	}

	public static Entity CreateEntity(int _et, int _id, BlockValue _blockValue, TextureFullArray _textureFull, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, string _skinName, int _spawnById = -1, string _spawnByName = "")
	{
		return CreateEntity(new EntityCreationData
		{
			entityClass = _et,
			id = _id,
			blockValue = _blockValue,
			textureFull = _textureFull,
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
		});
	}

	public static Entity CreateEntity(int _et, int _id, ItemValue _itemValue, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, string _skinName, int _spawnById = -1, string _spawnByName = "")
	{
		return CreateEntity(new EntityCreationData
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
		});
	}

	public static Entity CreateEntity(EntityCreationData _ecd)
	{
		if (_ecd.id == -1)
		{
			_ecd.id = nextEntityID++;
		}
		else
		{
			nextEntityID = Math.Max(_ecd.id + 1, nextEntityID);
		}
		EntityClass entityClass = EntityClass.GetEntityClass(_ecd.entityClass);
		if (entityClass == null)
		{
			Log.Error("EntityFactory CreateEntity: unknown type ({0}) {1}", _ecd.entityClass, _ecd.entityName);
			return null;
		}
		int num;
		int num2;
		if (_ecd.entityClass != EntityClass.playerMaleClass)
		{
			num = ((_ecd.entityClass == EntityClass.playerFemaleClass) ? 1 : 0);
			if (num == 0)
			{
				num2 = 0;
				goto IL_009e;
			}
		}
		else
		{
			num = 1;
		}
		num2 = ((_ecd.id == _ecd.belongsPlayerId) ? 1 : 0);
		goto IL_009e;
		IL_009e:
		bool flag = (byte)num2 != 0;
		Transform original;
		if (flag)
		{
			original = Resources.Load<Transform>("Prefabs/prefabEntityPlayerLocal");
		}
		else
		{
			LoadPrefabs(entityClass);
			original = entityClass.prefabT;
		}
		original = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity);
		Transform transform = original;
		Transform transform2 = original.Find("GameObject");
		if ((bool)transform2)
		{
			transform = transform2;
		}
		transform.position = _ecd.pos - Origin.position;
		GameObject gameObject = transform.gameObject;
		Entity entity;
		if (num != 0)
		{
			EntityPlayer entityPlayer;
			if (flag)
			{
				entity = addEntityComponent(gameObject, entityClass.classname.FullName + "Local");
				entityPlayer = (EntityPlayer)entity;
				entity.RootTransform = original;
				entity.ModelTransform = original.Find("Graphics");
				entity.PhysicsTransform = original;
				entityPlayer.playerProfile = _ecd.playerProfile;
				entity.Init(_ecd.entityClass);
				gameObject.AddComponent<LocalPlayer>();
			}
			else
			{
				entity = addEntityComponent(gameObject, entityClass.classname);
				entityPlayer = (EntityPlayer)entity;
				entity.RootTransform = original;
				entity.ModelTransform = transform;
				entity.PhysicsTransform = original.Find("Physics");
				entityPlayer.playerProfile = _ecd.playerProfile;
				entity.Init(_ecd.entityClass);
				gameObject.AddComponent<GUIHUDEntityName>();
			}
			if (!_ecd.holdingItem.IsEmpty())
			{
				entityPlayer.inventory.AddItem(new ItemStack(_ecd.holdingItem, 1));
				entityPlayer.inventory.SetHoldingItemIdx(0);
			}
			entityPlayer.TeamNumber = _ecd.teamNumber;
			entityPlayer.emodel.SetSkinTexture(_ecd.skinTexture);
			original.SetParent(ParentNameToTransform[entityClass.parentGameObjectName], worldPositionStays: false);
			original.name = "Player_" + _ecd.id;
			Log.Out("Created player with id=" + _ecd.id);
		}
		else if (_ecd.entityClass == EntityClass.itemClass)
		{
			Entity entity2 = (entity = gameObject.AddComponent<EntityItem>());
			entity.RootTransform = original;
			entity.ModelTransform = transform;
			entity.clientEntityId = _ecd.clientEntityId;
			((EntityItem)entity2).OwnerId = _ecd.belongsPlayerId;
			entity.Init(_ecd.entityClass);
			original.SetParent(ParentNameToTransform["Items"], worldPositionStays: false);
			original.name = "Item_" + _ecd.id;
			((EntityItem)entity2).SetItemStack(_ecd.itemStack);
		}
		else if (_ecd.entityClass == EntityClass.fallingBlockClass)
		{
			Entity entity3 = (entity = gameObject.AddComponent<EntityFallingBlock>());
			entity.RootTransform = original;
			entity.ModelTransform = transform;
			entity.Init(_ecd.entityClass);
			original.SetParent(ParentNameToTransform["FallingBlocks"], worldPositionStays: false);
			original.name = "FallingBlock_" + _ecd.id;
			((EntityFallingBlock)entity3).SetBlockValue(_ecd.blockValue);
			((EntityFallingBlock)entity3).SetTextureFull(_ecd.textureFull);
		}
		else if (_ecd.entityClass == EntityClass.fallingTreeClass)
		{
			Entity entity4 = (entity = gameObject.AddComponent<EntityFallingTree>());
			entity.RootTransform = original;
			entity.ModelTransform = transform;
			entity.Init(_ecd.entityClass);
			original.SetParent(ParentNameToTransform["FallingTrees"], worldPositionStays: false);
			original.name = "FallingTree_" + _ecd.id;
			((EntityFallingTree)entity4).SetBlockPos(_ecd.blockPos, _ecd.fallTreeDir);
		}
		else
		{
			if (entityClass.classname == null)
			{
				Log.Error("Unknown entity " + _ecd.entityClass);
				return null;
			}
			entity = addEntityComponent(gameObject, entityClass.classname);
			if (!entity)
			{
				return null;
			}
			transform.eulerAngles = _ecd.rot;
			entity.entityId = _ecd.id;
			entity.RootTransform = original;
			entity.ModelTransform = transform;
			entity.Init(_ecd.entityClass);
			if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks) && entity is EntityAlive)
			{
				gameObject.AddComponent<GUIHUDEntityName>();
			}
			if (entityClass.parentGameObjectName != null)
			{
				original.SetParent(ParentNameToTransform[entityClass.parentGameObjectName], worldPositionStays: false);
			}
			original.name = entityClass.entityClassName + "_" + _ecd.id;
			entity.SetEntityName(entityClass.entityClassName);
			entity.emodel.SetSkinTexture(entityClass.skinTexture);
			CapsuleCollider[] componentsInChildren = original.GetComponentsInChildren<CapsuleCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObject gameObject2 = componentsInChildren[i].gameObject;
				if (!gameObject2.CompareTag("LargeEntityBlocker") && !gameObject2.CompareTag("Physics"))
				{
					gameObject2.layer = 14;
				}
			}
			BoxCollider[] componentsInChildren2 = original.GetComponentsInChildren<BoxCollider>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].gameObject.layer = 14;
			}
		}
		_ecd.ApplyToEntity(entity);
		if (entity.GetSpawnerSource() == EnumSpawnerSource.Delete)
		{
			UnityEngine.Object.Destroy(original.gameObject);
			return null;
		}
		entity.lifetime = _ecd.lifetime;
		entity.entityId = _ecd.id;
		entity.belongsPlayerId = _ecd.belongsPlayerId;
		entity.InitLocation(_ecd.pos, _ecd.rot);
		entity.onGround = _ecd.onGround;
		if (entityClass.SizeScale != 1f)
		{
			entity.SetScale(entityClass.SizeScale);
		}
		if (_ecd.overrideSize != 1f)
		{
			entity.SetScale(_ecd.overrideSize);
		}
		if (_ecd.overrideHeadSize != 1f && entity is EntityAlive entityAlive)
		{
			entityAlive.SetHeadSize(_ecd.overrideHeadSize);
		}
		entity.PostInit();
		return entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadPrefabs(EntityClass ec)
	{
		if (!ec.prefabT)
		{
			ec.prefabT = DataLoader.LoadAsset<Transform>(ec.prefabPath);
			if (!ec.prefabT)
			{
				Log.Error("Could not load file '" + ec.prefabPath + "' for entity_class '" + ec.entityClassName + "'");
				return;
			}
			MeshLodOptimization.Apply(ref ec.prefabT);
		}
		if (!ec.mesh && !string.IsNullOrEmpty(ec.meshPath))
		{
			ec.mesh = DataLoader.LoadAsset<Transform>(ec.meshPath);
			if (!ec.mesh)
			{
				Log.Error("Could not load file '" + ec.meshPath + "' for entity_class '" + ec.entityClassName + "'");
			}
			else
			{
				MeshLodOptimization.Apply(ref ec.mesh);
			}
		}
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
