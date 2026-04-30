using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChunkProvider
{
	GameUtils.WorldInfo WorldInfo { get; }

	IEnumerator Init(World _worldData);

	void Update();

	void StopUpdate();

	void Cleanup();

	void RequestChunk(int _x, int _y);

	HashSetList<long> GetRequestedChunks();

	void SaveAll();

	void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet);

	void ReloadAllChunks();

	void ClearCaches();

	EnumChunkProviderId GetProviderId();

	void UnloadChunk(Chunk _chunk);

	DynamicPrefabDecorator GetDynamicPrefabDecorator();

	SpawnPointList GetSpawnPointList();

	void SetSpawnPointList(SpawnPointList _spawnPointList);

	bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors);

	void SetDecorationsEnabled(bool _bEnable);

	bool IsDecorationsEnabled();

	bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize);

	BoundsInt GetWorldBounds();

	Vector2i GetWorldSize();

	IBiomeProvider GetBiomeProvider();

	ITerrainGenerator GetTerrainGenerator();

	int GetPOIBlockIdOverride(int x, int z);

	float GetPOIHeightOverride(int x, int z);

	IEnumerator FillOccupiedMap(int xStart, int zStart, DecoOccupiedMap occupiedMap, List<PrefabInstance> overridePOIList = null);

	void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _isStopStabilityCalc, bool _isRegenChunk, bool _isFillEmptyBlocks, bool _isReset);

	ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos);
}
