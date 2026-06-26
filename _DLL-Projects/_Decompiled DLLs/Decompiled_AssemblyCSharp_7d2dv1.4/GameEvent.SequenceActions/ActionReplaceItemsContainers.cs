using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionReplaceItemsContainers : ActionBaseContainersAction
{
	public string ReplacedByItem = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeOutputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string itemTags = "";

	public static string PropReplacedByItem = "replaced_by_item";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeOutputs = "include_outputs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemTag = "items_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> fastItemTags = FastTags<TagGroup.Global>.none;

	public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
	{
		isEmpty = true;
		switch (te.GetTileEntityType())
		{
		case TileEntityType.Loot:
		case TileEntityType.SecureLoot:
		case TileEntityType.SecureLootSigned:
		case TileEntityType.Composite:
		{
			if (!te.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe))
			{
				break;
			}
			for (int j = 0; j < _typedTe.items.Length; j++)
			{
				ItemStack itemStack2 = _typedTe.items[j];
				if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != ReplacedByItem)
				{
					isEmpty = false;
				}
			}
			return true;
		}
		case TileEntityType.Workstation:
			if (!includeOutputs || !(te is TileEntityWorkstation { Output: var output }))
			{
				break;
			}
			foreach (ItemStack itemStack in output)
			{
				if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != ReplacedByItem)
				{
					isEmpty = false;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleContainerAction(List<TileEntity> tileEntityList)
	{
		new List<ItemStack>();
		List<TileEntity> list = new List<TileEntity>();
		bool flag = false;
		for (int i = 0; i < tileEntityList.Count; i++)
		{
			bool flag2 = false;
			switch (tileEntityList[i].GetTileEntityType())
			{
			case TileEntityType.Loot:
			case TileEntityType.SecureLoot:
			case TileEntityType.SecureLootSigned:
			case TileEntityType.Composite:
			{
				if (!tileEntityList[i].TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe) || _typedTe.EntityId != -1)
				{
					break;
				}
				for (int k = 0; k < _typedTe.items.Length; k++)
				{
					ItemStack itemStack2 = _typedTe.items[k];
					if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != ReplacedByItem)
					{
						_typedTe.items[k] = new ItemStack(ItemClass.GetItem(ReplacedByItem), itemStack2.count);
						flag = true;
						flag2 = true;
					}
				}
				break;
			}
			case TileEntityType.Workstation:
			{
				if (!includeOutputs || !(tileEntityList[i] is TileEntityWorkstation { EntityId: -1, Output: var output } tileEntityWorkstation))
				{
					break;
				}
				for (int j = 0; j < output.Length; j++)
				{
					ItemStack itemStack = output[j];
					if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != ReplacedByItem)
					{
						output[j] = new ItemStack(ItemClass.GetItem(ReplacedByItem), itemStack.count);
						flag = true;
						flag2 = true;
					}
				}
				if (flag2)
				{
					tileEntityWorkstation.Output = output;
					list.Add(tileEntityWorkstation);
				}
				break;
			}
			}
			if (flag2)
			{
				tileEntityList[i].SetModified();
			}
		}
		if (flag && changeName && base.Owner.Target != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
			for (int l = 0; l < tileEntityList.Count; l++)
			{
				if ((tileEntityList[l].GetTileEntityType() == TileEntityType.SecureLootSigned || tileEntityList[l].GetTileEntityType() == TileEntityType.Composite) && tileEntityList[l].TryGetSelfOrFeature<ITileEntitySignable>(out var _typedTe2) && _typedTe2.EntityId == -1)
				{
					_typedTe2.SetText(base.ModifiedName, _syncData: true, playerDataFromEntityID?.PrimaryId);
				}
			}
		}
		return flag;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropIncludeOutputs, ref includeOutputs);
		properties.ParseString(PropReplacedByItem, ref ReplacedByItem);
		properties.ParseString(PropItemTag, ref itemTags);
		fastItemTags = FastTags<TagGroup.Global>.Parse(itemTags);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionReplaceItemsContainers
		{
			TargetingType = TargetingType,
			maxDistance = maxDistance,
			newName = newName,
			changeName = changeName,
			includeOutputs = includeOutputs,
			tileEntityList = tileEntityList,
			fastItemTags = fastItemTags,
			ReplacedByItem = ReplacedByItem
		};
	}
}
