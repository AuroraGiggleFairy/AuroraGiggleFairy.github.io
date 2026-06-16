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

	public struct PropData : IEquatable<PropData>
	{
		public static readonly PropData Empty = new PropData
		{
			PropRef = PropRef.Default,
			PropValue = PropValue.AIR
		};

		[field: PublicizedFrom(EAccessModifier.Private)]
		public PropRef PropRef
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public PropValue PropValue
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public static PropData GetFrom(ChunkCluster cc, Transform transform)
		{
			if (!transform || !transform.TryGetComponent<PropReference>(out var component))
			{
				return Empty;
			}
			PropRef propRef = component.PropRef;
			PropValue prop = cc.GetProp(propRef);
			if (prop.IsAir)
			{
				return Empty;
			}
			return new PropData
			{
				PropRef = propRef,
				PropValue = prop
			};
		}

		public override string ToString()
		{
			return $"PropRef=[{PropRef}], PropValue=[{PropValue}]";
		}

		public void Clear()
		{
			PropRef = PropRef.Default;
			PropValue = Empty.PropValue;
		}

		public bool Equals(PropData other)
		{
			if (PropRef.Equals(other.PropRef))
			{
				return PropValue.Equals(other.PropValue);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PropData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(PropRef, PropValue);
		}

		public static bool operator ==(PropData left, PropData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PropData left, PropData right)
		{
			return !left.Equals(right);
		}
	}

	public Vector3 pos;

	public Vector3i blockPos;

	public VoxelData voxelData;

	public PropData propData;

	public BlockFace blockFace;

	public float distanceSq;

	public BlockValue blockValue => voxelData.BlockValue;

	public WaterValue waterValue => voxelData.WaterValue;

	public PropRef propRef => propData.PropRef;

	public PropValue propValue => propData.PropValue;

	public BlockValueRef blockValueRef
	{
		get
		{
			if (!propValue.IsAir)
			{
				return new BlockValueRef(propData.PropRef);
			}
			if (!blockValue.isair)
			{
				return new BlockValueRef(blockPos);
			}
			return BlockValueRef.None;
		}
	}

	public void Clear()
	{
		pos = Vector3.zero;
		blockPos = Vector3i.zero;
		blockFace = BlockFace.Top;
		voxelData.Clear();
		propData.Clear();
		distanceSq = 0f;
	}

	public void CopyFrom(HitInfoDetails _other)
	{
		pos = _other.pos;
		blockPos = _other.blockPos;
		blockFace = _other.blockFace;
		voxelData = _other.voxelData;
		propData = _other.propData;
		distanceSq = _other.distanceSq;
	}

	public HitInfoDetails Clone()
	{
		HitInfoDetails result = default(HitInfoDetails);
		result.CopyFrom(this);
		return result;
	}
}
