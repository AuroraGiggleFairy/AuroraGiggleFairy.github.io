using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionSpawnTurret : ItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataSpawnTurret : ItemActionAttackData
	{
		public Transform TurretPreviewT;

		public Renderer[] PreviewRenderers;

		public bool ValidPosition;

		public Vector3 Position;

		public bool Placing;

		public ItemActionDataSpawnTurret(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cColliderMask = 28901376;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityClassId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 turretSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 previewSize;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataSpawnTurret(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Turret"))
		{
			entityToSpawn = _props.Values["Turret"];
		}
		turretSize = new Vector3(0.5f, 0.5f, 0.5f);
		if (_props.Values.ContainsKey("Scale"))
		{
			turretSize = StringParsers.ParseVector3(_props.Values["Scale"]);
		}
		previewSize = new Vector3(1f, 1f, 1f);
		if (_props.Values.ContainsKey("PreviewSize"))
		{
			previewSize = StringParsers.ParseVector3(_props.Values["PreviewSize"]);
		}
		foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
		{
			if (item.Value.entityClassName == entityToSpawn)
			{
				entityClassId = item.Key;
				break;
			}
		}
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionDataSpawnTurret)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataSpawnTurret.invData.holdingEntity as EntityPlayerLocal;
		if ((bool)entityPlayerLocal)
		{
			if (itemActionDataSpawnTurret.TurretPreviewT != null)
			{
				Object.DestroyImmediate(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
			}
			GameObject original = DataLoader.LoadAsset<GameObject>(entityPlayerLocal.inventory.holdingItem.MeshFile);
			itemActionDataSpawnTurret.TurretPreviewT = Object.Instantiate(original).transform;
			setupPreview(itemActionDataSpawnTurret);
			updatePreview(itemActionDataSpawnTurret);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupPreview(ItemActionDataSpawnTurret data)
	{
		if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
		{
			data.PreviewRenderers = data.TurretPreviewT.GetComponentsInChildren<Renderer>();
		}
		data.TurretPreviewT.localScale = previewSize;
		data.TurretPreviewT.GetComponent<SphereCollider>().enabled = false;
		for (int i = 0; i < data.PreviewRenderers.Length; i++)
		{
			data.PreviewRenderers[i].material.color = new Color(2f, 0.25f, 0.25f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(ItemActionDataSpawnTurret data)
	{
		if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
		{
			data.PreviewRenderers = data.TurretPreviewT.GetComponentsInChildren<Renderer>();
		}
		World world = data.invData.world;
		bool flag = (data.ValidPosition = CalcSpawnPosition(data, ref data.Position) && world.CanPlaceBlockAt(new Vector3i(data.Position), world.GetGameManager().GetPersistentLocalPlayer()));
		for (int i = 0; i < data.PreviewRenderers.Length; i++)
		{
			data.PreviewRenderers[i].material.color = (flag ? new Color(0.25f, 2f, 0.25f) : new Color(2f, 0.25f, 0.25f));
		}
		Quaternion localRotation = data.TurretPreviewT.localRotation;
		localRotation.eulerAngles = new Vector3(0f, data.invData.holdingEntity.rotation.y, 0f);
		data.TurretPreviewT.localRotation = localRotation;
		data.TurretPreviewT.position = data.Position - Origin.position;
		data.TurretPreviewT.gameObject.SetActive(data.Placing);
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			Object.Destroy(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
		}
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		base.StopHolding(_actionData);
		ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			Object.Destroy(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.invData.item.Actions[0] != null && itemActionDataSpawnTurret.invData.item.Actions[0].IsActionRunning(itemActionDataSpawnTurret.invData.actionData[0]))
		{
			itemActionDataSpawnTurret.Placing = false;
		}
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			updatePreview(itemActionDataSpawnTurret);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionDataSpawnTurret)_actionData;
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		if (!itemActionDataSpawnTurret.Placing)
		{
			if (_bReleased)
			{
				itemActionDataSpawnTurret.Placing = true;
			}
		}
		else
		{
			if (!itemActionDataSpawnTurret.Placing || !_bReleased)
			{
				return;
			}
			float time = Time.time;
			if (time - _actionData.lastUseTime < Delay || time - _actionData.lastUseTime < 2f || !itemActionDataSpawnTurret.ValidPosition)
			{
				return;
			}
			ItemInventoryData invData = _actionData.invData;
			if (entityClassId < 0)
			{
				foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
				{
					if (item.Value.entityClassName == entityToSpawn)
					{
						entityClassId = item.Key;
						break;
					}
				}
				if (entityClassId == 0)
				{
					return;
				}
			}
			if (EntityClass.list[entityClassId].entityClassName == "entityJunkDrone")
			{
				if (!EntityDrone.IsValidForLocalPlayer())
				{
					return;
				}
				GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
			}
			_actionData.lastUseTime = time;
			bool flag = false;
			bool flag2 = false;
			if (itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("drone")))
			{
				flag2 = true;
				if (DroneManager.CanAddMoreDrones())
				{
					flag = true;
				}
			}
			else if ((itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretRanged")) || itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretMelee"))) && TurretTracker.CanAddMoreTurrets())
			{
				flag = true;
			}
			if (flag)
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTurretSpawn>().Setup(entityClassId, itemActionDataSpawnTurret.Position, new Vector3(0f, invData.holdingEntity.rotation.y, 0f), invData.holdingEntity.inventory.holdingItemItemValue.Clone(), invData.holdingEntity.entityId), _flush: true);
				}
				else
				{
					Entity entity = EntityFactory.CreateEntity(entityClassId, itemActionDataSpawnTurret.Position, new Vector3(0f, _actionData.invData.holdingEntity.rotation.y, 0f));
					entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
					if (entity is EntityTurret entityTurret)
					{
						entityTurret.factionId = itemActionDataSpawnTurret.invData.holdingEntity.factionId;
						entityTurret.belongsPlayerId = itemActionDataSpawnTurret.invData.holdingEntity.entityId;
						entityTurret.factionRank = (byte)(itemActionDataSpawnTurret.invData.holdingEntity.factionRank - 1);
						entityTurret.OriginalItemValue = itemActionDataSpawnTurret.invData.itemValue.Clone();
						entityTurret.groundPosition = itemActionDataSpawnTurret.Position;
						entityTurret.OwnerID = PlatformManager.InternalLocalUserIdentifier;
						entityTurret.ForceOn = true;
						entityTurret.rotation = new Vector3(0f, _actionData.invData.holdingEntity.rotation.y, 0f);
						itemActionDataSpawnTurret.invData.holdingEntity.AddOwnedEntity(entityTurret);
					}
					else if (entity is EntityDrone entityDrone)
					{
						entityDrone.factionId = itemActionDataSpawnTurret.invData.holdingEntity.factionId;
						entityDrone.belongsPlayerId = itemActionDataSpawnTurret.invData.holdingEntity.entityId;
						entityDrone.factionRank = (byte)(itemActionDataSpawnTurret.invData.holdingEntity.factionRank - 1);
						entityDrone.OriginalItemValue = itemActionDataSpawnTurret.invData.itemValue.Clone();
						entityDrone.SetItemValueToLoad(entityDrone.OriginalItemValue);
						entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
						entityDrone.PlayWakeupAnim = true;
						itemActionDataSpawnTurret.invData.holdingEntity.AddOwnedEntity(entityDrone);
					}
					GameManager.Instance.World.SpawnEntityInWorld(entity);
				}
				itemActionDataSpawnTurret.Placing = false;
				invData.holdingEntity.RightArmAnimationUse = true;
				(invData.holdingEntity as EntityPlayerLocal).DropTimeDelay = 0.5f;
				if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
				{
					ClearPreview(itemActionDataSpawnTurret);
				}
				invData.holdingEntity.inventory.DecHoldingItem(1);
				invData.holdingEntity.PlayOneShot((soundStart != null) ? soundStart : "placeblock");
			}
			else if (flag2)
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uiCannotAddDrone");
			}
			else
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uiCannotAddTurret");
			}
		}
	}

	public override bool AllowConcurrentActions()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalcSpawnPosition(ItemActionDataSpawnTurret _actionData, ref Vector3 position)
	{
		World world = _actionData.invData.world;
		Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
		if (Voxel.Raycast(world, lookRay, 4f + turretSize.x, 8454144, 69, 0f))
		{
			position = Voxel.voxelRayHitInfo.hit.pos;
		}
		else
		{
			position = lookRay.origin + lookRay.direction * (4f + turretSize.x);
		}
		Collider[] array = Physics.OverlapSphere(position - Origin.position + Vector3.up * 0.525f, 0.5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.layer != 18 && !(array[i].gameObject == _actionData.TurretPreviewT.gameObject))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForSpace(Vector3 localPos, Vector3 dirN, float length, Vector3 axis1N, float axis1Length, Vector3 axis2N, float axis2Length)
	{
		Vector3 vector = dirN * length * 0.5f;
		for (float num = (0f - axis1Length) * 0.5f; num <= axis1Length * 0.5f; num += 0.2499f)
		{
			Vector3 vector2 = localPos + axis1N * num;
			for (float num2 = (0f - axis2Length) * 0.5f; num2 <= axis2Length * 0.5f; num2 += 0.2499f)
			{
				Vector3 vector3 = vector2 + axis2N * num2;
				if (Physics.Raycast(vector3 - vector, dirN, length, 28901376))
				{
					return false;
				}
				if (Physics.Raycast(vector3 + vector, -dirN, length, 28901376))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ClearPreview(ItemActionData _data)
	{
		ItemActionDataSpawnTurret actionData = _data as ItemActionDataSpawnTurret;
		ClearPreview(actionData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearPreview(ItemActionDataSpawnTurret _actionData)
	{
		if ((bool)_actionData.TurretPreviewT)
		{
			Object.Destroy(_actionData.TurretPreviewT.gameObject);
		}
	}

	public override void Cleanup(ItemActionData _data)
	{
		base.Cleanup(_data);
		if (_data is ItemActionDataSpawnTurret itemActionDataSpawnTurret && itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			ClearPreview(itemActionDataSpawnTurret);
		}
	}
}
