using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionEmptyContainers : ActionBaseContainersAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeInputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeOutputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeFuel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeTools;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeInputs = "include_inputs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeOutputs = "include_outputs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeFuel = "include_fuel";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIncludeTools = "include_tools";

	public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
	{
		TileEntityType tileEntityType = te.GetTileEntityType();
		isEmpty = true;
		switch (tileEntityType)
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
			if (!(te is TileEntityWorkstation tileEntityWorkstation))
			{
				break;
			}
			if (includeInputs)
			{
				ItemStack[] input = tileEntityWorkstation.Input;
				for (int i = 0; i < input.Length; i++)
				{
					if (!input[i].IsEmpty())
					{
						isEmpty = false;
					}
				}
			}
			if (includeOutputs)
			{
				ItemStack[] output = tileEntityWorkstation.Output;
				for (int j = 0; j < output.Length; j++)
				{
					if (!output[j].IsEmpty())
					{
						isEmpty = false;
					}
				}
			}
			if (includeFuel)
			{
				tileEntityWorkstation.IsBurning = false;
				tileEntityWorkstation.ResetTickTime();
				ItemStack[] fuel = tileEntityWorkstation.Fuel;
				for (int k = 0; k < fuel.Length; k++)
				{
					if (!fuel[k].IsEmpty())
					{
						isEmpty = false;
					}
				}
			}
			if (includeTools)
			{
				ItemStack[] tools = tileEntityWorkstation.Tools;
				for (int l = 0; l < tools.Length; l++)
				{
					if (!tools[l].IsEmpty())
					{
						isEmpty = false;
					}
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleContainerAction(List<TileEntity> tileEntityList)
	{
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
				if (tileEntityList[i].TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe) && _typedTe.EntityId == -1 && !_typedTe.IsEmpty())
				{
					_typedTe.SetEmpty();
					flag = true;
				}
				break;
			}
			case TileEntityType.Workstation:
				if (!(tileEntityList[i] is TileEntityWorkstation tileEntityWorkstation))
				{
					break;
				}
				if (includeInputs)
				{
					ItemStack[] input = tileEntityWorkstation.Input;
					for (int j = 0; j < input.Length; j++)
					{
						if (!input[j].IsEmpty())
						{
							input[j] = ItemStack.Empty;
							flag = true;
						}
					}
					tileEntityWorkstation.ClearSlotTimersForInputs();
					tileEntityWorkstation.Input = input;
				}
				if (includeOutputs)
				{
					ItemStack[] output = tileEntityWorkstation.Output;
					for (int k = 0; k < output.Length; k++)
					{
						if (!output[k].IsEmpty())
						{
							output[k] = ItemStack.Empty;
							flag = true;
						}
					}
					tileEntityWorkstation.Output = output;
				}
				if (includeFuel)
				{
					tileEntityWorkstation.IsBurning = false;
					tileEntityWorkstation.ResetTickTime();
					ItemStack[] fuel = tileEntityWorkstation.Fuel;
					for (int l = 0; l < fuel.Length; l++)
					{
						if (!fuel[l].IsEmpty())
						{
							fuel[l] = ItemStack.Empty;
							flag = true;
						}
					}
					tileEntityWorkstation.Fuel = fuel;
				}
				if (includeTools)
				{
					ItemStack[] tools = tileEntityWorkstation.Tools;
					for (int m = 0; m < tools.Length; m++)
					{
						if (!tools[m].IsEmpty())
						{
							tools[m] = ItemStack.Empty;
							flag = true;
						}
					}
					tileEntityWorkstation.Tools = tools;
				}
				if (includeTools || includeOutputs)
				{
					tileEntityWorkstation.ResetCraftingQueue();
				}
				break;
			}
		}
		if (flag && changeName && base.Owner.Target != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
			for (int n = 0; n < tileEntityList.Count; n++)
			{
				if ((tileEntityList[n].GetTileEntityType() == TileEntityType.SecureLootSigned || tileEntityList[n].GetTileEntityType() == TileEntityType.Composite) && tileEntityList[n].TryGetSelfOrFeature<ITileEntitySignable>(out var _typedTe2) && _typedTe2.EntityId == -1)
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
		properties.ParseBool(PropIncludeInputs, ref includeInputs);
		properties.ParseBool(PropIncludeOutputs, ref includeOutputs);
		properties.ParseBool(PropIncludeFuel, ref includeFuel);
		properties.ParseBool(PropIncludeTools, ref includeTools);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionEmptyContainers
		{
			TargetingType = TargetingType,
			maxDistance = maxDistance,
			newName = newName,
			changeName = changeName,
			includeInputs = includeInputs,
			includeOutputs = includeOutputs,
			includeFuel = includeFuel,
			includeTools = includeTools,
			tileEntityList = tileEntityList
		};
	}
}
