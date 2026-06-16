using System;
using System.Runtime.CompilerServices;

namespace PrefabVolumes;

public abstract class PrefabVolumeAbs
{
	public enum EVolumeType : byte
	{
		Sleeper,
		Teleport,
		Info,
		Wall,
		Trigger,
		Marker
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool used;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i startPosInternal;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i sizeInternal;

	public bool Used => used;

	public virtual Vector3i startPos
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return startPosInternal;
		}
		set
		{
			startPosInternal = value;
		}
	}

	public virtual Vector3i size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return sizeInternal;
		}
		set
		{
			sizeInternal = value;
		}
	}

	public abstract EVolumeType VolumeType { get; }

	public abstract int SerializedSize { get; }

	public void Use(Vector3i _startPos, Vector3i _size)
	{
		used = true;
		startPos = _startPos;
		size = _size;
	}

	public virtual void Reset()
	{
		used = false;
		startPos = default(Vector3i);
		size = default(Vector3i);
	}

	public void MarkUnused()
	{
		used = false;
	}

	public virtual void Read(PooledBinaryReader _br)
	{
		used = _br.ReadBoolean();
		startPos = StreamUtils.ReadVector3i(_br);
		size = StreamUtils.ReadVector3i(_br);
	}

	public virtual void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(Used);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
	}

	public virtual void RotateY(bool _bLeft, Vector3i _prefabSize)
	{
		Vector3i vector3i = size;
		Vector3i vector3i2 = startPos;
		Vector3i vector3i3 = vector3i2 + vector3i;
		if (_bLeft)
		{
			vector3i2 = new Vector3i(_prefabSize.z - vector3i2.z, vector3i2.y, vector3i2.x);
			vector3i3 = new Vector3i(_prefabSize.z - vector3i3.z, vector3i3.y, vector3i3.x);
		}
		else
		{
			vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, _prefabSize.x - vector3i2.x);
			vector3i3 = new Vector3i(vector3i3.z, vector3i3.y, _prefabSize.x - vector3i3.x);
		}
		if (vector3i2.x > vector3i3.x)
		{
			MathUtils.Swap(ref vector3i2.x, ref vector3i3.x);
		}
		if (vector3i2.z > vector3i3.z)
		{
			MathUtils.Swap(ref vector3i2.z, ref vector3i3.z);
		}
		startPos = vector3i2;
		MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		size = vector3i;
	}

	public void Move(Vector3i _moveVector)
	{
		startPos += _moveVector;
	}

	public void Resize(int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest, bool _allowXBelow1 = false, bool _allowYBelow1 = false, bool _allowZBelow1 = false)
	{
		Vector3i vector3i = size;
		vector3i += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
		startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
		if (!_allowXBelow1 && vector3i.x < 2)
		{
			vector3i = new Vector3i(1, vector3i.y, vector3i.z);
		}
		if (!_allowYBelow1 && vector3i.y < 2)
		{
			vector3i = new Vector3i(vector3i.x, 1, vector3i.z);
		}
		if (!_allowZBelow1 && vector3i.z < 2)
		{
			vector3i = new Vector3i(vector3i.x, vector3i.y, 1);
		}
		size = vector3i;
	}

	public abstract PrefabVolumeAbs Clone();

	public abstract void CopyValues(PrefabVolumeAbs _target, bool _nonBasicOnly = false);

	public static PrefabVolumeAbs CreateByType(EVolumeType _volumeType)
	{
		return _volumeType switch
		{
			EVolumeType.Sleeper => new PrefabSleeperVolume(), 
			EVolumeType.Teleport => new PrefabTeleportVolume(), 
			EVolumeType.Info => new PrefabInfoVolume(), 
			EVolumeType.Wall => new PrefabWallVolume(), 
			EVolumeType.Trigger => new PrefabTriggerVolume(), 
			EVolumeType.Marker => new Marker(), 
			_ => throw new ArgumentOutOfRangeException("_volumeType", $"Invalid volumeType {_volumeType}"), 
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public PrefabVolumeAbs()
	{
	}
}
public abstract class PrefabVolumeAbs<T> : PrefabVolumeAbs where T : PrefabVolumeAbs<T>, new()
{
	public override PrefabVolumeAbs Clone()
	{
		return CloneGeneric();
	}

	public virtual T CloneGeneric()
	{
		T val = new T();
		CopyValues(val);
		return val;
	}

	public override void CopyValues(PrefabVolumeAbs _target, bool _nonBasicOnly = false)
	{
		if (!(_target is T target))
		{
			throw new ArgumentException("Can not copy values to volume of type " + _target.VolumeType.ToStringCached() + " from volume of type " + VolumeType.ToStringCached(), "_target");
		}
		CopyValues(target, _nonBasicOnly);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CopyValues(PrefabVolumeAbs<T> _target, bool _nonBasicOnly = false)
	{
		_target.used = base.Used;
		_target.startPos = startPos;
		_target.size = size;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public PrefabVolumeAbs()
	{
	}
}
