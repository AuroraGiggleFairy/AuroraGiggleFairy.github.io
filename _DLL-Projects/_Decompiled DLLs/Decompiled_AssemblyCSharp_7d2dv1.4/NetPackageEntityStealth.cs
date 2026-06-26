using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStealth : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsCrouching = 1;

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
		if (_world != null && ValidEntityIdForSender(id))
		{
			EntityPlayer entityPlayer = _world.GetEntity(id) as EntityPlayer;
			if (entityPlayer == null)
			{
				Log.Out("Discarding " + GetType().Name);
				return;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				entityPlayer.Crouching = (data & 1) > 0;
				return;
			}
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
