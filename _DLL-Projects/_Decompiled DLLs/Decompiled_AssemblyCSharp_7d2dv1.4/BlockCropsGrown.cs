using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockCropsGrown : BlockPlant
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockBeforeHarvesting = "BlockBeforeHarvesting";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingBonusHarvestDivisor = "CropsGrown.BonusHarvestDivisor";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("pickup", "hand", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue babyPlant = BlockValue.Air;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bonusHarvestDivisor = float.MaxValue;

	public BlockCropsGrown()
	{
		CanPickup = true;
		IsRandomlyTick = false;
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(PropBlockBeforeHarvesting))
		{
			babyPlant = ItemClass.GetItem(base.Properties.Values[PropBlockBeforeHarvesting]).ToBlockValue();
		}
		if (base.Properties.Values.ContainsKey(PropGrowingBonusHarvestDivisor))
		{
			bonusHarvestDivisor = StringParsers.ParseFloat(base.Properties.Values[PropGrowingBonusHarvestDivisor]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPlantBackToBaby(WorldBase _world, int _cIdx, Vector3i _myBlockPos, BlockValue _blockValue)
	{
		babyPlant.rotation = _blockValue.rotation;
		_world.SetBlockRPC(_cIdx, _myBlockPos, babyPlant);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		List<SItemDropProb> value = null;
		int num = 0;
		if (itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out value) && (num = Utils.FastMax(0, value[0].minCount)) > 0)
		{
			if (_blockPos.y > 1)
			{
				int num2 = (int)((float)_world.GetBlock(_blockPos - Vector3i.up).Block.blockMaterial.FertileLevel / bonusHarvestDivisor);
				num += num2;
			}
			return string.Format(Localization.Get("pickupCrops"), num, Localization.Get(value[0].name));
		}
		return null;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		List<SItemDropProb> value = null;
		int num = 0;
		if (itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out value) && (num = Utils.FastMax(0, value[0].minCount)) > 0)
		{
			if (_blockPos.y > 1)
			{
				int num2 = (int)((float)_world.GetBlock(_blockPos - Vector3i.up).Block.blockMaterial.FertileLevel / bonusHarvestDivisor);
				num += num2;
			}
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(value[0].name), num);
			ItemStack itemStack2 = itemStack.Clone();
			if ((_player.inventory.CanStackNoEmpty(itemStack) && _player.inventory.AddItem(itemStack)) || _player.bag.AddItem(itemStack) || _player.inventory.AddItem(itemStack))
			{
				_player.PlayOneShot("item_plant_pickup");
				setPlantBackToBaby(_world, _cIdx, _blockPos, _blockValue);
				QuestEventManager.Current.BlockPickedUp(_blockValue.Block.GetBlockName(), _blockPos);
				_player.AddUIHarvestingItem(itemStack2);
				return true;
			}
			Manager.PlayInsidePlayerHead("ui_denied");
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "pickup")
		{
			OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}
}
