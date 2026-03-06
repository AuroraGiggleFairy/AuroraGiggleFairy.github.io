using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class TEFeatureAbs : ITileEntityFeature, ITileEntity
{
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

	public List<ITileEntityChangedListener> listeners => Parent.listeners;

	public BlockValue blockValue => Parent.blockValue;

	public virtual int EntityId
	{
		get
		{
			return Parent.EntityId;
		}
		set
		{
		}
	}

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
		Parent = _parent;
		FeatureData = _featureData;
	}

	public virtual void CopyFrom(TileEntityComposite _other)
	{
		throw new NotImplementedException();
	}

	public void OnRemove(World _world)
	{
	}

	public virtual void OnUnload(World _world)
	{
	}

	public virtual void OnDestroy()
	{
	}

	public virtual void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
	{
	}

	public virtual void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
	}

	public virtual void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
	}

	public virtual void Reset(FastTags<TagGroup.Global> _questTags)
	{
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
	}

	public virtual void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
	}

	public virtual bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		return false;
	}

	public virtual void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
	}

	public virtual void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
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

	public int GetClrIdx()
	{
		return Parent.GetClrIdx();
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
