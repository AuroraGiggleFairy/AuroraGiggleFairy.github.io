using System;
using UnityEngine;

public struct SpawnPosition : IEquatable<SpawnPosition>
{
	public static SpawnPosition Undef;

	public int ClrIdx;

	public Vector3 position;

	public float heading;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInvalid;

	public SpawnPosition(bool _bInvalid)
	{
		ClrIdx = 0;
		position = Vector3.zero;
		heading = 0f;
		bInvalid = true;
	}

	public SpawnPosition(Vector3i _blockPos, float _heading)
	{
		ClrIdx = 0;
		position = _blockPos.ToVector3() + new Vector3(0.5f, 0f, 0.5f);
		heading = _heading;
		bInvalid = false;
	}

	public SpawnPosition(Vector3 _position, float _heading)
	{
		ClrIdx = 0;
		position = _position;
		heading = _heading;
		bInvalid = false;
	}

	public Vector3i ToBlockPos()
	{
		return new Vector3i(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y), Utils.Fastfloor(position.z));
	}

	public void Read(IBinaryReaderOrWriter _readerOrWriter, uint _version)
	{
		if (_version > 1)
		{
			ClrIdx = _readerOrWriter.ReadWrite((ushort)0);
		}
		position = _readerOrWriter.ReadWrite(Vector3.zero);
		heading = _readerOrWriter.ReadWrite(0f);
	}

	public void Read(PooledBinaryReader _br, uint _version)
	{
		if (_version > 1)
		{
			ClrIdx = _br.ReadUInt16();
		}
		position = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		heading = _br.ReadSingle();
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write((ushort)ClrIdx);
		_bw.Write(position.x);
		_bw.Write(position.y);
		_bw.Write(position.z);
		_bw.Write(heading);
	}

	public bool IsUndef()
	{
		return Equals(Undef);
	}

	public bool Equals(SpawnPosition _other)
	{
		if (position.Equals(_other.position) && heading == _other.heading)
		{
			return bInvalid == _other.bInvalid;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SpawnPosition)
		{
			return Equals((SpawnPosition)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((position.GetHashCode() * 397) ^ heading.GetHashCode()) * 397) ^ bInvalid.GetHashCode()) * 397) ^ ClrIdx.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("SpawnPoint {0}/{1}", position.ToCultureInvariantString(), heading.ToCultureInvariantString("0.0"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static SpawnPosition()
	{
		Undef = new SpawnPosition(_bInvalid: true);
	}
}
