using System;
using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPowerSource : Block
{
	public string SlotItemName;

	public int OutputPerStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass slotItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[3]
	{
		new BlockActivationCommand("open", "hand", _enabled: false),
		new BlockActivationCommand("light", "electric_switch", _enabled: false),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockPowerSource()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("SlotItem"))
		{
			SlotItemName = base.Properties.Values["SlotItem"];
		}
		else
		{
			SlotItemName = "carBattery";
		}
		if (base.Properties.Values.ContainsKey("OutputPerStack"))
		{
			OutputPerStack = Convert.ToInt32(base.Properties.Values["OutputPerStack"]);
		}
		else
		{
			OutputPerStack = 25;
		}
		if (base.Properties.Values.ContainsKey("TakeDelay"))
		{
			TakeDelay = StringParsers.ParseFloat(base.Properties.Values["TakeDelay"]);
		}
		else
		{
			TakeDelay = 2f;
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPowerSource obj = _world.GetTileEntity(_blockPos) as TileEntityPowerSource;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_blockPos) is TileEntityPowerSource;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPowerSource tileEntityPowerSource))
		{
			return BlockActivationCommand.Empty;
		}
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].icon = GetPowerSourceIcon();
		cmds[0].enabled = enabled;
		cmds[1].enabled = enabled;
		bool flag2 = false;
		if (tileEntityPowerSource != null)
		{
			flag2 = tileEntityPowerSource.IsPlayerPlaced;
		}
		cmds[2].enabled = flag && flag2 && TakeDelay > 0f;
		return cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntityPowerSource tileEntityPowerSource = world.GetTileEntity(_blockPos) as TileEntityPowerSource;
		if (tileEntityPowerSource == null)
		{
			tileEntityPowerSource = CreateTileEntity(_chunk);
			tileEntityPowerSource.SetDisableModifiedCheck(_b: true);
			tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPowerSource.SetOwner(_addedByPlayer);
				tileEntityPowerSource.IsPlayerPlaced = true;
			}
			tileEntityPowerSource.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowerSource);
			tileEntityPowerSource.SetDisableModifiedCheck(_b: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPowerSource.SetModified();
			}
		}
		BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
		blockEntityData.bNeedsTemperature = true;
		_chunk.AddEntityBlockStub(blockEntityData);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (_chunk.GetTileEntity(World.toBlock(_blockPos)) is TileEntityPowered tileEntityPowered)
		{
			if (!GameManager.IsDedicatedServer)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionConnectPower)
				{
					(primaryPlayer.inventory.holdingItem.Actions[1] as ItemActionConnectPower).CheckForWireRemoveNeeded(primaryPlayer, _blockPos);
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerManager.Instance.RemovePowerNode(tileEntityPowered.GetPowerItem());
			}
			if (tileEntityPowered.GetParent().y != -9999 && world.GetTileEntity(tileEntityPowered.GetParent()) is IPowered powered && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				powered.SendWireData();
			}
			tileEntityPowered.RemoveWires();
		}
		_chunk.RemoveTileEntityAt<TileEntityPowerSource>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_bvRef) is TileEntityPowerSource tileEntityPowerSource)
		{
			tileEntityPowerSource.OnDestroy();
		}
		return DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPowerSource tileEntityPowerSource))
		{
			return false;
		}
		switch (_commandName)
		{
		case "open":
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(tileEntityPowerSource, null, 0);
			return true;
		case "light":
		{
			if (tileEntityPowerSource.MaxOutput == 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied");
				GameManager.ShowTooltip(_player, Localization.Get("ttRequiresOneComponent"));
				return false;
			}
			if (tileEntityPowerSource.PowerItemType == PowerItem.PowerItemTypes.Generator && tileEntityPowerSource.CurrentFuel == 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied");
				GameManager.ShowTooltip(_player, Localization.Get("ttGeneratorRequiresFuel"));
				return false;
			}
			bool flag = (_blockValue.meta & 2) != 0;
			if (!flag && (0u | (_world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) ? 1u : 0u) | (_world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) ? 1u : 0u) | (_world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) ? 1u : 0u) | (_world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) ? 1u : 0u) | (_world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1) ? 1u : 0u)) != 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied");
				GameManager.ShowTooltip(_player, Localization.Get("ttPowerSourceUnderwater"));
				return false;
			}
			_blockValue.meta = (byte)((_blockValue.meta & -3) | ((!flag) ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
			return true;
		}
		case "take":
			takeItemWithTimer(_blockPos, _blockValue, _player, 4f);
			return true;
		default:
			return false;
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		TileEntityPowerSource tileEntityPowerSource = (TileEntityPowerSource)_world.GetTileEntity(_blockPos);
		if (tileEntityPowerSource == null)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPowerSource = CreateTileEntity(chunk);
			tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowerSource.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowerSource);
		}
		tileEntityPowerSource.BlockTransform = _ebcd.transform;
		GameManager.Instance.StartCoroutine(drawWiresLater(tileEntityPowerSource));
		if (tileEntityPowerSource.GetParent().y != -9999 && _world.GetTileEntity(tileEntityPowerSource.GetParent()) is IPowered powered)
		{
			GameManager.Instance.StartCoroutine(drawWiresLater(powered));
		}
		updateState(_world, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawWiresLater(IPowered powered)
	{
		yield return new WaitForSeconds(0.5f);
		powered.DrawWires();
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_blockPos) as TileEntityPowerSource;
			if (tileEntityPowerSource == null)
			{
				ChunkCluster chunkCache = _world.ChunkCache;
				if (chunkCache == null)
				{
					return;
				}
				Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
				if (chunk == null)
				{
					return;
				}
				tileEntityPowerSource = CreateTileEntity(chunk);
				tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
				chunk.AddTileEntity(tileEntityPowerSource);
				Vector3i vector3i = _blockPos;
				Log.Out("TileEntityPowerSource not found (" + vector3i.ToString() + ")");
			}
			PowerSource powerSource = tileEntityPowerSource.GetPowerItem() as PowerSource;
			if (powerSource == null)
			{
				powerSource = PowerManager.Instance.GetPowerItemByWorldPos(tileEntityPowerSource.ToWorldPos()) as PowerSource;
				if (powerSource == null)
				{
					powerSource = tileEntityPowerSource.CreatePowerItemForTileEntity((ushort)_newBlockValue.type) as PowerSource;
					tileEntityPowerSource.SetModified();
					powerSource.AddTileEntity(tileEntityPowerSource);
					Vector3i vector3i = _blockPos;
					Log.Out("PowerSource not found (" + vector3i.ToString() + ")");
				}
			}
			bool isOn = (_newBlockValue.meta & 2) != 0;
			powerSource.IsOn = isOn;
		}
		updateState(_world, _blockPos, _newBlockValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false)
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
		bool flag = (_blockValue.meta & 2) != 0;
		if (_bSwitchLight)
		{
			flag = !flag;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.Find("Activated");
		if (transform != null)
		{
			transform.gameObject.SetActive(flag);
		}
		return true;
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		if ((_blockValue.meta & 2) != 0 != isOn)
		{
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		updateState(_world, _blockPos, _blockValue);
		return true;
	}

	public virtual TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetPowerSourceIcon()
	{
		return "";
	}
}
