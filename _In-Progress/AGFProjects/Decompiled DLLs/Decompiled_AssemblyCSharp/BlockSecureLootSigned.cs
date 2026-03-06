using System;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSecureLootSigned : BlockSecureLoot
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int characterWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[7]
	{
		new BlockActivationCommand("Search", "search", _enabled: false),
		new BlockActivationCommand("lock", "lock", _enabled: false),
		new BlockActivationCommand("unlock", "unlock", _enabled: false),
		new BlockActivationCommand("keypad", "keypad", _enabled: false),
		new BlockActivationCommand("pick", "unlock", _enabled: false),
		new BlockActivationCommand("edit", "pen", _enabled: false),
		new BlockActivationCommand("report", "report", _enabled: false)
	};

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(BlockSecureLoot.PropLootList))
		{
			throw new Exception("Block with name " + GetBlockName() + " doesnt have a loot list");
		}
		lootList = base.Properties.Values[BlockSecureLoot.PropLootList];
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickTime))
		{
			lockPickTime = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickTime]);
		}
		else
		{
			lockPickTime = 15f;
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickItem))
		{
			lockPickItem = base.Properties.Values[BlockSecureLoot.PropLockPickItem];
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickBreakChance))
		{
			lockPickBreakChance = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickBreakChance]);
		}
		else
		{
			lockPickBreakChance = 0f;
		}
		if (base.Properties.Values.ContainsKey("LineWidth"))
		{
			characterWidth = int.Parse(base.Properties.Values["LineWidth"]);
		}
		if (base.Properties.Values.ContainsKey("LineCount"))
		{
			lineCount = int.Parse(base.Properties.Values["LineCount"]);
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned)
		{
			tileEntitySecureLootContainerSigned.SetEmpty();
			if (_ea != null && _ea.entityType == EntityType.Player)
			{
				tileEntitySecureLootContainerSigned.bPlayerStorage = true;
				tileEntitySecureLootContainerSigned.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (tileEntitySecureLootContainerSigned != null)
		{
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			if (!tileEntitySecureLootContainerSigned.IsLocked())
			{
				return string.Format(Localization.Get("tooltipUnlocked"), arg, arg2);
			}
			if (lockPickItem == null && !tileEntitySecureLootContainerSigned.LocalPlayerIsOwner())
			{
				return string.Format(Localization.Get("tooltipJammed"), arg, arg2);
			}
			return string.Format(Localization.Get("tooltipLocked"), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainerSigned;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned))
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureLootContainerSigned.GetOwner());
		bool flag = tileEntitySecureLootContainerSigned.LocalPlayerIsOwner();
		bool flag2 = !flag && playerData != null && playerData.ACL != null && playerData.ACL.Contains(internalLocalUserIdentifier);
		cmds[0].enabled = true;
		cmds[1].enabled = !tileEntitySecureLootContainerSigned.IsLocked() && (flag || flag2);
		cmds[2].enabled = tileEntitySecureLootContainerSigned.IsLocked() && flag;
		cmds[3].enabled = (!tileEntitySecureLootContainerSigned.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySecureLootContainerSigned.HasPassword() && tileEntitySecureLootContainerSigned.IsLocked()) || flag;
		cmds[4].enabled = lockPickItem != null && tileEntitySecureLootContainerSigned.IsLocked() && !flag;
		cmds[5].enabled = true;
		bool flag3 = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(tileEntitySecureLootContainerSigned.GetAuthoredText().Text) && !internalLocalUserIdentifier.Equals(tileEntitySecureLootContainerSigned.GetAuthoredText().Author);
		bool flag4 = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntitySecureLootContainerSigned.GetAuthoredText().Author)?.PlatformData.Blocked[EBlockType.TextChat].IsBlocked() ?? false;
		cmds[6].enabled = flag3 && !flag4;
		return cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySecureLootContainerSigned;
			if (tileEntitySecureLootContainerSigned == null)
			{
				tileEntitySecureLootContainerSigned = new TileEntitySecureLootContainerSigned(_chunk);
				tileEntitySecureLootContainerSigned.localChunkPos = World.toBlock(_blockPos);
				tileEntitySecureLootContainerSigned.lootListName = lootList;
				tileEntitySecureLootContainerSigned.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
				_chunk.AddTileEntity(tileEntitySecureLootContainerSigned);
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (_ebcd != null)
		{
			Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
			TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = (TileEntitySecureLootContainerSigned)_world.GetTileEntity(_cIdx, _blockPos);
			if (tileEntitySecureLootContainerSigned == null)
			{
				tileEntitySecureLootContainerSigned = new TileEntitySecureLootContainerSigned(chunk);
				tileEntitySecureLootContainerSigned.localChunkPos = World.toBlock(_blockPos);
				tileEntitySecureLootContainerSigned.lootListName = lootList;
				tileEntitySecureLootContainerSigned.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
				chunk.AddTileEntity(tileEntitySecureLootContainerSigned);
			}
			tileEntitySecureLootContainerSigned.SetBlockEntityData(_ebcd);
			base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureLootContainerSigned>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned)
		{
			tileEntitySecureLootContainerSigned.OnDestroy();
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
		TileEntitySecureLootContainerSigned te = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		if (te == null)
		{
			return false;
		}
		switch (_commandName)
		{
		case "Search":
			if (!te.IsLocked() || te.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		case "lock":
			te.SetLocked(_isLocked: true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked");
			return true;
		case "unlock":
			te.SetLocked(_isLocked: false);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			GameManager.ShowTooltip(_player, "containerUnlocked");
			return true;
		case "keypad":
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uIForPlayer != null)
			{
				XUiC_KeypadWindow.Open(uIForPlayer, te);
			}
			return true;
		}
		case "pick":
		{
			LocalPlayerUI playerUI = _player.PlayerUI;
			ItemValue item = ItemClass.GetItem(lockPickItem);
			if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
			{
				playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), _bAddOnlyIfNotExisting: true);
				GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing"));
				return true;
			}
			_player.AimingGun = false;
			Vector3i blockPos = te.ToWorldPos();
			te.bWasTouched = te.bTouched;
			_world.GetGameManager().TELockServer(_cIdx, blockPos, te.entityId, _player.entityId, "lockpick");
			return true;
		}
		case "edit":
			if (GameManager.Instance.IsEditMode() || !te.IsLocked() || te.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player, "sign");
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		case "report":
			GeneratedTextManager.GetDisplayText(te.GetAuthoredText(), [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
			{
				ThreadManager.AddSingleTaskMainThread("OpenReportWindow", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
				{
					XUiC_ReportPlayer.Open(GameManager.Instance.persistentPlayers.GetPlayerData(te.GetAuthoredText().Author)?.PlayerData, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage"), _filtered));
				});
			}, _runCallbackIfReadyNow: true, _checkBlockState: false);
			return true;
		default:
			return false;
		}
	}

	public bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player, string _customUi = null)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_world, _cIdx, parentPos, block, _player, _customUi);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntitySecureLootContainerSigned.ToWorldPos();
		tileEntitySecureLootContainerSigned.bWasTouched = tileEntitySecureLootContainerSigned.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainerSigned.entityId, _player.entityId, _customUi);
		return true;
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return base.IsTileEntitySavedInPrefab();
	}
}
