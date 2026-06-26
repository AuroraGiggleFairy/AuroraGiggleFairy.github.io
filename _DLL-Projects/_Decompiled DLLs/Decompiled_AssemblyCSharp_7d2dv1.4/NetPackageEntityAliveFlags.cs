using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAliveFlags : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFApproachingEnemy = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFApproachingPlayer = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFAimingGun = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFSpawned = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFJumping = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsBreakingBlocks = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsAlert = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsFlashlightOn = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsGodMode = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFCrouching = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort flags;

	public NetPackageEntityAliveFlags Setup(EntityAlive _entity)
	{
		entityId = _entity.entityId;
		flags = 0;
		if (_entity.AimingGun)
		{
			flags |= 4;
		}
		if (_entity.Spawned)
		{
			flags |= 8;
		}
		if (_entity.Jumping)
		{
			flags |= 16;
		}
		if (_entity.IsBreakingBlocks)
		{
			flags |= 32;
		}
		if (_entity.IsAlert)
		{
			flags |= 64;
		}
		if (_entity.inventory.IsFlashlightOn)
		{
			flags |= 128;
		}
		if (_entity.IsGodMode.Value)
		{
			flags |= 256;
		}
		if (_entity.IsCrouching)
		{
			flags |= 512;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		flags = _reader.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(flags);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityId))
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if ((bool)entityAlive)
		{
			entityAlive.AimingGun = (flags & 4) > 0;
			entityAlive.Spawned = (flags & 8) > 0;
			entityAlive.Jumping = (flags & 0x10) > 0;
			entityAlive.IsBreakingBlocks = (flags & 0x20) > 0;
			entityAlive.IsGodMode.Value = (flags & 0x100) > 0;
			entityAlive.Crouching = (flags & 0x200) > 0;
			if (entityAlive.isEntityRemote)
			{
				entityAlive.bReplicatedAlertFlag = (flags & 0x40) > 0;
			}
			entityAlive.inventory.SetFlashlight((flags & 0x80) > 0);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}
}
