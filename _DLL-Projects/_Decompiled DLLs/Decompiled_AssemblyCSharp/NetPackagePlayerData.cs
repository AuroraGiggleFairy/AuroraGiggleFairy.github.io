using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerData : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public PlayerDataFile playerDataFile;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackagePlayerData Setup(EntityPlayer _player)
	{
		playerDataFile = new PlayerDataFile();
		if (_player != null)
		{
			playerDataFile.FromPlayer(_player);
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		playerDataFile = new PlayerDataFile();
		playerDataFile.Read(_reader, uint.MaxValue);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		playerDataFile.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidEntityIdForSender(playerDataFile.id))
		{
			_callbacks.SavePlayerData(base.Sender, playerDataFile);
		}
	}

	public override int GetLength()
	{
		return 50;
	}
}
