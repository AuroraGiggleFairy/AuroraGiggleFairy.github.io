using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockVendingMachine : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTraderID = "TraderID";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int traderID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[4]
	{
		new BlockActivationCommand("trade", "vending", _enabled: false),
		new BlockActivationCommand("take", "hand", _enabled: false),
		new BlockActivationCommand("keypad", "keypad", _enabled: false),
		new BlockActivationCommand("restock", "coin", _enabled: false)
	};

	public BlockVendingMachine()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(PropTraderID))
		{
			throw new Exception("Block with name " + GetBlockName() + " doesnt have a trader ID.");
		}
		int.TryParse(base.Properties.Values[PropTraderID], out traderID);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityVendingMachine tileEntityVendingMachine && _ea != null && _ea.entityType == EntityType.Player && TraderInfo.traderInfoList[traderID].PlayerOwned)
		{
			tileEntityVendingMachine.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.persistentPlayers.Players[PlatformManager.InternalLocalUserIdentifier].AddVendingMachinePosition(_result.blockPos);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerVendingMachine>().Setup(PlatformManager.InternalLocalUserIdentifier, _result.blockPos, _removing: false));
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (tileEntityVendingMachine != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			if ((tileEntityVendingMachine.IsRentable || tileEntityVendingMachine.TraderData.TraderInfo.PlayerOwned) && tileEntityVendingMachine.GetOwner() != null)
			{
				PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntityVendingMachine.GetOwner());
				if (playerData != null)
				{
					GameServerInfo obj = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
					if ((obj != null && obj.AllowsCrossplay) || playerData.PlayGroup != DeviceFlag.StandaloneWindows.ToPlayGroup())
					{
						string text = "[sp=" + PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playerData.PlayGroup, _fetchGenericIcons: true, playerData.PlatformData.NativeId.PlatformIdentifier) + "]";
						arg2 = string.Format(Localization.Get("xuiVendingWithOwner"), GameUtils.SafeStringFormat(text + " " + playerData.PlayerName.DisplayName));
					}
					else
					{
						arg2 = string.Format(Localization.Get("xuiVendingWithOwner"), GameUtils.SafeStringFormat(playerData.PlayerName.DisplayName));
					}
				}
				else
				{
					arg2 = string.Format(Localization.Get("xuiVendingWithOwner"), Localization.Get("sleepingBagPlayerUnknown"));
				}
			}
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityVendingMachine;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityVendingMachine tileEntityVendingMachine))
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntityVendingMachine.GetOwner());
		bool flag = tileEntityVendingMachine.LocalPlayerIsOwner();
		if (!flag)
		{
			if (playerData != null && playerData.ACL != null)
			{
				playerData.ACL.Contains(internalLocalUserIdentifier);
			}
			else
				_ = 0;
		}
		else
			_ = 0;
		bool playerOwned = TraderInfo.traderInfoList[traderID].PlayerOwned;
		cmds[0].enabled = true;
		cmds[1].enabled = playerOwned && flag && tileEntityVendingMachine.TraderData.PrimaryInventory.Count == 0;
		cmds[2].enabled = playerOwned && ((!tileEntityVendingMachine.IsUserAllowed(internalLocalUserIdentifier) && tileEntityVendingMachine.HasPassword()) || flag);
		cmds[3].enabled = !playerOwned && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		return cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild && !(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityVendingMachine))
		{
			TileEntityVendingMachine tileEntityVendingMachine = new TileEntityVendingMachine(_chunk);
			tileEntityVendingMachine.localChunkPos = World.toBlock(_blockPos);
			tileEntityVendingMachine.TraderData = new TraderData();
			tileEntityVendingMachine.TraderData.TraderID = traderID;
			_chunk.AddTileEntity(tileEntityVendingMachine);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityVendingMachine tileEntityVendingMachine)
		{
			PlatformUserIdentifierAbs owner = tileEntityVendingMachine.GetOwner();
			if (owner != null && GameManager.Instance.persistentPlayers.Players.TryGetValue(owner, out var value))
			{
				value.TryRemoveVendingMachinePosition(_blockPos);
			}
		}
		_chunk.RemoveTileEntityAt<TileEntityVendingMachine>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		_ = _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityVendingMachine;
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
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityVendingMachine tileEntityVendingMachine))
		{
			return false;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		if (null != uIForPlayer)
		{
			switch (_commandName)
			{
			case "trade":
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			case "take":
			{
				ItemStack itemStack = new ItemStack(_blockValue.ToItemValue(), 1);
				if (uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
				{
					_world.SetBlockRPC(_cIdx, _blockPos, BlockValue.Air);
				}
				return true;
			}
			case "keypad":
				XUiC_KeypadWindow.Open(uIForPlayer, tileEntityVendingMachine);
				return true;
			case "restock":
				_player.PlayOneShot("ui_trader_inv_reset");
				tileEntityVendingMachine.TraderData.lastInventoryUpdate = 0uL;
				return true;
			}
		}
		return false;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_world.GetBlock(_blockPos.x, _blockPos.y - 1, _blockPos.z).Block.HasTag(BlockTags.Door))
		{
			_blockPos = new Vector3i(_blockPos.x, _blockPos.y - 1, _blockPos.z);
			return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityVendingMachine tileEntityVendingMachine))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityVendingMachine.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityVendingMachine.entityId, _player.entityId);
		return true;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_damagePoints > 0 && base.Properties.Values.ContainsKey("Buff"))
		{
			EntityAlive entityAlive = _world.GetEntity(_entityIdThatDamaged) as EntityAlive;
			if (entityAlive != null && entityAlive as EntityTurret == null)
			{
				bool flag = true;
				if (_attackHitInfo != null && _attackHitInfo.WeaponTypeTag.Equals(ItemActionAttack.ThrownTag))
				{
					flag = true;
				}
				else if (!(entityAlive.inventory.holdingItemData.item.Actions[0] is ItemActionRanged itemActionRanged) || (itemActionRanged.Hitmask & 0x80) != 0)
				{
					flag = false;
				}
				if (!flag)
				{
					string[] array = base.Properties.Values["Buff"].Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						entityAlive.Buffs.AddBuff(array[i].Trim(), _blockPos, entityAlive.entityId);
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}
}
