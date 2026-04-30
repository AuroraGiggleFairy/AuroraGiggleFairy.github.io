using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPoweredLight : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRuntimeSwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("light", "electric_switch", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockPoweredLight()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RuntimeSwitch"))
		{
			isRuntimeSwitch = StringParsers.ParseBool(base.Properties.Values["RuntimeSwitch"]);
		}
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		if ((_blockValue.meta & 2) == 0)
		{
			return 0;
		}
		return base.GetLightValue(_blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (isRuntimeSwitch)
		{
			TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_clrIdx, _blockPos);
			if (tileEntityPoweredBlock != null)
			{
				PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
				string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
				if (tileEntityPoweredBlock.IsToggled)
				{
					return string.Format(Localization.Get("useSwitchLightOff"), arg);
				}
				return string.Format(Localization.Get("useSwitchLightOn"), arg);
			}
		}
		else if (_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) && TakeDelay > 0f)
		{
			Block block = _blockValue.Block;
			return string.Format(Localization.Get("pickupPrompt"), block.GetLocalizedBlockName());
		}
		return null;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "light"))
		{
			if (_commandName == "take")
			{
				TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
				return true;
			}
		}
		else
		{
			TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_cIdx, _blockPos);
			if (!_world.IsEditor() && tileEntityPoweredBlock != null)
			{
				tileEntityPoweredBlock.IsToggled = !tileEntityPoweredBlock.IsToggled;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateLightState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(_blockPos);
		if (chunkFromWorldPos == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkFromWorldPos.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) != 0;
		if (_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredBlock tileEntityPoweredBlock)
		{
			flag = flag && tileEntityPoweredBlock.IsToggled;
		}
		if (_bSwitchLight)
		{
			flag = !flag;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.Find("MainLight");
		if ((bool)transform)
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			if ((bool)component)
			{
				component.SwitchOnOff(flag);
				component.SetBlockEntityData(blockEntity);
			}
		}
		transform = blockEntity.transform.Find("Point light");
		if (transform != null)
		{
			LightLOD component2 = transform.GetComponent<LightLOD>();
			if (component2 != null)
			{
				component2.SwitchOnOff(flag);
				component2.SetBlockEntityData(blockEntity);
			}
		}
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		updateLightState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = _world.IsEditor() || isRuntimeSwitch;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		updateLightState(_world, _cIdx, _blockPos, _blockValue);
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		updateLightState(_world, _cIdx, _blockPos, _blockValue);
		return true;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		PowerItem.PowerItemTypes powerItemType = PowerItem.PowerItemTypes.Consumer;
		if (isRuntimeSwitch)
		{
			powerItemType = PowerItem.PowerItemTypes.ConsumerToggle;
		}
		return new TileEntityPoweredBlock(chunk)
		{
			PowerItemType = powerItemType
		};
	}
}
