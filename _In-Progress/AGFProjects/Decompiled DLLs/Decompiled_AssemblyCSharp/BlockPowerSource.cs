using System;
using System.Collections;
using Audio;
using Platform;
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

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityPowerSource tileEntityPowerSource && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPowerSource.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			tileEntityPowerSource.IsPlayerPlaced = true;
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPowerSource obj = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPowerSource;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPowerSource;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPowerSource tileEntityPowerSource))
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
		if (!_blockValue.ischild)
		{
			if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPowerSource))
			{
				TileEntityPowerSource tileEntityPowerSource = CreateTileEntity(_chunk);
				tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
				tileEntityPowerSource.InitializePowerData();
				_chunk.AddTileEntity(tileEntityPowerSource);
			}
			BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
			blockEntityData.bNeedsTemperature = true;
			_chunk.AddEntityBlockStub(blockEntityData);
		}
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
			if (tileEntityPowered.GetParent().y != -9999 && world.GetTileEntity(0, tileEntityPowered.GetParent()) is IPowered powered && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				powered.SendWireData();
			}
			tileEntityPowered.RemoveWires();
		}
		_chunk.RemoveTileEntityAt<TileEntityPowerSource>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPowerSource tileEntityPowerSource)
		{
			tileEntityPowerSource.OnDestroy();
		}
		return DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPowerSource tileEntityPowerSource))
		{
			return false;
		}
		switch (_commandName)
		{
		case "open":
		{
			_player.AimingGun = false;
			Vector3i blockPos = tileEntityPowerSource.ToWorldPos();
			_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityPowerSource.entityId, _player.entityId);
			return true;
		}
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
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
			return true;
		}
		case "take":
			TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return true;
		default:
			return false;
		}
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", _bModal: true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[4] { _cIdx, _blockValue, _blockPos, _player };
		timerEventData.Event += EventData_Event;
		childByType.SetTimer(4f, timerEventData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = obj[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), string.Empty, "ui_denied");
			return;
		}
		if ((world.GetTileEntity(clrIdx, vector3i) as TileEntityPowerSource).IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uIForPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		TileEntityPowerSource tileEntityPowerSource = (TileEntityPowerSource)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPowerSource == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
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
		if (tileEntityPowerSource.GetParent().y != -9999 && _world.GetTileEntity(_cIdx, tileEntityPowerSource.GetParent()) is IPowered powered)
		{
			GameManager.Instance.StartCoroutine(drawWiresLater(powered));
		}
		updateState(_world, _cIdx, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawWiresLater(IPowered powered)
	{
		yield return new WaitForSeconds(0.5f);
		powered.DrawWires();
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPowerSource;
			if (tileEntityPowerSource == null)
			{
				ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
				if (chunkCluster == null)
				{
					return;
				}
				Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
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
		updateState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
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
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.Find("Activated");
		if (transform != null)
		{
			transform.gameObject.SetActive(flag);
		}
		return true;
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		if ((_blockValue.meta & 2) != 0 != isOn)
		{
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		updateState(_world, _cIdx, _blockPos, _blockValue);
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
