using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeDistantDeco : BlockShapeModelEntity
{
	public override void Init(Block _block)
	{
		base.Init(_block);
	}

	public override void OnBlockValueChanged(WorldBase _world, Vector3i _blockPos, int _clrIdx, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockValueChanged(_world, _blockPos, _clrIdx, _oldBlockValue, _newBlockValue);
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		}
		else if (_blockValue.Block.IsDistantDecoration)
		{
			DecoManager.Instance.AddDecorationAt((World)_world, _blockValue, _blockPos, _bForceBlockYPos: true);
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		}
		DecoManager.Instance.RemoveDecorationAt(_blockPos);
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		}
		else if (_world.IsRemote() && _blockValue.Block.IsDistantDecoration)
		{
			DecoManager.Instance.AddDecorationAt((World)_world, _blockValue, _blockPos);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
			return;
		}
		_ebcd.transform.tag = "T_Deco";
		Collider[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			gameObject.tag = "T_Deco";
			gameObject.layer = 16;
		}
	}
}
