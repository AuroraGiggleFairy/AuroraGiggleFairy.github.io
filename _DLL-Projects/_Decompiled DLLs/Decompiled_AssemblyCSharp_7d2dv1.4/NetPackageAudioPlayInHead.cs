using Audio;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAudioPlayInHead : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string soundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnique;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageAudioPlayInHead Setup(string _soundName, bool _isUnique)
	{
		soundName = _soundName;
		isUnique = _isUnique;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		soundName = _br.ReadString();
		isUnique = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(soundName);
		_bw.Write(isUnique);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			Manager.PlayInsidePlayerHead(soundName, -1, 0f, isLooping: false, isUnique);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
