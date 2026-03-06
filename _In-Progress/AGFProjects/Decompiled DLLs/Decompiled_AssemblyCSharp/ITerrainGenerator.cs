using UnityEngine;

public interface ITerrainGenerator
{
	void Init(World _world, IBiomeProvider _biomeProvider, int _seed);

	void GenerateTerrain(World worldData, Chunk chunk, GameRandom _random);

	void GenerateTerrain(World worldData, Chunk chunk, GameRandom _random, Vector3i _areaStart, Vector3i _areaSize, bool _bFillEmptyBlocks, bool _isReset);

	byte GetTerrainHeightByteAt(int _xWorld, int _zWorld);

	sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity);

	Vector3 GetTerrainNormalAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity);

	float GetTerrainHeightAt(int _xWorld, int _zWorld);
}
