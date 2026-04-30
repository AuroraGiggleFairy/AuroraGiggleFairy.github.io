using UnityEngine;

public class WorldConstants
{
	public const int ChunkBlockXPow = 4;

	public const int ChunkBlockYPow = 8;

	public const int ChunkBlockZPow = 4;

	public const int ChunkBlockLayerHeight = 4;

	public const int ChunkBlockLayerHeightPow = 2;

	public const int ChunkBlockLayerHeightMask = 3;

	public const int ChunkBlockLayers = 64;

	public const int ChunkBlockXDim = 16;

	public const int ChunkBlockYDim = 256;

	public const int ChunkBlockZDim = 16;

	public const int ChunkBlockXDimM1 = 15;

	public const int ChunkBlockYDimM1 = 255;

	public const int ChunkBlockZDimM1 = 15;

	public const int ChunkAreaDim = 256;

	public const int ChunkVolumeDim = 65536;

	public const int ChunkBlockXMask = 15;

	public const int ChunkBlockYMask = 255;

	public const int ChunkBlockZMask = 15;

	public const int ChunkMeshLayerHeight = 16;

	public const int ChunkMeshLayerShift = 4;

	public const int ChunkMeshLayerHeightMask = 65535;

	public const int ChunkDensityXPow = 4;

	public const int ChunkDensityYPow = 8;

	public const int ChunkDensityZPow = 4;

	public const int ChunkDensityXDim = 16;

	public const int ChunkDensityYDim = 256;

	public const int ChunkDensityZDim = 16;

	public const int ChunkDensityXMask = 15;

	public const int ChunkDensityYMask = 255;

	public const int ChunkDensityZMask = 15;

	public const int NumberOfLightShades = 16;

	public const int MaxDarkness = 16;

	public const int cTimePerHour = 1000;

	public const int cTimePerDay = 24000;

	public const int cDuskHour = 22;

	public static float WaterLevel = Block.cWaterLevel;

	public static Rect uvRectZero = new Rect(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Rect[] rectCracks = new Rect[10]
	{
		new Rect(0f, 0f, 0.1f, 1f),
		new Rect(0.1f, 0f, 0.1f, 1f),
		new Rect(0.2f, 0f, 0.1f, 1f),
		new Rect(0.3f, 0f, 0.1f, 1f),
		new Rect(0.4f, 0f, 0.1f, 1f),
		new Rect(0.5f, 0f, 0.1f, 1f),
		new Rect(0.6f, 0f, 0.1f, 1f),
		new Rect(0.7f, 0f, 0.1f, 1f),
		new Rect(0.8f, 0f, 0.1f, 1f),
		new Rect(0.9f, 0f, 0.1f, 1f)
	};

	public static Rect MapBlockToUVRect(int _meshIndex, BlockValue _blockValue, BlockFace _blockFace)
	{
		int sideTextureId = _blockValue.Block.GetSideTextureId(_blockValue, _blockFace, 0);
		if (sideTextureId >= 0 && sideTextureId < MeshDescription.meshes[_meshIndex].textureAtlas.uvMapping.Length)
		{
			return MeshDescription.meshes[_meshIndex].textureAtlas.uvMapping[sideTextureId].uv;
		}
		return uvRectZero;
	}

	public static Rect MapDamageToUVRect(BlockValue _blockValue)
	{
		if (_blockValue.hasdecal || _blockValue.ischild || _blockValue.damage == 0)
		{
			return uvRectZero;
		}
		return rectCracks[Mathf.Min((int)((float)_blockValue.damage * 10f / (float)_blockValue.Block.MaxDamage), rectCracks.Length - 1)];
	}
}
