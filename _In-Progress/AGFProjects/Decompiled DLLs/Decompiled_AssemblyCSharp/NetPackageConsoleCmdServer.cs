using UnityEngine.Scripting;

[Preserve]
public class NetPackageConsoleCmdServer : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string cmd;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageConsoleCmdServer Setup(string _cmd)
	{
		cmd = _cmd;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		cmd = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(cmd);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.ServerConsoleCommand(base.Sender, cmd);
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
