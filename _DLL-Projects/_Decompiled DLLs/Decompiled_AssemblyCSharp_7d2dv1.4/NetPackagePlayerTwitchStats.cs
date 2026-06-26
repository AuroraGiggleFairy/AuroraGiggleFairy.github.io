using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerTwitchStats : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchSafe;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteLockTypes twitchVoteLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchVisionDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer.TwitchActionsStates twitchActionsEnabled;

	public NetPackagePlayerTwitchStats Setup(EntityAlive _entity)
	{
		entityId = _entity.entityId;
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		if ((bool)entityPlayer)
		{
			twitchEnabled = entityPlayer.TwitchEnabled;
			twitchSafe = entityPlayer.TwitchSafe;
			twitchVoteLock = entityPlayer.TwitchVoteLock;
			twitchVisionDisabled = entityPlayer.TwitchVisionDisabled;
			twitchActionsEnabled = entityPlayer.TwitchActionsEnabled;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		twitchEnabled = _reader.ReadBoolean();
		twitchSafe = _reader.ReadBoolean();
		twitchVoteLock = (TwitchVoteLockTypes)_reader.ReadByte();
		twitchVisionDisabled = _reader.ReadBoolean();
		twitchActionsEnabled = (EntityPlayer.TwitchActionsStates)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(twitchEnabled);
		_writer.Write(twitchSafe);
		_writer.Write((byte)twitchVoteLock);
		_writer.Write(twitchVisionDisabled);
		_writer.Write((byte)twitchActionsEnabled);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayer entityPlayer = _world.GetEntity(entityId) as EntityPlayer;
			if ((bool)entityPlayer)
			{
				entityPlayer.TwitchEnabled = twitchEnabled;
				entityPlayer.TwitchSafe = twitchSafe;
				entityPlayer.TwitchVoteLock = twitchVoteLock;
				entityPlayer.TwitchVisionDisabled = twitchVisionDisabled;
				entityPlayer.TwitchActionsEnabled = twitchActionsEnabled;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityPlayer), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}
}
