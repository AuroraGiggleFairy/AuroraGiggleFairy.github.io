using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageExplosionInitiate : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosionData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoveBlockAtExplPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValueExplosive;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageExplosionInitiate Setup(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, float _delay, bool _bRemoveBlockAtExplPosition, ItemValue _itemValueExplosive)
	{
		worldPos = _worldPos;
		blockPos = _blockPos;
		rotation = _rotation;
		explosionData = _explosionData;
		entityId = _entityId;
		delay = _delay;
		bRemoveBlockAtExplPosition = _bRemoveBlockAtExplPosition;
		itemValueExplosive = _itemValueExplosive;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		worldPos = StreamUtils.ReadVector3(_br);
		blockPos = StreamUtils.ReadVector3i(_br);
		rotation = StreamUtils.ReadQuaterion(_br);
		int count = _br.ReadUInt16();
		explosionData = new ExplosionData(_br.ReadBytes(count));
		entityId = _br.ReadInt32();
		delay = _br.ReadSingle();
		bRemoveBlockAtExplPosition = _br.ReadBoolean();
		if (_br.ReadBoolean())
		{
			itemValueExplosive = new ItemValue();
			itemValueExplosive.Read(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, worldPos);
		StreamUtils.Write(_bw, blockPos);
		StreamUtils.Write(_bw, rotation);
		byte[] array = explosionData.ToByteArray();
		_bw.Write((ushort)array.Length);
		_bw.Write(array);
		_bw.Write(entityId);
		_bw.Write(delay);
		_bw.Write(bRemoveBlockAtExplPosition);
		_bw.Write(itemValueExplosive != null);
		if (itemValueExplosive != null)
		{
			itemValueExplosive.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().ExplosionServer(worldPos, blockPos, rotation, explosionData, entityId, delay, bRemoveBlockAtExplPosition, itemValueExplosive);
	}

	public override int GetLength()
	{
		return 70;
	}
}
