using UnityEngine.Scripting;

[Preserve]
public class BlockTorchHeatMap : BlockTorch
{
	public BlockTorchHeatMap()
	{
		IsRandomlyTick = true;
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		if (HeatMapStrength > 0f)
		{
			AIDirector aIDirector = _world.GetAIDirector();
			if (aIDirector != null)
			{
				float num = 1f;
				num *= 0.4f;
				aIDirector.NotifyActivity(EnumAIDirectorChunkEvent.Torch, _blockPos, HeatMapStrength * num);
			}
		}
		return true;
	}
}
