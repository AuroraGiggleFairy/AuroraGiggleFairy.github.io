using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseContainersAction : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ContainerActionStates
	{
		FindContainers,
		Action
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TargetingTypes
	{
		SafeZone,
		Distance
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string newName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool changeName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<TileEntity> tileEntityList = new List<TileEntity>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ContainerActionStates ActionState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TargetingTypes TargetingType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNewName = "new_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetingType = "targeting_type";

	public string ModifiedName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetTextWithElements(newName);
		}
	}

	public override bool CanPerform(Entity target)
	{
		if (base.CanPerform(target))
		{
			return GetTileEntityList(target);
		}
		return false;
	}

	public virtual bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
	{
		isEmpty = true;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetTileEntityList(Entity target)
	{
		World world = GameManager.Instance.World;
		Vector3i blockPosition = target.GetBlockPosition();
		int num = World.toChunkXZ(blockPosition.x);
		int num2 = World.toChunkXZ(blockPosition.z);
		int num3 = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num4 = num3 / 16 + 1;
		int num5 = num3 / 16 + 1;
		tileEntityList.Clear();
		bool result = false;
		for (int i = -num5; i <= num5; i++)
		{
			for (int j = -num4; j <= num4; j++)
			{
				Chunk chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
				if (chunk == null)
				{
					continue;
				}
				DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
				for (int k = 0; k < tileEntities.list.Count; k++)
				{
					TileEntity tileEntity = tileEntities.list[k];
					if (tileEntity == null || tileEntity.EntityId != -1)
					{
						continue;
					}
					bool flag = false;
					switch (TargetingType)
					{
					case TargetingTypes.SafeZone:
					{
						if (tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe2) && !_typedTe2.bPlayerStorage)
						{
							continue;
						}
						flag = world.IsMyLandProtectedBlock(tileEntity.ToWorldPos(), world.gameManager.GetPersistentPlayerList().GetPlayerDataFromEntityID(target.entityId));
						break;
					}
					case TargetingTypes.Distance:
						if (target.GetDistanceSq(tileEntity.ToWorldPos().ToVector3()) < maxDistance)
						{
							if (tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe) && !_typedTe.bPlayerStorage)
							{
								continue;
							}
							flag = true;
						}
						break;
					}
					if (!flag)
					{
						continue;
					}
					bool isEmpty = false;
					if (!CheckValidTileEntity(tileEntity, out isEmpty))
					{
						continue;
					}
					tileEntityList.Add(tileEntity);
					if (!isEmpty)
					{
						result = true;
					}
					int entityIDForLockedTileEntity = GameManager.Instance.GetEntityIDForLockedTileEntity(tileEntity);
					if (entityIDForLockedTileEntity != -1)
					{
						EntityPlayer entityPlayer = world.GetEntity(entityIDForLockedTileEntity) as EntityPlayer;
						if (entityPlayer.isEntityRemote)
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityIDForLockedTileEntity), _onlyClientsAttachedToAnEntity: false, entityIDForLockedTileEntity);
						}
						else
						{
							(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenWindows();
						}
					}
				}
			}
		}
		return result;
	}

	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		for (int i = 0; i < tileEntityList.Count; i++)
		{
			TileEntity te = tileEntityList[i];
			int entityIDForLockedTileEntity = GameManager.Instance.GetEntityIDForLockedTileEntity(te);
			if (entityIDForLockedTileEntity != -1)
			{
				EntityPlayer entityPlayer = world.GetEntity(entityIDForLockedTileEntity) as EntityPlayer;
				if (entityPlayer.isEntityRemote)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityIDForLockedTileEntity), _onlyClientsAttachedToAnEntity: false, entityIDForLockedTileEntity);
				}
				else
				{
					(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenWindows();
				}
				return ActionCompleteStates.InComplete;
			}
		}
		if (!HandleContainerAction(tileEntityList))
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool HandleContainerAction(List<TileEntity> tileEntityList)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string ParseTextElement(string element)
	{
		if (element == "viewer")
		{
			if (base.Owner.ExtraData.Length <= 12)
			{
				return base.Owner.ExtraData;
			}
			return base.Owner.ExtraData.Insert(12, "\n");
		}
		return element;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Contains(PropNewName))
		{
			changeName = true;
			properties.ParseString(PropNewName, ref newName);
		}
		properties.ParseEnum(PropTargetingType, ref TargetingType);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		maxDistance *= maxDistance;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return null;
	}
}
