using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEAC : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override bool AllowedBeforeAuth => true;

	public NetPackageEAC Setup(int _size, byte[] _data)
	{
		data = new byte[_size];
		Array.Copy(_data, data, _size);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadInt32();
		data = new byte[num];
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = _reader.ReadByte();
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(data.Length);
		for (int i = 0; i < data.Length; i++)
		{
			_writer.Write(data[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PlatformManager.MultiPlatform.AntiCheatServer?.HandleMessageFromClient(base.Sender, data);
		}
		else
		{
			PlatformManager.MultiPlatform.AntiCheatClient?.HandleMessageFromServer(data);
		}
	}

	public override int GetLength()
	{
		return 4 + ((data != null) ? data.Length : 0);
	}
}
