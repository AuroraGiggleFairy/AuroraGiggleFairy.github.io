using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageExplosionClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> explosionChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int expType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blastPower;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blastRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageExplosionClient Setup(int _clrIdx, Vector3 _center, Quaternion _rotation, int _expType, int _blastPower, float _blastRadius, float _blockDamage, int _entityId, List<BlockChangeInfo> _explosionChanges)
	{
		clrIdx = _clrIdx;
		center = _center;
		rotation = _rotation;
		expType = _expType;
		blastPower = _blastPower;
		blastRadius = (int)_blastRadius;
		blockDamage = (int)_blockDamage;
		entityId = _entityId;
		explosionChanges.Clear();
		explosionChanges.AddRange(_explosionChanges);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		clrIdx = _br.ReadUInt16();
		center = StreamUtils.ReadVector3(_br);
		rotation = StreamUtils.ReadQuaterion(_br);
		expType = _br.ReadInt16();
		blastPower = _br.ReadInt16();
		blastRadius = _br.ReadInt16();
		blockDamage = _br.ReadInt16();
		entityId = _br.ReadInt32();
		int num = _br.ReadUInt16();
		explosionChanges.Clear();
		for (int i = 0; i < num; i++)
		{
			BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
			blockChangeInfo.Read(_br);
			explosionChanges.Add(blockChangeInfo);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((ushort)clrIdx);
		StreamUtils.Write(_bw, center);
		StreamUtils.Write(_bw, rotation);
		_bw.Write((short)expType);
		_bw.Write((ushort)blastPower);
		_bw.Write((ushort)blastRadius);
		_bw.Write((ushort)blockDamage);
		_bw.Write(entityId);
		_bw.Write((ushort)explosionChanges.Count);
		for (int i = 0; i < explosionChanges.Count; i++)
		{
			explosionChanges[i].Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().ExplosionClient(clrIdx, center, rotation, expType, blastPower, blastRadius, blockDamage, entityId, explosionChanges);
	}

	public override int GetLength()
	{
		return 24 + explosionChanges.Count * 30;
	}
}
