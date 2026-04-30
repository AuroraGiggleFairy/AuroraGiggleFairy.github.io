using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStealth : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsCrouching = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsSmellData = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsEating = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsSheltered = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsAlert = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort data;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public NetPackageEntityStealth Setup(EntityPlayer player, bool _isCrouching)
	{
		id = player.entityId;
		data = (ushort)(_isCrouching ? 1 : 0);
		return this;
	}

	public NetPackageEntityStealth Setup(EntityPlayerLocal player, int _smellRadius, bool _eating, bool _sheltered)
	{
		id = player.entityId;
		data = (ushort)(2 | (Utils.FastMin(_smellRadius, 255) << 8));
		if (_eating)
		{
			data |= 4;
		}
		if (_sheltered)
		{
			data |= 8;
		}
		return this;
	}

	public NetPackageEntityStealth Setup(EntityPlayer player, int _lightLevel, int _noiseVolume, bool _isAlert)
	{
		id = player.entityId;
		data = (ushort)((byte)_lightLevel | ((_noiseVolume & 0x7F) << 8));
		if (_isAlert)
		{
			data |= 32768;
		}
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		id = _br.ReadInt32();
		data = _br.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(id);
		_bw.Write(data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(id))
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(id) as EntityPlayer;
		if (entityPlayer == null)
		{
			Log.Out("Discarding " + GetType().Name);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if ((data & 2) > 0)
			{
				entityPlayer.Stealth.SetSmellRadiusTarget(data >> 8, (data & 4) > 0, (data & 8) > 0);
			}
			else
			{
				entityPlayer.Crouching = (data & 1) > 0;
			}
		}
		else
		{
			float lightLevel = (int)(byte)data;
			float noiseVolume = (data >> 8) & 0x7F;
			entityPlayer.Stealth.SetClientLevels(lightLevel, noiseVolume, (data & 0x8000) != 0);
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
