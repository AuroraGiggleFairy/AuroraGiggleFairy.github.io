using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockCactus : BlockDamage
{
	public override void Init()
	{
		base.Init();
		IsTerrainDecoration = true;
		CanDecorateOnSlopes = false;
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		base.GetCollisionAABB(_blockValue, _x, _y, _z, _distortedY, _result);
		Vector3 vector = new Vector3(0.15f, 0.05f, 0.15f);
		Block block = _blockValue.Block;
		if (block.isMultiBlock && block.multiBlockPos.dim.y == 1)
		{
			vector = new Vector3(0.15f, -0.75f, 0.15f);
		}
		for (int i = 0; i < _result.Count; i++)
		{
			Bounds value = _result[i];
			value.SetMinMax(value.min - vector, value.max + vector);
			_result[i] = value;
		}
	}
}
