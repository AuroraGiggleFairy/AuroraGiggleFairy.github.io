using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlayerSign : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int characterWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[5]
	{
		new BlockActivationCommand("edit", "pen", _enabled: false),
		new BlockActivationCommand("lock", "lock", _enabled: false),
		new BlockActivationCommand("unlock", "unlock", _enabled: false),
		new BlockActivationCommand("keypad", "keypad", _enabled: false),
		new BlockActivationCommand("report", "report", _enabled: false)
	};

	public BlockPlayerSign()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("LineWidth"))
		{
			characterWidth = int.Parse(base.Properties.Values["LineWidth"]);
		}
		if (base.Properties.Values.ContainsKey("LineCount"))
		{
			lineCount = int.Parse(base.Properties.Values["LineCount"]);
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntitySign tileEntitySign = (TileEntitySign)world.GetTileEntity(_chunk.ClrIdx, _blockPos);
		if (tileEntitySign == null)
		{
			tileEntitySign = new TileEntitySign(_chunk);
			if (tileEntitySign != null)
			{
				tileEntitySign.localChunkPos = World.toBlock(_blockPos);
				_chunk.AddTileEntity(tileEntitySign);
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (_ebcd == null)
		{
			return;
		}
		Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
		TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntitySign == null)
		{
			tileEntitySign = new TileEntitySign(chunk);
			if (tileEntitySign != null)
			{
				tileEntitySign.localChunkPos = World.toBlock(_blockPos);
				chunk.AddTileEntity(tileEntitySign);
			}
		}
		if (tileEntitySign == null)
		{
			Log.Error("Tile Entity Sign was unable to be created!");
			return;
		}
		tileEntitySign.SetBlockEntityData(_ebcd);
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		((TileEntitySign)_world.GetTileEntity(_result.clrIdx, _result.blockPos))?.SetOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySign>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer)
		{
			tileEntitySecureLootContainer.OnDestroy();
		}
		return DestroyedResult.Downgrade;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useWorkstation");
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntitySign te = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
		if (te == null)
		{
			return false;
		}
		switch (_commandName)
		{
		case "edit":
			if (GameManager.Instance.IsEditMode() || !te.IsLocked() || te.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
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
			XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player), te);
			return true;
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

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntitySign == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntitySign.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySign.entityId, _player.entityId);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySign;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySign == null)
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySign.GetOwner());
		bool flag = tileEntitySign.LocalPlayerIsOwner();
		bool flag2 = !tileEntitySign.LocalPlayerIsOwner() && playerData != null && playerData.ACL != null && playerData.ACL.Contains(internalLocalUserIdentifier);
		cmds[0].enabled = true;
		cmds[1].enabled = !tileEntitySign.IsLocked() && (flag || flag2);
		cmds[2].enabled = tileEntitySign.IsLocked() && flag;
		cmds[3].enabled = (!tileEntitySign.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || flag;
		bool flag3 = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(tileEntitySign.GetAuthoredText().Text) && !internalLocalUserIdentifier.Equals(tileEntitySign.GetAuthoredText().Author);
		bool flag4 = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntitySign.GetAuthoredText().Author)?.PlatformData.Blocked[EBlockType.TextChat].IsBlocked() ?? false;
		cmds[4].enabled = playerData != null && flag3 && !flag4;
		return cmds;
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return true;
	}
}
