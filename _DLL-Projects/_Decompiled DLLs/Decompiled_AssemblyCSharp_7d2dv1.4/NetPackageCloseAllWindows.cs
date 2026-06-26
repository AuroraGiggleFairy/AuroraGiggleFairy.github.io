using UnityEngine.Scripting;

[Preserve]
public class NetPackageCloseAllWindows : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int _playerIdToClose = -1;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageCloseAllWindows Setup(int entityToClose)
	{
		_playerIdToClose = entityToClose;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		_playerIdToClose = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(_playerIdToClose);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(_playerIdToClose);
			if (localPlayerFromID != null)
			{
				localPlayerFromID.PlayerUI.windowManager.CloseAllOpenWindows();
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
