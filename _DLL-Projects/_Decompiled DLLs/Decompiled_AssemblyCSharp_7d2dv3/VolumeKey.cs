using System;

public struct VolumeKey : IEquatable<VolumeKey>
{
	public Vector3i boxMin;

	public Vector3i boxMax;

	public VolumeKey(Vector3i boxMin, Vector3i boxMax)
	{
		this.boxMin = boxMin;
		this.boxMax = boxMax;
	}

	public VolumeKey(SleeperVolume volume)
	{
		boxMin = volume.BoxMin;
		boxMax = volume.BoxMax;
	}

	public VolumeKey(TriggerVolume volume)
	{
		boxMin = volume.BoxMin;
		boxMax = volume.BoxMax;
	}

	public VolumeKey(WallVolume volume)
	{
		boxMin = volume.BoxMin;
		boxMax = volume.BoxMax;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(boxMin.GetHashCode(), boxMax.GetHashCode());
	}

	public bool Equals(VolumeKey other)
	{
		if (other.boxMin == boxMin)
		{
			return other.boxMax == boxMax;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is VolumeKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		return $"({boxMin}) ({boxMax})";
	}
}
