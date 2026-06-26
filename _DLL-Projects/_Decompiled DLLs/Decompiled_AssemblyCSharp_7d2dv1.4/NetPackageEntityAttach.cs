using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAttach : NetPackage
{
	public enum AttachType : byte
	{
		AttachServer,
		AttachClient,
		DetachServer,
		DetachClient
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int riderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int vehicleId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slot;

	[PublicizedFrom(EAccessModifier.Private)]
	public AttachType attachType;

	public NetPackageEntityAttach Setup(AttachType _attachType, int _riderId, int _vehicleId, int _slot)
	{
		attachType = _attachType;
		riderId = _riderId;
		vehicleId = _vehicleId;
		slot = _slot;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		attachType = (AttachType)_br.ReadByte();
		riderId = _br.ReadInt32();
		vehicleId = _br.ReadInt32();
		slot = _br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)attachType);
		_bw.Write(riderId);
		_bw.Write(vehicleId);
		_bw.Write((short)slot);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(riderId);
		if (entity == null)
		{
			return;
		}
		Entity entity2 = GameManager.Instance.World.GetEntity(vehicleId);
		switch (attachType)
		{
		case AttachType.AttachServer:
			if (!(entity2 == null))
			{
				int num = entity2.FindAttachSlot(entity);
				if (num < 0)
				{
					num = entity.AttachToEntity(entity2, slot);
				}
				if (num >= 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(AttachType.AttachClient, riderId, vehicleId, num));
				}
			}
			break;
		case AttachType.AttachClient:
			if (!(entity2 == null))
			{
				entity.AttachToEntity(entity2, slot);
			}
			break;
		case AttachType.DetachServer:
			entity.Detach();
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(AttachType.DetachClient, riderId, -1, -1), _onlyClientsAttachedToAnEntity: false, -1, riderId);
			break;
		case AttachType.DetachClient:
			entity.Detach();
			break;
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
