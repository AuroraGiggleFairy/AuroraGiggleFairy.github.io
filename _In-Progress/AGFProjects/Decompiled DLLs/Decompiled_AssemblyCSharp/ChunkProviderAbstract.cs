using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkProviderAbstract : IChunkProvider
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDecorationsEnabled;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameUtils.WorldInfo WorldInfo
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public virtual IEnumerator Init(World _worldData)
	{
		yield return null;
	}

	public virtual void Update()
	{
	}

	public virtual void StopUpdate()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void RequestChunk(int _x, int _y)
	{
	}

	public virtual HashSetList<long> GetRequestedChunks()
	{
		return null;
	}

	public virtual void SaveAll()
	{
	}

	public virtual void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet)
	{
	}

	public virtual void ReloadAllChunks()
	{
	}

	public virtual void ClearCaches()
	{
	}

	public virtual EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.None;
	}

	public virtual void UnloadChunk(Chunk _chunk)
	{
	}

	public virtual DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return null;
	}

	public virtual SpawnPointList GetSpawnPointList()
	{
		return null;
	}

	public virtual void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
	}

	public virtual bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		return false;
	}

	public virtual void SetDecorationsEnabled(bool _bEnable)
	{
		bDecorationsEnabled = _bEnable;
	}

	public virtual bool IsDecorationsEnabled()
	{
		return bDecorationsEnabled;
	}

	public virtual bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = Vector3i.zero;
		_maxSize = Vector3i.zero;
		return false;
	}

	public virtual BoundsInt GetWorldBounds()
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		return new BoundsInt(_minSize.x, _minSize.y, _minSize.z, _maxSize.x, _maxSize.y, _maxSize.z);
	}

	public virtual Vector2i GetWorldSize()
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		return new Vector2i(_maxSize.x - _minSize.x + 1, _maxSize.y - _minSize.y + 1);
	}

	public virtual IBiomeProvider GetBiomeProvider()
	{
		return null;
	}

	public virtual ITerrainGenerator GetTerrainGenerator()
	{
		return null;
	}

	public virtual int GetPOIBlockIdOverride(int x, int z)
	{
		return 0;
	}

	public virtual float GetPOIHeightOverride(int x, int z)
	{
		return 0f;
	}

	public virtual IEnumerator FillOccupiedMap(int w, int h, DecoOccupiedMap occupiedMap, List<PrefabInstance> overridePOIList = null)
	{
		yield break;
	}

	public virtual void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _isStopStabilityCalc, bool _isRegenChunk, bool _isFillEmptyBlocks, bool _isReset)
	{
	}

	public virtual ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return ChunkProtectionLevel.None;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkProviderAbstract()
	{
	}
}
