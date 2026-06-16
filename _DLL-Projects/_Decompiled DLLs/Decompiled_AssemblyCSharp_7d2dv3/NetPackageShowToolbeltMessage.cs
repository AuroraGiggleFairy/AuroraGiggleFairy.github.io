using UnityEngine.Scripting;

[Preserve]
public class NetPackageShowToolbeltMessage : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string toolbeltMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sound = "";

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageShowToolbeltMessage Setup(string _toolbeltMessage, string _sound)
	{
		toolbeltMessage = _toolbeltMessage;
		sound = _sound;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		toolbeltMessage = _br.ReadString();
		sound = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(toolbeltMessage);
		_bw.Write(sound);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			GameManager.ShowTooltip(_world.GetLocalPlayers()[0], toolbeltMessage, sound);
		}
	}

	public override int GetLength()
	{
		return 80;
	}
}
