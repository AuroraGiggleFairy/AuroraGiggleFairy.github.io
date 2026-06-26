using UnityEngine;

public interface IChunk
{
	int X { get; set; }

	int Y { get; }

	int Z { get; set; }

	Vector3i ChunkPos { get; set; }

	bool GetAvailable();

	BlockValue GetBlock(int _x, int _y, int _z);

	BlockValue GetBlockNoDamage(int _x, int _y, int _z);

	bool IsOnlyTerrain(int _y);

	bool IsOnlyTerrainLayer(int _idx);

	bool IsEmpty();

	bool IsEmpty(int _y);

	bool IsEmptyLayer(int _idx);

	byte GetStability(int _x, int _y, int _z);

	void SetStability(int _x, int _y, int _z, byte _v);

	byte GetLight(int x, int y, int z, Chunk.LIGHT_TYPE type);

	int GetLightValue(int x, int y, int z, int _darknessV);

	float GetLightBrightness(int x, int y, int z, int _ss);

	Vector3i GetWorldPos();

	byte GetHeight(int _blockOffset);

	byte GetHeight(int _x, int _z);

	sbyte GetDensity(int _x, int _y, int _z);

	bool HasSameDensityValue(int _y);

	sbyte GetSameDensityValue(int _y);

	int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _blockFace);

	long GetTextureFull(int _x, int _y, int _z);

	BlockEntityData GetBlockEntity(Vector3i _blockPos);

	BlockEntityData GetBlockEntity(Transform _transform);

	void SetTopSoilBroken(int _x, int _z);

	bool IsTopSoil(int _x, int _z);

	byte GetTerrainHeight(int _x, int _z);

	WaterValue GetWater(int _x, int _y, int _z);

	bool IsWater(int _x, int _y, int _z);

	bool IsAir(int _x, int _y, int _z);
}
