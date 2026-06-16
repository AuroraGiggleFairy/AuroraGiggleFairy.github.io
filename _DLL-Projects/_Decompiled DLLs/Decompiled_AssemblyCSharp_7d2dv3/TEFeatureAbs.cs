using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class TEFeatureAbs : ITileEntityFeature, ITileEntity, ILockTarget
{
	public static bool DebugLogCTE;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TileEntityFeatureData FeatureData
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TileEntityComposite Parent
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public virtual LockTargetType LockTargetType => LockTargetType.TEFeature;

	public List<ITileEntityChangedListener> listeners => Parent.listeners;

	public BlockValue blockValue => Parent.blockValue;

	public virtual bool IsRemoving
	{
		get
		{
			return Parent.IsRemoving;
		}
		set
		{
			Parent.IsRemoving = value;
		}
	}

	public event XUiEvent_TileEntityDestroyed Destroyed
	{
		add
		{
			Parent.Destroyed += value;
		}
		remove
		{
			Parent.Destroyed -= value;
		}
	}

	public virtual void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Init (on " + GetType().Name + ")");
		}
		Parent = _parent;
		FeatureData = _featureData;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CopyFromInternal(TileEntityComposite _other);

	public void CopyFrom(TileEntityComposite _other)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.CopyFrom (on " + GetType().Name + ")");
		}
		CopyFromInternal(_other);
	}

	public virtual void OnRemove(World _world)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnRemove (on " + GetType().Name + ")");
		}
	}

	public virtual void OnLoad()
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnLoad (on " + GetType().Name + ")");
		}
	}

	public virtual void OnUnload(World _world)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnUnload (on " + GetType().Name + ")");
		}
	}

	public virtual void OnDestroy()
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnDestroy (on " + GetType().Name + ")");
		}
	}

	public virtual void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnAdded (on " + GetType().Name + ")");
		}
	}

	public virtual void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockValueChanged (on " + GetType().Name + ")");
		}
	}

	public virtual void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockReset (on " + GetType().Name + ")");
		}
	}

	public virtual void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockStartsToFall (on " + GetType().Name + ")");
		}
	}

	public virtual Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockDestroyedBy (on " + GetType().Name + ")");
		}
		return Block.DestroyedResult.None;
	}

	public virtual Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockDestroyedByExplosion (on " + GetType().Name + ")");
		}
		return Block.DestroyedResult.None;
	}

	public virtual void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.SetBlockEntityData (on " + GetType().Name + ")");
		}
	}

	public virtual void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.UpgradeDowngradeFrom (on " + GetType().Name + ")");
		}
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.ReplacedBy (on " + GetType().Name + ")");
		}
	}

	public virtual void Reset(FastTags<TagGroup.Global> _questTags)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Reset (on " + GetType().Name + ")");
		}
	}

	public virtual void UpdateTick(World _world)
	{
	}

	public virtual string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		return null;
	}

	public virtual void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.InitBlockActivationCommands (on " + GetType().Name + ")");
		}
		if (this is IFeatureTriggerCapability)
		{
			_addCallback(new BlockActivationCommand("trigger", "wrench", _enabled: false), TileEntityComposite.EBlockCommandOrder.Last, FeatureData);
		}
	}

	public virtual bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!Equals(_module))
		{
			return true;
		}
		if (CommandIs(_commandName, "trigger"))
		{
			if (_world.IsEditor())
			{
				return this is IFeatureTriggerCapability;
			}
			return false;
		}
		return true;
	}

	public virtual bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockActivated (on " + GetType().Name + "), command " + _commandName);
		}
		if (CommandIs(_commandName, "trigger"))
		{
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, _showTriggers: false, _showTriggeredBy: true);
			return true;
		}
		return false;
	}

	public virtual void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Read (on " + GetType().Name + ")");
		}
	}

	public virtual void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		if (DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Write (on " + GetType().Name + ")");
		}
	}

	public virtual bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public virtual bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
	}

	public void SetUserAccessing(bool _bUserAccessing)
	{
		Parent.SetUserAccessing(_bUserAccessing);
	}

	public bool IsUserAccessing()
	{
		return Parent.IsUserAccessing();
	}

	public void SetModified()
	{
		Parent.SetModified();
	}

	public Chunk GetChunk()
	{
		return Parent.GetChunk();
	}

	public Vector3i ToWorldPos()
	{
		return Parent.ToWorldPos();
	}

	public Vector3 ToWorldCenterPos()
	{
		return Parent.ToWorldCenterPos();
	}

	public LockTargetType GetLockTargetType()
	{
		return LockTargetType.TileEntity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CommandIs(ReadOnlySpan<char> _givenCommand, string _compareCommand)
	{
		return MemoryExtensions.Equals(_givenCommand, _compareCommand, StringComparison.Ordinal);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TEFeatureAbs()
	{
	}
}
