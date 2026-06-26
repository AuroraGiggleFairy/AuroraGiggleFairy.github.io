using UnityEngine;

public class MarchingCubes : IMarchingCubes
{
	public static readonly sbyte DensityAir = sbyte.MaxValue;

	public static readonly sbyte DensityAirHi = 100;

	public static readonly sbyte DensityTerrain = sbyte.MinValue;

	public static readonly sbyte DensityTerrainHi = -100;

	public void Polygonize(INeighborBlockCache _nBlocks, Vector3i _localPos, Vector3 _offsetPos, byte _sunLight, byte _blockLight, VoxelMesh _mesh)
	{
	}

	public static float GetDecorationOffsetY(sbyte _densY, sbyte _densYm1)
	{
		float num = _densY + _densYm1;
		return Utils.FastClamp(-0.0035f * num, -0.4f, 0.4f);
	}
}
