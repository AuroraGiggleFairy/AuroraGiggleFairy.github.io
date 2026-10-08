using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockCompositeTileEntity : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockActivationCommand[] commands;

	public override bool AllowBlockTriggers => true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCompositeData CompositeData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public BlockCompositeTileEntity()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		CompositeData = TileEntityCompositeData.ParseBlock(this);
		if (TEFeatureAbs.DebugLogCTE)
		{
			CompositeData.PrintConfig();
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockAdded (_, {_chunk.ChunkPos}, {_blockPos}, {_blockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			TileEntityComposite tileEntityComposite = _world.GetTileEntity(_blockPos) as TileEntityComposite;
			if (tileEntityComposite == null)
			{
				tileEntityComposite = new TileEntityComposite(_chunk, _blockValue)
				{
					localChunkPos = World.toBlock(_blockPos)
				};
			}
			tileEntityComposite.OnBlockAdded(_blockPos, _blockValue, _addedByPlayer);
			_chunk.AddTileEntity(tileEntityComposite);
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockRemoved (_, {_chunk.ChunkPos}, {_blockPos}, {_blockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			_chunk.RemoveTileEntityAt<TileEntityComposite>((World)_world, World.toBlock(_blockPos));
		}
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		if (!_blockValue.ischild && TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockLoaded (_, _, {_blockPos}, {_blockValue}), HasTE: {_world.GetTileEntity(_blockPos) is TileEntityComposite}, from {StackTraceUtility.ExtractStackTrace()}");
		}
	}

	public override void OnBlockUnloaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockUnloaded (_, _, {_blockPos}, {_blockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		base.OnBlockUnloaded(_world, _blockPos, _blockValue);
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockPlaced (_, _, {_blockPos}, {_blockValue}, _) from {StackTraceUtility.ExtractStackTrace()}");
		}
		return base.OnBlockPlaced(_world, _blockPos, _blockValue, _rnd);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockValueChanged (_, {_chunk.ChunkPos}, _, {_blockPos}, {_oldBlockValue}, {_newBlockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (TryGetParentBlockAndTileEntity(ref _blockPos, ref _newBlockValue, out var _te))
		{
			_te.OnBlockValueChanged(_blockPos, _oldBlockValue, _newBlockValue);
		}
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockReset (_, {_chunk.ChunkPos}, {_blockPos}, {_blockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			_te.OnBlockReset(_blockPos, _blockValue);
		}
		base.OnBlockReset(_world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockStartsToFall(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockStartsToFall ({_blockPos}, {_blockValue}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			_te.OnBlockStartsToFall(_blockPos, _blockValue);
		}
		base.OnBlockStartsToFall(_world, _blockPos, _blockValue);
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockDestroyedBy (_, _, {_bvRef}, {_blockValue}, {_entityId}, {_bUseHarvestTool}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		DestroyedResult destroyedResult = DestroyedResult.None;
		if (TryGetParentBlockAndTileEntity(ref _bvRef, ref _blockValue, out var _te))
		{
			destroyedResult = _te.OnBlockDestroyedBy(_bvRef, _blockValue, _entityId, _bUseHarvestTool);
		}
		if (destroyedResult != DestroyedResult.None)
		{
			return destroyedResult;
		}
		return DestroyedResult.Downgrade;
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		DestroyedResult result = base.OnBlockDestroyedByExplosion(_world, _bvRef, _blockValue, _playerThatStartedExpl);
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockDestroyedByExplosion (_, _, {_bvRef}, {_blockValue}, {_playerThatStartedExpl}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		DestroyedResult destroyedResult = DestroyedResult.None;
		if (TryGetParentBlockAndTileEntity(ref _bvRef, ref _blockValue, out var _te))
		{
			destroyedResult = _te.OnBlockDestroyedByExplosion(_bvRef, _blockValue, _playerThatStartedExpl);
		}
		if (destroyedResult != DestroyedResult.None)
		{
			return destroyedResult;
		}
		return result;
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockDamaged (_, _, {_bvRef}, {_blockValue}, {_damagePoints}, {_entityIdThatDamaged}, _, {_bUseHarvestTool}, {_bBypassMaxDamage}, {_recDepth}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		return base.OnBlockDamaged(_world, _bvRef, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _bed)
	{
		if (_bed != null)
		{
			Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
			TileEntityComposite tileEntityComposite = _world.GetTileEntity(_blockPos) as TileEntityComposite;
			if (tileEntityComposite == null)
			{
				tileEntityComposite = new TileEntityComposite(chunk, _blockValue)
				{
					localChunkPos = World.toBlock(_blockPos)
				};
				chunk.AddTileEntity(tileEntityComposite);
			}
			tileEntityComposite.SetBlockEntityData(_bed);
			base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _bed);
		}
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockTriggered ({_player}, {_blockPos}, {_blockValue}, {_blockChanges.Count}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			_te.OnBlockTriggered(_player, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		}
		base.OnTriggered(_player, _world, _blockPos, _blockValue, _blockChanges, _triggeredBy);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetParentBlockAndTileEntity(ref BlockValueRef _bvRef, ref BlockValue _blockValue, out TileEntityComposite _te)
	{
		switch (_bvRef.Type)
		{
		case BlockValueRefType.None:
			_te = null;
			return false;
		case BlockValueRefType.Block:
		{
			Vector3i _blockPos = _bvRef.BlockPosition;
			bool result = TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out _te);
			_bvRef = new BlockValueRef(_blockPos);
			return result;
		}
		case BlockValueRefType.Prop:
			_te = null;
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetParentBlockAndTileEntity(ref Vector3i _blockPos, ref BlockValue _blockValue, out TileEntityComposite _te)
	{
		_te = null;
		World world = GameManager.Instance.World;
		if (isMultiBlock && _blockValue.ischild)
		{
			if (world.ChunkCache == null)
			{
				throw new Exception("ChunkCluster null in " + StackTraceUtility.ExtractStackTrace());
			}
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error($"Block on position {parentPos} with name '{block.Block.GetBlockName()}' should be a parent but is not! (6)");
				return false;
			}
			_blockPos = parentPos;
			_blockValue = block;
		}
		_te = world.GetTileEntity(_blockPos) as TileEntityComposite;
		return _te != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetTileEntity(Vector3i _blockPos, BlockValue _blockValue, out TileEntityComposite _te)
	{
		_te = null;
		World world = GameManager.Instance.World;
		if (isMultiBlock && _blockValue.ischild)
		{
			if (world.ChunkCache == null)
			{
				throw new Exception("ChunkCluster null in " + StackTraceUtility.ExtractStackTrace());
			}
			_blockPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			_blockValue = world.GetBlock(_blockPos);
			if (_blockValue.ischild)
			{
				Log.Error($"Block on position {_blockPos} with name '{_blockValue.Block.GetBlockName()}' should be a parent but is not! (6)");
				return false;
			}
		}
		_te = world.GetTileEntity(_blockPos) as TileEntityComposite;
		return _te != null;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.GetActivationText (_, {_blockValue}, _, {_blockPos}, {_entityFocusing}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (!TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			return "";
		}
		if (commands == null)
		{
			commands = _te.InitBlockActivationCommands();
		}
		if (commands.Length == 0)
		{
			return null;
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string activateHotkeyMarkup = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string focusedTileEntityName = _blockValue.Block.GetLocalizedBlockName();
		return _te.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, activateHotkeyMarkup, focusedTileEntityName);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.HasBlockActivationCommands (_, {_blockValue}, _, {_blockPos}, {_entityFocusing}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (!TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			return false;
		}
		if (commands == null)
		{
			commands = _te.InitBlockActivationCommands();
		}
		if (commands.Length == 0)
		{
			return false;
		}
		return _te.UpdateBlockActivationCommands(commands, _world, _blockPos, _blockValue, _entityFocusing);
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.GetBlockActivationCommands (_, {_blockValue}, _, {_blockPos}, {_entityFocusing}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (!TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			return BlockActivationCommand.Empty;
		}
		if (commands == null)
		{
			commands = _te.InitBlockActivationCommands();
		}
		if (commands.Length == 0)
		{
			return commands;
		}
		_te.UpdateBlockActivationCommands(commands, _world, _blockPos, _blockValue, _entityFocusing);
		return commands;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Out($"BlockComposite.OnBlockActivated ({_commandName}, _, _, {_blockPos}, {_blockValue}, {_player}) from {StackTraceUtility.ExtractStackTrace()}");
		}
		if (!TryGetParentBlockAndTileEntity(ref _blockPos, ref _blockValue, out var _te))
		{
			return false;
		}
		if (commands == null)
		{
			commands = _te.InitBlockActivationCommands();
		}
		return _te.OnBlockActivated(commands, _commandName, _world, _blockPos, _blockValue, _player);
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (TryGetTileEntity(_blockPos, _blockValue, out var _te) && _te.OverridesPhysicalChecks)
		{
			foreach (IFeaturePhysicalCapabilities overridesPhysicalChecksModule in _te.GetOverridesPhysicalChecksModules())
			{
				if (overridesPhysicalChecksModule.IsMovementBlocked(_blockPos, _blockValue, _face))
				{
					return true;
				}
			}
			return false;
		}
		return base.IsMovementBlocked(_world, _blockPos, _blockValue, _face);
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (TryGetTileEntity(_blockPos, _blockValue, out var _te) && _te.OverridesPhysicalChecks)
		{
			foreach (IFeaturePhysicalCapabilities overridesPhysicalChecksModule in _te.GetOverridesPhysicalChecksModules())
			{
				if (overridesPhysicalChecksModule.IsMovementBlocked(_blockPos, _blockValue, _sides))
				{
					return true;
				}
			}
			return false;
		}
		return base.IsMovementBlocked(_world, _blockPos, _blockValue, _sides);
	}

	public override bool IsSeeThrough(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TryGetTileEntity(_blockPos, _blockValue, out var _te) && _te.OverridesPhysicalChecks)
		{
			foreach (IFeaturePhysicalCapabilities overridesPhysicalChecksModule in _te.GetOverridesPhysicalChecksModules())
			{
				if (!overridesPhysicalChecksModule.IsSeeThrough(_blockPos, _blockValue))
				{
					return false;
				}
			}
			return true;
		}
		return base.IsSeeThrough(_world, _blockPos, _blockValue);
	}

	public override float GetStepHeight(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _crossingFace)
	{
		if (TryGetTileEntity(_blockPos, _blockValue, out var _te) && _te.OverridesPhysicalChecks)
		{
			float num = 0f;
			{
				foreach (IFeaturePhysicalCapabilities overridesPhysicalChecksModule in _te.GetOverridesPhysicalChecksModules())
				{
					num = Utils.FastMax(num, overridesPhysicalChecksModule.GetStepHeight(_blockPos, _blockValue, _crossingFace));
				}
				return num;
			}
		}
		return base.GetStepHeight(_world, _blockPos, _blockValue, _crossingFace);
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return CompositeData.HasFeature<IFeatureSavedInPrefab>();
	}
}
