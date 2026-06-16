using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationCalcFlows : IJobParallelFor
{
	public NativeArray<ChunkKey> processingChunks;

	public NativeArray<WaterStats> waterStats;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;

	public void Execute(int chunkIndex)
	{
		ChunkKey chunkKey = processingChunks[chunkIndex];
		stats = waterStats[chunkIndex];
		neighborCache = WaterNeighborCacheNative.InitializeCache(waterDataHandles);
		ProcessFlows(chunkKey);
		waterStats[chunkIndex] = stats;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessFlows(ChunkKey chunkKey)
	{
		if (!waterDataHandles.TryGetValue(chunkKey, out var item))
		{
			return;
		}
		stats.NumChunksProcessed++;
		if (!item.HasActiveWater)
		{
			return;
		}
		stats.NumChunksActive++;
		neighborCache.SetChunk(chunkKey);
		UnsafeBitArraySetIndicesEnumerator activeVoxelIndices = item.ActiveVoxelIndices;
		while (activeVoxelIndices.MoveNext())
		{
			stats.NumVoxelsProcessed++;
			int current = activeVoxelIndices.Current;
			int3 voxelCoords = WaterDataHandle.GetVoxelCoords(current);
			int num = item.voxelData.Get(current);
			neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
			if (item.IsInGroundWater(voxelCoords.x, voxelCoords.y, voxelCoords.z))
			{
				WaterVoxelState fromVoxelState = item.voxelState.Get(voxelCoords.x, voxelCoords.y, voxelCoords.z);
				if (fromVoxelState.IsSolid())
				{
					item.SetVoxelInactive(current);
					stats.NumVoxelsPutToSleep++;
				}
				else if (num != 19500)
				{
					item.ApplyFlow(current, 19500);
				}
				else if (num > 195 && ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num, WaterNeighborCacheNative.X_NEG) + ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num, WaterNeighborCacheNative.X_POS) + ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num, WaterNeighborCacheNative.Z_NEG) + ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num, WaterNeighborCacheNative.Z_POS) < 195)
				{
					item.SetVoxelInactive(current);
					stats.NumVoxelsPutToSleep++;
				}
				continue;
			}
			int num2 = num;
			if (num2 < 195)
			{
				item.SetVoxelInactive(current);
				stats.NumVoxelsPutToSleep++;
				continue;
			}
			int num3 = num2;
			num2 -= ProcessFlowBelow(item, current, voxelCoords.x, voxelCoords.y, voxelCoords.z, num2);
			if (num2 > 0)
			{
				num2 -= ProcessOverfull(item, current, voxelCoords.x, voxelCoords.y, voxelCoords.z, num2);
				int num4 = ProcessFlowSide(chunkKey, item, current, num2, WaterNeighborCacheNative.X_NEG);
				num4 += ProcessFlowSide(chunkKey, item, current, num2, WaterNeighborCacheNative.X_POS);
				num4 += ProcessFlowSide(chunkKey, item, current, num2, WaterNeighborCacheNative.Z_NEG);
				num4 += ProcessFlowSide(chunkKey, item, current, num2, WaterNeighborCacheNative.Z_POS);
				num2 -= num4;
				if (num2 > 0 && num3 - num2 < 195)
				{
					item.SetVoxelInactive(current);
					stats.NumVoxelsPutToSleep++;
				}
			}
		}
		activeVoxelIndices.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowBelow(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		int num = _y - 1;
		if (num < 0)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYNeg() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYPos())
		{
			return 0;
		}
		if (_chunkData.IsInGroundWater(_x, num, _z))
		{
			_chunkData.ApplyFlow(_voxelIndex, -_mass);
			stats.NumFlowEvents++;
			return _mass;
		}
		if (!TryGetMass(_chunkData, _x, num, _z, out var _mass2))
		{
			return 0;
		}
		int num2 = WaterConstants.GetStableMassBelow(_mass, _mass2) - _mass2;
		if (num2 > 0)
		{
			num2 = (int)((float)num2 * 0.5f);
			num2 = math.clamp(num2, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num2);
			_chunkData.ApplyFlow(_x, num, _z, num2);
			stats.NumFlowEvents++;
			return num2;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessOverfull(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		if (_mass < 19500)
		{
			return 0;
		}
		int num = _y + 1;
		if (num > 255)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYPos() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYNeg())
		{
			return 0;
		}
		if (!TryGetMass(_chunkData, _x, num, _z, out var _mass2))
		{
			return 0;
		}
		int num2 = math.min(_mass - 19500, 58500 - _mass2);
		if (num2 > 195)
		{
			num2 = math.clamp(num2, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num2);
			_chunkData.ApplyFlow(_x, num, _z, num2);
			stats.NumFlowEvents++;
			return num2;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowSide(ChunkKey _chunkKey, WaterDataHandle _chunkData, int _voxelIndex, int _mass, int2 _xzOffset)
	{
		WaterVoxelState waterVoxelState = _chunkData.voxelState.Get(_voxelIndex);
		if (waterVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		if (neighborCache.TryGetNeighbor(_xzOffset, out var _chunkKey2, out var _dataHandle, out var _x, out var _y, out var _z))
		{
			WaterVoxelState waterVoxelState2 = _dataHandle.voxelState.Get(_x, _y, _z);
			if (waterVoxelState2.IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			if (!TryGetMass(_dataHandle, _x, _y, _z, out var _mass2))
			{
				return 0;
			}
			int num = _y - 1;
			bool flag = true;
			bool flag2 = true;
			if (num >= 0)
			{
				flag = _chunkData.voxelState.Get(neighborCache.voxelX, num, neighborCache.voxelZ).IsSolidYPos() || waterVoxelState.IsSolidYNeg();
				flag2 = _dataHandle.voxelState.Get(_x, num, _z).IsSolidYPos() || waterVoxelState2.IsSolidYNeg();
			}
			int num2 = 195;
			if (flag == flag2)
			{
				if (_mass <= 4875)
				{
					return 0;
				}
			}
			else
			{
				num2 = 0;
			}
			int x = (int)((float)(_mass - _mass2) * 0.5f);
			x = math.clamp(x, 0, (int)((float)_mass * 0.25f));
			if (x > num2)
			{
				x = (int)((float)x * 0.5f);
				x = math.clamp(x, 1, _mass);
				_chunkData.ApplyFlow(_voxelIndex, -x);
				if (_chunkKey.Equals(_chunkKey2))
				{
					_dataHandle.ApplyFlow(_x, _y, _z, x);
				}
				else
				{
					_dataHandle.EnqueueFlow(WaterDataHandle.GetVoxelIndex(_x, _y, _z), x);
				}
				stats.NumFlowEvents++;
				return x;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessGroundWaterFlowSide(ChunkKey _chunkKey, WaterVoxelState _fromVoxelState, int _mass, int2 _xzOffset)
	{
		if (_fromVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		if (neighborCache.TryGetNeighbor(_xzOffset, out var _chunkKey2, out var _dataHandle, out var _x, out var _y, out var _z))
		{
			if (_dataHandle.voxelState.Get(_x, _y, _z).IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			if (!TryGetMass(_dataHandle, _x, _y, _z, out var _mass2))
			{
				return 0;
			}
			int num = math.max(19500 - _mass2, 0);
			int num2 = math.min(_mass, (int)((float)num * 0.25f));
			if (num2 > 195)
			{
				num2 = (int)((float)num2 * 0.5f);
				num2 = math.clamp(num2, 1, _mass);
				if (_chunkKey.Equals(_chunkKey2))
				{
					_dataHandle.ApplyFlow(_x, _y, _z, num2);
				}
				else
				{
					_dataHandle.EnqueueFlow(WaterDataHandle.GetVoxelIndex(_x, _y, _z), num2);
				}
				stats.NumFlowEvents++;
				return num2;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMass(WaterDataHandle _chunkData, int _x, int _y, int _z, out int _mass)
	{
		if (_chunkData.IsInGroundWater(_x, _y, _z))
		{
			_mass = 19500;
			return false;
		}
		int num = _chunkData.voxelData.Get(_x, _y, _z);
		if (num > 195)
		{
			_mass = num;
			return true;
		}
		_mass = 0;
		return !_chunkData.voxelState.Get(_x, _y, _z).IsSolid();
	}
}
