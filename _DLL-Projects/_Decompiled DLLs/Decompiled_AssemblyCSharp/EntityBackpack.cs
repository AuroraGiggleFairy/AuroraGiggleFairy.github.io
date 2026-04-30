using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityBackpack : EntityItem
{
	public int RefPlayerId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int deathUpdateTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksStayAfterDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoved;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int safetyCounter;

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass obj = EntityClass.list[entityClass];
		float optionalValue = 5f;
		obj.Properties.ParseFloat(EntityClass.PropTimeStayAfterDeath, ref optionalValue);
		ticksStayAfterDeath = (int)(optionalValue * 20f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		LogBackpack("Start");
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		foreach (Collider obj in componentsInChildren)
		{
			obj.gameObject.tag = "E_BP_Body";
			obj.gameObject.layer = 13;
			obj.enabled = true;
			obj.gameObject.AddMissingComponent<RootTransformRefEntity>().RootTransform = base.transform;
		}
		SetDead();
		if (lootContainer != null)
		{
			lootContainer.entityId = entityId;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		Vector3 _pos = position;
		if (world.AdjustBoundsForPlayers(ref _pos, 0.06f))
		{
			itemRB.velocity *= 0.5f;
			_pos.y = itemRB.position.y + Origin.position.y;
			itemRB.position = _pos - Origin.position;
			SetPosition(_pos);
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (deathUpdateTicks > 0)
		{
			bool flag = GameManager.Instance.GetEntityIDForLockedTileEntity(lootContainer) != -1;
			if (!bRemoved && lootContainer != null && !lootContainer.IsUserAccessing() && !flag)
			{
				if (lootContainer.bTouched && lootContainer.IsEmpty())
				{
					RemoveBackpack("empty");
				}
				if (deathUpdateTicks >= ticksStayAfterDeath - 1)
				{
					RemoveBackpack("old");
				}
			}
		}
		deathUpdateTicks++;
		if (!bRemoved && !isEntityRemote && base.transform.position.y + Origin.position.y < 1f)
		{
			Vector3 vector = new Vector3(position.x, (float)(world.GetHeight(Utils.Fastfloor(position.x), Utils.Fastfloor(position.z)) + 5) + rand.RandomFloat * 20f, position.z);
			Log.Warning("EntityBackpack below world {0}, moving to {1}", position.ToCultureInvariantString(), vector.ToCultureInvariantString());
			SetPosition(vector);
			base.transform.position = vector - Origin.position;
			if (++safetyCounter > 500)
			{
				RemoveBackpack("retries");
			}
		}
		if (bRemoved || isEntityRemote || RefPlayerId == -1)
		{
			return;
		}
		using IEnumerator<PersistentPlayerData> enumerator = GameManager.Instance.persistentPlayers.Players.Values.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.TryUpdateBackpackPosition(entityId, new Vector3i(position)))
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveBackpack(string reason)
	{
		LogBackpack("RemoveBackpack empty {0}, reason {1}", lootContainer == null || lootContainer.IsEmpty(), reason);
		deathUpdateTicks = ticksStayAfterDeath;
		_ = Vector3i.zero;
		if (!isEntityRemote && RefPlayerId != -1)
		{
			foreach (PersistentPlayerData value in GameManager.Instance.persistentPlayers.Players.Values)
			{
				if (value.TryRemoveDroppedBackpack(entityId))
				{
					_ = value.MostRecentBackpackPosition;
					break;
				}
			}
		}
		EntityPlayer entityPlayer = world.GetEntity(RefPlayerId) as EntityPlayer;
		if (entityPlayer != null)
		{
			if (!entityPlayer.isEntityRemote && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				entityPlayer.SetDroppedBackpackPositions(GameManager.Instance.persistentLocalPlayer.GetDroppedBackpackPositions());
			}
			else if (!world.IsRemote())
			{
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers.EntityToPlayerMap[entityPlayer.entityId];
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(entityPlayer.entityId, persistentPlayerData.GetDroppedBackpackPositions()), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
			}
		}
		bRemoved = true;
	}

	public override void OnEntityUnload()
	{
		LogBackpack("OnEntityUnload markedForUnload {0}, IsDead {1}, IsDespawned {2}", markedForUnload, IsDead(), IsDespawned);
		base.OnEntityUnload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createMesh()
	{
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		LogBackpack("Write");
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(RefPlayerId);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		RefPlayerId = _br.ReadInt32();
		LogBackpack("Read");
	}

	public override bool IsMarkedForUnload()
	{
		if (base.IsMarkedForUnload())
		{
			return bRemoved;
		}
		return false;
	}

	public override string GetLootList()
	{
		return lootListOnDeath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (RefPlayerId == -1)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = world.GetEntity(RefPlayerId) as EntityPlayerLocal;
		if (entityPlayerLocal != null && RefPlayerId == entityPlayerLocal.entityId)
		{
			if (GamePrefs.GetInt(EnumGamePrefs.DeathPenalty) == 3 && entityPlayerLocal.GetDroppedBackpackPositions().Count == 0)
			{
				RefPlayerId = -1;
			}
			else if (EntityClass.list[entityClass].NavObject != "")
			{
				NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, base.transform);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogBackpack(string format, params object[] args)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !world.IsLocalPlayer(RefPlayerId))
		{
			return;
		}
		string text = "?";
		if (lootContainer != null)
		{
			text = string.Empty;
			int num = 0;
			ItemStack[] items = lootContainer.GetItems();
			foreach (ItemStack itemStack in items)
			{
				if (!itemStack.IsEmpty())
				{
					num++;
					if (num == 1)
					{
						text = itemStack.itemValue.ItemClass.Name;
					}
					if (num == 2)
					{
						text = text + ", " + itemStack.itemValue.ItemClass.Name;
					}
				}
			}
			Vector3i vector3i = lootContainer.ToWorldPos();
			text = $"{num} {text} at ({vector3i}, xz {World.toBlockXZ(vector3i.x)} {World.toBlockXZ(vector3i.z)})";
		}
		int num2 = 0;
		if (ThreadManager.IsMainThread())
		{
			num2 = Time.frameCount;
		}
		format = $"{num2} EntityBackpack id {entityId}, plyrId {RefPlayerId}, {position.ToCultureInvariantString()} ({World.toChunkXZ(position)}), chunk {addedToChunk} ({chunkPosAddedEntityTo}), items {text} : {format}";
		Log.Out(format, args);
	}
}
