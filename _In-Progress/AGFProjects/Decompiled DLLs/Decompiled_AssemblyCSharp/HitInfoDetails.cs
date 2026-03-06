using System;
using UnityEngine;

public struct HitInfoDetails
{
	public struct VoxelData : IEquatable<VoxelData>
	{
		public BlockValue BlockValue;

		public WaterValue WaterValue;

		public void Set(BlockValue _bv, WaterValue _wv)
		{
			BlockValue = _bv;
			WaterValue = _wv;
		}

		public static VoxelData GetFrom(ChunkCluster _cc, Vector3i _blockPos)
		{
			return new VoxelData
			{
				BlockValue = _cc.GetBlock(_blockPos),
				WaterValue = _cc.GetWater(_blockPos)
			};
		}

		public static VoxelData GetFrom(World _world, Vector3i _blockPos)
		{
			return new VoxelData
			{
				BlockValue = _world.GetBlock(_blockPos),
				WaterValue = _world.GetWater(_blockPos)
			};
		}

		public static VoxelData GetFrom(IChunk _chunk, int _x, int _y, int _z)
		{
			return new VoxelData
			{
				BlockValue = _chunk.GetBlock(_x, _y, _z),
				WaterValue = _chunk.GetWater(_x, _y, _z)
			};
		}

		public bool IsOnlyAir()
		{
			if (BlockValue.isair)
			{
				return !WaterValue.HasMass();
			}
			return false;
		}

		public bool IsOnlyWater()
		{
			if (BlockValue.isair)
			{
				return WaterValue.HasMass();
			}
			return false;
		}

		public bool Equals(VoxelData _other)
		{
			if (BlockValue.Equals(_other.BlockValue))
			{
				return WaterValue.HasMass() == _other.WaterValue.HasMass();
			}
			return false;
		}

		public void Clear()
		{
			BlockValue = BlockValue.Air;
			WaterValue = WaterValue.Empty;
		}
	}

	public Vector3 pos;

	public Vector3i blockPos;

	public VoxelData voxelData;

	public BlockFace blockFace;

	public float distanceSq;

	public int clrIdx;

	public BlockValue blockValue => voxelData.BlockValue;

	public WaterValue waterValue => voxelData.WaterValue;

	public void Clear()
	{
		pos = Vector3.zero;
		blockPos = Vector3i.zero;
		blockFace = BlockFace.Top;
		voxelData.Clear();
		clrIdx = 0;
		distanceSq = 0f;
	}

	public void CopyFrom(HitInfoDetails _other)
	{
		pos = _other.pos;
		blockPos = _other.blockPos;
		blockFace = _other.blockFace;
		voxelData = _other.voxelData;
		clrIdx = _other.clrIdx;
		distanceSq = _other.distanceSq;
	}

	public HitInfoDetails Clone()
	{
		return new HitInfoDetails
		{
			pos = pos,
			blockPos = blockPos,
			blockFace = blockFace,
			voxelData = voxelData,
			clrIdx = clrIdx,
			distanceSq = distanceSq
		};
	}
}
