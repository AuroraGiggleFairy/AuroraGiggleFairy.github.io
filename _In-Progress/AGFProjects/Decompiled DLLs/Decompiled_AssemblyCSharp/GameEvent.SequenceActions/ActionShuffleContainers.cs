using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionShuffleContainers : ActionBaseContainersAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeOutputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeOutputs = "include_outputs";

	public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
	{
		isEmpty = false;
		switch (te.GetTileEntityType())
		{
		case TileEntityType.Loot:
		case TileEntityType.SecureLoot:
		case TileEntityType.SecureLootSigned:
		case TileEntityType.Composite:
		{
			if (te.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe) && _typedTe.EntityId == -1)
			{
				isEmpty = _typedTe.IsEmpty();
				return true;
			}
			break;
		}
		case TileEntityType.Workstation:
			if (includeOutputs && te is TileEntityWorkstation { EntityId: -1 } tileEntityWorkstation)
			{
				isEmpty = tileEntityWorkstation.OutputEmpty();
				return true;
			}
			break;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleContainerAction(List<TileEntity> tileEntityList)
	{
		List<ItemStack> list = new List<ItemStack>();
		List<TileEntity> list2 = new List<TileEntity>();
		bool flag = false;
		for (int i = 0; i < tileEntityList.Count; i++)
		{
			switch (tileEntityList[i].GetTileEntityType())
			{
			case TileEntityType.Loot:
			case TileEntityType.SecureLoot:
			case TileEntityType.SecureLootSigned:
			case TileEntityType.Composite:
			{
				if (tileEntityList[i].TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe) && _typedTe.EntityId == -1)
				{
					list2.Add(tileEntityList[i]);
					list.AddRange(_typedTe.items);
					if (!_typedTe.IsEmpty())
					{
						flag = true;
					}
				}
				break;
			}
			case TileEntityType.Workstation:
				if (includeOutputs && tileEntityList[i] is TileEntityWorkstation { EntityId: -1 } tileEntityWorkstation)
				{
					list2.Add(tileEntityWorkstation);
					list.AddRange(tileEntityWorkstation.Output);
					if (!tileEntityWorkstation.OutputEmpty())
					{
						flag = true;
					}
				}
				break;
			}
		}
		if (flag && changeName && base.Owner.Target != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
			for (int j = 0; j < tileEntityList.Count; j++)
			{
				if ((tileEntityList[j].GetTileEntityType() == TileEntityType.SecureLootSigned || tileEntityList[j].GetTileEntityType() == TileEntityType.Composite) && tileEntityList[j].TryGetSelfOrFeature<ITileEntitySignable>(out var _typedTe2) && _typedTe2.EntityId == -1)
				{
					_typedTe2.SetText(base.ModifiedName, _syncData: true, playerDataFromEntityID?.PrimaryId);
				}
			}
		}
		GameRandom random = GameEventManager.Current.Random;
		if (list2.Count > 0)
		{
			for (int k = 0; k < list.Count * 2; k++)
			{
				int index = random.RandomRange(list.Count);
				int index2 = random.RandomRange(list.Count);
				ItemStack value = list[index];
				list[index] = list[index2];
				list[index2] = value;
			}
			for (int l = 0; l < list2.Count; l++)
			{
				switch (list2[l].GetTileEntityType())
				{
				case TileEntityType.Loot:
				case TileEntityType.SecureLoot:
				case TileEntityType.SecureLootSigned:
				case TileEntityType.Composite:
				{
					if (list2[l].TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe3))
					{
						for (int n = 0; n < _typedTe3.items.Length; n++)
						{
							_typedTe3.items[n] = list[0];
							list.RemoveAt(0);
						}
					}
					break;
				}
				case TileEntityType.Workstation:
					if (includeOutputs && list2[l] is TileEntityWorkstation { Output: var output } tileEntityWorkstation2)
					{
						for (int m = 0; m < output.Length; m++)
						{
							output[m] = list[0];
							list.RemoveAt(0);
						}
						tileEntityWorkstation2.Output = output;
					}
					break;
				}
				list2[l].SetModified();
			}
		}
		return flag;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropIncludeOutputs, ref includeOutputs);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionShuffleContainers
		{
			TargetingType = TargetingType,
			maxDistance = maxDistance,
			newName = newName,
			changeName = changeName,
			includeOutputs = includeOutputs,
			tileEntityList = tileEntityList
		};
	}
}
