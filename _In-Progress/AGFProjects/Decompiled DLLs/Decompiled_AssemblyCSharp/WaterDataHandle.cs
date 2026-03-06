using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public struct WaterDataHandle : IDisposable
{
	public UnsafeChunkData<int> voxelData;

	public UnsafeChunkData<WaterVoxelState> voxelState;

	public UnsafeChunkXZMap<GroundWaterBounds> groundWaterHeights;

	public UnsafeBitArray activeVoxels;

	public UnsafeParallelHashMap<int, int> flowVoxels;

	public UnsafeFixedBuffer<WaterFlow> flowsFromOtherChunks;

	public UnsafeFixedBuffer<int> activationsFromOtherChunks;

	public UnsafeParallelHashSet<int> voxelsToWakeup;

	public UnsafeBitArraySetIndicesEnumerator ActiveVoxelIndices => new UnsafeBitArraySetIndicesEnumerator(activeVoxels);

	public UnsafeParallelHashMap<int, int>.Enumerator FlowVoxels => flowVoxels.GetEnumerator();

	public bool HasActiveWater => activeVoxels.TestAny(0, activeVoxels.Length);

	public bool HasFlows => !flowVoxels.IsEmpty;

	public static WaterDataHandle AllocateNew(Allocator allocator)
	{
		WaterDataHandle result = default(WaterDataHandle);
		result.Allocate(allocator);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Allocate(Allocator allocator)
	{
		voxelData = new UnsafeChunkData<int>(allocator);
		voxelState = new UnsafeChunkData<WaterVoxelState>(allocator);
		groundWaterHeights = new UnsafeChunkXZMap<GroundWaterBounds>(allocator);
		activeVoxels = new UnsafeBitArray(65536, allocator);
		flowVoxels = new UnsafeParallelHashMap<int, int>(1000, allocator);
		flowsFromOtherChunks = new UnsafeFixedBuffer<WaterFlow>(16384, allocator);
		activationsFromOtherChunks = new UnsafeFixedBuffer<int>(16384, allocator);
		voxelsToWakeup = new UnsafeParallelHashSet<int>(256, allocator);
	}

	public bool IsInGroundWater(int _x, int _y, int _z)
	{
		GroundWaterBounds groundWaterBounds = groundWaterHeights.Get(_x, _z);
		if (groundWaterBounds.IsGroundWater)
		{
			if (_y >= groundWaterBounds.bottom)
			{
				return _y <= groundWaterBounds.waterHeight;
			}
			return false;
		}
		return false;
	}

	public void SetVoxelActive(int _x, int _y, int _z)
	{
		activeVoxels.Set(GetVoxelIndex(_x, _y, _z), value: true);
	}

	public void SetVoxelActive(int _index)
	{
		activeVoxels.Set(_index, value: true);
	}

	public void EnqueueVoxelActive(int _x, int _y, int _z)
	{
		EnqueueVoxelActive(GetVoxelIndex(_x, _y, _z));
	}

	public void EnqueueVoxelActive(int _index)
	{
		activationsFromOtherChunks.AddThreadSafe(_index);
	}

	public void EnqueueVoxelWakeup(int _x, int _y, int _z)
	{
		EnqueueVoxelWakeup(GetVoxelIndex(_x, _y, _z));
	}

	public void EnqueueVoxelWakeup(int _index)
	{
		voxelsToWakeup.Add(_index);
	}

	public void ApplyEnqueuedActivations()
	{
		NativeArray<int> nativeArray = activationsFromOtherChunks.AsNativeArray();
		for (int i = 0; i < nativeArray.Length; i++)
		{
			int voxelActive = nativeArray[i];
			SetVoxelActive(voxelActive);
		}
		activationsFromOtherChunks.Clear();
	}

	public void SetVoxelInactive(int _index)
	{
		activeVoxels.Set(_index, value: false);
	}

	public void SetVoxelMass(int _x, int _y, int _z, int _mass)
	{
		int voxelIndex = GetVoxelIndex(_x, _y, _z);
		SetVoxelMass(voxelIndex, _mass);
	}

	public void SetVoxelMass(int _index, int _mass)
	{
		if (_mass > 195)
		{
			activeVoxels.Set(_index, value: true);
		}
		else
		{
			activeVoxels.Set(_index, value: false);
		}
		voxelData.Set(_index, _mass);
	}

	public void SetVoxelSolid(int _x, int _y, int _z, BlockFaceFlag _flags)
	{
		int voxelIndex = GetVoxelIndex(_x, _y, _z);
		WaterVoxelState waterVoxelState = voxelState.Get(voxelIndex);
		WaterVoxelState value = default(WaterVoxelState);
		value.SetSolid(_flags);
		voxelState.Set(voxelIndex, value);
		GroundWaterBounds value2 = groundWaterHeights.Get(_x, _z);
		if (!value2.IsGroundWater)
		{
			return;
		}
		if (waterVoxelState.IsSolidYNeg() && !value.IsSolidYNeg() && _y == value2.bottom)
		{
			value2.bottom = (byte)FindGroundWaterBottom(voxelIndex);
			groundWaterHeights.Set(_x, _z, value2);
		}
		else if (waterVoxelState.IsSolidYPos() && !value.IsSolidYPos() && _y + 1 == value2.bottom)
		{
			value2.bottom = (byte)FindGroundWaterBottom(voxelIndex);
			groundWaterHeights.Set(_x, _z, value2);
		}
		else if (!waterVoxelState.IsSolidYNeg() && value.IsSolidYNeg() && _y > value2.bottom && _y <= value2.waterHeight)
		{
			value2.bottom = (byte)_y;
			groundWaterHeights.Set(_x, _z, value2);
		}
		else if (!waterVoxelState.IsSolidYPos() && value.IsSolidYPos())
		{
			int num = _y + 1;
			if (num > value2.bottom && num <= value2.waterHeight)
			{
				value2.bottom = (byte)num;
				groundWaterHeights.Set(_x, _z, value2);
			}
		}
	}

	public void ApplyFlow(int _x, int _y, int _z, int _flow)
	{
		ApplyFlow(GetVoxelIndex(_x, _y, _z), _flow);
	}

	public void ApplyFlow(int _index, int _flow)
	{
		if (flowVoxels.TryGetValue(_index, out var item))
		{
			_flow += item;
		}
		flowVoxels[_index] = _flow;
	}

	public void EnqueueFlow(int _voxelIndex, int _flow)
	{
		flowsFromOtherChunks.AddThreadSafe(new WaterFlow
		{
			voxelIndex = _voxelIndex,
			flow = _flow
		});
	}

	public void ApplyEnqueuedFlows()
	{
		NativeArray<WaterFlow> nativeArray = flowsFromOtherChunks.AsNativeArray();
		for (int i = 0; i < nativeArray.Length; i++)
		{
			WaterFlow waterFlow = nativeArray[i];
			ApplyFlow(waterFlow.voxelIndex, waterFlow.flow);
		}
		flowsFromOtherChunks.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindGroundWaterBottom(int _fromIndex)
	{
		for (int num = _fromIndex; num >= 0; num -= 256)
		{
			WaterVoxelState waterVoxelState = voxelState.Get(num);
			if (waterVoxelState.IsSolidYNeg())
			{
				return GetVoxelY(num);
			}
			if (waterVoxelState.IsSolidYPos())
			{
				int num2 = math.min(num + 256, 255);
				if (num2 <= _fromIndex)
				{
					return GetVoxelY(num2);
				}
			}
		}
		return 0;
	}

	public void InitializeFromChunk(Chunk _chunk, GroundWaterHeightMap _groundWaterHeightMap)
	{
		if (!voxelData.IsCreated || !activeVoxels.IsCreated)
		{
			Debug.LogError("Could not initialize WaterDataHandle because it has not been allocated");
			return;
		}
		Clear();
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					WaterVoxelState value = default(WaterVoxelState);
					BlockValue blockNoDamage = _chunk.GetBlockNoDamage(k, i, j);
					Block block = blockNoDamage.Block;
					byte rotation = blockNoDamage.rotation;
					value.SetSolid(BlockFaceFlags.RotateFlags(block.WaterFlowMask, rotation));
					int voxelIndex = GetVoxelIndex(k, i, j);
					int mass = _chunk.GetWater(k, i, j).GetMass();
					if (mass > 195)
					{
						activeVoxels.Set(voxelIndex, value: true);
						voxelData.Set(voxelIndex, mass);
					}
					if (!value.IsDefault())
					{
						voxelState.Set(voxelIndex, value);
					}
				}
			}
		}
		voxelState.CheckSameValues();
		if (!_groundWaterHeightMap.TryInit())
		{
			return;
		}
		for (int l = 0; l < 16; l++)
		{
			for (int m = 0; m < 16; m++)
			{
				Vector3i vector3i = _chunk.ToWorldPos(m, 0, l);
				if (_groundWaterHeightMap.TryGetWaterHeightAt(vector3i.x, vector3i.z, out var _height))
				{
					int groundHeight = FindGroundWaterBottom(GetVoxelIndex(m, _height, l));
					groundWaterHeights.Set(m, l, new GroundWaterBounds(groundHeight, _height));
				}
			}
		}
	}

	public void Clear()
	{
		if (voxelData.IsCreated)
		{
			voxelData.Clear();
		}
		if (voxelState.IsCreated)
		{
			voxelState.Clear();
		}
		if (groundWaterHeights.IsCreated)
		{
			groundWaterHeights.Clear();
		}
		if (activeVoxels.IsCreated)
		{
			activeVoxels.Clear();
		}
		if (flowVoxels.IsCreated)
		{
			flowVoxels.Clear();
		}
		if (flowsFromOtherChunks.IsCreated)
		{
			flowsFromOtherChunks.Clear();
		}
		if (activationsFromOtherChunks.IsCreated)
		{
			activationsFromOtherChunks.Clear();
		}
		if (voxelsToWakeup.IsCreated)
		{
			voxelsToWakeup.Clear();
		}
	}

	public void Dispose()
	{
		if (voxelData.IsCreated)
		{
			voxelData.Dispose();
		}
		if (voxelState.IsCreated)
		{
			voxelState.Dispose();
		}
		if (groundWaterHeights.IsCreated)
		{
			groundWaterHeights.Dispose();
		}
		if (activeVoxels.IsCreated)
		{
			activeVoxels.Dispose();
		}
		if (flowVoxels.IsCreated)
		{
			flowVoxels.Dispose();
		}
		if (flowsFromOtherChunks.IsCreated)
		{
			flowsFromOtherChunks.Dispose();
		}
		if (activationsFromOtherChunks.IsCreated)
		{
			activationsFromOtherChunks.Dispose();
		}
		if (voxelsToWakeup.IsCreated)
		{
			voxelsToWakeup.Dispose();
		}
	}

	public int CalculateOwnedBytes()
	{
		return 0 + voxelData.CalculateOwnedBytes() + voxelState.CalculateOwnedBytes() + groundWaterHeights.CalculateOwnedBytes() + ProfilerUtils.CalculateUnsafeBitArrayBytes(activeVoxels) + ProfilerUtils.CalculateUnsafeParallelHashMapBytes(flowVoxels) + flowsFromOtherChunks.CalculateOwnedBytes() + activationsFromOtherChunks.CalculateOwnedBytes() + ProfilerUtils.CalculateUnsafeParallelHashSetBytes(voxelsToWakeup);
	}

	public string GetMemoryStats()
	{
		return $"voxelData: {(double)voxelData.CalculateOwnedBytes() * 0.0009765625:F2} KB, voxelState: {(double)voxelState.CalculateOwnedBytes() * 0.0009765625:F2} KB, groundWaterHeights: {(double)groundWaterHeights.CalculateOwnedBytes() * 0.0009765625:F2} KB, activeVoxels: ({(double)ProfilerUtils.CalculateUnsafeBitArrayBytes(activeVoxels) * 0.0009765625:F2} KB), flowVoxels: ({flowVoxels.Count()},{flowVoxels.Capacity},{(double)ProfilerUtils.CalculateUnsafeParallelHashMapBytes(flowVoxels) * 0.0009765625:F2} KB), flowsFromOtherChunks: {(double)flowsFromOtherChunks.CalculateOwnedBytes() * 0.0009765625:F2} KB, activationsFromOtherChunks: {(double)activationsFromOtherChunks.CalculateOwnedBytes() * 0.0009765625:F2} KB, voxelsToWakeup {(double)ProfilerUtils.CalculateUnsafeParallelHashSetBytes(voxelsToWakeup) * 0.0009765625:F2} KB, Total: {(double)CalculateOwnedBytes() * 9.5367431640625E-07:F2} MB";
	}

	public static int GetVoxelIndex(int _x, int _y, int _z)
	{
		return _x + _y * 256 + _z * 16;
	}

	public static int3 GetVoxelCoords(int index)
	{
		int3 result = new int3
		{
			y = index / 256
		};
		int num = index % 256;
		result.z = num / 16;
		result.x = num % 16;
		return result;
	}

	public static int GetVoxelY(int _index)
	{
		return _index / 256;
	}
}
