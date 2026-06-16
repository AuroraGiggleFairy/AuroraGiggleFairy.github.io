using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionSpawnVehicle : ItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataSpawnVehicle : ItemActionAttackData
	{
		public Transform VehiclePreviewT;

		public Renderer[] PreviewRenderers;

		public bool ValidPosition;

		public Vector3 Position;

		public ItemActionDataSpawnVehicle(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cColliderMask = 28901376;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 vehicleSize;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataSpawnVehicle(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Vehicle"))
		{
			entityToSpawn = _props.Values["Vehicle"];
		}
		vehicleSize = new Vector3(1f, 1.9f, 2f);
		if (_props.Values.ContainsKey("VehicleSize"))
		{
			vehicleSize = StringParsers.ParseVector3(_props.Values["VehicleSize"]);
		}
		foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
		{
			if (item.Value.entityClassName == entityToSpawn)
			{
				entityId = item.Key;
				break;
			}
		}
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		ItemActionDataSpawnVehicle itemActionDataSpawnVehicle = (ItemActionDataSpawnVehicle)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataSpawnVehicle.invData.holdingEntity as EntityPlayerLocal;
		if ((bool)entityPlayerLocal)
		{
			if ((bool)itemActionDataSpawnVehicle.VehiclePreviewT)
			{
				Object.DestroyImmediate(itemActionDataSpawnVehicle.VehiclePreviewT.gameObject);
			}
			GameObject original = DataLoader.LoadAsset<GameObject>(entityPlayerLocal.inventory.holdingItem.MeshFile);
			itemActionDataSpawnVehicle.VehiclePreviewT = Object.Instantiate(original).transform;
			Vehicle.SetupPreview(itemActionDataSpawnVehicle.VehiclePreviewT);
			SetupPreview(itemActionDataSpawnVehicle);
			GameManager.Instance.StartCoroutine(UpdatePreview(itemActionDataSpawnVehicle));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupPreview(ItemActionDataSpawnVehicle data)
	{
		data.ValidPosition = false;
		if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
		{
			data.PreviewRenderers = data.VehiclePreviewT.GetComponentsInChildren<Renderer>();
		}
		for (int i = 0; i < data.PreviewRenderers.Length; i++)
		{
			data.PreviewRenderers[i].material.color = new Color(2f, 0.25f, 0.25f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdatePreview(ItemActionDataSpawnVehicle data)
	{
		World world = data.invData.world;
		while ((bool)data.VehiclePreviewT)
		{
			bool flag = CalcSpawnPosition(data, ref data.Position) && world.CanPlaceBlockAt(new Vector3i(data.Position), world.GetGameManager().GetPersistentLocalPlayer());
			if (data.ValidPosition != flag)
			{
				data.ValidPosition = flag;
				if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
				{
					data.PreviewRenderers = data.VehiclePreviewT.GetComponentsInChildren<Renderer>();
				}
				Color color = (flag ? new Color(0.25f, 2f, 0.25f) : new Color(2f, 0.25f, 0.25f));
				for (int i = 0; i < data.PreviewRenderers.Length; i++)
				{
					data.PreviewRenderers[i].material.color = color;
				}
			}
			if (data.Position.y < 9999f)
			{
				Quaternion localRotation = Quaternion.Euler(0f, data.invData.holdingEntity.rotation.y + 90f, 0f);
				data.VehiclePreviewT.localRotation = localRotation;
				data.VehiclePreviewT.position = data.Position - Origin.position;
				data.VehiclePreviewT.gameObject.SetActive(value: true);
			}
			else
			{
				data.VehiclePreviewT.gameObject.SetActive(value: false);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionDataSpawnVehicle itemActionDataSpawnVehicle = (ItemActionDataSpawnVehicle)_actionData;
		if ((bool)itemActionDataSpawnVehicle.VehiclePreviewT && itemActionDataSpawnVehicle.invData.holdingEntity is EntityPlayerLocal)
		{
			Object.Destroy(itemActionDataSpawnVehicle.VehiclePreviewT.gameObject);
		}
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		base.StopHolding(_actionData);
		ItemActionDataSpawnVehicle itemActionDataSpawnVehicle = (ItemActionDataSpawnVehicle)_actionData;
		if ((bool)itemActionDataSpawnVehicle.VehiclePreviewT && itemActionDataSpawnVehicle.invData.holdingEntity is EntityPlayerLocal)
		{
			Object.Destroy(itemActionDataSpawnVehicle.VehiclePreviewT.gameObject);
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		float time = Time.time;
		if (time - _actionData.lastUseTime < Delay || time - _actionData.lastUseTime < 2f)
		{
			return;
		}
		ItemActionDataSpawnVehicle itemActionDataSpawnVehicle = (ItemActionDataSpawnVehicle)_actionData;
		if (!itemActionDataSpawnVehicle.ValidPosition)
		{
			return;
		}
		if (entityId < 0)
		{
			foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
			{
				if (item.Value.entityClassName == entityToSpawn)
				{
					entityId = item.Key;
					break;
				}
			}
			if (entityId == 0)
			{
				return;
			}
		}
		_actionData.lastUseTime = time;
		ItemValue holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
		if (VehicleManager.CanAddMoreVehicles())
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageVehicleSpawn>().Setup(entityId, itemActionDataSpawnVehicle.Position, new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f), holdingItemItemValue.Clone(), entityPlayerLocal.entityId), _flush: true);
			}
			else
			{
				Entity entity = EntityFactory.CreateEntity(entityId, itemActionDataSpawnVehicle.Position + Vector3.up * 0.25f, new Vector3(0f, entityPlayerLocal.rotation.y + 90f, 0f));
				entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
				if (entity is EntityVehicle entityVehicle)
				{
					entityVehicle.GetVehicle().SetItemValue(holdingItemItemValue);
					entityVehicle.SetOwner(PlatformManager.InternalLocalUserIdentifier);
				}
				else if (entity is EntityAlive entityAlive)
				{
					entityAlive.factionId = entityPlayerLocal.factionId;
					entityAlive.belongsPlayerId = entityPlayerLocal.entityId;
					entityAlive.factionRank = (byte)(entityPlayerLocal.factionRank - 1);
				}
				GameManager.Instance.World.SpawnEntityInWorld(entity);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup());
			}
			entityPlayerLocal.RightArmAnimationUse = true;
			entityPlayerLocal.DropTimeDelay = 0.5f;
			entityPlayerLocal.inventory.DecHoldingItem(1);
			entityPlayerLocal.PlayOneShot((soundStart != null) ? soundStart : "placeblock");
			ClearPreview(_actionData);
		}
		else
		{
			GameManager.ShowTooltip(entityPlayerLocal, "uiCannotAddVehicle");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalcSpawnPosition(ItemActionDataSpawnVehicle _actionData, ref Vector3 position)
	{
		World world = _actionData.invData.world;
		Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
		if (Vector3.Dot(lookRay.direction, Vector3.up) == 1f)
		{
			return false;
		}
		position.y = float.MaxValue;
		float num = 4f + vehicleSize.x;
		if (Voxel.Raycast(world, lookRay, num + 1.5f, 8454144, 69, 0f))
		{
			if ((Voxel.voxelRayHitInfo.hit.pos - lookRay.origin).magnitude > num)
			{
				position = Voxel.voxelRayHitInfo.hit.pos;
				return false;
			}
			for (float num2 = 0.14f; num2 < 1.15f; num2 += 0.25f)
			{
				position = Voxel.voxelRayHitInfo.hit.pos;
				position.y += num2;
				Vector3 normalized = Vector3.Cross(lookRay.direction, Vector3.up).normalized;
				Vector3 vector = Vector3.Cross(normalized, Vector3.up);
				Vector3 localPos = position - Origin.position;
				localPos.y += vehicleSize.y * 0.5f + 0.05f;
				if (CheckForSpace(localPos, normalized, vehicleSize.z, vector, vehicleSize.x, Vector3.up, vehicleSize.y) && CheckForSpace(localPos, vector, vehicleSize.x, normalized, vehicleSize.z, Vector3.up, vehicleSize.y) && CheckForSpace(localPos, Vector3.up, vehicleSize.y, normalized, vehicleSize.z, vector, vehicleSize.x))
				{
					return true;
				}
			}
		}
		return false;
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
		ItemActionDataSpawnVehicle actionData = _data as ItemActionDataSpawnVehicle;
		ClearPreview(actionData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearPreview(ItemActionDataSpawnVehicle _actionData)
	{
		if ((bool)_actionData.VehiclePreviewT)
		{
			Object.Destroy(_actionData.VehiclePreviewT.gameObject);
		}
	}

	public override void Cleanup(ItemActionData _data)
	{
		base.Cleanup(_data);
		if (_data is ItemActionDataSpawnVehicle { invData: not null } itemActionDataSpawnVehicle && itemActionDataSpawnVehicle.invData.holdingEntity is EntityPlayerLocal)
		{
			ClearPreview(itemActionDataSpawnVehicle);
		}
	}
}
