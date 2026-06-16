using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeDistantDecoTree : BlockShapeDistantDeco
{
	public BlockShapeDistantDecoTree()
	{
		Has45DegreeRotations = true;
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
			gameObject.layer = 23;
		}
		Transform transform = _ebcd.transform.Find("rootBall");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!DecoManager.Instance.IsEnabled)
		{
			base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		}
		else
		{
			DecoManager.Instance.RemoveDecorationAt(_blockPos);
		}
	}
}
