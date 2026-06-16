using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockMotionSensor : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("options", "tool", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockMotionSensor()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		TileEntityPoweredTrigger tileEntity = _world.GetTileEntity(_blockPos) as TileEntityPoweredTrigger;
		MotionSensorController component = _ebcd.transform.gameObject.GetComponent<MotionSensorController>();
		if (component != null)
		{
			component.Init(base.Properties);
			component.TileEntity = tileEntity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bChangeState = false)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 1) != 0;
		bool flag2 = (_blockValue.meta & 2) != 0;
		if (_bChangeState)
		{
			flag2 = !flag2;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag2 ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
			if (flag2)
			{
				Manager.PlayInsidePlayerHead("switch_up");
			}
			else
			{
				Manager.PlayInsidePlayerHead("switch_down");
			}
		}
		TileEntityPoweredTrigger tileEntity = _world.GetTileEntity(_blockPos) as TileEntityPoweredTrigger;
		MotionSensorController component = blockEntity.transform.gameObject.GetComponent<MotionSensorController>();
		if (component != null)
		{
			component.Init(base.Properties);
			component.TileEntity = tileEntity;
			component.IsOn = flag;
		}
		BlockEntityData blockEntity2 = ((World)_world).ChunkCache.GetBlockEntity(_blockPos);
		if (blockEntity2 != null && blockEntity2.transform != null && blockEntity2.transform.gameObject != null)
		{
			Renderer[] componentsInChildren = blockEntity2.transform.gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
					{
						componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
					}
					if (flag)
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", flag2 ? Color.green : Color.red);
					}
					else
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
					}
					componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
				}
			}
		}
		return true;
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue blockDef, BlockFace face)
	{
		return false;
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		updateState(_world, _blockPos, _blockValue);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		updateState(_world, _blockPos, _newBlockValue);
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		updateState(_world, _blockPos, _blockValue);
		return true;
	}

	public static bool IsSwitchOn(byte _metadata)
	{
		return (_metadata & 2) != 0;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredTrigger(chunk)
		{
			TriggerType = PowerTrigger.TriggerTypes.Motion
		};
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		TileEntityPoweredTrigger tileEntityPoweredTrigger = _world.GetTileEntity(_blockPos) as TileEntityPoweredTrigger;
		if (tileEntityPoweredTrigger == null)
		{
			tileEntityPoweredTrigger = CreateTileEntity(_chunk) as TileEntityPoweredTrigger;
			tileEntityPoweredTrigger.SetDisableModifiedCheck(_b: true);
			tileEntityPoweredTrigger.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null && tileEntityPoweredTrigger != null)
			{
				TileEntityPoweredTrigger tileEntityPoweredTrigger2 = tileEntityPoweredTrigger;
				tileEntityPoweredTrigger2.SetOwner(_addedByPlayer);
			}
			tileEntityPoweredTrigger.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPoweredTrigger);
			tileEntityPoweredTrigger.SetDisableModifiedCheck(_b: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPoweredTrigger.SetModified();
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPoweredTrigger obj = _world.GetTileEntity(_blockPos) as TileEntityPoweredTrigger;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredTrigger target))
		{
			return false;
		}
		if (!(_commandName == "options"))
		{
			if (_commandName == "take")
			{
				takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
				return true;
			}
			return false;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(target, null, 0);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = enabled;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}
}
